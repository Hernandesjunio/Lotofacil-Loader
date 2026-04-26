using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Lotofacil.Loader.Application;
using Lotofacil.Loader.Composition;
using Lotofacil.Loader.FunctionApp;
using Lotofacil.Loader.FunctionApp.Functions;
using Lotofacil.Loader.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests.FunctionApp.V0;

public sealed class LotofacilFunctionIntegrationTests
{
    [SkippableFact]
    public async Task Full_flow_trigger_to_usecase_to_persistences_uses_fake_lotodicas_and_writes_blob_then_table()
    {
        // Determinismo: inputs explícitos + fake HTTP com fixtures fixas.
        const string token = "test-token";

        var storageConn = Environment.GetEnvironmentVariable("LOT0_AZURITE_CONNECTION_STRING")
                         ?? Environment.GetEnvironmentVariable("AZURITE_CONNECTION_STRING")
                         ?? "UseDevelopmentStorage=true";

        var reachable = await AzuriteProbe.IsStorageReachableAsync(storageConn, CancellationToken.None);
        Skip.IfNot(reachable, "Azurite não está acessível. Inicie o Azurite local e/ou defina AZURITE_CONNECTION_STRING.");

        var containerName = $"lotofacil-it-{Guid.NewGuid():n}";
        const string blobName = "Lotofacil";
        const string tableName = "LotofacilState";

        await using var fake = new LotodicasFakeServer(token)
            .WithLatestResponseJson(LatestJson(latestId: 7))
            .WithContestResponseJson(6, ContestJson(id: 6, date: "2026-04-27", winners15: 0))
            .WithContestResponseJson(7, ContestJson(id: 7, date: "2026-04-27", winners15: 5));

        await fake.StartAsync(CancellationToken.None);

        // Seed (pré-condição): Table state inicial => LastLoadedContestId = 5.
        var table = new TableClient(storageConn, tableName);
        await table.CreateIfNotExistsAsync();
        await table.UpsertEntityAsync(new TableEntity("Lotofacil", "Loader")
        {
            ["LastLoadedContestId"] = 5,
            ["LastLoadedDrawDate"] = "2026-04-01",
            ["LastUpdatedAtUtc"] = DateTimeOffset.Parse("2026-04-01T00:00:00Z")
        });

        // Seed opcional: blob inicial vazio (para garantir que o fluxo escreve um documento canônico).
        var blobContainer = new BlobContainerClient(storageConn, containerName);
        await blobContainer.CreateIfNotExistsAsync();
        var blob = blobContainer.GetBlobClient(blobName);
        await blob.UploadAsync(BinaryData.FromString("{\"draws\":[]}"), overwrite: true);

        // DI: infra real, mas com stores "recording" para provar ordem blob→table.
        // Observação: o validator V0 exige HTTPS, mas a integração controlada usa Fake HTTP local.
        // Para manter o trigger completo, usamos um config "dummy" (HTTPS válido) apenas para o validator.
        var infraCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Lotodicas:BaseUrl"] = fake.BaseUrl.ToString().TrimEnd('/'),
                ["Lotodicas:Token"] = token,
                ["Storage:ConnectionString"] = storageConn,
                ["Storage:BlobContainer"] = containerName,
                ["Storage:LotofacilBlobName"] = blobName,
                ["Storage:LotofacilStateTable"] = tableName
            })
            .Build();

        var validatorCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Lotodicas:BaseUrl"] = "https://example.invalid",
                ["Lotodicas:Token"] = token,
                ["Storage:ConnectionString"] = storageConn,
                ["Storage:BlobContainer"] = containerName,
                ["Storage:LotofacilBlobName"] = blobName,
                ["Storage:LotofacilStateTable"] = tableName
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLotofacilLoaderV0Core();
        services.AddLotofacilLoaderV0Infrastructure(infraCfg);

        // Validator roda no trigger (com config HTTPS dummy, para não bloquear o fluxo em testes).
        services.AddSingleton(new V0EnvironmentValidator(validatorCfg));
        services.AddSingleton<LotofacilLoaderTimerFunction>();

        var seq = new RecordingSequence();
        services.AddSingleton(seq);

        // Decorators: preservam persistência real + provam ordem.
        services.AddSingleton<RecordingBlobStore>(sp =>
        {
            var inner = new AzureBlobLotofacilBlobStore(Microsoft.Extensions.Options.Options.Create(new StorageOptions
            {
                ConnectionString = storageConn,
                BlobContainer = containerName,
                LotofacilBlobName = blobName,
                LotofacilStateTable = tableName
            }));
            return new RecordingBlobStore(inner, seq);
        });
        services.AddSingleton<RecordingStateStore>(sp =>
        {
            var inner = new AzureTableLotofacilStateStore(Microsoft.Extensions.Options.Options.Create(new StorageOptions
            {
                ConnectionString = storageConn,
                BlobContainer = containerName,
                LotofacilBlobName = blobName,
                LotofacilStateTable = tableName
            }));
            return new RecordingStateStore(inner, seq);
        });

        // Rewire ports para usar os recording stores.
        services.AddSingleton<ILotofacilBlobStore>(sp => sp.GetRequiredService<RecordingBlobStore>());
        services.AddSingleton<ILotofacilStateStore>(sp => sp.GetRequiredService<RecordingStateStore>());

        await using var sp = services.BuildServiceProvider(validateScopes: true);

        // Execute: trigger -> use case -> persistências.
        var fn = sp.GetRequiredService<LotofacilLoaderTimerFunction>();
        await fn.RunAsync(timer: null!, ct: CancellationToken.None);

        // Asserts: chamadas esperadas ao fake do Lotodicas (endpoints + parâmetros).
        var calls = fake.Calls;
        Assert.Collection(
            calls,
            c =>
            {
                Assert.Equal("GET", c.Method);
                Assert.Equal("/api/v2/lotofacil/results/last", c.Path);
                Assert.Contains("token=", c.QueryString, StringComparison.Ordinal);
                Assert.Equal(token, c.Token);
            },
            c =>
            {
                Assert.Equal("GET", c.Method);
                Assert.Equal("/api/v2/lotofacil/results/6", c.Path);
                Assert.Equal(token, c.Token);
            },
            c =>
            {
                Assert.Equal("GET", c.Method);
                Assert.Equal("/api/v2/lotofacil/results/7", c.Path);
                Assert.Equal(token, c.Token);
            }
        );

        // Asserts: blob final (comparação canônica).
        var dl = await blob.DownloadContentAsync();
        var gotJson = dl.Value.Content.ToString();

        var expected = JsonNode.Parse(ExpectedBlobJsonFor6And7())!;
        var got = JsonNode.Parse(gotJson)!;
        Assert.True(JsonNode.DeepEquals(expected, got), $"Blob final não bate com golden canônico.\nGot: {gotJson}");

        // Asserts: state final no Table (checkpoint consistente).
        var persisted = await table.GetEntityAsync<TableEntity>("Lotofacil", "Loader");
        Assert.Equal(7, persisted.Value.GetInt32("LastLoadedContestId"));
        Assert.Equal("2026-04-27", persisted.Value.GetString("LastLoadedDrawDate"));

        // Asserts: ordem blob→table (prova via recording wrappers).
        var recBlob = sp.GetRequiredService<RecordingBlobStore>();
        var recState = sp.GetRequiredService<RecordingStateStore>();
        Assert.True(recBlob.SequenceIdOfLastWrite > 0);
        Assert.True(recState.SequenceIdOfLastWrite > 0);
        Assert.True(
            recBlob.SequenceIdOfLastWrite < recState.SequenceIdOfLastWrite,
            "Contrato V0: persistir blob antes do Table state."
        );
    }

    private static string LatestJson(int latestId) =>
        JsonSerializer.Serialize(new { data = new { draw_number = latestId } });

    private static string ContestJson(int id, string date, int winners15)
    {
        var obj = new
        {
            data = new
            {
                draw_number = id,
                draw_date = date,
                drawing = new { draw = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } },
                prizes = new[] { new { name = "15 acertos", winners = winners15 } }
            }
        };
        return JsonSerializer.Serialize(obj);
    }

    private static string ExpectedBlobJsonFor6And7() =>
        """
        {
          "draws": [
            {
              "contest_id": 6,
              "draw_date": "2026-04-27",
              "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15],
              "winners_15": 0,
              "has_winner_15": false
            },
            {
              "contest_id": 7,
              "draw_date": "2026-04-27",
              "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15],
              "winners_15": 5,
              "has_winner_15": true
            }
          ]
        }
        """;

}


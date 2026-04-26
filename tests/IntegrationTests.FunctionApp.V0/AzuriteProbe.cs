using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;

namespace IntegrationTests.FunctionApp.V0;

internal static class AzuriteProbe
{
    // Preferência: Azurite sem Docker. Este probe só verifica "está acessível?" para decidir skip.
    internal static async Task<bool> IsStorageReachableAsync(string connectionString, CancellationToken ct)
    {
        try
        {
            var blobService = new BlobServiceClient(connectionString);
            _ = await blobService.GetPropertiesAsync(cancellationToken: ct);

            var tableService = new TableServiceClient(connectionString);
            _ = await tableService.GetPropertiesAsync(cancellationToken: ct);

            return true;
        }
        catch (RequestFailedException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}


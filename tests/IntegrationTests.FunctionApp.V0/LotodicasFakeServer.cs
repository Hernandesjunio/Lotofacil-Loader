using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.FunctionApp.V0;

internal sealed class LotodicasFakeServer : IAsyncDisposable
{
    private readonly string _token;
    private readonly ConcurrentQueue<RecordedCall> _calls = new();
    private readonly Dictionary<int, string> _byId = new();
    private string? _lastJson;
    private IHost? _host;

    public LotodicasFakeServer(string token)
    {
        _token = token;
    }

    public Uri BaseUrl { get; private set; } = new Uri("http://127.0.0.1:0");

    public IReadOnlyList<RecordedCall> Calls => _calls.ToArray();

    public LotodicasFakeServer WithLatestResponseJson(string json)
    {
        _lastJson = json;
        return this;
    }

    public LotodicasFakeServer WithContestResponseJson(int id, string json)
    {
        _byId[id] = json;
        return this;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (_host is not null)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel(o => { o.ListenLocalhost(0); });

        var app = builder.Build();

        app.MapGet("/api/v2/lotofacil/results/last", async context =>
        {
            await HandleAsync(context, endpoint: "last", contestId: null);
        });

        app.MapGet("/api/v2/lotofacil/results/{id:int}", async context =>
        {
            var id = int.Parse(context.Request.RouteValues["id"]!.ToString()!);
            await HandleAsync(context, endpoint: "by_id", contestId: id);
        });

        _host = app;
        await app.StartAsync(ct);

        var addrs = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
            .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()
            ?.Addresses;

        var first = addrs?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first))
        {
            throw new InvalidOperationException("Fake server did not expose listening address.");
        }

        BaseUrl = new Uri(first.TrimEnd('/') + "/");
    }

    private async Task HandleAsync(HttpContext ctx, string endpoint, int? contestId)
    {
        var token = ctx.Request.Query["token"].ToString();
        _calls.Enqueue(new RecordedCall(
            Method: ctx.Request.Method,
            Path: ctx.Request.Path.Value ?? "",
            QueryString: ctx.Request.QueryString.Value ?? "",
            Endpoint: endpoint,
            ContestId: contestId,
            Token: token
        ));

        if (!string.Equals(token, _token, StringComparison.Ordinal))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("{\"error\":\"invalid_token\"}");
            return;
        }

        string? payload = endpoint switch
        {
            "last" => _lastJson,
            "by_id" when contestId is not null && _byId.TryGetValue(contestId.Value, out var j) => j,
            _ => null
        };

        if (payload is null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await ctx.Response.WriteAsync("{\"error\":\"fixture_not_found\"}");
            return;
        }

        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        await ctx.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(payload));
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is null)
        {
            return;
        }

        try
        {
            await _host.StopAsync();
        }
        finally
        {
            _host.Dispose();
            _host = null;
        }
    }

    internal sealed record RecordedCall(
        string Method,
        string Path,
        string QueryString,
        string Endpoint,
        int? ContestId,
        string Token
    );
}


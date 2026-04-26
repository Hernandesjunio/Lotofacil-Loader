using System.Collections.Concurrent;
using Lotofacil.Loader.Application;

namespace IntegrationTests.FunctionApp.V0;

internal sealed class RecordingSequence
{
    private int _n;
    public int Next() => Interlocked.Increment(ref _n);
}

internal sealed class RecordingBlobStore : ILotofacilBlobStore
{
    private readonly ILotofacilBlobStore _inner;
    private readonly RecordingSequence _seq;

    public RecordingBlobStore(ILotofacilBlobStore inner, RecordingSequence seq)
    {
        _inner = inner;
        _seq = seq;
    }

    public ConcurrentQueue<string> Events { get; } = new();
    public int SequenceIdOfLastWrite { get; private set; } = -1;

    public Task<object?> TryReadRawAsync(CancellationToken ct) => _inner.TryReadRawAsync(ct);

    public async Task WriteRawAsync(object document, CancellationToken ct)
    {
        SequenceIdOfLastWrite = _seq.Next();
        Events.Enqueue($"BlobWrite:{SequenceIdOfLastWrite}");
        await _inner.WriteRawAsync(document, ct);
    }
}

internal sealed class RecordingStateStore : ILotofacilStateStore
{
    private readonly ILotofacilStateStore _inner;
    private readonly RecordingSequence _seq;

    public RecordingStateStore(ILotofacilStateStore inner, RecordingSequence seq)
    {
        _inner = inner;
        _seq = seq;
    }

    public ConcurrentQueue<string> Events { get; } = new();
    public int SequenceIdOfLastWrite { get; private set; } = -1;

    public Task<object?> TryReadRawAsync(CancellationToken ct) => _inner.TryReadRawAsync(ct);

    public async Task WriteRawAsync(object state, CancellationToken ct)
    {
        SequenceIdOfLastWrite = _seq.Next();
        Events.Enqueue($"StateWrite:{SequenceIdOfLastWrite}");
        await _inner.WriteRawAsync(state, ct);
    }
}


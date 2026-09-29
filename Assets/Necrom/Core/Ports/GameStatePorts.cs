using System.Threading;
using System.Threading.Tasks;

namespace Necrom.Core.Ports
{
    public interface IPlayerIdentity
    {
        string CurrentPlayerId { get; }
    }

    public interface IGameStateStore
    {
        ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default);
        ValueTask SaveAsync(string playerId, string schemaVersion, string serializedState, CancellationToken cancellationToken = default);
    }

    public interface IGameEventSink
    {
        ValueTask RecordAsync(string eventName, string schemaVersion, CancellationToken cancellationToken = default);
    }
}

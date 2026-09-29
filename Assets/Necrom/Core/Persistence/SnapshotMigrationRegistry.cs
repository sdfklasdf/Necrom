using System;
using System.Collections.Generic;

namespace Necrom.Core.Persistence
{
    public interface IGameStateSnapshotMigration
    {
        string FromVersion { get; }
        string ToVersion { get; }
        GameStateSnapshot Migrate(GameStateSnapshot source);
    }

    public sealed class SnapshotMigrationRegistry
    {
        private readonly Dictionary<string, IGameStateSnapshotMigration> _bySource =
            new Dictionary<string, IGameStateSnapshotMigration>(StringComparer.Ordinal);

        public void Register(IGameStateSnapshotMigration migration)
        {
            if (migration == null) throw new ArgumentNullException(nameof(migration));
            if (string.IsNullOrWhiteSpace(migration.FromVersion) || string.IsNullOrWhiteSpace(migration.ToVersion))
                throw new InvalidOperationException("Migration versions are required.");
            if (string.Equals(migration.FromVersion, migration.ToVersion, StringComparison.Ordinal))
                throw new InvalidOperationException("Migration must change schema version.");
            if (_bySource.ContainsKey(migration.FromVersion))
                throw new InvalidOperationException("Only one migration path may start from a schema version.");
            _bySource.Add(migration.FromVersion, migration);
        }

        public GameStateSnapshot MigrateTo(GameStateSnapshot source, string targetVersion)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(targetVersion)) throw new ArgumentException("Target version is required.", nameof(targetVersion));

            GameStateSnapshotValidator.Validate(source);
            var current = source;
            var visited = new HashSet<string>(StringComparer.Ordinal);

            while (!string.Equals(current.SchemaVersion, targetVersion, StringComparison.Ordinal))
            {
                if (!visited.Add(current.SchemaVersion)) throw new InvalidOperationException("Migration cycle detected.");
                if (!_bySource.TryGetValue(current.SchemaVersion, out var migration))
                    throw new InvalidOperationException("No migration path to requested schema version.");

                var next = migration.Migrate(current);
                if (next == null) throw new InvalidOperationException("Migration returned no snapshot.");
                if (!string.Equals(next.SchemaVersion, migration.ToVersion, StringComparison.Ordinal))
                    throw new InvalidOperationException("Migration returned an unexpected schema version.");

                GameStateSnapshotValidator.Validate(next);
                current = next;
            }

            return current;
        }
    }
}

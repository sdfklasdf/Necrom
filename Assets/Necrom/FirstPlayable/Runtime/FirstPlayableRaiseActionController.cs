using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableRaiseActionController : MonoBehaviour
    {
        private EnemySpawnController _enemySpawn;
        private FirstPlayableRaiseCommandInputHook _inputHook;
        private Func<RaiseSource, RaiseIntoFormationCommand> _commandFactory;
        private Func<string> _raisedEventIdProvider;
        private Func<string> _assignedEventIdProvider;

        public void Initialize(
            EnemySpawnController enemySpawn,
            FirstPlayableRaiseCommandInputHook inputHook,
            Func<RaiseSource, RaiseIntoFormationCommand> commandFactory,
            Func<string> raisedEventIdProvider,
            Func<string> assignedEventIdProvider)
        {
            if (enemySpawn == null) throw new ArgumentNullException(nameof(enemySpawn));
            if (inputHook == null) throw new ArgumentNullException(nameof(inputHook));
            if (commandFactory == null) throw new ArgumentNullException(nameof(commandFactory));
            if (raisedEventIdProvider == null) throw new ArgumentNullException(nameof(raisedEventIdProvider));
            if (assignedEventIdProvider == null) throw new ArgumentNullException(nameof(assignedEventIdProvider));
            if (_enemySpawn != null)
                throw new InvalidOperationException("Raise action controller is already initialized.");

            _enemySpawn = enemySpawn;
            _inputHook = inputHook;
            _commandFactory = commandFactory;
            _raisedEventIdProvider = raisedEventIdProvider;
            _assignedEventIdProvider = assignedEventIdProvider;
        }

        public bool CanExecute()
            => TryGetCurrentSource(out _);

        public CommandResult Execute()
        {
            EnsureInitialized();

            if (!TryGetCurrentSource(out var source))
                throw new InvalidOperationException("Current target is not eligible for Raise.");

            var command = _commandFactory(source);
            if (command == null)
                throw new InvalidOperationException("Raise command factory returned no command.");
            if (!ReferenceEquals(command.Source, source))
                throw new InvalidOperationException("Raise command source does not match the current eligible target.");

            var raisedEventId = _raisedEventIdProvider();
            var assignedEventId = _assignedEventIdProvider();

            return _inputHook.Submit(
                command,
                raisedEventId,
                assignedEventId);
        }

        private bool TryGetCurrentSource(out RaiseSource source)
        {
            source = null;

            if (_enemySpawn == null || _inputHook == null || _commandFactory == null ||
                _raisedEventIdProvider == null || _assignedEventIdProvider == null)
                return false;

            var target = _enemySpawn.CurrentTarget;
            return target != null && target.TryGetAvailableRaiseSource(out source);
        }

        private void EnsureInitialized()
        {
            if (_enemySpawn == null || _inputHook == null || _commandFactory == null ||
                _raisedEventIdProvider == null || _assignedEventIdProvider == null)
                throw new InvalidOperationException("Raise action controller is not initialized.");
        }
    }
}
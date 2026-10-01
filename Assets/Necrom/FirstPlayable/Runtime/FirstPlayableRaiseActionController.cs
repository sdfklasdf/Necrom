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
        private FirstPlayableAlliedRosterController _alliedRoster;
        private Func<RaiseIntoFormationCommand, BasicAutoBehaviorSpec>
            _alliedBehaviorSpecProvider;

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

        public void ConfigureAlliedActivation(
            FirstPlayableAlliedRosterController alliedRoster,
            Func<RaiseIntoFormationCommand, BasicAutoBehaviorSpec>
                alliedBehaviorSpecProvider)
        {
            EnsureInitialized();
            if (alliedRoster == null)
                throw new ArgumentNullException(nameof(alliedRoster));
            if (alliedBehaviorSpecProvider == null)
                throw new ArgumentNullException(nameof(alliedBehaviorSpecProvider));
            if (_alliedRoster != null)
                throw new InvalidOperationException(
                    "Allied activation is already configured.");

            _alliedRoster = alliedRoster;
            _alliedBehaviorSpecProvider = alliedBehaviorSpecProvider;
        }

        public bool CanExecute()
            => TryGetCurrentSource(out _);

        public CommandResult Execute()
        {
            EnsureInitialized();

            if (!TryGetCurrentSource(out var source))
                throw new InvalidOperationException(
                    "Current target is not eligible for Raise.");

            var command = _commandFactory(source);
            if (command == null)
                throw new InvalidOperationException(
                    "Raise command factory returned no command.");
            if (!ReferenceEquals(command.Source, source))
                throw new InvalidOperationException(
                    "Raise command source does not match the current eligible target.");

            BasicAutoBehaviorSpec alliedSpec = null;
            if (_alliedRoster != null)
            {
                alliedSpec = _alliedBehaviorSpecProvider(command);
                if (alliedSpec == null)
                    throw new InvalidOperationException(
                        "Allied behavior policy returned no spec.");
                _alliedRoster.EnsureCanActivate(
                    command,
                    alliedSpec);
            }

            var raisedEventId = _raisedEventIdProvider();
            var assignedEventId = _assignedEventIdProvider();

            var result = _inputHook.Submit(
                command,
                raisedEventId,
                assignedEventId);

            if (_alliedRoster != null)
            {
                _alliedRoster.ActivateCommitted(
                    command,
                    alliedSpec);
            }

            return result;
        }

        private bool TryGetCurrentSource(out RaiseSource source)
        {
            source = null;

            if (_enemySpawn == null ||
                _inputHook == null ||
                _commandFactory == null ||
                _raisedEventIdProvider == null ||
                _assignedEventIdProvider == null)
                return false;

            var target = _enemySpawn.CurrentTarget;
            return target != null &&
                target.TryGetAvailableRaiseSource(out source);
        }

        private void EnsureInitialized()
        {
            if (_enemySpawn == null ||
                _inputHook == null ||
                _commandFactory == null ||
                _raisedEventIdProvider == null ||
                _assignedEventIdProvider == null)
            {
                throw new InvalidOperationException(
                    "Raise action controller is not initialized.");
            }
        }
    }
}
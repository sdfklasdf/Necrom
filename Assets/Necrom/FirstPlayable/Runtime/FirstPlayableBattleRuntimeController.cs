using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableBattleRuntimeController : MonoBehaviour
    {
        private FirstPlayableApplicationService _application;
        private EncounterBoundaryController _boundary;
        private EncounterLayoutConfig _config;
        private NecromancerAnchorController _necromancer;
        private EnemySpawnController _enemies;

        public BattlePhase Phase
        {
            get
            {
                EnsureInitialized();
                return _application.BattlePhase;
            }
        }

        public long Revision
        {
            get
            {
                EnsureInitialized();
                return _application.BattleRevision;
            }
        }

        public void Initialize(
            FirstPlayableApplicationService application,
            EncounterBoundaryController boundary,
            EncounterLayoutConfig config,
            NecromancerAnchorController necromancer,
            EnemySpawnController enemies)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _boundary = boundary ?? throw new ArgumentNullException(nameof(boundary));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _necromancer = necromancer ?? throw new ArgumentNullException(nameof(necromancer));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
        }

        public CommandResult StartBattle(StartBattleCommand command)
        {
            EnsureInitialized();
            if (command == null) throw new ArgumentNullException(nameof(command));

            var player = _necromancer.CurrentNecromancer;
            if (player == null || player.Model == null || player.Model.Faction != Faction.Player)
                throw new InvalidOperationException("A bound Player necromancer is required before battle start.");

            var enemy = _enemies.CurrentTarget;
            if (enemy == null ||
                enemy.Model == null ||
                enemy.Model.Faction != Faction.Enemy ||
                enemy.Model.LifeState != CombatantLifeState.Active)
            {
                throw new InvalidOperationException("An active Enemy target is required before battle start.");
            }

            _boundary.StartBoundary(_config);
            return _application.Execute(command);
        }

        public CommandResult ResolveFromCombatResult(ResolveBattleCommand command)
        {
            EnsureInitialized();
            if (command == null) throw new ArgumentNullException(nameof(command));
            return _application.Execute(command);
        }

        public CommandResult FinalizeBattle(FinalizeBattleCommand command)
        {
            EnsureInitialized();
            if (command == null) throw new ArgumentNullException(nameof(command));
            return _application.Execute(command);
        }

        public CommandResult RestartBattle(RestartBattleCommand command)
        {
            EnsureInitialized();
            if (command == null) throw new ArgumentNullException(nameof(command));

            _boundary.RestartBoundary(_config);
            return _application.Execute(command);
        }

        private void EnsureInitialized()
        {
            if (_application == null ||
                _boundary == null ||
                _config == null ||
                _necromancer == null ||
                _enemies == null)
            {
                throw new InvalidOperationException("Battle runtime controller is not initialized.");
            }
        }
    }
}

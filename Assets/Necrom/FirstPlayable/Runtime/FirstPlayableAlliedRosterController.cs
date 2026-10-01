using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using DomainEntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableAlliedRosterController : MonoBehaviour
    {
        private readonly RaisedAllyRuntimeEntity[] _slots =
            new RaisedAllyRuntimeEntity[Formation.Capacity];
        private Formation _formation;
        private RectTransform _spawnZone;

        public int ActiveCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i] != null) count++;
                }
                return count;
            }
        }

        public void Initialize(
            Formation formation,
            RectTransform spawnZone)
        {
            if (formation == null) throw new ArgumentNullException(nameof(formation));
            if (spawnZone == null) throw new ArgumentNullException(nameof(spawnZone));
            if (_formation != null)
                throw new InvalidOperationException("Allied roster controller is already initialized.");

            _formation = formation;
            _spawnZone = spawnZone;
        }

        public void EnsureCanActivate(
            RaiseIntoFormationCommand command,
            BasicAutoBehaviorSpec autoBehaviorSpec)
        {
            EnsureInitialized();
            ValidateCommandAndSpec(command, autoBehaviorSpec);

            var formationUnit = _formation.GetSlot(command.Slot);
            if (formationUnit.HasValue)
                throw new InvalidOperationException(
                    "Target formation slot must be empty before Raise activation.");

            EnsureRuntimeSlotAvailable(command.Slot, command.UndeadId);
        }

        public RaisedAllyRuntimeEntity ActivateCommitted(
            RaiseIntoFormationCommand command,
            BasicAutoBehaviorSpec autoBehaviorSpec)
        {
            EnsureInitialized();
            ValidateCommandAndSpec(command, autoBehaviorSpec);

            var formationUnit = _formation.GetSlot(command.Slot);
            if (!formationUnit.HasValue ||
                !formationUnit.Value.Equals(command.UndeadId))
            {
                throw new InvalidOperationException(
                    "Committed formation slot does not match the raised ally.");
            }

            if (command.Source.State != RaiseSourceState.Consumed)
                throw new InvalidOperationException(
                    "Raised ally activation requires a consumed Raise source.");

            EnsureRuntimeSlotAvailable(command.Slot, command.UndeadId);

            var entityObject = new GameObject(
                $"Ally:{command.UndeadId.Value}",
                typeof(RectTransform),
                typeof(RaisedAllyRuntimeEntity));
            entityObject.transform.SetParent(_spawnZone, false);

            var model = new Combatant(
                command.UndeadId,
                command.Source.ArchetypeId,
                Faction.Player,
                command.RestoredHealth);
            var runtimeEntity =
                entityObject.GetComponent<RaisedAllyRuntimeEntity>();
            runtimeEntity.Initialize(
                model,
                command.Slot,
                autoBehaviorSpec);

            _slots[command.Slot] = runtimeEntity;
            return runtimeEntity;
        }

        public RaisedAllyRuntimeEntity GetSlot(int slot)
        {
            ValidateSlot(slot);
            return _slots[slot];
        }

        private void EnsureRuntimeSlotAvailable(
            int slot,
            DomainEntityId unitId)
        {
            ValidateSlot(slot);
            if (_slots[slot] != null)
                throw new InvalidOperationException(
                    "Allied runtime slot is already activated.");

            for (var i = 0; i < _slots.Length; i++)
            {
                var existing = _slots[i];
                if (existing != null &&
                    existing.Model != null &&
                    existing.Model.Id.Equals(unitId))
                {
                    throw new InvalidOperationException(
                        "Raised ally is already active in the runtime roster.");
                }
            }
        }

        private static void ValidateCommandAndSpec(
            RaiseIntoFormationCommand command,
            BasicAutoBehaviorSpec autoBehaviorSpec)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (command.Source == null)
                throw new InvalidOperationException("Raise command source is required.");
            if (autoBehaviorSpec == null)
                throw new ArgumentNullException(nameof(autoBehaviorSpec));
            ValidateSlot(command.Slot);
        }

        private void EnsureInitialized()
        {
            if (_formation == null || _spawnZone == null)
                throw new InvalidOperationException(
                    "Allied roster controller is not initialized.");
        }

        private static void ValidateSlot(int slot)
        {
            if (slot < 0 || slot >= Formation.Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }
}
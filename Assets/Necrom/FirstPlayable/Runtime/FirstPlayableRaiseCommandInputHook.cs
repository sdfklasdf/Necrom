using System;
using Necrom.Core.Application;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableRaiseCommandInputHook : MonoBehaviour
    {
        private FirstPlayableProgression _progression;

        public void Initialize(FirstPlayableProgression progression)
        {
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            if (_progression != null)
                throw new InvalidOperationException("Raise command input hook is already initialized.");

            _progression = progression;
        }

        public CommandResult Submit(
            RaiseIntoFormationCommand command,
            string raisedEventId,
            string assignedEventId)
        {
            if (_progression == null)
                throw new InvalidOperationException("Raise command input hook is not initialized.");
            if (command == null) throw new ArgumentNullException(nameof(command));

            return _progression.RaiseForNextCombat(
                command,
                raisedEventId,
                assignedEventId);
        }
    }
}
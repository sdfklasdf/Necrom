using System;
using System.Collections.Generic;

namespace Necrom.Core.Application
{
    public sealed class LocalSerializedCommandQueue
    {
        private readonly Queue<Action> _pending = new Queue<Action>();
        private bool _draining;

        public int PendingCount => _pending.Count;

        public void Enqueue(Action command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _pending.Enqueue(command);
        }

        public void Drain()
        {
            if (_draining) throw new InvalidOperationException("Command queue is already draining.");

            _draining = true;
            try
            {
                while (_pending.Count > 0)
                {
                    var command = _pending.Dequeue();
                    command();
                }
            }
            finally
            {
                _draining = false;
            }
        }
    }
}

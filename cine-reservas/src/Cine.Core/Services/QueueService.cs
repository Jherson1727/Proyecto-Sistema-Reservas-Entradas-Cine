using System;
using System.Collections.Generic;
using Cine.Core.Interfaces;
using Cine.Core.Models;

namespace Cine.Core.Services
{
    public class QueueService : IQueueService
    {
        private readonly Queue<Cliente> _queue = new();
        private readonly object _gate = new();

        public void Encolar(Cliente cliente)
        {
            if (cliente is null) throw new ArgumentNullException(nameof(cliente));
            lock (_gate)
            {
                _queue.Enqueue(cliente);
            }
        }

        public Cliente? AtenderSiguiente()
        {
            lock (_gate)
            {
                if (_queue.Count == 0) return null;
                return _queue.Dequeue();
            }
        }

        public IEnumerable<Cliente> VerCola()
        {
            lock (_gate)
            {
                return _queue.ToArray(); // snapshot
            }
        }

        public int Count
        {
            get
            {
                lock (_gate) return _queue.Count;
            }
        }
    }
}

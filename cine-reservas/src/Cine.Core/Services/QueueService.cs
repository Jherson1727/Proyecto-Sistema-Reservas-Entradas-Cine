using Cine.Core.Interfaces;
using Cine.Core.Models;

namespace Cine.Core.Services
{
    public class QueueService : IQueueService
    {
        private readonly Queue<Cliente> _queue = new();

        public void EncolarCliente(Cliente cliente) => _queue.Enqueue(cliente);
        public Cliente? ProcesarSiguienteCliente() => _queue.Count > 0 ? _queue.Dequeue() : null;
        public int Conteo() => _queue.Count;
        public IEnumerable<Cliente> VerCola() => _queue.ToArray();
    }
}

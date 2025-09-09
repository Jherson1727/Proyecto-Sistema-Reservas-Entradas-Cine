using Cine.Core.Models;

namespace Cine.Core.Interfaces
{
    public interface IQueueService
    {
        void EncolarCliente(Cliente cliente);
        Cliente? ProcesarSiguienteCliente();
        int Conteo();
        IEnumerable<Cliente> VerCola();
    }
}

using Cine.Core.Models;

namespace Cine.Core.Interfaces
{
    public interface ICancellationService
    {
        void Push(Reserva reserva);
        Reserva? Pop();
        Reserva? Peek();
        int Conteo();
    }
}

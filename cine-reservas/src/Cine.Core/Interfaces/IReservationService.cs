using Cine.Core.Models;

namespace Cine.Core.Interfaces
{
    public interface IReservationService
    {
        Reserva HacerReserva(Guid clienteId, Guid funcionId, string asiento);
        IEnumerable<Reserva> ListarReservas();
        bool CancelarReserva(Guid reservaId);
        bool DeshacerCancelacion();
        bool AsientoDisponible(Guid funcionId, string asiento);
    }
}

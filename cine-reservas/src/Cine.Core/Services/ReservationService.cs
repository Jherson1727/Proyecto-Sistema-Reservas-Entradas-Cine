using Cine.Core.Interfaces;
using Cine.Core.Models;

namespace Cine.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly List<Reserva> _reservas;
        private readonly ICancellationService _cancellations;

        public ReservationService(ICancellationService cancellations, List<Reserva>? backingList = null)
        {
            _cancellations = cancellations;
            _reservas = backingList ?? new List<Reserva>();
        }

        public IEnumerable<Reserva> ListarReservas() => _reservas.ToList();

        public bool AsientoDisponible(Guid funcionId, string asiento) =>
            !_reservas.Any(r => r.FuncionId == funcionId && 
                string.Equals(r.Asiento, asiento, StringComparison.OrdinalIgnoreCase));

        public Reserva HacerReserva(Guid clienteId, Guid funcionId, string asiento)
        {
            if (!AsientoDisponible(funcionId, asiento))
                throw new InvalidOperationException("Asiento ya reservado para esa función.");

            var reserva = new Reserva { ClienteId = clienteId, FuncionId = funcionId, Asiento = asiento };
            _reservas.Add(reserva);
            return reserva;
        }

        public bool CancelarReserva(Guid reservaId)
        {
            var idx = _reservas.FindIndex(r => r.Id == reservaId);
            if (idx < 0) return false;

            var reserva = _reservas[idx];
            _reservas.RemoveAt(idx);
            _cancellations.Push(reserva);
            return true;
        }

        public bool DeshacerCancelacion()
        {
            var cancelada = _cancellations.Pop();
            if (cancelada is null) return false;

            if (!AsientoDisponible(cancelada.FuncionId, cancelada.Asiento))
            {
                _cancellations.Push(cancelada);
                return false;
            }

            _reservas.Add(cancelada);
            return true;
        }
    }
}

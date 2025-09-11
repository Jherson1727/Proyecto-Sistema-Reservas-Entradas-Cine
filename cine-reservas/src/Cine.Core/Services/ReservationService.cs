using System;
using System.Collections.Generic;
using System.Linq;
using Cine.Core.Interfaces;
using Cine.Core.Models;

namespace Cine.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly List<Reserva> _reservas;
        private readonly ICancellationService _cancellations;
        private readonly object _gate = new();

        public ReservationService(ICancellationService cancellations, List<Reserva>? backingList = null)
        {
            _cancellations = cancellations ?? throw new ArgumentNullException(nameof(cancellations));
            _reservas = backingList ?? new List<Reserva>();
        }

        public IEnumerable<Reserva> ListarReservas()
        {
            lock (_gate) return _reservas.ToList(); // snapshot defensivo
        }

        public bool AsientoDisponible(Guid funcionId, string asiento)
        {
            if (funcionId == Guid.Empty) throw new ArgumentException("FuncionId inválido.", nameof(funcionId));
            if (string.IsNullOrWhiteSpace(asiento)) return false;

            var seat = NormalizarAsiento(asiento);

            lock (_gate)
            {
                return !_reservas.Any(r =>
                    r.FuncionId == funcionId &&
                    string.Equals(NormalizarAsiento(r.Asiento ?? string.Empty), seat, StringComparison.Ordinal));
            }
        }

        public Reserva HacerReserva(Guid clienteId, Guid funcionId, string asiento)
        {
            if (clienteId == Guid.Empty) throw new ArgumentException("ClienteId inválido.", nameof(clienteId));
            if (funcionId == Guid.Empty) throw new ArgumentException("FuncionId inválido.", nameof(funcionId));
            if (string.IsNullOrWhiteSpace(asiento)) throw new ArgumentException("Asiento requerido.", nameof(asiento));

            var seat = NormalizarAsiento(asiento);

            lock (_gate)
            {
                if (!AsientoDisponible(funcionId, seat))
                    throw new InvalidOperationException("El asiento ya está reservado para esa función.");

                var reserva = new Reserva
                {
                    ClienteId = clienteId,
                    FuncionId = funcionId,
                    Asiento = seat
                };

                _reservas.Add(reserva);
                return reserva;
            }
        }

        public bool CancelarReserva(Guid reservaId)
        {
            if (reservaId == Guid.Empty) return false;

            lock (_gate)
            {
                var idx = _reservas.FindIndex(r => r.Id == reservaId);
                if (idx < 0) return false;

                var reserva = _reservas[idx];
                _reservas.RemoveAt(idx);

                _cancellations.Push(reserva); // guardar para posible undo
                return true;
            }
        }

        public bool DeshacerCancelacion()
        {
            lock (_gate)
            {
                var cancelada = _cancellations.Pop();
                if (cancelada is null) return false;

                // si alguien ocupó el asiento mientras tanto, no se puede restaurar
                if (!AsientoDisponible(cancelada.FuncionId, cancelada.Asiento!))
                {
                    _cancellations.Push(cancelada); // la devolvemos a la pila
                    return false;
                }

                _reservas.Add(cancelada);
                return true;
            }
        }

        // ---- Helper ----
        private static string NormalizarAsiento(string asiento) =>
            asiento?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}


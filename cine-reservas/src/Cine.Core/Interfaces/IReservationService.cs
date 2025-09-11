using System;
using System.Collections.Generic;
using Cine.Core.Models;

namespace Cine.Core.Interfaces
{
    public interface IReservationService
    {
        IEnumerable<Reserva> ListarReservas();

        
        bool AsientoDisponible(Guid funcionId, string asiento);

       
        Reserva HacerReserva(Guid clienteId, Guid funcionId, string asiento);

        
        bool CancelarReserva(Guid reservaId);

       
        bool DeshacerCancelacion();
    }
}

using System.Collections.Generic;
using Cine.Core.Interfaces;
using Cine.Core.Models;

namespace Cine.Core.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly Stack<Reserva> _stack = new();

        public void Push(Reserva reserva) => _stack.Push(reserva);
        public Reserva? Pop() => _stack.Count > 0 ? _stack.Pop() : null;

        public int Count => _stack.Count;

       
        public Reserva? Peek() => _stack.Count > 0 ? _stack.Peek() : null;
    }
}


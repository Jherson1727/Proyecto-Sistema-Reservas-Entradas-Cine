using System.Collections.Generic;
using Cine.Core.Models;

namespace Cine.Core.Interfaces
{
    public interface IQueueService
    {
       
        void Encolar(Cliente cliente);

        
        Cliente? AtenderSiguiente();

      
        IEnumerable<Cliente> VerCola();

        
        int Count { get; }
    }
}


namespace Cine.Core.Models
{
    public class Reserva
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClienteId { get; set; }
        public Guid FuncionId { get; set; }
        public string Asiento { get; set; } = string.Empty;

        public override string ToString() =>
            $"Reserva {Id} -> Cliente:{ClienteId} Funcion:{FuncionId} Asiento:{Asiento}";
    }
}

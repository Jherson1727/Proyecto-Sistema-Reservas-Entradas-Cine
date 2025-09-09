namespace Cine.Core.Models
{
    public class Funcion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Sala { get; set; } = string.Empty;

        public override string ToString() => $"{Titulo} - {FechaHora:g} - Sala {Sala}";
    }
}

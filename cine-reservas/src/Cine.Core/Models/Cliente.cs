namespace Cine.Core.Models
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Cedula { get; set; }

        public override string ToString()
        {
            var idTxt = !string.IsNullOrWhiteSpace(Cedula) ? $"CI {Cedula}"
                      : !string.IsNullOrWhiteSpace(Email)  ? Email
                      : "sin datos";
            return $"{Nombre ?? "Cliente"} ({idTxt})";
        }
    }
}

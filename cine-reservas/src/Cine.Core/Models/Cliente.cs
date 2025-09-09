namespace Cine.Core.Models
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;

        public override string ToString() => $"{Nombre} ({Id})";
    }
}

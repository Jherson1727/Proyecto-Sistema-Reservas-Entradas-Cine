using Cine.Core.Models;
using Cine.Core.Services;
using Xunit;

public class QueueServiceTests
{
    [Fact]
    public void FIFOFunciona()
    {
        var q = new QueueService();
        var a = new Cliente { Nombre = "A" };
        var b = new Cliente { Nombre = "B" };

        q.EncolarCliente(a);
        q.EncolarCliente(b);

        Assert.Equal("A", q.ProcesarSiguienteCliente()?.Nombre);
        Assert.Equal("B", q.ProcesarSiguienteCliente()?.Nombre);
    }
}

using Cine.Core.Models;
using Cine.Core.Services;
using Xunit;

public class ReservationServiceTests
{
    [Fact]
    public void NoPermiteDuplicarAsiento()
    {
        var svc = new ReservationService(new CancellationService());
        var cliente = Guid.NewGuid();
        var funcion = Guid.NewGuid();

        svc.HacerReserva(cliente, funcion, "A1");

        Assert.Throws<InvalidOperationException>(() =>
            svc.HacerReserva(Guid.NewGuid(), funcion, "A1"));
    }
}

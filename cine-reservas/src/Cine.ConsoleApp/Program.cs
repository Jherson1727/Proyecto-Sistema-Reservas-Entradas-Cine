using Cine.Core.Interfaces;
using Cine.Core.Models;
using Cine.Core.Services;

class Program
{
    static IReservationService reservationService = new ReservationService(new CancellationService());
    static IQueueService queueService = new QueueService();

    // Datos de ejemplo
    static List<Cliente> clientes = new()
    {
        new Cliente { Nombre = "Ana",  Cedula = "123456", Email = "ana@mail.com"  },
        new Cliente { Nombre = "Luis", Cedula = "987654", Email = "luis@mail.com" }
    };

    static List<Funcion> funciones = new()
    {
        new Funcion { Titulo = "Interestelar", FechaHora = DateTime.Today.AddHours(18), Sala = "1" },
        new Funcion { Titulo = "Avatar", FechaHora = DateTime.Today.AddHours(20), Sala = "2" },
        new Funcion { Titulo = "Titanic", FechaHora = DateTime.Today.AddHours(22), Sala = "3" }
    };

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            MostrarMenu();
            var opt = Console.ReadLine();

            if      (opt == "1") HacerReservaUI();
            else if (opt == "2") ListarReservasUI();
            else if (opt == "3") CancelarReservaUI();
            else if (opt == "4") DeshacerCancelacionUI();
            else if (opt == "5") EncolarClienteUI();
            else if (opt == "6") ProcesarClienteUI();
            else if (opt == "0") return;
            else Console.WriteLine("Opción no válida, intenta otra vez.");

            Console.WriteLine("\nPresiona ENTER para continuar...");
            Console.ReadLine();
        }
    }

    static void MostrarMenu()
    {
        Console.WriteLine();
        Console.WriteLine(" *****Menu De Reservas De Entradas*****");
        Console.WriteLine("1) Hacer reserva");
        Console.WriteLine("2) Listar reservas");
        Console.WriteLine("3) Cancelar");
        Console.WriteLine("4) Deshacer cancelación");
        Console.WriteLine("5) Encolar cliente");
        Console.WriteLine("6) Procesar cliente");
        Console.WriteLine("0) Salir");
        Console.Write("\nElige una opción: ");
    }

    // --------- Hacer reserva simplificada ----------
    static void HacerReservaUI()
    {
        // 1. Nombre del cliente
        Console.Write("Nombre del cliente: ");
        var nombreCliente = Console.ReadLine() ?? "Cliente";

        // 2. Cédula de identidad
        Console.Write("Cédula de identidad (CI): ");
        var cedulaCliente = Console.ReadLine() ?? "";

        // Buscar cliente existente por nombre + CI
        var cliente = clientes.FirstOrDefault(c =>
            c.Nombre?.Equals(nombreCliente, StringComparison.OrdinalIgnoreCase) == true &&
            c.Cedula == cedulaCliente);

        // Si no existe, crearlo
        if (cliente == null)
        {
            cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nombre = nombreCliente,
                Cedula = cedulaCliente
            };
            clientes.Add(cliente);
        }

        // 3. Mostrar 3 funciones disponibles
        Console.WriteLine("\nFunciones disponibles:");
        for (int i = 0; i < funciones.Count && i < 3; i++)
        {
            Console.WriteLine($"{i + 1}) {funciones[i]}");
        }

        Console.Write("Elige la función (1-3): ");
        int.TryParse(Console.ReadLine(), out int opcionFuncion);

        Funcion funcion;
        if (opcionFuncion >= 1 && opcionFuncion <= 3 && opcionFuncion <= funciones.Count)
        {
            funcion = funciones[opcionFuncion - 1];
        }
        else
        {
            Console.WriteLine("Opción inválida, se tomará la primera función.");
            funcion = funciones.First();
        }

        // 4. Asiento
        Console.Write("Asiento: ");
        var asiento = Console.ReadLine() ?? "A1";

        // 5. Confirmación de la reserva
        Console.WriteLine("\nResumen de la reserva:");
        Console.WriteLine($"Cliente: {cliente.Nombre}");
        Console.WriteLine($"CI: {cliente.Cedula}");
        Console.WriteLine($"Función: {funcion.Titulo} - Sala {funcion.Sala} - {funcion.FechaHora:g}");
        Console.WriteLine($"Asiento: {asiento}");
        Console.Write("¿Confirmar reserva? (S/N): ");
        var confirmar = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(confirmar) && confirmar.Trim().ToUpper() == "S")
        {
            try
            {
                var reserva = reservationService.HacerReserva(cliente.Id, funcion.Id, asiento);
                Console.WriteLine("\nReserva creada con éxito!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\nNo se pudo crear la reserva: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("\nReserva cancelada por el usuario.");
        }
    }
    // -------------------------------------------------

    static void ListarReservasUI()
    {
        var reservas = reservationService.ListarReservas().ToList();
        if (reservas.Count == 0)
        {
            Console.WriteLine("No hay reservas.");
            return;
        }

        for (int i = 0; i < reservas.Count; i++)
        {
            var r = reservas[i];
            var cliente = clientes.FirstOrDefault(c => c.Id == r.ClienteId);
            var funcion = funciones.FirstOrDefault(f => f.Id == r.FuncionId);
            var etiqueta = cliente?.Cedula ?? "desconocido";

            Console.WriteLine($"{i + 1}. {funcion?.Titulo ?? "Función"} - Asiento {r.Asiento} - {cliente?.Nombre ?? "Cliente"} (CI {etiqueta})");
        }
    }

    static void CancelarReservaUI()
    {
        var reservas = reservationService.ListarReservas().ToList();
        if (reservas.Count == 0)
        {
            Console.WriteLine("No hay reservas para cancelar.");
            return;
        }

        ListarReservasUI();
        Console.Write("Número de reserva a cancelar: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= reservas.Count)
        {
            var ok = reservationService.CancelarReserva(reservas[idx - 1].Id);
            Console.WriteLine(ok ? "Reserva cancelada." : "No se pudo cancelar.");
        }
        else
        {
            Console.WriteLine("Entrada inválida.");
        }
    }

    static void DeshacerCancelacionUI()
    {
        var ok = reservationService.DeshacerCancelacion();
        Console.WriteLine(ok ? "Cancelación deshecha." : "No hay cancelaciones para deshacer.");
    }

    static void EncolarClienteUI()
    {
        Console.Write("Nombre del cliente: ");
        var nombre = Console.ReadLine();

        Console.Write("Cédula de identidad (opcional): ");
        var ci = Console.ReadLine();

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = string.IsNullOrWhiteSpace(nombre) ? "Cliente" : nombre!.Trim(),
            Cedula = string.IsNullOrWhiteSpace(ci) ? null : ci!.Trim()
        };

        clientes.Add(cliente);
        queueService.EncolarCliente(cliente);
        Console.WriteLine($"{cliente.Nombre} encolado.");
    }

    static void ProcesarClienteUI()
    {
        var c = queueService.ProcesarSiguienteCliente();
        if (c is null)
        {
            Console.WriteLine("Cola vacía");
            return;
        }

        var atLabel = c.Cedula ?? "desconocido";
        Console.WriteLine($"Cliente atendido: {c.Nombre ?? "Cliente"} (CI {atLabel})");
    }
}


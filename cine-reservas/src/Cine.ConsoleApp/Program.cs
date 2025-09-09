using System;
using System.Collections.Generic;
using System.Linq;
using Cine.Core.Models;
using Cine.Core.Interfaces;
using Cine.Core.Services; 


class Program
{
    // === Servicios compartidos (una sola instancia) ===
    static readonly ICancellationService cancellationService = new CancellationService();
    static readonly IReservationService reservationService = new ReservationService(cancellationService);
    static readonly IQueueService queueService = new QueueService();

    // === Repositorio en memoria de clientes (para lookup por ClienteId) ===
    static readonly Dictionary<Guid, Cliente> clientesById = new();

    // === Catálogo de funciones/películas (ejemplo) ===
    static readonly List<Funcion> funciones = new()
    {
        new Funcion { Id = Guid.NewGuid(), Titulo = "Superman", Sala = "Sala 1", FechaHora = DateTime.Today.AddHours(22) },
        new Funcion { Id = Guid.NewGuid(), Titulo = "El Conjuro 4: Ultimos Ritos", Sala = "Sala 2", FechaHora = DateTime.Today.AddHours(19) },
        new Funcion { Id = Guid.NewGuid(), Titulo = "The Batman: Part II", Sala = "Sala 3", FechaHora = DateTime.Today.AddHours(18) },
        new Funcion { Id = Guid.NewGuid(), Titulo = "La Quimera De oro", Sala = "Sala 4", FechaHora = DateTime.Today.AddHours(17) },
        new Funcion { Id = Guid.NewGuid(), Titulo = "Los Tipos Malos 2", Sala = "Sala 5", FechaHora = DateTime.Today.AddHours(15) }
    };

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            MostrarMenu();
            var opt = Console.ReadLine();

            switch (opt)
            {
                case "1": HacerReservaUI(); break;
                case "2": ListarReservasUI(); break;
                case "3": CancelarReservaUI(); break;
                case "4": DeshacerCancelacionUI(); break;
                case "5": EncolarClienteUI(); break;
                case "6": ProcesarClienteUI(); break;
                case "0": return;
                default:
                    Console.WriteLine("Opción no válida, intenta otra vez.");
                    break;
            }

            Console.WriteLine();
            Console.Write("Presiona ENTER para continuar...");
            Console.ReadLine();
        }
    }

    static void MostrarMenu()
    {
        Console.WriteLine("***** Menu De Reservas De Entradas *****");
        Console.WriteLine("1) Hacer reserva");
        Console.WriteLine("2) Listar reservas");
        Console.WriteLine("3) Cancelar");
        Console.WriteLine("4) Deshacer cancelación");
        Console.WriteLine("5) Encolar cliente");
        Console.WriteLine("6) Procesar cliente");
        Console.WriteLine("0) Salir");
        Console.Write("\nElige una opción: ");
    }

    // === 1) Hacer reserva ===
    static void HacerReservaUI()
    {
        Console.Clear();
        Console.WriteLine("== Hacer reserva ==");

        // ¿Hay alguien en la cola?
        var cola = queueService.VerCola()?.ToArray() ?? Array.Empty<Cliente>();
        Cliente? cliente = null;

        if (cola.Length > 0)
        {
            Console.WriteLine($"Hay {cola.Length} cliente(s) en cola. ¿Usar el primero?");
            Console.Write("Responder S/N: ");
            var useQueued = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (useQueued == "S" || useQueued == "SI" || useQueued == "SÍ")
            {
                cliente = queueService.AtenderSiguiente(); // dequeue
                if (cliente != null) clientesById[cliente.Id] = cliente; // asegurar persistencia
                Console.WriteLine($"Usando cliente: {Ui.FormatearClienteInline(cliente!)}");
            }
        }

        // Si no tomamos de la cola, pedir datos mínimos del cliente y guardar
        if (cliente is null)
            cliente = Ui.LeerClienteMinimoYGuardar(clientesById);

        // 1) Elegir función/película
        var funcion = Ui.SeleccionarFuncion(funciones);
        if (funcion is null)
        {
            Console.WriteLine("No hay funciones disponibles.");
            Ui.Pausa();
            return;
        }

        // 2) Selector de asientos (A–J x 1–14)
        var asiento = Ui.SeleccionarAsientoInteractivo(reservationService, funcion.Id, filas: 10, columnas: 14);
        if (string.IsNullOrEmpty(asiento))
        {
            Console.WriteLine("Operación cancelada.");
            Ui.Pausa();
            return;
        }

        try
        {
            var reserva = reservationService.HacerReserva(cliente.Id, funcion.Id, asiento);
            Console.WriteLine();
            Console.WriteLine($"Asiento: {reserva.Asiento}");
            Console.WriteLine($"Reserva creada para: {Ui.FormatearClienteInline(cliente)}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"No se pudo crear la reserva: {ex.Message}");
        }

        Ui.Pausa();
    }

    // === 2) Listar reservas ===
static void ListarReservasUI()
{
    Console.Clear();
    Console.WriteLine("== Listar reservas ==");

    var reservas = reservationService.ListarReservas().ToList();
    if (reservas.Count == 0)
    {
        Console.WriteLine("No hay reservas.");
        return;
    }

    for (int i = 0; i < reservas.Count; i++)
    {
        var r = reservas[i];

        // Buscar el cliente
        clientesById.TryGetValue(r.ClienteId, out var c);
        var clienteInfo = c != null ? Ui.FormatearClienteInline(c) : r.ClienteId.ToString();

        // Buscar la función
        var funcion = funciones.FirstOrDefault(f => f.Id == r.FuncionId);
        var funcionInfo = funcion != null
            ? $"{funcion.Titulo} — Sala {funcion.Sala} — {funcion.FechaHora:g}"
            : r.FuncionId.ToString();

        Console.WriteLine($"{i + 1}) {clienteInfo} | {funcionInfo} | Asiento: {r.Asiento}");
    }
}


    // === 3) Cancelar reserva ===
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

    // === 4) Deshacer cancelación ===
    static void DeshacerCancelacionUI()
    {
        var ok = reservationService.DeshacerCancelacion();
        Console.WriteLine(ok ? "Cancelación deshecha." : "No hay cancelaciones para deshacer.");
    }

    // === 5) Encolar cliente ===
    static void EncolarClienteUI()
    {
        Console.Clear();
        Console.WriteLine("== Encolar cliente ==");

        Console.Write("Nombre del cliente: ");
        var nombre = Console.ReadLine();

        Console.Write("Cédula de identidad (opcional): ");
        var ci = Console.ReadLine();

        Console.Write("Correo electrónico (opcional): ");
        var email = Console.ReadLine();

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre!.Trim(),
            Cedula = string.IsNullOrWhiteSpace(ci) ? null : ci!.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email!.Trim()
        };

        // Guardar y encolar
        clientesById[cliente.Id] = cliente;
        queueService.Encolar(cliente);

        Console.WriteLine();
        Console.WriteLine($"{(cliente.Nombre ?? "Cliente")} encolado.");
    }

    // === 6) Procesar cliente (saca de cola y reserva) ===
    static void ProcesarClienteUI()
    {
        Console.Clear();
        Console.WriteLine("== Procesar cliente ==");

        var cliente = queueService.AtenderSiguiente();
        if (cliente is null)
        {
            Console.WriteLine("No hay clientes en cola.");
            return;
        }

        // Asegurar persistencia
        clientesById[cliente.Id] = cliente;

        Console.WriteLine($"Procesando a: {Ui.FormatearClienteInline(cliente)}");

        var funcion = Ui.SeleccionarFuncion(funciones);
        if (funcion is null)
        {
            Console.WriteLine("Operación cancelada: no hay función válida.");
            return;
        }

        var asiento = Ui.SeleccionarAsientoInteractivo(reservationService, funcion.Id, filas: 10, columnas: 14);
        if (string.IsNullOrEmpty(asiento))
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        try
        {
            var reserva = reservationService.HacerReserva(cliente.Id, funcion.Id, asiento);
            Console.WriteLine();
            Console.WriteLine($"Asiento: {reserva.Asiento}");
            Console.WriteLine($"Reserva creada para: {Ui.FormatearClienteInline(cliente)}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"No se pudo crear la reserva: {ex.Message}");
        }
    }
}

// ===================== Helpers de UI =====================
static class Ui
{
    public static void Pausa()
    {
        Console.WriteLine();
        Console.Write("Presiona ENTER para continuar...");
        Console.ReadLine();
    }

    public static string FormatearClienteInline(Cliente c)
    {
        if (!string.IsNullOrWhiteSpace(c.Cedula))
            return $"{(c.Nombre ?? "Cliente")} (CI {c.Cedula})";
        if (!string.IsNullOrWhiteSpace(c.Email))
            return $"{(c.Nombre ?? "Cliente")} ({c.Email})";
        return c.Nombre ?? "Cliente";
    }

    public static Cliente LeerClienteMinimoYGuardar(Dictionary<Guid, Cliente> repo)
    {
        Console.Write("Nombre del cliente: ");
        var nombre = Console.ReadLine();

        Console.Write("Cédula de identidad (opcional): ");
        var ci = Console.ReadLine();

        Console.Write("Correo electrónico (opcional): ");
        var email = Console.ReadLine();

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre!.Trim(),
            Cedula = string.IsNullOrWhiteSpace(ci) ? null : ci!.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email!.Trim()
        };

        repo[cliente.Id] = cliente;
        return cliente;
    }

    public static Funcion? SeleccionarFuncion(List<Funcion> funciones)
    {
        if (funciones == null || funciones.Count == 0) return null;

        Console.WriteLine();
        Console.WriteLine("Funciones disponibles:");
        for (int i = 0; i < funciones.Count; i++)
        {
            var f = funciones[i];
            Console.WriteLine($"{i + 1}) {f.Titulo} — {f.Sala} — {f.FechaHora:g}");
        }
        Console.Write("Elige una función (número): ");
        if (!int.TryParse(Console.ReadLine(), out var idx) || idx < 1 || idx > funciones.Count)
        {
            Console.WriteLine("Selección inválida.");
            return null;
        }
        return funciones[idx - 1];
    }

    public static string? SeleccionarAsientoInteractivo(IReservationService reservationService, Guid funcionId, int filas = 10, int columnas = 14)
    {
        var letras = Enumerable.Range(0, filas).Select(i => (char)('A' + i)).ToArray();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("== Selección de asiento ==");
            Console.WriteLine();
            RenderMapa(reservationService, funcionId, filas, columnas);

            Console.WriteLine();
            Console.WriteLine("Elige una fila:");
            for (int i = 0; i < filas; i++)
                Console.WriteLine($"{i + 1}) Fila {letras[i]}");

            Console.Write("Fila (número) o 0 para cancelar: ");
            if (!int.TryParse(Console.ReadLine(), out var filaSel) || filaSel < 0 || filaSel > filas)
            {
                Console.WriteLine("Valor inválido.");
                Pausa();
                continue;
            }
            if (filaSel == 0) return null;

            var filaLetra = letras[filaSel - 1];

            Console.Write($"Asiento en fila {filaLetra} (1-{columnas}) o 0 para cancelar: ");
            if (!int.TryParse(Console.ReadLine(), out var colSel) || colSel < 0 || colSel > columnas)
            {
                Console.WriteLine("Valor inválido.");
                Pausa();
                continue;
            }
            if (colSel == 0) return null;

            var asiento = $"{filaLetra}{colSel}".ToUpperInvariant();

            // Validación y mensaje claro si está ocupado
            if (!reservationService.AsientoDisponible(funcionId, asiento))
            {
                Console.WriteLine($"El asiento {asiento} ya está reservado. Elige otro.");
                Pausa();
                continue;
            }

            Console.Write($"Confirmar asiento {asiento}? (S/N): ");
            var conf = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (conf == "S" || conf == "SI" || conf == "SÍ")
                return asiento;
        }
    }

    public static void RenderMapa(IReservationService reservationService, Guid funcionId, int filas, int columnas)
    {
        var letras = Enumerable.Range(0, filas).Select(i => (char)('A' + i)).ToArray();

        // Encabezado columnas
        Console.Write("     ");
        for (int c = 1; c <= columnas; c++)
            Console.Write($"{c,2} ");
        Console.WriteLine();

        // Filas
        for (int f = 0; f < filas; f++)
        {
            var letra = letras[f];
            Console.Write($" {letra} | ");
            for (int c = 1; c <= columnas; c++)
            {
                var asiento = $"{letra}{c}";
                var libre = reservationService.AsientoDisponible(funcionId, asiento);
                Console.Write(libre ? "[ ]" : "[X]");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.WriteLine("[ ] Libre   [X] Ocupado");
    }
}

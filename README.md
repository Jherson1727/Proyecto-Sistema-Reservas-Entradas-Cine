
# Proyecto: Sistema de Reserva de Entradas para Cine

Bienvenido/a. Este repositorio contiene una **un sistema de gestión de reservas de cine en C#** que funciona en **memoria** (sin base de datos). Acá podras ver: **qué hace**, **cómo está organizado** y **cómo ejecutarlo**.

> Fecha de esta guía: 2025-09-09
---

## Conceptos Principales

### Requisitos
- **.NET SDK 9.0** (o 8.0+ ajustando `TargetFramework`).
- Windows, Linux o macOS con terminal.

### Clonar, compilar y ejecutar
```bash
# (1) Restaurar dependencias (normalmente automático al build)
dotnet --version

# (2) Ir al proyecto de consola
cd cine-reservas/src/Cine.ConsoleApp

# (3) Compilar y ejecutar
dotnet run
```

> Si usas Visual Studio / VS Code, abre la solución y ejecuta el **proyecto de inicio** `Cine.ConsoleApp`.

---

## Estructura del repositorio (resumen)

```
  - Proyecto-Sistema-Reservas-Entradas-Cine/
    · NOTAS.md
    · cine-reservas.sln
    - cine-reservas/
      · cine-reservas.sln
      - src/
        - Cine.ConsoleApp/
          · Cine.ConsoleApp.csproj
          · Program.cs
        - Cine.Core/
          · Cine.Core.csproj
          · Class1.cs
          - Interfaces/
            · ICancellationService.cs
            · IQueueService.cs
            · IReservationService.cs
          - Models/
            · Cliente.cs
            · Funcion.cs
            · Reserva.cs
          - Services/
            · CancellationService.cs
            · QueueService.cs
            · ReservationService.cs
      - tests/
        - Cine.Tests/
          · Cine.Tests.csproj
          · QueueServiceTests.cs
          · ReservationServiceTests.cs
    
```

- **Cine.Core** → Librería de clases con el **dominio**, **interfaces** y **servicios**.
- **Cine.ConsoleApp** → Aplicación de consola (menú, validaciones de entrada y flujo interactivo).

---

## Arquitectura y capas

- **Dominio (Models)**: `Cliente`, `Funcion`, `Reserva`.
- **Contratos (Interfaces)**: 
  - `IReservationService` (reservas, cancelaciones y deshacer)
  - `IQueueService` (cola FIFO de clientes)
  - `ICancellationService` (pila LIFO de cancelaciones)
- **Servicios (Services)**: 
  - `ReservationService`: lógica central de reservas (normaliza asientos, evita duplicados, cancela y deshace).
  - `QueueService`: maneja `Queue<Cliente>` de forma segura (locks) para `Encolar`/`AtenderSiguiente`/`VerCola`.
  - `CancellationService`: maneja `Stack<Reserva>` para `Push`/`Pop` (y `Count`).

- **UI (Consola)**: `Program.cs` + helper `Ui`  
  Presenta menú, lee datos, muestra el mapa de asientos y coordina con los servicios.

---

## Modelo de dominio

- **Cliente**
  - `Id : Guid`
  - `Nombre : string?`
  - `Cedula : string?`
  - `Email : string?`

- **Funcion**
  - `Id : Guid`
  - `Titulo : string`
  - `FechaHora : DateTime`
  - `Sala : string`

- **Reserva**
  - `Id : Guid` (autogenerado)
  - `ClienteId : Guid`
  - `FuncionId : Guid`
  - `Asiento : string` (formato “A1”, “B12”…; **normalizado** a trim + mayúsculas)

---

##  Reglas clave

1) **Un asiento no se duplica** en una misma función.  
   - `ReservationService.HacerReserva` lanza `InvalidOperationException` si está ocupado.

2) **Cancelar** una reserva → se quita de la lista y se **apila** en `CancellationService`.  

3) **Deshacer cancelación** → se hace `Pop` de la pila y se reintenta agregar **solo si el asiento sigue libre**.

4) **Procesar cliente** → `QueueService` atiende en **FIFO** (primero en entrar, primero en salir).

5) **Normalización de asientos** → `a1`, ` A1 ` y `A1` son equivalentes.

---

## 🖥️ Experiencia en consola

**Menú principal**  
1. Hacer reserva → pide cliente (o usa el primero en cola), permite elegir **función** y **asiento** con un **mapa** (A–J x 1–14).  
2. Listar reservas → muestra **Cliente | Película | Sala | Hora | Asiento**.  
3. Cancelar → quita la reserva y la **apila** para deshacer.  
4. Deshacer cancelación → recupera la **última** cancelada (si el asiento sigue libre).  
5. Encolar cliente → guarda datos y lo agrega al final de la cola.  
6. Procesar cliente → atiende el siguiente en cola y lo guía a reservar.  
0. Salir.

**Mapa de asientos**  
- Renderizado con `[ ]` libre y `[X]` ocupado.  
- Filas **A–J**, columnas **1–14**.  
- Validación antes de confirmar.

---

## API de servicios (resumen rápido)

### `IReservationService`
```csharp
IEnumerable<Reserva> ListarReservas();
bool AsientoDisponible(Guid funcionId, string asiento);
Reserva HacerReserva(Guid clienteId, Guid funcionId, string asiento);
bool CancelarReserva(Guid reservaId);
bool DeshacerCancelacion();
```

### `IQueueService`
```csharp
void Encolar(Cliente cliente);
Cliente? AtenderSiguiente();
IEnumerable<Cliente> VerCola();
int Count { get; }
```

### `ICancellationService`
```csharp
void Push(Reserva reserva);
Reserva? Pop();
int Count { get; }
```

---

## Pruebas sugeridas (xUnit)

- **Reserva duplicada**: reservar el mismo asiento dos veces en la misma función → falla.  
- **Cancelar/Deshacer**: cancelar una reserva y luego deshacer → se restaura si sigue libre.  
- **Cola FIFO**: encolar 3 clientes y atender en el orden correcto.  
- **Normalización**: `a1` y `A1` deben tratarse igual.

> Aún no hay proyecto de tests en el repo; se recomienda crear `Cine.Tests` con xUnit.

---

## Extensiones futuras (ideas)

- Persistencia en **JSON** o **BD** (EF Core).  
- **Capacidad** por sala/función y asiento prebloqueado.  
- **Precios**, **descuentos** y **pagos** (mock).  
- UI con **WinForms/WPF/Web** (manteniendo `Cine.Core` intacto).  
- **Logs** y métricas.  
- **CI/CD** con GitHub Actions (build + test).

---

## Problemas comunes y soluciones

- **“El archivo se ha bloqueado por: Cine.ConsoleApp (…)”**  
  Cierra la app si sigue corriendo. En PowerShell:
  ```powershell
  Get-Process Cine.ConsoleApp | Stop-Process -Force
  # o
  taskkill /IM Cine.ConsoleApp.exe /F
  # luego limpia y recompila
  dotnet clean
  rmdir /s /q bin obj
  dotnet run
  ```

- **Métodos que “no existen” en interfaces**  
  Asegúrate de que `IQueueService` tenga **Encolar/AtenderSiguiente/VerCola/Count**  
  e `ICancellationService` exponga **Push/Pop/Count** (no `Conteo()`), o agrega ambas para compatibilidad.

- **No se muestran títulos de película en la lista**  
  En `ListarReservasUI()`, resolver `r.FuncionId` contra la lista `funciones` para mostrar **Título/Sala/Hora**.

---

## Estilo de colaboración (sugerido)

- Ramas: `main` (estable) · `develop` (integración) · `task/...` (tareas)  
- Commits: `feat:` · `fix:` · `test:`  
- Flujo: Pull de `develop` → rama `task/...` → PR → revisión → merge a `develop`.

---

## Créditos y licencia

- Autores/as: _Kevin Cortez, Kevin Fuertes, Noelia Mamani y Jherson Rivera_.  


# PROYECTO: Sistema de Reserva de Entradas para Cine

Se aplicara y usara el lenguaje de programación **C#** para gestionar reservas de cine.  
Trabaja en memoria (sin base de datos) y aplica tres estructuras simples:
- **Lista** de reservas vigentes
- **Cola (FIFO)** de clientes
- **Pila (LIFO)** de cancelaciones (para deshacer)

---

## 1) Requisitos del proyecto (estado actual)

- [x] Hacer una reserva
- [x] Mostrar la lista de reservas
- [x] Cancelar una reserva (se apila para deshacer)
- [x] Deshacer última cancelación (si el asiento sigue libre)
- [x] Encolar clientes a medida que llegan
- [x] Procesar clientes en orden de llegada (FIFO)
- [x] **Selección visual de butacas** con mapa tipo cine (filas A–J, asientos 1–14), `[ ]` libre / `[X]` ocupado

> **No incluido por ahora:** persistencia en BD, autenticación/usuarios.  
> (Opcional futuro: persistencia simple en JSON, tests unitarios, CI).

---

## 2) Modelo de dominio

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
  - `Id : Guid`
  - `ClienteId : Guid`
  - `FuncionId : Guid`
  - `Asiento : string`  _(formato “A1”, “B12”; se normaliza: trim + mayúsculas)_

---

## 3) Estructuras de datos

- `List<Reserva>` → **fuente de verdad** de reservas activas.  
- `Queue<Cliente>` → orden de llegada y atención de clientes (FIFO).  
- `Stack<Reserva>` → historial reciente de **cancelaciones** (LIFO) para **deshacer**.

---

## 4) Reglas del programa

1. **No duplicar asiento** en la misma función.  
2. **Cancelar**: quitar de la lista y **apilar** la reserva.  
3. **Deshacer**: tomar la **última** cancelación y reinsertar **solo si** el asiento sigue libre.  
4. **Procesar cliente**: atender siempre al **primero** de la cola (FIFO).  
5. **Normalización de asientos**: `" a1 "`, `a1` y `A1` se consideran el **mismo** asiento.

---

## 5) Casos de uso → Métodos (idea general)

- `HacerReserva(clienteId, funcionId, asiento)`
- `ListarReservas() : IEnumerable<Reserva>`
- `CancelarReserva(reservaId) : bool`
- `DeshacerCancelacion() : bool`
- `Encolar(cliente)`
- `AtenderSiguiente() : Cliente?`
- `VerCola() : IEnumerable<Cliente>`

---

## 6) Interfaces (contratos principales)

- **`IReservationService`** — reservas, cancelaciones y deshacer  
  ```csharp
  IEnumerable<Reserva> ListarReservas();
  bool AsientoDisponible(Guid funcionId, string asiento);
  Reserva HacerReserva(Guid clienteId, Guid funcionId, string asiento);
  bool CancelarReserva(Guid reservaId);
  bool DeshacerCancelacion();
  ```

- **`IQueueService`** — cola de clientes  
  ```csharp
  void Encolar(Cliente cliente);
  Cliente? AtenderSiguiente();
  IEnumerable<Cliente> VerCola();
  int Count { get; }
  ```

- **`ICancellationService`** — pila de cancelaciones  
  ```csharp
  void Push(Reserva reserva);
  Reserva? Pop();
  int Count { get; }
  ```

---

## 7) Comportamiento de la UI (consola)

- **1) Hacer reserva**
  - Si hay cola, ofrece usar el primer cliente.
  - Si no, solicita datos del cliente (se guardan en memoria).
  - Permite **elegir función** de una lista.
  - Muestra **mapa de asientos** (A–J, 1–14); valida disponibilidad.
  - Si está ocupado, avisa y permite elegir otro.
  - Confirma y crea la reserva.

- **2) Listar reservas**  
  Muestra **Cliente | Película | Sala | Hora | Asiento** (resolviendo `FuncionId`).

- **3) Cancelar**  
  Elimina la reserva y la **apila** para posible deshacer.

- **4) Deshacer cancelación**  
  Restaura la **última** cancelación si el asiento sigue libre.

- **5) Encolar cliente**  
  Carga datos y lo agrega al final de la cola.

- **6) Procesar cliente**  
  Atiende al siguiente en la cola y lo guía directo a función + asiento.

---

## 8) Flujo de trabajo (Git / GitHub)

- **Ramas**
  - `main` → estable
  - `develop` → integración previa
  - `task/...` → trabajo por tarea (p. ej., `task/agregar-funcion-reserva`)

- **Convención de commits**
  - `feat:` nueva funcionalidad
  - `fix:` corrección
  - `test:` pruebas

- **Ciclo sugerido**
  1. Pull de `develop`
  2. Crear `task/...`
  3. Commit → Push → Pull Request → Revisión
  4. Merge a `develop` (no directo a `main`)

---

## 9) Próximos PRs sugeridos

1. Modelos + Interfaces  
2. Menú de consola (opciones)  
3. Implementación de servicios (reservas, cola, cancelaciones) + normalización de asientos  
4. Conexión UI ↔ servicios (selector de función + mapa de asientos)  
5. **Pendiente**: tests xUnit (duplicados, cancelar/deshacer, FIFO)  
6. **Opcional**: persistencia JSON y CI (GitHub Actions)



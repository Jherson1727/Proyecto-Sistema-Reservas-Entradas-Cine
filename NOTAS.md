# Sistema de Reserva de Entradas para Cine

Proyecto: Sistema de reservas de cine en **C#**. Se manejarán reservas con listas, clientes en **cola FIFO** y cancelaciones en **pila LIFO**.

---

## 1. Requisitos para el proyecto
- [  ] Hacer una reserva.
- [  ] Mostrar la lista de reservas.
- [  ] Cancelar una reserva (se guarda en la pila de cancelaciones).
- [  ] Deshacer última cancelación.
- [  ] Encolar clientes a medida que llegan.
- [  ] Procesar clientes en orden de llegada (FIFO).


_No aplicaremos aún: selección visual de butacas, no usaremos un motor de base de datos (como por ejemplo MySQL, para guardar la información de reservas, clientes y funciones), y no se incluirá autenticación (el poder verificar quién es el usuario que realiza cada acción)._

---

## 2. Modelo de dominio (entidades)

- **Cliente**
  - `Id : Guid`
  - `Nombre : string`

- **Funcion**
  - `Id : Guid`
  - `Titulo : string`
  - `FechaHora : DateTime`
  - `Sala : string`

- **Reserva**
  - `Id : Guid`
  - `ClienteId : Guid`
  - `FuncionId : Guid`
  - `Asiento : string`

---

## 3. Estructuras de datos

- `List<Reserva>`  
  Fuente de verdad de todas las reservas activas _(registro oficial, donde se guardara toda la información)_.

- `Queue<Cliente>`  
  Maneja el orden de llegada y atención de clientes.

- `Stack<Reserva>`  
  Almacena cancelaciones recientes para poder deshacerlas.

---

## 4. Reglas para el programa

1. **Un asiento no puede duplicarse** en la misma función.
2. **Cancelar una reserva**: quitar de la lista y enviar a la pila.
3. **Deshacer cancelación**: sacar el último de la pila e intentar reinsertar (sólo si el asiento sigue libre).
4. **Procesar cliente**: atender el primero de la cola (FIFO). Si la cola está vacía, no hace nada.

---

## 5. Casos de uso → Métodos

- `HacerReserva(clienteId, funcionId, asiento)`
- `ListarReservas()`
- `CancelarReserva(reservaId)`
- `DeshacerCancelacion()`
- `EncolarCliente(cliente)`
- `ProcesarSiguienteCliente()`

---

## 6. Interfaces (contratos)

- `IReservationService`  
  Gestiona reservas, cancelaciones y deshacer.

- `IQueueService`  
  Gestiona la cola de clientes.

- `ICancellationService`  
  Gestiona la pila de cancelaciones.

---

## 7. División de roles

### Jherson/ Kevin Fuertes/ Kevin Cortez – Lógica de programa
- Crear **Models** (`Cliente`, `Funcion`, `Reserva`).
- Crear **Interfaces**.
- Implementar **Services** usando `List<>`, `Queue<>`, `Stack<>`.
- Escribir **tests unitarios** con xUnit.

### Camila – Interfaz y pruebas de integración
- Crear **menú de consola** con opciones.
- Conectar UI con interfaces (sin tocar lógica interna).
- Hacer **pruebas de humo** (end-to-end).
- Persistencia simple en JSON. - - _(Talvez)_- -
- Agregar y usarCI básico en GitHub Actions. - - _(Talvez)_- -

---

## 8. Flujo de Git / GitHub

- Ramas:
  - `main` → Codigo principal, que funcione sin errores.
  - `develop` → Verfificacion previa del codigo trabajado
  - `task/...` → Trabajo individual.
    - Ej: `(task/agregar-funcion-reserva)`
  - `commits` → Commits de trabajo individual.
    
    - feat: →nueva funcionalidad.
    - fix: → corrección de error.
    - test: → cambios relacionados con pruebas.


- Pasos diarios:
  1. Hacer **Pull** de `develop`.
  2. Crear rama `task/...`.
  3. Commit → Push → Pull Request → Revisión.
  4. Merge a `develop` (*nunca directo a `main`*).

---

## 9. Primeros PRs planificados

1. **PR #1 (Jherson)**: Modelos + Interfaces (sin implementación).  
2. **PR #2 (Camila)**: Menú de consola vacío (opciones sin lógica).  
3. **PR #3 (Jherson)**: Implementación de servicios + tests unitarios.  
4. **PR #4 (Camila)**: Conexión de la UI con los servicios.

---


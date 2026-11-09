# Kepler-7 — Continuación del desarrollo (pasos 9 a 12)

## Contexto del proyecto

Kepler-7 es el trabajo práctico final de patrones de diseño: un videojuego de escritorio en C# con .NET 8 y Windows Forms, ambientado en la Estación Orbital Kepler-7, que atraviesa una crisis. El enunciado completo está en `TP_Final_Kepler-7.docx`. Exige doce patrones obligatorios: dos creacionales, cuatro estructurales y seis de comportamiento. Cada uno tiene que resolver un problema concreto del juego y verse durante la partida. Solo se pueden usar las bibliotecas incluidas en .NET: nada de motores, frameworks gráficos ni paquetes NuGet de terceros.

La solución `Kepler7.sln` tiene dos proyectos:

- `Kepler7.Core` (biblioteca de clases, `net8.0`): contiene la lógica del juego y los patrones, en `Kepler7.Core/Patrones/<NombreDelPatron>`. `Class1.cs` es un resto de la plantilla.
- `Kepler7.Game` (WinForms, `net8.0-windows`): conserva el `Form1` vacío de la plantilla. Todavía no hay pantallas conectadas.

Los ocho primeros patrones están implementados en el Core y publicados en `main`. [Repositorio](https://github.com/juanNH/kelper_tp) · [commit e2ef998](https://github.com/juanNH/kelper_tp/commit/e2ef998) con los pasos 5 a 8, sobre `cbd01c6`, que tiene los pasos 1 a 4.

## Cómo trabajar

- Implementá **un patrón por vez**. Empezá por **Command**. Después de implementarlo y verificarlo, presentá el resultado y esperá la indicación para seguir con el próximo.
- Trabajá primero en el Core, sin conectar pantallas. Colocá las clases nuevas en `Kepler7.Core/Patrones/<NombreDelPatron>`, con el namespace `Kepler7.Core.Patrones.<NombreDelPatron>`, y reutilizá los modelos que ya existen. Los patrones se encadenan a propósito; por ejemplo, State usa la cadena de Chain of Responsibility y Visitor aplica el estado Envenenado de State.
- Para la estructura de cada patrón usá su ficha en [Refactoring.Guru](https://refactoring.guru/es/design-patterns), y para los requisitos del juego, el enunciado. Respetá los nombres de interfaces, clases y métodos que pide el enunciado.
- Priorizá código fácil de seguir: clases `sealed`, validación de argumentos con `ArgumentNullException.ThrowIfNull` y `ArgumentOutOfRangeException.ThrowIf…`, comentarios breves que expliquen el porqué, y textos en español para lo que vaya a ver el jugador.
- **La persistencia queda para el final del proyecto.** Por ahora todo vive en memoria y no se escribe nada a disco. El registro de intentos denegados del Proxy es una lista en memoria y hay que hacerlo persistente al final, porque el enunciado lo exige.
- Al cerrar cada paso, ejecutá `dotnet build .\Kepler7.sln` (tiene que dar 0 advertencias y 0 errores) y verificá el comportamiento desde el Core. Para eso usá una consola temporal **fuera del repositorio**, en el directorio temporal de la sesión, con un `ProjectReference` a `Kepler7.Core.csproj` y chequeos que impriman `OK` o `FALLA`. Después resumí las clases principales, cómo usar el patrón y qué se verificó.
- Los commits van **directo a `main`**, con mensaje en español en modo imperativo (por ejemplo: «Implementa Command…»). La identidad de Git está configurada en este repositorio. En la máquina está instalado el SDK de .NET 10, pero los proyectos apuntan a .NET 8 y compilan sin problema.

## Orden de los 12 patrones

| Paso | Patrón | Estado | Responsabilidad |
| --- | --- | --- | --- |
| 1 | Builder | Implementado | Construir naves a partir de recetas y componentes tipados. |
| 2 | Flyweight | Implementado | Compartir cinco especies entre 10.000 tripulantes. |
| 3 | Prototype | Implementado | Clonar un tripulante con habilidades independientes y especie compartida. |
| 4 | Bridge | Implementado | Combinar tipos de mensaje y cifrados. |
| 5 | Proxy | Implementado | Controlar el acceso a la bóveda y registrar los intentos denegados (por ahora en memoria). |
| 6 | Chain of Responsibility | Implementado | Pasar el daño por una cadena configurable de defensas. |
| 7 | State | Implementado | Delegar las acciones del personaje en su estado actual y reutilizar la cadena para resolver el daño. |
| 8 | Visitor | Implementado | Aplicar operaciones sobre los ítems del inventario y reutilizar State para el envenenamiento. |
| 9 | Command | **Siguiente** | Encapsular las acciones del jugador para registrar, deshacer y rehacer. |
| 10 | Strategy | Pendiente | Elegir la acción de cada enemigo con una IA intercambiable. |
| 11 | Template Method | Pendiente | Fijar la secuencia del turno y reutilizar las reglas de acción y decisión. |
| 12 | Facade | Pendiente | Coordinar los seis subsistemas detrás de `ControlCentral`. |

Después del paso 12 quedan: la persistencia (con `System.Text.Json`), las pantallas de `Kepler7.Game` que muestran cada patrón en la partida y la gamificación (progreso, recompensas y respuestas visuales).

## Qué está implementado en el Core

### 1. Builder — `Patrones/Builder`
- `INaveBuilder` tiene `Reiniciar()` y los métodos `PonerCasco`, `PonerMotor`, `PonerEscudo`, `PonerArmas`, `PonerRadar`, `PonerTanqueCombustible` y `PonerInteligenciaArtificial`.
- `NaveBuilder` los implementa y entrega la nave con `ConstruirNave()`, que también reinicia el builder.
- `IngenieroJefe` es el director, con `RecetaExploradora`, `RecetaCombate` y `RecetaCarga`.
- `Nave` expone sus componentes como propiedades opcionales.
- Los componentes están en `Builder/Componentes`: son clases abstractas (`Casco`, `Motor`, `Escudo`, `Armamento`, `Radar`, `TanqueCombustible` e `InteligenciaArtificial`) con variantes que, por ahora, solo exponen su `Nombre`.

```csharp
var builder = new NaveBuilder();
new IngenieroJefe().RecetaCombate(builder);
Nave nave = builder.ConstruirNave();
```

### 2. Flyweight — `Patrones/Flyweight`
- `IEspecie` es el estado intrínseco: nombre, `System.Drawing.Color`, planeta de origen y dieta.
- `FabricaEspecies` usa un `Dictionary<string, IEspecie>` con cinco especies: Humana, Aureliana, Khariana, Nyxiana y Pelágica.
- `Tripulante` (clase `partial`) guarda el estado extrínseco (identificador, nombre, rango y `List<string> Habilidades`) y la referencia a su especie.
- `RegistroTripulantes.Generar(10_000)` reparte las cinco especies por turnos.
- `BibliotecaTripulantes.GenerarDemostracion()` devuelve la lista y la comparación simulada: 5 objetos de especie frente a 10.000.

### 3. Prototype — `Patrones/Prototype`
- `IPrototipo<T>.Clonar()`.
- `Tripulante.Prototype.cs` extiende `Tripulante` como clase parcial. El clon copia los datos, comparte la especie y crea su propia lista de habilidades, sin usar `MemberwiseClone`.

### 4. Bridge — `Patrones/Bridge`
- La abstracción `Mensaje` tiene una referencia a `ICifrado` y expone `Enviar(texto)`.
- Los tipos de mensaje son `MensajeTexto`, `MensajeAlerta` y `MensajeEmergencia`.
- Los cifrados son `SinCifrado`, `CifradoAES` (AES-GCM con clave por instancia) y `CifradoCuantico` (una simulación del juego).

### 5. Proxy — `Patrones/Proxy`
- `IBoveda` tiene `LeerArchivo(id)` y `EscribirArchivo(id, contenido)`.
- `BovedaDatos` es la bóveda real, con un diccionario en memoria y tres archivos iniciales: `coordenadas-salto`, `codigos-defensa` e `informe-crisis`.
- `ProxyBoveda(boveda, usuario, registro)` deja pasar solo a `RolUsuario.Capitan` y `RolUsuario.OficialSeguridad`. A cualquier otro rol lo anota en el registro y le lanza `UnauthorizedAccessException`.
- `Usuario(Nombre, Rol)` es un `record`. `IntentoDenegado` guarda fecha, usuario, rol, operación (`Lectura` o `Escritura`) e id del archivo.
- `RegistroIntentosDenegados` tiene `Registrar`, `ObtenerIntentos` y `CantidadIntentos`. Es una lista **en memoria** y la persistencia queda pendiente.

### 6. Chain of Responsibility — `Patrones/ChainOfResponsibility`
- `Danio` es la solicitud. Tiene `Cantidad`, `Restante`, `Esquivable`, `Recorrido` (una lista de `PasoDanio` con eslabón, recibido, absorbido, transferido y detalle) y `Registrar(...)`.
- `IManejadorDanio` tiene `Nombre`, `Siguiente` y `Manejar(Danio)`.
- `ManejadorDanioBase` registra el paso y transfiere el resto al siguiente. Cada eslabón solo implementa `Absorber(Danio)`.
- Eslabones:
  - `Esquiva(probabilidad, Random?)` evita el golpe entero, salvo que `Esquivable` sea falso.
  - `Bloqueo(porcentaje)` detiene un porcentaje del daño.
  - `Armadura(reduccion)` resta una cantidad fija a cada golpe.
  - `Escudo(puntos)` tiene una reserva que se gasta y se puede recargar con `Recargar`.
  - `Vida(puntosMaximos)` recibe todo lo que llega. Tiene `Puntos`, `EstaVivo` y `Curar`.
- `CadenaDefensas` tiene `Agregar`, `Insertar`, `Quitar` y `Mover`, y vuelve a enlazar los eslabones en cada cambio. `ResolverAtaque(cantidad, esquivable = true)` devuelve el `Danio` procesado. `CrearPredeterminada(Random?)` arma Esquiva 20 %, Bloqueo 25 %, Armadura 5, Escudo 30 y Vida 100.

### 7. State — `Patrones/State`
- `IEstadoPersonaje` tiene `Nombre`, `Color`, `PuedeAtacar` y `PuedeMover`, más los métodos `Atacar(p)`, `Mover(p)`, `RecibirDanio(p, cantidad)` y `FinalizarTurno(p)`.
- `ResultadoAccion(Realizada, Valor, Descripcion)` es el resultado de una acción. `Valor` es el daño causado al atacar o las casillas recorridas al moverse.
- `Personaje(nombre, ataque, movimiento, defensas)` es el contexto. Toma la `Vida` de su cadena y arranca en `Normal`. Expone `CambiarEstado`, `Atacar()`, `Mover()`, `RecibirDanio(cantidad)` y `FinalizarTurno()`. Es `sealed` y no es `partial`: si otro patrón necesita agregarle datos (por ejemplo, una posición), conviene convertirlo en `partial`, como `Tripulante`.
- Estados:
  - `Normal` usa los valores base.
  - `Envenenado(turnos = 3, danioPorTurno = 5)` ataca con el 75 % de la fuerza y se mueve la mitad. Al terminar cada turno el veneno le quita vida directo con `Vida.Manejar` y, al acabarse, vuelve a Normal.
  - `Aturdido(turnos = 1)` no puede atacar ni moverse y recibe el daño como no esquivable. Al terminar vuelve a Normal.
  - `Berserk(turnos = 2)` ataca el doble, se mueve una casilla más y recibe un 50 % más de daño. Al terminar pasa a Aturdido por un turno.

### 8. Visitor — `Patrones/Visitor`
- `IItem` tiene `Nombre`, `Precio` y `Aceptar(IVisitor)`.
- `IVisitor` tiene las sobrecargas `Visitar(Arma)`, `Visitar(Pocion)` y `Visitar(Armadura)`, y para la interfaz `Nombre`, `Resumen` y `Detalle`.
- Ítems:
  - `Arma(nombre, precio, danio)` puede envenenarse con `Envenenar()`.
  - `Pocion(nombre, precio, curacion, cantidad)` puede contaminarse con `Contaminar()`.
  - `Armadura(nombre, precio, defensa, resisteVeneno)` es distinta del eslabón `Armadura` de la cadena. Si un archivo usa los dos namespaces, hay que ponerle un alias a una.
- `Inventario.Aplicar(visitante)` recorre los ítems y devuelve el mismo visitante. Hay que usar uno nuevo en cada análisis.
- Visitantes:
  - `CalculadorPrecio` suma el valor total: arma envenenada +50 %, poción por cantidad (contaminada vale 0).
  - `ContadorArmas` cuenta las armas, las envenenadas y el daño total.
  - `AplicadorVeneno(portador?)` envenena armas y contamina pociones. Si una armadura no está sellada, pasa al portador a `Envenenado`.

## Próximos cuatro pasos

### 9. Command — Sistema de turnos y repetición

Definí `IComando` con `Ejecutar()` y `Deshacer()`, e implementá `MoverComando`, `AtacarComando`, `CurarComando` y `ConstruirComando`. El invocador `GestorComandos` tiene una pila para deshacer y otra para rehacer, y guarda un historial de los comandos ejecutados con su descripción. Ejecutar un comando nuevo vacía la pila de rehacer. [Command en Refactoring.Guru](https://refactoring.guru/es/design-patterns/command)

- `MoverComando` y `AtacarComando` actúan sobre un `Personaje` de State, así que el resultado depende del estado actual. Para mover hace falta una posición, que el personaje todavía no tiene. `AtacarComando` aplica el daño con `objetivo.RecibirDanio(valor)`, que pasa por la cadena de defensas.
- Deshacer un ataque tiene que devolver lo que consumió: la vida (con `Vida.Curar`) y los puntos de escudo (con `Escudo.Recargar`). Para eso sirve el `Recorrido` del `Danio`.
- `CurarComando` usa `Vida.Curar`, que no pasa del máximo, y tiene que guardar cuánto curó en realidad. Para deshacerlo hace falta un método que reste vida directo.
- `ConstruirComando` arma una nave con `IngenieroJefe` y `NaveBuilder` y la guarda en un hangar. Deshacerlo la saca del hangar.

Verificá ejecutar, deshacer y rehacer cada comando, que un comando nuevo vacíe la pila de rehacer y que el historial quede bien.

### 10. Strategy — Inteligencia artificial intercambiable

Definí `IEstrategiaIA.DecidirAccion(Enemigo, Contexto)` e implementá `IAAgresiva`, `IADefensiva`, `IAAleatoria` e `IACalculadora`. Cada `Enemigo` recibe una estrategia que se puede cambiar durante la partida, y el cambio tiene que notarse en sus decisiones. Conviene que el enemigo tenga o reutilice un `Personaje` (con State y su cadena de defensas). El `Contexto` puede incluir el objetivo, la vida de ambos y la distancia. La decisión puede devolverse como un comando del paso 9. `IAAleatoria` recibe un `Random` para que las pruebas sean predecibles. Verificá que el mismo enemigo, en el mismo contexto, decida distinto con cada estrategia. [Strategy en Refactoring.Guru](https://refactoring.guru/es/design-patterns/strategy)

### 11. Template Method — Turno de unidad

Creá la clase abstracta `TurnoUnidad` con el método **no virtual** `EjecutarTurno()`, que llama siempre en el mismo orden a inicio, decisión, acción y finalización. Implementá `TurnoSoldado`, `TurnoMedico`, `TurnoFrancotirador` y `TurnoComandante`, que redefinen los pasos variables. Registrá cada etapa en un historial que se pueda consultar. Reutilizá lo anterior: la decisión puede salir de una estrategia del paso 10, la acción puede ejecutarse como comando del paso 9 y la finalización llama a `Personaje.FinalizarTurno()` de State. Verificá que el orden de las etapas sea el mismo en todas las subclases y que cada una se comporte distinto. [Template Method en Refactoring.Guru](https://refactoring.guru/es/design-patterns/template-method)

### 12. Facade — Control central

Implementá los seis subsistemas: Energía, Soporte Vital, Navegación, Comunicaciones, Seguridad y Defensa. La fachada `ControlCentral` expone `IniciarTurno()`, `EntrarEnEmergencia()` y `ApagarEstacion()`, y devuelve la secuencia de acciones que coordinó. Los subsistemas tienen que ser `internal` para que el formulario no pueda llamarlos directamente. Reutilizá los patrones anteriores donde tenga sentido: Comunicaciones envía con `Mensaje`/`ICifrado` (Bridge), Seguridad usa `ProxyBoveda` (Proxy), Defensa usa `CadenaDefensas` (Chain) y Navegación usa las naves del Builder. Verificá las tres operaciones y el orden de su secuencia. [Facade en Refactoring.Guru](https://refactoring.guru/es/design-patterns/facade)

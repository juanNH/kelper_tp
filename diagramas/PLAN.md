# Plan de diagramas UML de Kepler-7

La entrega requiere 13 archivos editables de Draw.io. El primero describe la arquitectura de la solución. Los doce siguientes muestran las clases y relaciones de cada patrón tal como se usan en el juego.

| Orden | Archivo | Qué dibujar | Dónde se ve en el juego |
| --- | --- | --- | --- |
| 01 | [01-arquitectura-general.drawio](01-arquitectura-general.drawio) | Componentes de WinForms, Core y persistencia. Puerto del repositorio, adaptador SQL y almacenes. | Inicio, guardado, reanudación, progreso y cierre de la misión. |
| 02 | [02-builder.drawio](02-builder.drawio) | `INaveBuilder`, `NaveBuilder`, `IngenieroJefe`, `Nave` y sus componentes opcionales. | Construcción de una receta en el Hangar. |
| 03 | `03-flyweight.drawio` | `Tripulante`, `IEspecie`, `Especie`, `FabricaEspecies` y `RegistroTripulantes`. Distinguir datos compartidos y propios. | Cinco instancias de especie para 10.000 tripulantes en Escuadrón. |
| 04 | `04-prototype.drawio` | `IPrototipo<T>`, `Tripulante.Clonar()` y los miembros que representan las habilidades y la especie. | Clonar un tripulante, compartir la especie y editar las habilidades de la copia. |
| 05 | `05-bridge.drawio` | Jerarquía `Mensaje` y jerarquía `ICifrado`. Incluir las tres variantes de cada lado y la asociación entre ambas. | Elegir el protocolo y transmitir al recuperar la antena. |
| 06 | `06-proxy.drawio` | `IBoveda`, `BovedaDatos`, `ProxyBoveda`, `Usuario` y `RegistroIntentosDenegados`. | Leer o escribir según el rol y consultar la auditoría de denegaciones. |
| 07 | `07-chain-of-responsibility.drawio` | `CadenaDefensas`, `IManejadorDanio`, `ManejadorDanioBase`, `Danio` y los eslabones. | Reordenar defensas y leer la absorción de un ataque en la bitácora. |
| 08 | `08-state.drawio` | `Personaje`, `IEstadoPersonaje`, los cuatro estados y las operaciones que intervienen en los cambios de estado. | Aplicar un estado y observar acciones, duración y efectos, incluida la transición de Berserk a Aturdido y el retorno a Normal. |
| 09 | `09-visitor.drawio` | `Inventario`, `IItem`, `IVisitor`, los tres ítems, los tres visitantes y las operaciones `Aceptar` y `Visitar`. | Calcular precio, contar armas o aplicar veneno en Equipamiento. |
| 10 | `10-command.drawio` | `IComando`, `GestorComandos`, los cuatro comandos concretos y sus receptores `Personaje` y `Hangar`. | Construir en el Hangar y deshacer o rehacer acciones en la misión. |
| 11 | `11-strategy.drawio` | `Enemigo`, `IEstrategiaIA`, las cuatro estrategias, `Contexto` y `DecisionIA`. | Cambiar la estrategia de un dron y comparar su siguiente decisión. |
| 12 | `12-template-method.drawio` | `TurnoUnidad`, `TurnoTripulante`, los cuatro roles y `TurnoDron`. Identificar los pasos de `EjecutarTurno()`. | Finalizar turno y revisar Inicio, Decisión, Acción y Finalización. |
| 13 | `13-facade.drawio` | `ControlCentral` y sus seis subsistemas. Identificar las tres operaciones públicas. | Desplegar, activar emergencia y apagar la estación. |

## Orden de trabajo

Primero se fija el mapa de capas. Builder y Flyweight introducen la nave y los tripulantes antes de reutilizarlos en otros patrones. Chain of Responsibility precede a State para que ya se entienda cómo se recibe el daño. Command precede a Strategy y Template Method porque las decisiones y los turnos pueden apoyarse en comandos. Facade queda al final porque reúne varios subsistemas ya dibujados.

## Convenciones de los diagramas de clases

- Cada patrón tiene su propio diagrama de clases. Incluye las clases, subclases, interfaces y enumeraciones que participan en el patrón. La presentación es minimalista y omite títulos, subtítulos, notas, etiquetas descriptivas en las relaciones y agrupaciones por capa.
- Usá los nombres del código para los tipos, atributos, operaciones y parámetros. Para tipos primitivos con equivalente en UML, usá la forma de UML, como `String` para `string`. Escribí los atributos como `visibilidad nombre: Tipo` y las operaciones como `visibilidad nombre(parámetro: Tipo): Retorno`. Omití el tipo de retorno para operaciones `void`. Omití los accesores de C# y las multiplicidades entre corchetes en los atributos. Conservá `{readOnly}` para las propiedades de solo lectura.
- Usá `+` para visibilidad pública, `-` para privada, `#` para protegida y `~` para visibilidad de paquete. Para un miembro `internal` de C#, usá `~` como aproximación. Subrayá los miembros estáticos y escribí en cursiva los nombres de las clases y operaciones abstractas. Encabezá las interfaces con `«interface»` y los tipos declarados con `enum` con `«enumeration»`, con sus valores en un compartimento.
- Representá un tipo u operación `partial` como un solo elemento. Omití los marcadores `sealed` e `isFinalSpecialization` y las anotaciones `override` y `redefines`.
- Usá una línea continua para asociación, con multiplicidades en los extremos cuando correspondan. Poné el rombo vacío de agregación o el rombo lleno de composición en el extremo del todo. Usá un triángulo vacío hacia la clase base para generalización y hacia la interfaz para realización, con línea continua y discontinua respectivamente. Dibujá las dependencias con línea discontinua y punta abierta hacia el elemento del que se depende.

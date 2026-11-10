using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.Facade;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.Strategy;
using Kepler7.Core.Patrones.Visitor;
using ArmaduraDefensiva = Kepler7.Core.Patrones.ChainOfResponsibility.Armadura;
using ArmaduraEquipada = Kepler7.Core.Patrones.Visitor.Armadura;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public const int Columnas = 8;
    public const int Filas = 6;

    private static readonly (int Columna, int Fila)[] direcciones =
    {
        (0, -1), (1, 0), (0, 1), (-1, 0)
    };

    private readonly UnidadMision[] unidades;
    private readonly List<string> bitacora;
    private readonly GestorComandos comandos = new();
    private readonly Random azar;
    private bool activa = true;

    internal SesionMision(ResumenDespliegue despliegue, List<string> bitacora,
        ILookup<int, IItem> equipoPorAliado, ControlCentral controlCentral, Random? azar = null)
    {
        azar ??= new AzarGuardable(Random.Shared.Next());
        this.azar = azar;
        azarGuardable = azar as AzarGuardable;
        this.bitacora = bitacora;
        this.controlCentral = controlCentral;
        protocoloMision = despliegue.Protocolo;
        Bitacora = bitacora.AsReadOnly();
        unidades = new[]
        {
            CrearAliada(despliegue.Escuadron[0], new CasillaMision(2, 3), equipoPorAliado, azar),
            CrearAliada(despliegue.Escuadron[1], new CasillaMision(1, 4), equipoPorAliado, azar),
            CrearDron(-1, "Dron 1", new CasillaMision(5, 2), azar),
            CrearDron(-2, "Dron 2", new CasillaMision(6, 4), azar)
        };
    }

    public IReadOnlyList<CasillaMision> Obstaculos { get; } =
        Array.AsReadOnly(new[] { new CasillaMision(4, 1), new CasillaMision(4, 2) });

    public CasillaMision Antena { get; } = new(6, 1);

    public IReadOnlyList<string> Bitacora { get; }

    public IReadOnlyList<FichaUnidadMision> Unidades => Array.AsReadOnly(unidades.Select(unidad =>
    {
        Personaje personaje = unidad.Personaje;
        return new FichaUnidadMision(unidad.Identificador, personaje.Nombre, unidad.EsAliada,
            ObtenerCasilla(unidad), personaje.Vida.Puntos, personaje.Vida.PuntosMaximos,
            ObtenerEscudo(personaje), CalcularAlcance(unidad) > 0)
        {
            PuedeAtacar = PuedeAtacar(unidad),
            EscudoMaximo = unidad.EscudoMaximo,
            PuedeUsarBotiquin = MotivoBloqueoBotiquin(unidad) == null,
            Botiquin = unidad.Botiquin?.Nombre,
            BotiquinUsado = unidad.BotiquinUsado,
            CuracionBotiquin = unidad.CuracionBotiquin,
            Arma = unidad.Arma?.Nombre,
            Danio = personaje.Ataque,
            DanioActual = personaje.Atacar().Valor,
            Armadura = unidad.Armadura?.Nombre,
            Defensa = personaje.Defensas.Eslabones.OfType<ArmaduraDefensiva>().Sum(armadura => armadura.Reduccion),
            Habilidades = unidad.Habilidades,
            Movimiento = personaje.Movimiento,
            MovimientoActual = ObtenerAlcanceMovimiento(personaje),
            Estado = ObtenerTipoEstado(personaje.Estado),
            TurnosEstadoRestantes = personaje.Estado.TurnosRestantes,
            PuedeRecibirEstado = MotivoBloqueoAcciones == null && EsUnidadViva(unidad) && personaje.Estado is Normal,
            EstrategiaDron = unidad.IA == null ? null : ObtenerTipoEstrategia(unidad.IA)
        };
    }).ToArray());

    public IReadOnlyList<CasillaMision> ObtenerDestinos(int identificador)
    {
        UnidadMision? unidad = BuscarUnidad(identificador);
        return unidad == null ? Array.Empty<CasillaMision>() : ObtenerDestinos(unidad);
    }

    public ResultadoMovimiento MoverUnidad(int identificador, CasillaMision destino)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoMovimiento(false, motivo);
        }

        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad is not { EsAliada: true })
        {
            return new ResultadoMovimiento(false, "Seleccioná una unidad aliada.");
        }

        if (unidad.Movio)
        {
            return new ResultadoMovimiento(false, $"{unidad.Personaje.Nombre} ya se movió en este turno.");
        }

        if (!unidad.Personaje.Estado.PuedeMover)
        {
            return new ResultadoMovimiento(false, unidad.Personaje.Mover().Descripcion);
        }

        if (!ObtenerDestinos(unidad).Contains(destino))
        {
            return new ResultadoMovimiento(false, "Esa casilla no está al alcance de la unidad.");
        }

        var comando = new MoverComando(unidad.Personaje, new Posicion(destino.Columna, destino.Fila));
        ResultadoMovimiento movimiento = EjecutarMovimiento(comando, bitacora.Add);
        if (movimiento.Realizado)
        {
            unidad.Movio = true;
            ataquesDeshacibles.Clear();
        }

        return movimiento;
    }

    private ResultadoMovimiento EjecutarMovimiento(MoverComando comando, Action<string> registrar)
    {
        CasillaMision origen = ObtenerCasilla(comando.Personaje.Posicion);
        if (!comandos.Ejecutar(comando))
        {
            return new ResultadoMovimiento(false, comando.Descripcion);
        }

        CasillaMision destino = ObtenerCasilla(comando.Destino);
        string mensaje = $"{comando.Personaje.Nombre} se movió de {FormatearCasilla(origen)} a {FormatearCasilla(destino)}.";
        Personaje personaje = comando.Personaje;
        string detalle = personaje.Estado is Normal
            ? mensaje
            : $"{mensaje} Alcance con {personaje.Estado.Nombre}: {ObtenerAlcanceMovimiento(personaje)} casillas.";
        registrar(detalle);
        return new ResultadoMovimiento(true, mensaje);
    }

    internal void Cerrar()
    {
        activa = false;
        ataquesDeshacibles.Clear();
    }

    private IReadOnlyList<CasillaMision> ObtenerDestinos(UnidadMision unidad)
    {
        int alcance = CalcularAlcance(unidad);
        if (alcance <= 0)
        {
            return Array.Empty<CasillaMision>();
        }

        CasillaMision origen = ObtenerCasilla(unidad);
        return BuscarRutas(origen, alcance).Keys.Where(casilla => casilla != origen).ToList().AsReadOnly();
    }

    private Dictionary<CasillaMision, PasoRuta> BuscarRutas(CasillaMision origen, int alcance)
    {
        var bloqueadas = unidades.Where(EsUnidadViva).Select(ObtenerCasilla).Concat(Obstaculos).ToHashSet();
        var rutas = new Dictionary<CasillaMision, PasoRuta> { [origen] = new(0, origen) };
        var pendientes = new Queue<CasillaMision>();
        pendientes.Enqueue(origen);
        while (pendientes.TryDequeue(out var actual))
        {
            int distancia = rutas[actual].Distancia;
            if (distancia == alcance)
            {
                continue;
            }

            foreach (var direccion in direcciones)
            {
                var siguiente = new CasillaMision(actual.Columna + direccion.Columna,
                    actual.Fila + direccion.Fila);
                if (DentroDelTablero(siguiente) && bloqueadas.Add(siguiente))
                {
                    rutas.Add(siguiente, new PasoRuta(distancia + 1, actual));
                    pendientes.Enqueue(siguiente);
                }
            }
        }

        return rutas;
    }

    private UnidadMision? BuscarUnidad(int identificador) =>
        Array.Find(unidades, unidad => unidad.Identificador == identificador);

    private int CalcularAlcance(UnidadMision unidad)
    {
        if (MotivoBloqueoAcciones != null || !unidad.EsAliada || unidad.Movio || !EsUnidadViva(unidad))
        {
            return 0;
        }

        return ObtenerAlcanceMovimiento(unidad.Personaje);
    }

    private static int ObtenerAlcanceMovimiento(Personaje personaje)
    {
        ResultadoAccion movimiento = personaje.Mover();
        return movimiento.Realizada ? movimiento.Valor : 0;
    }

    private static bool DentroDelTablero(CasillaMision casilla) =>
        casilla.Columna >= 0 && casilla.Columna < Columnas
        && casilla.Fila >= 0 && casilla.Fila < Filas;

    private static CasillaMision ObtenerCasilla(UnidadMision unidad) =>
        ObtenerCasilla(unidad.Personaje.Posicion);

    private static CasillaMision ObtenerCasilla(Posicion posicion) => new(posicion.X, posicion.Y);

    private static string FormatearCasilla(CasillaMision casilla) =>
        $"({casilla.Columna + 1}, {casilla.Fila + 1})";

    private static bool EsUnidadViva(UnidadMision unidad) => unidad.Personaje.Vida.EstaVivo;

    private static int ObtenerEscudo(Personaje personaje) =>
        personaje.Defensas.Eslabones.OfType<Escudo>().Sum(escudo => escudo.Puntos);

    private static UnidadMision CrearAliada(FichaTripulante tripulante, CasillaMision casilla,
        ILookup<int, IItem> equipoPorAliado, Random? azar)
    {
        IEnumerable<IItem> equipo = equipoPorAliado[tripulante.Identificador];
        Arma arma = equipo.OfType<Arma>().Single();
        ArmaduraEquipada armadura = equipo.OfType<ArmaduraEquipada>().Single();
        Pocion? botiquin = equipo.OfType<Pocion>().SingleOrDefault();
        int ataque = arma.Danio + (TieneHabilidad(tripulante, CatalogoHabilidades.Punteria)
            ? CatalogoHabilidades.BonoDanio : 0);
        int movimiento = 2 + (TieneHabilidad(tripulante, CatalogoHabilidades.Agilidad)
            ? CatalogoHabilidades.BonoMovimiento : 0);
        int curacion = botiquin == null ? 0 : botiquin.Curacion
            + (TieneHabilidad(tripulante, CatalogoHabilidades.PrimerosAuxilios) ? CatalogoHabilidades.BonoCuracion : 0);
        CadenaDefensas defensas = CrearDefensas(esquiva: 10, bloqueo: 15, armadura: armadura.Defensa,
            escudo: 10, vida: 70, azar: azar);
        var personaje = new Personaje(tripulante.Nombre, ataque, movimiento, defensas,
            new Posicion(casilla.Columna, casilla.Fila));
        return new UnidadMision(tripulante.Identificador, personaje)
        {
            Arma = arma,
            Armadura = armadura,
            Botiquin = botiquin,
            CuracionBotiquin = curacion,
            Habilidades = Array.AsReadOnly(tripulante.Habilidades.ToArray())
        };
    }

    private static bool TieneHabilidad(FichaTripulante tripulante, string habilidad) =>
        tripulante.Habilidades.Contains(habilidad, StringComparer.OrdinalIgnoreCase);

    private static UnidadMision CrearDron(int identificador, string nombre, CasillaMision casilla, Random? azar)
    {
        CadenaDefensas defensas = CrearDefensas(esquiva: 5, bloqueo: 10, armadura: 2, escudo: 6, vida: 36, azar: azar);
        var personaje = new Personaje(nombre, ataque: 30, movimiento: 2,
            defensas, new Posicion(casilla.Columna, casilla.Fila));
        return new UnidadMision(identificador, personaje, new Enemigo(nombre, new IAAgresiva(), personaje));
    }

    private static CadenaDefensas CrearDefensas(int esquiva, int bloqueo, int armadura, int escudo, int vida, Random? azar)
    {
        var defensas = new CadenaDefensas();
        defensas.Agregar(new Esquiva(esquiva, azar));
        defensas.Agregar(new Bloqueo(bloqueo));
        defensas.Agregar(new ArmaduraDefensiva(armadura));
        defensas.Agregar(new Escudo(escudo));
        defensas.Agregar(new Vida(vida));
        return defensas;
    }

    private readonly record struct PasoRuta(int Distancia, CasillaMision Anterior);

    private sealed record UnidadMision(int Identificador, Personaje Personaje, Enemigo? IA = null)
    {
        public bool EsAliada => IA == null;
        public IReadOnlyList<int> EscudosMaximos { get; } = ObtenerPuntosEscudos(Personaje);
        public int EscudoMaximo => EscudosMaximos.Sum();
        public Arma? Arma { get; init; }
        public ArmaduraEquipada? Armadura { get; init; }
        public Pocion? Botiquin { get; init; }
        public int CuracionBotiquin { get; init; }
        public IReadOnlyList<string> Habilidades { get; init; } = Array.Empty<string>();
        public bool BotiquinUsado => RondaUsoBotiquin.HasValue;
        public int? RondaUsoBotiquin { get; set; }
        public bool Movio { get; set; }
        public bool Ataco { get; set; }
    }
}

public readonly record struct CasillaMision(int Columna, int Fila);

public sealed record FichaUnidadMision(int Identificador, string Nombre, bool EsAliada,
    CasillaMision Posicion, int Vida, int VidaMaxima, int Escudo, bool PuedeMoverse)
{
    public bool PuedeAtacar { get; init; }
    public int EscudoMaximo { get; init; }
    public bool PuedeUsarBotiquin { get; init; }
    public string? Botiquin { get; init; }
    public bool BotiquinUsado { get; init; }
    public int CuracionBotiquin { get; init; }
    public string? Arma { get; init; }
    public int Danio { get; init; }
    public int DanioActual { get; init; }
    public string? Armadura { get; init; }
    public int Defensa { get; init; }
    public IReadOnlyList<string> Habilidades { get; init; } = Array.Empty<string>();
    public int Movimiento { get; init; }
    public int MovimientoActual { get; init; }
    public TipoEstadoUnidad Estado { get; init; }
    public int TurnosEstadoRestantes { get; init; }
    public bool PuedeRecibirEstado { get; init; }
    public TipoEstrategiaDron? EstrategiaDron { get; init; }
}

public sealed record ResultadoMovimiento(bool Realizado, string Mensaje);

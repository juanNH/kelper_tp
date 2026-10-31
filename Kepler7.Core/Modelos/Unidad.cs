using Kepler7.Core.Patrones.Prototype;

namespace Kepler7.Core.Modelos;

public class Unidad : IPrototipoUnidad
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Nombre { get; set; } = "";
    public string Rol { get; set; } = "";
    public bool EsEnemigo { get; set; }
    public int Vida { get; set; }
    public int Ataque { get; set; }
    public int Defensa { get; set; }
    public int X { get; set; } = -1;
    public int Y { get; set; } = -1;
    public string Estado { get; set; } = "Normal";
    public List<string> Equipamiento { get; set; } = new List<string>();
    public List<string> Habilidades { get; set; } = new List<string>();

    public Unidad Clonar()
    {
        return new Unidad
        {
            Nombre = Nombre,
            Rol = Rol,
            EsEnemigo = EsEnemigo,
            Vida = Vida,
            Ataque = Ataque,
            Defensa = Defensa,
            X = X,
            Y = Y,
            Estado = Estado,
            Equipamiento = new List<string>(Equipamiento),
            Habilidades = new List<string>(Habilidades)
        };
    }

    public override string ToString()
    {
        string posicion = X < 0 || Y < 0 ? "sin asignar" : X + ", " + Y;
        return Nombre + " (" + Rol + ", " + (EsEnemigo ? "enemigo" : "aliado") + ")" + Environment.NewLine +
               "ID: " + Id + Environment.NewLine +
               "Vida: " + Vida + " | Ataque: " + Ataque + " | Defensa: " + Defensa + Environment.NewLine +
               "Posición: " + posicion + " | Estado: " + Estado + Environment.NewLine +
               "Equipamiento: " + string.Join(", ", Equipamiento) + Environment.NewLine +
               "Habilidades: " + string.Join(", ", Habilidades);
    }
}

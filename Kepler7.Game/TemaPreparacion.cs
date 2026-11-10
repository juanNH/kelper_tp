namespace Kepler7.Game;

internal static class TemaPreparacion
{
    public static readonly Color Fondo = Color.FromArgb(16, 12, 40);
    public static readonly Color Superficie = Color.FromArgb(26, 21, 55);
    public static readonly Color Panel = Color.FromArgb(42, 34, 82);
    public static readonly Color Borde = Color.FromArgb(83, 71, 144);
    public static readonly Color Acento = Color.FromArgb(255, 212, 91);
    public static readonly Color Texto = Color.FromArgb(246, 241, 255);
    public static readonly Color Secundario = Color.FromArgb(199, 187, 230);

    public static readonly Font Titulo = new("Consolas", 30, FontStyle.Bold);
    public static readonly Font Subtitulo = new("Consolas", 15, FontStyle.Bold);
    public static readonly Font Codigo = new("Consolas", 11, FontStyle.Bold);
    public static readonly Font Cuerpo = new("Segoe UI", 12);

    public static Label Etiqueta(string texto, Font fuente, Color color, int margenInferior = 8) => new()
    {
        Text = texto,
        Font = fuente,
        ForeColor = color,
        AutoSize = true,
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 0, 0, margenInferior),
        UseMnemonic = false
    };

    public static Button Boton(string texto, bool principal = false)
    {
        var boton = new Button
        {
            Text = texto,
            AutoSize = true,
            MinimumSize = new Size(120, 38),
            Margin = new Padding(0),
            Padding = new Padding(12, 3, 12, 3),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Font = Codigo
        };
        EstiloBoton(boton, principal);
        return boton;
    }

    public static void EstiloBoton(Button boton, bool principal)
    {
        boton.BackColor = principal ? Acento : Panel;
        boton.ForeColor = principal ? Fondo : Texto;
        boton.FlatAppearance.BorderColor = principal ? Acento : Borde;
        boton.FlatAppearance.BorderSize = 2;
    }
}

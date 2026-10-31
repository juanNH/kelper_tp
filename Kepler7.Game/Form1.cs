using Kepler7.Game.Secciones.Integrante1;

namespace Kepler7.Game;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        Text = "Kepler-7";
        ClientSize = new Size(1000, 700);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(new Integrante1Panel { Dock = DockStyle.Fill });
    }
}

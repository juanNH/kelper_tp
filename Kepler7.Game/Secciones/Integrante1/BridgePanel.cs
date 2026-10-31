using Kepler7.Core.Patrones.Bridge;
using Kepler7.Core.Modelos;

namespace Kepler7.Game.Secciones.Integrante1;

public class BridgePanel : UserControl
{
    private readonly ComboBox tipoMensaje = new ComboBox();
    private readonly ComboBox tipoCifrado = new ComboBox();
    private readonly TextBox textoMensaje = new TextBox();
    private readonly TextBox resultado = new TextBox();
    private readonly CifradoAES cifradoAES = new CifradoAES();
    private readonly Mision mision;

    public BridgePanel(Mision mision)
    {
        this.mision = mision;
        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);

        panel.Controls.Add(new Label { Text = "Paso necesario: enviar una solicitud de ayuda a la Tierra", AutoSize = true });
        panel.Controls.Add(new Label { Text = "Una combinación alcanza. Elegí texto, alerta o emergencia y el cifrado que quieras.", AutoSize = true });
        FlowLayoutPanel opciones = new FlowLayoutPanel { AutoSize = true };
        opciones.Controls.Add(new Label { Text = "Mensaje:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        tipoMensaje.DropDownStyle = ComboBoxStyle.DropDownList;
        tipoMensaje.Items.AddRange(new object[] { "Texto", "Alerta", "Emergencia" });
        tipoMensaje.SelectedIndex = 0;
        opciones.Controls.Add(tipoMensaje);
        opciones.Controls.Add(new Label { Text = "Cifrado:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        tipoCifrado.DropDownStyle = ComboBoxStyle.DropDownList;
        tipoCifrado.Width = 180;
        tipoCifrado.Items.AddRange(new object[] { "Sin cifrado", "AES", "Cuántico (simulado)" });
        tipoCifrado.SelectedIndex = 0;
        opciones.Controls.Add(tipoCifrado);
        panel.Controls.Add(opciones);

        panel.Controls.Add(new Label { Text = "Escribí el mensaje:", AutoSize = true });
        textoMensaje.Width = 600;
        textoMensaje.Text = "Kepler-7 necesita asistencia.";
        panel.Controls.Add(textoMensaje);

        Button enviar = new Button { Text = "Enviar mensaje", AutoSize = true };
        enviar.Click += EnviarMensaje;
        panel.Controls.Add(enviar);

        resultado.Multiline = true;
        resultado.ReadOnly = true;
        resultado.Width = 600;
        resultado.Height = 150;
        resultado.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(resultado);
        Controls.Add(panel);
    }

    private void EnviarMensaje(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textoMensaje.Text))
        {
            MessageBox.Show("Escribí un mensaje antes de enviarlo.");
            return;
        }

        ICifrado cifrado;
        if (tipoCifrado.SelectedIndex == 1)
            cifrado = cifradoAES;
        else if (tipoCifrado.SelectedIndex == 2)
            cifrado = new CifradoCuantico();
        else
            cifrado = new SinCifrado();

        Mensaje mensaje;
        if (tipoMensaje.SelectedIndex == 1)
            mensaje = new MensajeAlerta(cifrado);
        else if (tipoMensaje.SelectedIndex == 2)
            mensaje = new MensajeEmergencia(cifrado);
        else
            mensaje = new MensajeTexto(cifrado);

        string textoEnviado;
        try
        {
            textoEnviado = mision.EnviarMensaje(mensaje, textoMensaje.Text);
        }
        catch (InvalidOperationException error)
        {
            MessageBox.Show(error.Message);
            return;
        }
        resultado.Text = "Mensaje original: " + textoMensaje.Text + "\r\n" +
            "Tipo: " + tipoMensaje.Text + " | Cifrado: " + tipoCifrado.Text + "\r\n\r\n" +
            "Resultado: " + textoEnviado;
    }
}

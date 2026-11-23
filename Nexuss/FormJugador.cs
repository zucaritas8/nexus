using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nexuss
{
    public enum ModoJugador { Nuevo, Editar, Reiniciar }

    /// <summary>Ventana pequeña que sirve para crear, editar o reiniciar un jugador.</summary>
    public class FormJugador : Form
    {
        private readonly ModoJugador modo;
        private readonly TextBox txtNombre = new TextBox { MaxLength = 50, Width = 200 };
        private readonly NumericUpDown numEdad = new NumericUpDown { Minimum = 18, Maximum = 50, Value = 18, Width = 80 };
        private readonly NumericUpDown numEnergia = new NumericUpDown { Minimum = 20, Maximum = 100, Value = 100, Width = 80 };
        private readonly NumericUpDown numConexion = new NumericUpDown { Minimum = 20, Maximum = 100, Value = 100, Width = 80 };
        private readonly ComboBox cmbRealidad = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };

        public string Nombre { get { return txtNombre.Text.Trim(); } }
        public int Edad { get { return (int)numEdad.Value; } }
        public int Energia { get { return (int)numEnergia.Value; } }
        public int Conexion { get { return (int)numConexion.Value; } }

        // El orden de la lista coincide con el orden en que NexusDB.sql insertó las realidades (Id 1..4).
        public int IdRealidad { get { return cmbRealidad.SelectedIndex + 1; } }

        public FormJugador(ModoJugador modo, JugadorResumen existente = null)
        {
            this.modo = modo;

            Text = modo == ModoJugador.Nuevo ? "Nueva partida"
                 : modo == ModoJugador.Editar ? "Editar jugador"
                 : "Reiniciar partida";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            cmbRealidad.Items.AddRange(new object[] { "NEXUS-07", "ECHO-13", "AURORA-21", "VOID-42" });
            cmbRealidad.SelectedIndex = 0;

            if (existente != null)
            {
                txtNombre.Text = existente.Nombre;
                numEdad.Value = Math.Max(18, Math.Min(50, existente.Edad));
            }

            int y = 20;
            if (modo != ModoJugador.Reiniciar)
            {
                Fila("Nombre", txtNombre, ref y);
                Fila("Edad", numEdad, ref y);
            }
            if (modo != ModoJugador.Editar)
            {
                Fila("Energía inicial", numEnergia, ref y);
                Fila("Conexión inicial", numConexion, ref y);
                Fila("Realidad", cmbRealidad, ref y);
            }

            var aceptar = new Button { Text = "Aceptar", Left = 70, Top = y + 10, Width = 100 };
            var cancelar = new Button { Text = "Cancelar", Left = 190, Top = y + 10, Width = 100, DialogResult = DialogResult.Cancel };
            aceptar.Click += Aceptar_Click;
            Controls.Add(aceptar);
            Controls.Add(cancelar);
            AcceptButton = aceptar;
            CancelButton = cancelar;

            ClientSize = new Size(360, y + 60);
        }

        private void Fila(string etiqueta, Control control, ref int y)
        {
            Controls.Add(new Label { Text = etiqueta, Left = 20, Top = y + 4, Width = 110 });
            control.Left = 140;
            control.Top = y;
            Controls.Add(control);
            y += 36;
        }

        private void Aceptar_Click(object sender, EventArgs e)
        {
            if (modo != ModoJugador.Reiniciar && Nombre.Length == 0)
            {
                MessageBox.Show("El nombre no puede estar vacío.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

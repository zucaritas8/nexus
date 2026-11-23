using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Nexuss
{
    /// <summary>Lista simple de jugadores: Seleccionar / Agregar jugador / Editar / Eliminar.</summary>
    public class ListaJugadores : Form
    {
        private readonly ListBox lista = new ListBox();
        private readonly List<JugadorResumen> jugadores = new List<JugadorResumen>();

        public ListaJugadores()
        {
            Text = "Jugadores";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(440, 412);
            BackColor = Color.FromArgb(14, 18, 38);
            KeyPreview = true;

            Controls.Add(new Label
            {
                Text = "Jugadores",
                ForeColor = Color.FromArgb(255, 226, 124),
                Font = new Font("Georgia", 16F, FontStyle.Bold),
                AutoSize = true,
                Left = 20,
                Top = 14
            });

            lista.Left = 20;
            lista.Top = 56;
            lista.Width = 400;
            lista.Height = 240;
            lista.Font = new Font("Segoe UI", 12F);
            lista.BackColor = Color.FromArgb(24, 28, 52);
            lista.ForeColor = Color.White;
            lista.BorderStyle = BorderStyle.FixedSingle;
            lista.IntegralHeight = false;
            lista.DoubleClick += delegate { Elegir(); };
            Controls.Add(lista);

            Boton("Seleccionar", 20, 312, 195, delegate { Elegir(); });
            Boton("Agregar jugador", 225, 312, 195, delegate { Agregar(); });
            Boton("Editar", 20, 358, 125, delegate { Editar(); });
            Boton("Eliminar", 157, 358, 125, delegate { Eliminar(); });
            Boton("Cerrar", 295, 358, 125, delegate { Close(); });

            KeyDown += delegate (object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) Close();
            };

            Load += delegate { CargarLista(Sesion.IdJugador); };
        }

        private void Boton(string texto, int x, int y, int ancho, EventHandler clic)
        {
            var b = new Button
            {
                Text = texto,
                Left = x,
                Top = y,
                Width = ancho,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 24, 45),
                ForeColor = Color.White,
                Font = new Font("Georgia", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(196, 156, 70);
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 46, 80);
            b.Click += clic;
            Controls.Add(b);
        }

        // ------------------------------------------------------------------
        //  Utilidades compartidas con el menú
        // ------------------------------------------------------------------
        public static void Seleccionar(JugadorResumen j)
        {
            Sesion.IdJugador = j.IdJugador;
            Sesion.Nombre = j.Nombre;
        }

        /// <summary>Abre el formulario de registro. Devuelve el jugador creado o null.</summary>
        public static JugadorResumen Registrar(IWin32Window owner)
        {
            using (var f = new FormJugador(ModoJugador.Nuevo))
            {
                if (f.ShowDialog(owner) != DialogResult.OK) return null;

                try
                {
                    int id = BaseDatos.Crear(f.Nombre, f.Edad, f.Energia, f.Conexion, f.IdRealidad);
                    return BaseDatos.Obtener(id);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(owner, ex.Message, "No se pudo crear el jugador",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
            }
        }

        // ------------------------------------------------------------------
        //  Lista
        // ------------------------------------------------------------------
        private void CargarLista(int idSeleccionar)
        {
            try
            {
                jugadores.Clear();
                lista.Items.Clear();

                int indice = 0;
                foreach (JugadorResumen j in BaseDatos.Listar())
                {
                    if (j.IdJugador == idSeleccionar) indice = jugadores.Count;
                    jugadores.Add(j);
                    string marca = (!string.IsNullOrEmpty(Sesion.Nombre) && j.IdJugador == Sesion.IdJugador) ? "▶ " : "    ";
                    lista.Items.Add(marca + j.Nombre + "   ·   Nivel " + j.NivelCadete);
                }

                if (lista.Items.Count > 0)
                    lista.SelectedIndex = indice;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo leer la base de datos NexusDB.\n\n" + ex.Message,
                                "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private JugadorResumen Elegido()
        {
            if (lista.SelectedIndex < 0 || lista.SelectedIndex >= jugadores.Count)
            {
                MessageBox.Show(this, "Selecciona un jugador de la lista.", "Nexus",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return jugadores[lista.SelectedIndex];
        }

        // ------------------------------------------------------------------
        //  Botones
        // ------------------------------------------------------------------
        private void Elegir()
        {
            JugadorResumen j = Elegido();
            if (j == null) return;

            Seleccionar(j);
            DialogResult = DialogResult.OK;
        }

        private void Agregar()
        {
            JugadorResumen nuevo = Registrar(this);
            if (nuevo == null) return;

            Seleccionar(nuevo);
            DialogResult = DialogResult.OK;
        }

        private void Editar()
        {
            JugadorResumen j = Elegido();
            if (j == null) return;

            using (var f = new FormJugador(ModoJugador.Editar, j))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    BaseDatos.Actualizar(j.IdJugador, f.Nombre, f.Edad);
                    if (!string.IsNullOrEmpty(Sesion.Nombre) && j.IdJugador == Sesion.IdJugador)
                        Sesion.Nombre = f.Nombre;
                    CargarLista(j.IdJugador);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(this, ex.Message, "No se pudo actualizar",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Eliminar()
        {
            JugadorResumen j = Elegido();
            if (j == null) return;

            var r = MessageBox.Show(this,
                "¿Eliminar a " + j.Nombre + " y todo su progreso?\nEsta acción no se puede deshacer.",
                "Eliminar jugador", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;

            try
            {
                BaseDatos.Eliminar(j.IdJugador);

                if (!string.IsNullOrEmpty(Sesion.Nombre) && j.IdJugador == Sesion.IdJugador)
                {
                    Sesion.IdJugador = 0;
                    Sesion.Nombre = null;
                }

                CargarLista(0);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(this, ex.Message, "No se pudo eliminar",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

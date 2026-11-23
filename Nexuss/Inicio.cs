using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Nexuss
{
    // Menú principal con el diseño de imágenes + jugadores guardados en NexusDB.
    // IMPORTANTE: este debe ser el ÚNICO archivo que define "partial class Inicio"
    // (borra la versión antigua con ListView).
    public partial class Inicio : Form
    {
        private PictureBox picEmblema;
        private PictureBox picCaballero;

        private PictureBox picNuevaPartida;
        private PictureBox picContinuar;
        private PictureBox picBestiario;
        private PictureBox picInventario;
        private PictureBox picSalir;

        private PictureBox[] picLuces;

        // Botón del jugador (arriba a la izquierda)
        private Button btnUsuario;
        private bool hayJugadores;

        private Timer animacionCapa;
        private Timer animacionLuces;

        private Bitmap[] framesCaballero;
        private Bitmap[] framesLuces;

        private Bitmap[] framesNuevaPartida;
        private Bitmap[] framesContinuar;
        private Bitmap[] framesBestiario;
        private Bitmap[] framesInventario;
        private Bitmap[] framesSalir;

        private int frameCaballero = 0;
        private int[] framesLucesActuales = { 0, 1, 2, 3 };

        public Inicio()
        {
            InitializeComponent();

            ConfigurarFormulario();
            CargarElementos();
            IniciarAnimaciones();

            Resize += (sender, e) =>
            {
                PosicionarElementos();
            };

            // Al abrir el menú se lee la base de datos y se muestra el jugador actual
            Shown += (sender, e) =>
            {
                RefrescarUsuario(true);
            };
        }

        private void ConfigurarFormulario()
        {
            Text = "NEXUSS";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(9, 12, 27);
            DoubleBuffered = true;

            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
        }

        private Bitmap[] ExtraerFrames(Bitmap imagen, int cantidad)
        {
            int ancho = imagen.Width / cantidad;
            int alto = imagen.Height;

            Bitmap[] frames = new Bitmap[cantidad];

            for (int i = 0; i < cantidad; i++)
            {
                frames[i] = imagen.Clone(
                    new Rectangle(i * ancho, 0, ancho, alto),
                    PixelFormat.Format32bppArgb
                );
            }

            return frames;
        }

        private PictureBox CrearPictureBox(Bitmap imagen, Size tamaño)
        {
            PictureBox picture = new PictureBox
            {
                Size = tamaño,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Image = imagen
            };

            return picture;
        }

        private void CargarElementos()
        {
            CargarLuces();
            CargarCaballero();
            CargarEmblema();
            CargarBotones();
            CargarUsuario();

            PosicionarElementos();
        }

        private void CargarCaballero()
        {
            Bitmap imagen = new Bitmap(Properties.Resources.caballero);

            framesCaballero = ExtraerFrames(imagen, 2);

            imagen.Dispose();

            picCaballero = CrearPictureBox(
                framesCaballero[0],
                new Size(250, 333)
            );

            Controls.Add(picCaballero);
        }

        private void CargarEmblema()
        {
            picEmblema = new PictureBox
            {
                Size = new Size(600, 230),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Properties.Resources.Emblema
            };

            Controls.Add(picEmblema);
        }

        private void CargarBotones()
        {
            framesNuevaPartida = ExtraerFrames(Properties.Resources.botones, 2);
            framesContinuar = ExtraerFrames(Properties.Resources.botones, 2);
            framesBestiario = ExtraerFrames(Properties.Resources.botones, 2);
            framesInventario = ExtraerFrames(Properties.Resources.botones, 2);
            framesSalir = ExtraerFrames(Properties.Resources.botones, 2);

            picNuevaPartida = CrearPictureBox(framesNuevaPartida[0], new Size(300, 150));
            picContinuar = CrearPictureBox(framesContinuar[0], new Size(300, 150));
            picBestiario = CrearPictureBox(framesBestiario[0], new Size(300, 150));
            picInventario = CrearPictureBox(framesInventario[0], new Size(300, 150));
            picSalir = CrearPictureBox(framesSalir[0], new Size(300, 150));

            Controls.Add(picNuevaPartida);
            Controls.Add(picContinuar);
            Controls.Add(picBestiario);
            Controls.Add(picInventario);
            Controls.Add(picSalir);

            ConfigurarBoton(picNuevaPartida, framesNuevaPartida, NuevaPartida, "NUEVA PARTIDA");
            ConfigurarBoton(picContinuar, framesContinuar, ContinuarPartida, "CONTINUAR");
            ConfigurarBoton(picBestiario, framesBestiario, AbrirBestiario, "BESTIARIO");
            ConfigurarBoton(picInventario, framesInventario, AbrirInventario, "INVENTARIO");
            ConfigurarBoton(picSalir, framesSalir, Salir, "SALIR");
        }

        private void CargarLuces()
        {
            Bitmap imagen = new Bitmap(Properties.Resources.orbes);

            framesLuces = ExtraerFrames(imagen, 4);

            imagen.Dispose();

            picLuces = new PictureBox[4];

            Size[] tamaños =
            {
                new Size(95, 125),
                new Size(75, 100),
                new Size(110, 145),
                new Size(85, 115)
            };

            for (int i = 0; i < 4; i++)
            {
                picLuces[i] = CrearPictureBox(framesLuces[i], tamaños[i]);
                Controls.Add(picLuces[i]);
            }
        }

        // ------------------------------------------------------------------
        //  Botón del jugador (arriba a la izquierda)
        // ------------------------------------------------------------------
        private void CargarUsuario()
        {
            btnUsuario = new Button
            {
                Size = new Size(270, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 24, 45),
                ForeColor = Color.White,
                Font = new Font("Georgia", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                AutoEllipsis = true,
                Text = "Registrarse"
            };

            btnUsuario.FlatAppearance.BorderColor = Color.FromArgb(196, 156, 70);
            btnUsuario.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 46, 80);

            btnUsuario.Click += (sender, e) => ClicUsuario();

            Controls.Add(btnUsuario);
        }

        // Lee los jugadores de la base de datos y actualiza el texto del botón.
        // Si el jugador actual ya no existe, toma el más reciente (o ninguno).
        // Devuelve false si no se pudo leer la base de datos.
        private bool RefrescarUsuario(bool avisarError)
        {
            try
            {
                hayJugadores = false;
                bool actualValido = false;
                JugadorResumen primero = null;     // la lista viene ordenada por último guardado

                foreach (JugadorResumen j in BaseDatos.Listar())
                {
                    hayJugadores = true;
                    if (primero == null) primero = j;

                    if (!string.IsNullOrEmpty(Sesion.Nombre) && j.IdJugador == Sesion.IdJugador)
                    {
                        actualValido = true;
                        Sesion.Nombre = j.Nombre;
                    }
                }

                if (!actualValido)
                {
                    if (primero != null)
                    {
                        Sesion.IdJugador = primero.IdJugador;
                        Sesion.Nombre = primero.Nombre;
                    }
                    else
                    {
                        Sesion.IdJugador = 0;
                        Sesion.Nombre = null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (avisarError)
                {
                    MessageBox.Show("No se pudo conectar con la base de datos NexusDB.\n\n" + ex.Message,
                                    "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            if (!string.IsNullOrEmpty(Sesion.Nombre))
                btnUsuario.Text = "👤 " + Sesion.Nombre + " ▾";
            else
                btnUsuario.Text = hayJugadores ? "Elegir jugador ▾" : "Registrarse";

            return true;
        }

        // Sin jugadores: abre el registro. Con jugadores: abre la lista simple.
        private void ClicUsuario()
        {
            if (!RefrescarUsuario(true))
                return;

            if (!hayJugadores)
            {
                JugadorResumen nuevo = ListaJugadores.Registrar(this);
                if (nuevo != null)
                    ListaJugadores.Seleccionar(nuevo);
            }
            else
            {
                using (ListaJugadores lista = new ListaJugadores())
                {
                    lista.ShowDialog(this);
                }
            }

            RefrescarUsuario(false);
        }

        private JugadorResumen JugadorActual()
        {
            if (string.IsNullOrEmpty(Sesion.Nombre))
                return null;

            try
            {
                return BaseDatos.Obtener(Sesion.IdJugador);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo leer el jugador.\n\n" + ex.Message,
                                "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Devuelve el jugador actual; si no hay, pide registrarse o elegir uno.
        private JugadorResumen AsegurarJugador()
        {
            JugadorResumen j = JugadorActual();
            if (j != null)
                return j;

            MessageBox.Show("Primero regístrate o elige un jugador.", "Nexus",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClicUsuario();
            return JugadorActual();
        }

        private void ConfigurarBoton(
            PictureBox boton,
            Bitmap[] frames,
            Action accion,
            string texto
        )
        {
            boton.Cursor = Cursors.Hand;

            boton.Paint += (sender, e) =>
            {
                using (Font fuente = new Font("Georgia", 16, FontStyle.Bold))
                using (Brush pincel = new SolidBrush(Color.White))
                using (StringFormat formato = new StringFormat())
                {
                    formato.Alignment = StringAlignment.Center;
                    formato.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString(texto, fuente, pincel, boton.ClientRectangle, formato);
                }
            };

            boton.MouseEnter += (sender, e) => { boton.Image = frames[1]; };
            boton.MouseLeave += (sender, e) => { boton.Image = frames[0]; };
            boton.Click += (sender, e) => { accion(); };
        }

        private void PosicionarElementos()
        {
            if (picCaballero == null)
                return;

            int ancho = ClientSize.Width;
            int alto = ClientSize.Height;

            // EMBLEMA
            picEmblema.Location = new Point((ancho - picEmblema.Width) / 2, 20);

            // BOTONES: se ajustan para que entren los cinco.
            PictureBox[] botones =
            {
                picNuevaPartida,
                picContinuar,
                picBestiario,
                picInventario,
                picSalir
            };

            int espacio = 8;

            int altoBoton = Math.Min(
                120,
                (alto - 60 - espacio * (botones.Length - 1)) / botones.Length
            );

            int anchoBoton = altoBoton * 2;

            int altoTotal = botones.Length * altoBoton + (botones.Length - 1) * espacio;

            int inicioY = (alto - altoTotal) / 2;
            int posicionX = (int)(ancho * 0.07);

            for (int i = 0; i < botones.Length; i++)
            {
                botones[i].Size = new Size(anchoBoton, altoBoton);
                botones[i].Location = new Point(posicionX, inicioY + i * (altoBoton + espacio));
            }

            // CABALLERO
            picCaballero.Location = new Point(
                (int)(ancho * 0.57) - picCaballero.Width / 2,
                alto - picCaballero.Height - 10
            );

            // LUCES MÁGICAS
            picLuces[0].Location = new Point((int)(ancho * 0.85), (int)(alto * 0.46));
            picLuces[1].Location = new Point((int)(ancho * 0.72), (int)(alto * 0.48));
            picLuces[2].Location = new Point((int)(ancho * 0.86), (int)(alto * 0.70));
            picLuces[3].Location = new Point((int)(ancho * 0.66), (int)(alto * 0.52));

            // JUGADOR (esquina superior izquierda)
            btnUsuario.Location = new Point(20, 20);

            // ORDEN VISUAL
            foreach (PictureBox luz in picLuces)
                luz.SendToBack();

            picCaballero.BringToFront();
            picEmblema.BringToFront();

            picNuevaPartida.BringToFront();
            picContinuar.BringToFront();
            picBestiario.BringToFront();
            picInventario.BringToFront();
            picSalir.BringToFront();

            btnUsuario.BringToFront();
        }

        private void IniciarAnimaciones()
        {
            // ANIMACIÓN DEL CABALLERO
            animacionCapa = new Timer { Interval = 350 };

            animacionCapa.Tick += (sender, e) =>
            {
                frameCaballero = (frameCaballero + 1) % framesCaballero.Length;
                picCaballero.Image = framesCaballero[frameCaballero];
            };

            animacionCapa.Start();

            // ANIMACIÓN DE LAS LUCES
            animacionLuces = new Timer { Interval = 140 };

            animacionLuces.Tick += (sender, e) =>
            {
                for (int i = 0; i < picLuces.Length; i++)
                {
                    framesLucesActuales[i] = (framesLucesActuales[i] + 1) % framesLuces.Length;
                    picLuces[i].Image = framesLuces[framesLucesActuales[i]];
                }
            };

            animacionLuces.Start();
        }

        // ------------------------------------------------------------------
        //  Entrar al juego
        // ------------------------------------------------------------------
        // Abre el juego con el nombre del jugador y oculta el menú.
        // Al cerrar el juego, el menú vuelve a aparecer.
        private void AbrirJuego(JugadorResumen j)
        {
            if (j == null)
                return;

            Sesion.IdJugador = j.IdJugador;
            Sesion.Nombre = j.Nombre;

            Juego juego = new Juego { NombreJugador = j.Nombre };

            juego.FormClosed += (sender, e) =>
            {
                Show();
                RefrescarUsuario(false);
            };

            Hide();
            juego.Show();
        }

        // ------------------------------------------------------------------
        //  ACCIONES DE LOS BOTONES
        // ------------------------------------------------------------------
        private void NuevaPartida()
        {
            JugadorResumen j = AsegurarJugador();
            if (j == null)
                return;

            bool conProgreso = j.NivelCadete > 1 || j.Experiencia > 0 ||
                               j.CriaturasDescubiertas > 0 || j.Anomalias > 0;

            if (conProgreso)
            {
                DialogResult respuesta = MessageBox.Show(
                    "Se borrará TODO el progreso de " + j.Nombre + " (nivel, criaturas, anomalías...).\n\n¿Empezar de cero?",
                    "Nueva partida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (respuesta != DialogResult.Yes)
                    return;
            }

            try
            {
                // energía 100, conexión 100, realidad 1 (NEXUS-07, boscosa)
                BaseDatos.Reiniciar(j.IdJugador, 100, 100, 1);
                AbrirJuego(BaseDatos.Obtener(j.IdJugador));
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo iniciar la partida",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ContinuarPartida()
        {
            JugadorResumen j = AsegurarJugador();
            if (j == null)
                return;

            AbrirJuego(j);
        }

        private void AbrirBestiario()
        {
            using (FormBestiario bestiario = new FormBestiario())
            {
                bestiario.ShowDialog(this);
            }
        }

        private void AbrirInventario()
        {
            MessageBox.Show("El inventario estará disponible próximamente.", "Inventario",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Salir()
        {
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animacionCapa?.Stop();
            animacionLuces?.Stop();

            animacionCapa?.Dispose();
            animacionLuces?.Dispose();

            LiberarFrames(framesCaballero);
            LiberarFrames(framesLuces);

            LiberarFrames(framesNuevaPartida);
            LiberarFrames(framesContinuar);
            LiberarFrames(framesBestiario);
            LiberarFrames(framesInventario);
            LiberarFrames(framesSalir);

            base.OnFormClosing(e);
        }

        private void LiberarFrames(Bitmap[] frames)
        {
            if (frames == null)
                return;

            foreach (Bitmap frame in frames)
            {
                frame?.Dispose();
            }
        }
    }
}

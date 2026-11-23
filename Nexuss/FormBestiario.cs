using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Nexuss
{
    /// <summary>Bestiario: una tarjeta por cada animal que aparece en el juego (imagen rectangular + nombre).</summary>
    public class FormBestiario : Form
    {
        const int AnchoTarjeta = 300, AltoTarjeta = 340, Separacion = 24;
        const int AnchoImg = 276, AltoImg = 170;

        private readonly List<Panel> tarjetas = new List<Panel>();
        private readonly List<Bitmap> imagenes = new List<Bitmap>();
        private readonly Label titulo = new Label();
        private readonly Label subtitulo = new Label();
        private readonly Button btnCerrar = new Button();

        public FormBestiario()
        {
            Text = "Bestiario";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.FromArgb(9, 12, 27);
            KeyPreview = true;
            DoubleBuffered = true;
            ShowInTaskbar = false;

            titulo.Text = "B E S T I A R I O";
            titulo.ForeColor = Color.FromArgb(255, 226, 124);
            titulo.Font = new Font("Georgia", 28F, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.BackColor = Color.Transparent;
            Controls.Add(titulo);

            subtitulo.Text = "Criaturas que habitan el bosque encantado";
            subtitulo.ForeColor = Color.FromArgb(190, 196, 230);
            subtitulo.Font = new Font("Georgia", 12F, FontStyle.Italic);
            subtitulo.AutoSize = true;
            subtitulo.BackColor = Color.Transparent;
            Controls.Add(subtitulo);

            btnCerrar.Text = "Cerrar";
            btnCerrar.Size = new Size(200, 44);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.BackColor = Color.FromArgb(20, 24, 45);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Font = new Font("Georgia", 12F, FontStyle.Bold);
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.FlatAppearance.BorderColor = Color.FromArgb(196, 156, 70);
            btnCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 46, 80);
            btnCerrar.Click += delegate { Close(); };
            Controls.Add(btnCerrar);

            KeyDown += delegate (object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) Close();
            };

            Resize += delegate { Distribuir(); };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Bounds = Owner != null ? Owner.Bounds : Screen.PrimaryScreen.Bounds;

            try
            {
                // Mismos animales (y mismas imágenes) que aparecen en el juego
                AgregarFicha(Especie.Zorro, Properties.Resources.ZorroRojo, 1, 6,
                    "Pequeño mamífero inteligente y cauteloso. Si te acercas despacio puede acostumbrarse a ti.");
                AgregarFicha(Especie.Ciervo, Properties.Resources.Ciervo, 2, 3,
                    "Herbívoro tranquilo y muy tímido. Un movimiento brusco lo hará huir.");
                AgregarFicha(Especie.Lobo, Properties.Resources.LoboGris, 2, 3,
                    "Cazador de mirada ámbar. Si la domesticación falla, se lanzará a atacar.");
                AgregarFicha(Especie.Oso, Properties.Resources.OsoPardo, 1, 6,
                    "Gran depredador territorial. No se puede domesticar: aléjate y corre si te persigue.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las imágenes del bestiario: " + ex.Message, "Bestiario");
                Close();
                return;
            }

            Distribuir();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (Bitmap b in imagenes) b.Dispose();
            imagenes.Clear();
            base.OnFormClosed(e);
        }

        // ------------------------------------------------------------------
        //  Tarjetas
        // ------------------------------------------------------------------
        private void AgregarFicha(Especie esp, Bitmap hoja, int filas, int cols, string descripcion)
        {
            DatosEspecie d = Fauna.D[(int)esp];

            Bitmap img = CrearImagen(d, hoja, filas, cols);
            imagenes.Add(img);

            Color colorRiesgo = d.Riesgo == "BAJO" ? Color.FromArgb(120, 220, 130)
                              : d.Riesgo == "MEDIO" ? Color.FromArgb(255, 200, 90)
                              : Color.FromArgb(255, 100, 90);

            var card = new Panel
            {
                Size = new Size(AnchoTarjeta, AltoTarjeta),
                BackColor = Color.FromArgb(20, 24, 45)
            };
            card.Paint += delegate (object s, PaintEventArgs e)
            {
                using (var pen = new Pen(Color.FromArgb(196, 156, 70), 2f))
                    e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
            };

            card.Controls.Add(new PictureBox
            {
                Image = img,
                SizeMode = PictureBoxSizeMode.Normal,
                Left = 12,
                Top = 12,
                Width = AnchoImg,
                Height = AltoImg
            });

            card.Controls.Add(new Label
            {
                Text = d.Titulo,
                ForeColor = Color.White,
                Font = new Font("Georgia", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0,
                Top = 192,
                Width = AnchoTarjeta,
                Height = 32
            });

            card.Controls.Add(new Label
            {
                Text = "Riesgo: " + d.Riesgo + (d.Domesticable ? "   ·   Domesticable" : "   ·   No domesticable"),
                ForeColor = colorRiesgo,
                Font = new Font("Segoe UI Semibold", 9.5F),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0,
                Top = 226,
                Width = AnchoTarjeta,
                Height = 22
            });

            card.Controls.Add(new Label
            {
                Text = descripcion,
                ForeColor = Color.FromArgb(200, 205, 230),
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.TopCenter,
                Left = 16,
                Top = 254,
                Width = AnchoTarjeta - 32,
                Height = 74
            });

            tarjetas.Add(card);
            Controls.Add(card);
        }

        // Imagen rectangular: fondo con degradado + el animal en reposo, centrado y con sombra
        private static Bitmap CrearImagen(DatosEspecie d, Bitmap hoja, int filas, int cols)
        {
            HojaAnimal h = HojaAnimal.Cargar(hoja, filas, cols, 150f, false, false);
            try
            {
                Cuadro c = h.Der[d.Idle];
                var bmp = new Bitmap(AnchoImg, AltoImg, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    var r = new Rectangle(0, 0, AnchoImg, AltoImg);
                    using (var gr = new LinearGradientBrush(r, Color.FromArgb(52, 74, 124), Color.FromArgb(16, 20, 44), 90f))
                        g.FillRectangle(gr, r);

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    float k = Math.Min((AnchoImg - 30f) / c.Img.Width, (AltoImg - 36f) / c.Img.Height);
                    if (k > 1f) k = 1f;
                    float w = c.Img.Width * k, hh = c.Img.Height * k;
                    float x = (AnchoImg - w) / 2f, y = AltoImg - 16f - hh;

                    using (var sombra = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                        g.FillEllipse(sombra, AnchoImg / 2f - w * 0.45f, AltoImg - 24f, w * 0.9f, 14f);

                    g.DrawImage(c.Img, x, y, w, hh);

                    using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                        g.DrawRectangle(pen, 0, 0, AnchoImg - 1, AltoImg - 1);
                }
                return bmp;
            }
            finally
            {
                h.Liberar();
            }
        }

        private void Distribuir()
        {
            int w = ClientSize.Width, hgt = ClientSize.Height;

            titulo.Left = (w - titulo.Width) / 2;
            titulo.Top = 28;
            subtitulo.Left = (w - subtitulo.Width) / 2;
            subtitulo.Top = titulo.Bottom + 4;
            btnCerrar.Left = (w - btnCerrar.Width) / 2;
            btnCerrar.Top = hgt - btnCerrar.Height - 30;

            if (tarjetas.Count == 0) return;

            int porFila = Math.Max(1, (w - 80 + Separacion) / (AnchoTarjeta + Separacion));
            porFila = Math.Min(porFila, tarjetas.Count);
            int filas = (int)Math.Ceiling(tarjetas.Count / (double)porFila);

            int totalW = porFila * AnchoTarjeta + (porFila - 1) * Separacion;
            int totalH = filas * AltoTarjeta + (filas - 1) * Separacion;
            int x0 = (w - totalW) / 2;
            int y0 = Math.Max(subtitulo.Bottom + 24, (hgt - totalH) / 2 + 20);

            for (int i = 0; i < tarjetas.Count; i++)
            {
                int fila = i / porFila, col = i % porFila;
                tarjetas[i].Location = new Point(x0 + col * (AnchoTarjeta + Separacion),
                                                 y0 + fila * (AltoTarjeta + Separacion));
            }
        }
    }
}

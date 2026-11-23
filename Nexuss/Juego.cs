using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nexuss
{
    // ===================================================================================
    //  RUIDO: números pseudoaleatorios "de valor" (siempre iguales para las mismas coordenadas)
    // ===================================================================================
    static class Ruido
    {
        public static float Hash(int x, int y, int semilla)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393) + (uint)(y * 668265263) + (uint)(semilla * 362437);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        static float Suave(float t) { return t * t * (3f - 2f * t); }

        public static float Valor(float x, float y, int semilla)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            float fx = Suave(x - x0), fy = Suave(y - y0);
            float a = Hash(x0, y0, semilla), b = Hash(x0 + 1, y0, semilla);
            float c = Hash(x0, y0 + 1, semilla), d = Hash(x0 + 1, y0 + 1, semilla);
            float ab = a + (b - a) * fx, cd = c + (d - c) * fx;
            return ab + (cd - ab) * fy;
        }

        public static float Fbm(float x, float y, int semilla, int octavas)
        {
            float suma = 0f, amp = 0.5f, total = 0f;
            for (int i = 0; i < octavas; i++)
            {
                suma += Valor(x, y, semilla + i * 101) * amp;
                total += amp;
                x *= 2.03f; y *= 2.03f; amp *= 0.5f;
            }
            return suma / total;
        }
    }

    enum Terreno { Profundo, Somera, Arena, Pasto, Roca }
    enum TipoObj { Arbol, Pino, Roca, Arbusto }

    sealed class Objeto
    {
        public TipoObj Tipo;
        public int Variante;
        public float X, Y;      // base del objeto en el mundo
        public float Radio;     // radio de colisión
    }

    sealed class Parcela
    {
        public int Cx, Cy;
        public Bitmap Suelo;
        public List<Objeto> Objetos = new List<Objeto>();
    }

    // ===================================================================================
    //  MUNDO INFINITO: se genera por parcelas de 512x512 px solo donde hace falta
    // ===================================================================================
    sealed class Mundo
    {
        public const int TamParcela = 512;
        const int Bloque = 4;      // tamaño del "pixel" del terreno
        const int Celda = 128;     // rejilla para colocar árboles y rocas

        public const float UmbralProfundo = 0.32f;
        public const float UmbralSomera = 0.38f;
        public const float UmbralArena = 0.415f;
        public const float UmbralRoca = 0.70f;

        public readonly int Semilla;
        readonly Dictionary<long, Parcela> _parcelas = new Dictionary<long, Parcela>();
        readonly List<Objeto> _tmp = new List<Objeto>();
        static readonly float[] RadioRoca = { 15f, 24f, 34f, 22f };

        public Mundo(int semilla) { Semilla = semilla; }

        public float Elev(float x, float y) { return Ruido.Fbm(x / 1500f, y / 1500f, Semilla, 3); }
        public float Humedad(float x, float y) { return Ruido.Fbm(x / 1100f + 500f, y / 1100f - 300f, Semilla + 77, 2); }

        public static Terreno Clasificar(float e)
        {
            if (e < UmbralProfundo) return Terreno.Profundo;
            if (e < UmbralSomera) return Terreno.Somera;
            if (e < UmbralArena) return Terreno.Arena;
            if (e >= UmbralRoca) return Terreno.Roca;
            return Terreno.Pasto;
        }

        public Terreno TerrenoEn(float x, float y) { return Clasificar(Elev(x, y)); }

        static long Clave(int cx, int cy) { return ((long)cx << 32) | unchecked((uint)cy); }
        public bool Existe(int cx, int cy) { return _parcelas.ContainsKey(Clave(cx, cy)); }

        public Parcela Obtener(int cx, int cy)
        {
            long k = Clave(cx, cy);
            Parcela p;
            if (!_parcelas.TryGetValue(k, out p))
            {
                p = Generar(cx, cy);
                _parcelas[k] = p;
            }
            return p;
        }

        // Genera como máximo UNA parcela faltante (la más cercana) para no congelar el juego.
        public void Precargar(int pcx, int pcy, int radio)
        {
            for (int r = 0; r <= radio; r++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                        if (!Existe(pcx + dx, pcy + dy)) { Obtener(pcx + dx, pcy + dy); return; }
                    }
        }

        public void Limpiar(int pcx, int pcy, int radio)
        {
            List<long> borrar = null;
            foreach (KeyValuePair<long, Parcela> kv in _parcelas)
            {
                Parcela p = kv.Value;
                if (Math.Abs(p.Cx - pcx) > radio || Math.Abs(p.Cy - pcy) > radio)
                {
                    if (borrar == null) borrar = new List<long>();
                    borrar.Add(kv.Key);
                }
            }
            if (borrar == null) return;
            foreach (long k in borrar)
            {
                _parcelas[k].Suelo.Dispose();
                _parcelas.Remove(k);
            }
        }

        public void Liberar()
        {
            foreach (KeyValuePair<long, Parcela> kv in _parcelas) kv.Value.Suelo.Dispose();
            _parcelas.Clear();
        }

        // ---------------------------------------------------------------- suelo
        static int B(float v) { return v < 0f ? 0 : (v > 255f ? 255 : (int)v); }

        static int Pack(int r, int g, int b)
        {
            return unchecked((int)(0xFF000000u | (uint)((r << 16) | (g << 8) | b)));
        }

        int ColorSuelo(float x, float y)
        {
            float e = Elev(x, y);
            float d = Ruido.Fbm(x / 70f, y / 70f, Semilla + 900, 2);   // detalle -> bandas de sombreado
            int nivel = (int)(d * 4f);
            if (nivel > 3) nivel = 3;
            if (nivel < 0) nivel = 0;
            float f = 0.90f + nivel * 0.045f;
            int r, g, b;

            if (e < UmbralProfundo)
            {
                if (e < UmbralProfundo - 0.05f) { r = 22; g = 52; b = 100; }
                else { r = 30; g = 70; b = 124; }
            }
            else if (e < UmbralSomera)
            {
                if (e > UmbralSomera - 0.010f && d > 0.36f) { r = 214; g = 238; b = 242; f = 1f; }   // espuma
                else if (e > UmbralSomera - 0.03f) { r = 74; g = 150; b = 190; }
                else { r = 48; g = 112; b = 164; }
            }
            else if (e < UmbralArena)
            {
                if (e < UmbralSomera + 0.012f) { r = 176; g = 156; b = 110; }   // arena mojada
                else { r = 220; g = 198; b = 142; }
            }
            else if (e >= UmbralRoca) { r = 122; g = 122; b = 132; }
            else if (e > UmbralRoca - 0.03f) { r = 134; g = 110; b = 76; }        // grava
            else if (e < UmbralArena + 0.012f) { r = 150; g = 172; b = 92; }      // pasto seco junto a la arena
            else
            {
                float m = Humedad(x, y);
                if (m < 0.42f) { r = 128; g = 170; b = 80; }
                else if (m < 0.58f) { r = 88; g = 144; b = 64; }
                else { r = 58; g = 112; b = 58; }
            }
            return Pack(B(r * f), B(g * f), B(b * f));
        }

        Parcela Generar(int cx, int cy)
        {
            var p = new Parcela();
            p.Cx = cx; p.Cy = cy;
            int ox = cx * TamParcela, oy = cy * TamParcela;
            int n = TamParcela / Bloque;

            var bmp = new Bitmap(TamParcela, TamParcela, PixelFormat.Format32bppPArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, TamParcela, TamParcela),
                                         ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            int[] fila = new int[TamParcela];
            for (int by = 0; by < n; by++)
            {
                for (int bx = 0; bx < n; bx++)
                {
                    int c = ColorSuelo(ox + bx * Bloque + 2, oy + by * Bloque + 2);
                    for (int k = 0; k < Bloque; k++) fila[bx * Bloque + k] = c;
                }
                for (int r = 0; r < Bloque; r++)
                    Marshal.Copy(fila, 0, IntPtr.Add(bd.Scan0, (by * Bloque + r) * bd.Stride), TamParcela);
            }
            bmp.UnlockBits(bd);

            Decorar(bmp, p);
            p.Suelo = bmp;
            GenerarObjetos(p);
            return p;
        }

        // Hierba, flores, conchas, guijarros, nenúfares... pintados dentro de la parcela
        void Decorar(Bitmap bmp, Parcela p)
        {
            int ox = p.Cx * TamParcela, oy = p.Cy * TamParcela;
            const int cel = 32;
            int n = TamParcela / cel;
            Color[] petalos = { Color.FromArgb(255, 226, 96), Color.FromArgb(240, 120, 164),
                                Color.FromArgb(248, 248, 248), Color.FromArgb(164, 144, 244) };

            using (Graphics g = Graphics.FromImage(bmp))
            using (var bHO = new SolidBrush(Color.FromArgb(48, 92, 40)))
            using (var bHC = new SolidBrush(Color.FromArgb(150, 198, 96)))
            using (var bHM = new SolidBrush(Color.FromArgb(104, 160, 70)))
            using (var bAre = new SolidBrush(Color.FromArgb(176, 150, 102)))
            using (var bCon = new SolidBrush(Color.FromArgb(240, 226, 186)))
            using (var bPO = new SolidBrush(Color.FromArgb(70, 70, 82)))
            using (var bPC = new SolidBrush(Color.FromArgb(168, 168, 178)))
            using (var bNen = new SolidBrush(Color.FromArgb(58, 140, 74)))
            using (var bNenC = new SolidBrush(Color.FromArgb(120, 190, 100)))
            using (var bCen = new SolidBrush(Color.FromArgb(255, 210, 60)))
            using (var bPet = new SolidBrush(Color.White))
            {
                g.SmoothingMode = SmoothingMode.None;
                for (int j = 0; j < n; j++)
                    for (int i = 0; i < n; i++)
                    {
                        int gx = ox / cel + i, gy = oy / cel + j;
                        if (Ruido.Hash(gx, gy, Semilla + 31) > 0.62f) continue;
                        int px = i * cel + 4 + (int)(Ruido.Hash(gx, gy, Semilla + 32) * 16f);
                        int py = j * cel + 9 + (int)(Ruido.Hash(gx, gy, Semilla + 33) * 16f);
                        float wx = ox + px, wy = oy + py;
                        float e = Elev(wx, wy);
                        float k = Ruido.Hash(gx, gy, Semilla + 34);

                        switch (Clasificar(e))
                        {
                            case Terreno.Pasto:
                                if (e > UmbralRoca - 0.03f)
                                {
                                    g.FillRectangle(bPO, px, py, 7, 4);
                                    g.FillRectangle(bPC, px + 1, py, 4, 2);
                                }
                                else if (k < 0.08f && Humedad(wx, wy) < 0.62f)
                                {
                                    bPet.Color = petalos[(int)(k * 1000f) % 4];
                                    g.FillRectangle(bPet, px, py + 2, 6, 2);
                                    g.FillRectangle(bPet, px + 2, py, 2, 6);
                                    g.FillRectangle(bCen, px + 2, py + 2, 2, 2);
                                }
                                else
                                {
                                    g.FillRectangle(bHO, px, py, 8, 1);
                                    g.FillRectangle(bHO, px, py - 5, 2, 5);
                                    g.FillRectangle(bHC, px + 3, py - 8, 2, 8);
                                    g.FillRectangle(bHM, px + 6, py - 6, 2, 6);
                                }
                                break;
                            case Terreno.Arena:
                                if (k < 0.45f)
                                {
                                    g.FillRectangle(bAre, px, py, 4, 2);
                                    if (k < 0.1f) g.FillRectangle(bCon, px + 1, py - 2, 3, 2);
                                }
                                break;
                            case Terreno.Roca:
                                g.FillRectangle(bPO, px, py, 7, 4);
                                g.FillRectangle(bPC, px + 1, py, 4, 2);
                                break;
                            case Terreno.Somera:
                                if (k < 0.07f && e < UmbralSomera - 0.025f)
                                {
                                    g.FillEllipse(bNen, px, py, 14, 8);
                                    g.FillEllipse(bNenC, px + 3, py + 1, 6, 3);
                                }
                                break;
                        }
                    }
            }
        }

        // ---------------------------------------------------------------- objetos
        void GenerarObjetos(Parcela p)
        {
            int ox = p.Cx * TamParcela, oy = p.Cy * TamParcela;
            int n = TamParcela / Celda;
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    int gx = p.Cx * n + i, gy = p.Cy * n + j;
                    float r0 = Ruido.Hash(gx, gy, Semilla + 11);
                    float r1 = Ruido.Hash(gx, gy, Semilla + 14);
                    float px = ox + i * Celda + 26 + Ruido.Hash(gx, gy, Semilla + 12) * (Celda - 52);
                    float py = oy + j * Celda + 26 + Ruido.Hash(gx, gy, Semilla + 13) * (Celda - 52);
                    if (px * px + py * py < 260f * 260f) continue;       // zona despejada en el origen

                    float e = Elev(px, py);
                    Terreno t = Clasificar(e);
                    int v = (int)(r1 * 997f);
                    Objeto o = null;

                    if (t == Terreno.Pasto)
                    {
                        float m = Humedad(px, py);
                        float pArbol = m > 0.58f ? 0.60f : (m > 0.46f ? 0.20f : 0.05f);
                        if (r0 < pArbol)
                        {
                            bool pino = (m > 0.62f && r1 < 0.4f);
                            int variante = v % (pino ? 2 : 4);
                            // ARBOLES ROSA (cerezos): variantes 4 y 5, aparecen en ~2 de cada 7 árboles
                            if (!pino && v % 7 < 2) variante = 4 + (v / 7) % 2;
                            o = new Objeto { Tipo = pino ? TipoObj.Pino : TipoObj.Arbol, Variante = variante, Radio = pino ? 12f : 14f };
                        }
                        else if (r0 < pArbol + 0.08f)
                            o = new Objeto { Tipo = TipoObj.Arbusto, Variante = v % 2, Radio = 12f };
                        else if (r0 < pArbol + 0.11f)
                        {
                            int rv = v % 2;   // rocas pequeñas/medianas
                            o = new Objeto { Tipo = TipoObj.Roca, Variante = rv, Radio = RadioRoca[rv] };
                        }
                    }
                    else if (t == Terreno.Roca)
                    {
                        if (r0 < 0.40f)
                        {
                            int rv = v % 4;
                            o = new Objeto { Tipo = TipoObj.Roca, Variante = rv, Radio = RadioRoca[rv] };
                        }
                        else if (r0 < 0.48f)
                            o = new Objeto { Tipo = TipoObj.Pino, Variante = v % 2, Radio = 12f };
                    }
                    else if (t == Terreno.Arena && r0 < 0.05f)
                        o = new Objeto { Tipo = TipoObj.Roca, Variante = 0, Radio = RadioRoca[0] };

                    if (o == null) continue;
                    o.X = px; o.Y = py;
                    p.Objetos.Add(o);
                }
        }

        public void ObjetosEnRect(float x0, float y0, float x1, float y1, List<Objeto> salida)
        {
            int c0x = (int)Math.Floor(x0 / TamParcela), c1x = (int)Math.Floor(x1 / TamParcela);
            int c0y = (int)Math.Floor(y0 / TamParcela), c1y = (int)Math.Floor(y1 / TamParcela);
            for (int cy = c0y; cy <= c1y; cy++)
                for (int cx = c0x; cx <= c1x; cx++)
                {
                    Parcela p = Obtener(cx, cy);
                    for (int i = 0; i < p.Objetos.Count; i++)
                    {
                        Objeto o = p.Objetos[i];
                        if (o.X >= x0 && o.X <= x1 && o.Y >= y0 && o.Y <= y1) salida.Add(o);
                    }
                }
        }

        // ---------------------------------------------------------------- colisiones
        public bool Bloqueado(float x, float y, float radio)
        {
            if (Elev(x, y) < UmbralProfundo) return true;
            if (Elev(x + radio, y) < UmbralProfundo || Elev(x - radio, y) < UmbralProfundo) return true;
            if (Elev(x, y + radio * 0.6f) < UmbralProfundo || Elev(x, y - radio * 0.6f) < UmbralProfundo) return true;

            _tmp.Clear();
            ObjetosEnRect(x - radio - 45f, y - radio - 45f, x + radio + 45f, y + radio + 45f, _tmp);
            for (int i = 0; i < _tmp.Count; i++)
            {
                Objeto o = _tmp[i];
                float dx = o.X - x, dy = o.Y - y, rr = o.Radio + radio;
                if (dx * dx + dy * dy < rr * rr) return true;
            }
            return false;
        }

        bool EsLlano(float x, float y)
        {
            float e = Elev(x, y);
            return e > UmbralArena + 0.02f && e < UmbralRoca - 0.04f;
        }

        // Busca un punto de salida seguro (tierra firme, sin árboles encima)
        public PointF BuscarInicio()
        {
            for (int r = 0; r < 6000; r += 64)
            {
                int pasos = r == 0 ? 1 : Math.Max(8, r / 16);
                for (int k = 0; k < pasos; k++)
                {
                    double a = k * 2.0 * Math.PI / pasos;
                    float x = (float)(Math.Cos(a) * r), y = (float)(Math.Sin(a) * r);
                    if (EsLlano(x, y) && EsLlano(x + 70f, y) && EsLlano(x - 70f, y) &&
                        EsLlano(x, y + 70f) && EsLlano(x, y - 70f) && !Bloqueado(x, y, 60f))
                        return new PointF(x, y);
                }
            }
            return new PointF(0f, 0f);
        }
    }

    // ===================================================================================
    //  ARTE: árboles, pinos, rocas y arbustos dibujados por código (con contorno oscuro)
    // ===================================================================================
    sealed class Sprite
    {
        public Bitmap Img;
        public int Ax, Ay;                              // punto de apoyo en el suelo
        public float SombraAncho, SombraAlto, SombraDx; // sombra proyectada
    }

    static class Arte
    {
        public static Sprite[] Arboles, Pinos, Rocas, Arbustos;
        static readonly Color Cont = Color.FromArgb(20, 28, 22);

        static Color C(int r, int g, int b) { return Color.FromArgb(r, g, b); }
        static void Circ(Graphics g, Brush b, float cx, float cy, float r) { g.FillEllipse(b, cx - r, cy - r, 2 * r, 2 * r); }

        public static void Crear()
        {
            if (Arboles != null) return;
            Arboles = new Sprite[6]; Pinos = new Sprite[2]; Rocas = new Sprite[4]; Arbustos = new Sprite[2];
            for (int i = 0; i < 6; i++) Arboles[i] = CrearArbol(i);
            for (int i = 0; i < 2; i++) Pinos[i] = CrearPino(i);
            for (int i = 0; i < 4; i++) Rocas[i] = CrearRoca(i);
            for (int i = 0; i < 2; i++) Arbustos[i] = CrearArbusto(i);
        }

        public static void Liberar()
        {
            if (Arboles == null) return;
            foreach (Sprite[] arr in new[] { Arboles, Pinos, Rocas, Arbustos })
                foreach (Sprite s in arr) s.Img.Dispose();
            Arboles = Pinos = Rocas = Arbustos = null;
        }

        public static Sprite De(Objeto o)
        {
            switch (o.Tipo)
            {
                case TipoObj.Arbol: return Arboles[o.Variante];
                case TipoObj.Pino: return Pinos[o.Variante];
                case TipoObj.Roca: return Rocas[o.Variante];
                default: return Arbustos[o.Variante];
            }
        }

        static Sprite CrearArbol(int v)
        {
            const int W = 150, H = 190;
            int bx = W / 2, by = H - 14;
            var rnd = new Random(100 + v * 17);
            // variantes 0-3: verde, azulado, amarillento, naranja (otoño) · 4-5: ROSA (cerezo)
            Color[] osc = { C(30, 84, 44), C(26, 70, 50), C(70, 100, 30), C(140, 60, 28), C(176, 70, 120), C(150, 56, 112) };
            Color[] med = { C(52, 128, 60), C(40, 106, 70), C(110, 150, 44), C(196, 98, 36), C(232, 118, 168), C(214, 98, 172) };
            Color[] cla = { C(110, 178, 84), C(88, 156, 104), C(172, 200, 84), C(240, 160, 66), C(255, 190, 214), C(255, 214, 234) };
            float[,] cir = { { 0, -92, 42 }, { -30, -78, 30 }, { 31, -80, 31 }, { 4, -122, 34 },
                             { -24, -110, 28 }, { 27, -108, 28 }, { 0, -64, 28 } };
            int nC = cir.GetLength(0);

            var bmp = new Bitmap(W, H, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var penC = new Pen(Cont, 3f))
            {
                penC.LineJoin = LineJoin.Round;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var br = new SolidBrush(C(62, 42, 26))) g.FillEllipse(br, bx - 17, by - 7, 34, 14);
                Point[] tronco = { new Point(bx - 9, by), new Point(bx - 6, by - 78), new Point(bx + 6, by - 78), new Point(bx + 10, by) };
                using (var gr = new LinearGradientBrush(new Rectangle(bx - 10, by - 78, 21, 78), C(132, 94, 58), C(70, 46, 30), 0f))
                    g.FillPolygon(gr, tronco);
                g.DrawPolygon(penC, tronco);
                using (var pen = new Pen(Color.FromArgb(90, 40, 26, 16), 2f))
                {
                    g.DrawLine(pen, bx - 2, by - 10, bx - 3, by - 34);
                    g.DrawLine(pen, bx + 3, by - 26, bx + 2, by - 52);
                }

                float[] xs = new float[nC], ys = new float[nC];
                for (int i = 0; i < nC; i++)
                {
                    xs[i] = bx + cir[i, 0] + (float)(rnd.NextDouble() * 6 - 3);
                    ys[i] = by + cir[i, 1] + (float)(rnd.NextDouble() * 6 - 3);
                }
                using (var b = new SolidBrush(Cont)) for (int i = 0; i < nC; i++) Circ(g, b, xs[i], ys[i], cir[i, 2] + 3);
                using (var b = new SolidBrush(osc[v])) for (int i = 0; i < nC; i++) Circ(g, b, xs[i], ys[i], cir[i, 2]);
                using (var b = new SolidBrush(med[v])) for (int i = 0; i < nC; i++) Circ(g, b, xs[i] - 3, ys[i] - 5, cir[i, 2] - 5);
                using (var b = new SolidBrush(cla[v])) for (int i = 0; i < nC; i++) Circ(g, b, xs[i] - 9, ys[i] - 12, cir[i, 2] * 0.5f);

                using (var bl = new SolidBrush(cla[v]))
                using (var bd = new SolidBrush(osc[v]))
                    for (int i = 0; i < 36; i++)
                    {
                        double a = rnd.NextDouble() * Math.PI * 2, rr = rnd.NextDouble() * 56;
                        float x = bx + (float)(Math.Cos(a) * rr), y = by - 90 + (float)(Math.Sin(a) * rr * 0.9);
                        g.FillEllipse(i % 2 == 0 ? bl : bd, x, y, 6, 4);
                    }
            }
            return new Sprite { Img = bmp, Ax = bx, Ay = by, SombraAncho = 120, SombraAlto = 36, SombraDx = 30 };
        }

        static Sprite CrearPino(int v)
        {
            const int W = 124, H = 204;
            int bx = W / 2, by = H - 14;
            Color osc = v == 0 ? C(24, 66, 48) : C(40, 80, 52);
            Color med = v == 0 ? C(44, 112, 74) : C(76, 124, 66);
            Color cla = v == 0 ? C(96, 160, 112) : C(130, 172, 92);
            int[,] capas = { { 112, 176, 54 }, { 80, 144, 46 }, { 50, 108, 36 }, { 20, 72, 26 } };

            var bmp = new Bitmap(W, H, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var penC = new Pen(Cont, 3f))
            {
                penC.LineJoin = LineJoin.Round;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(C(86, 58, 36))) g.FillRectangle(br, bx - 6, by - 34, 12, 34);
                g.DrawRectangle(penC, bx - 6, by - 34, 12, 34);

                for (int k = 0; k < capas.GetLength(0); k++)
                {
                    float ap = capas[k, 0], ba = capas[k, 1], hw = capas[k, 2];
                    PointF[] poly = {
                        new PointF(bx, ap), new PointF(bx + hw, ba), new PointF(bx + hw * 0.55f, ba - 8),
                        new PointF(bx, ba - 1), new PointF(bx - hw * 0.55f, ba - 8), new PointF(bx - hw, ba) };
                    using (var gr = new LinearGradientBrush(new RectangleF(bx - hw, ap, 2 * hw, ba - ap), med, osc, 0f))
                        g.FillPolygon(gr, poly);
                    g.DrawPolygon(penC, poly);
                    PointF[] luz = { new PointF(bx, ap + 5), new PointF(bx - hw * 0.62f, ba - 12), new PointF(bx - hw * 0.25f, ba - 14) };
                    using (var bl = new SolidBrush(Color.FromArgb(120, cla))) g.FillPolygon(bl, luz);
                }
            }
            return new Sprite { Img = bmp, Ax = bx, Ay = by, SombraAncho = 96, SombraAlto = 28, SombraDx = 26 };
        }

        static Sprite CrearRoca(int v)
        {
            int[] ws = { 64, 96, 140, 80 }, hs = { 50, 74, 106, 96 };
            int W = ws[v], H = hs[v];
            var rnd = new Random(50 + v * 7);
            var bmp = new Bitmap(W, H, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var penC = new Pen(C(28, 28, 40), 3f))
            {
                penC.LineJoin = LineJoin.Round;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float cx = W / 2f, cy = H / 2f - 2f, rx = W / 2f - 5f, ry = H / 2f - 5f;
                int n = 10;
                PointF[] pts = new PointF[n];
                for (int i = 0; i < n; i++)
                {
                    double a = i * 2 * Math.PI / n;
                    float j = 0.82f + (float)rnd.NextDouble() * 0.18f;
                    pts[i] = new PointF(cx + (float)Math.Cos(a) * rx * j, cy + (float)Math.Sin(a) * ry * j);
                }
                using (var gr = new LinearGradientBrush(new Rectangle(0, 0, W, H), C(160, 162, 174), C(80, 82, 96), 90f))
                    g.FillPolygon(gr, pts);
                g.DrawPolygon(penC, pts);

                PointF[] luz = new PointF[n];
                for (int i = 0; i < n; i++)
                    luz[i] = new PointF(cx - rx * 0.20f + (pts[i].X - cx) * 0.55f, cy - ry * 0.28f + (pts[i].Y - cy) * 0.50f);
                using (var bl = new SolidBrush(Color.FromArgb(95, 236, 240, 250))) g.FillPolygon(bl, luz);

                using (var pen = new Pen(Color.FromArgb(110, 30, 30, 44), 2f))
                {
                    g.DrawLine(pen, cx + rx * 0.1f, cy - ry * 0.1f, cx + rx * 0.25f, cy + ry * 0.35f);
                    g.DrawLine(pen, cx - rx * 0.3f, cy + ry * 0.15f, cx - rx * 0.1f, cy + ry * 0.45f);
                }
                if (v == 1 || v == 2)
                    using (var bm = new SolidBrush(Color.FromArgb(210, 84, 130, 60)))
                        g.FillEllipse(bm, cx - rx * 0.35f, cy - ry * 0.95f, rx * 0.9f, ry * 0.38f);
            }
            return new Sprite { Img = bmp, Ax = W / 2, Ay = H - 6, SombraAncho = W * 1.1f, SombraAlto = H * 0.38f, SombraDx = W * 0.12f };
        }

        static Sprite CrearArbusto(int v)
        {
            const int W = 92, H = 76;
            float[,] cir = { { 26, 40, 20 }, { 46, 30, 25 }, { 66, 40, 20 }, { 46, 44, 22 } };
            Color osc = v == 0 ? C(34, 92, 46) : C(30, 78, 62);
            Color med = v == 0 ? C(60, 138, 64) : C(48, 118, 88);
            Color cla = v == 0 ? C(124, 190, 92) : C(104, 170, 120);
            var bmp = new Bitmap(W, H, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Cont)) for (int i = 0; i < 4; i++) Circ(g, b, cir[i, 0], cir[i, 1], cir[i, 2] + 3);
                using (var b = new SolidBrush(osc)) for (int i = 0; i < 4; i++) Circ(g, b, cir[i, 0], cir[i, 1], cir[i, 2]);
                using (var b = new SolidBrush(med)) for (int i = 0; i < 4; i++) Circ(g, b, cir[i, 0] - 2, cir[i, 1] - 4, cir[i, 2] - 5);
                using (var b = new SolidBrush(cla)) for (int i = 0; i < 4; i++) Circ(g, b, cir[i, 0] - 6, cir[i, 1] - 9, cir[i, 2] * 0.4f);
                if (v == 1)
                    using (var b = new SolidBrush(C(214, 52, 62)))
                        for (int i = 0; i < 6; i++) Circ(g, b, 22 + i * 9 + (i % 2) * 3, 34 + (i % 3) * 7, 3f);
            }
            return new Sprite { Img = bmp, Ax = W / 2, Ay = H - 8, SombraAncho = 76, SombraAlto = 22, SombraDx = 8 };
        }
    }

    // ===================================================================================
    //  HOJA DEL CABALLERO: analiza los píxeles de la imagen para recortar y alinear cada cuadro
    // ===================================================================================
    sealed class Cuadro
    {
        public Bitmap Img;
        public float AnclaX, AnclaY;   // dónde "pisa" el personaje dentro de la imagen recortada
    }

    sealed class HojaCaballero
    {
        public const int Filas = 4, Columnas = 3;   // filas: abajo, arriba, izquierda, derecha
        const int UmbralAlfa = 48;                  // píxeles con menos opacidad se consideran ruido/fondo
        const int Margen = 2;

        public Cuadro[,] Cuadros = new Cuadro[Filas, Columnas];

        struct Medida
        {
            public int MinX, MinY, MaxX, MaxY;
            public float CabezaX;
            public bool Valida;
        }

        // Lista de tramos continuos (con píxeles) de un perfil, uniendo huecos pequeños
        static List<int[]> Tramos(int[] perfil, int hueco, int minLargo)
        {
            var res = new List<int[]>();
            int ini = -1, ult = -1;
            for (int i = 0; i < perfil.Length; i++)
            {
                if (perfil[i] <= 0) continue;
                if (ini < 0) { ini = i; ult = i; }
                else if (i - ult - 1 > hueco) { res.Add(new[] { ini, ult }); ini = i; ult = i; }
                else ult = i;
            }
            if (ini >= 0) res.Add(new[] { ini, ult });
            res.RemoveAll(t => t[1] - t[0] + 1 < minLargo);
            return res;
        }

        static Medida Medir(bool[] masc, int w, Rectangle z)
        {
            var m = new Medida();
            m.MinX = int.MaxValue; m.MinY = int.MaxValue; m.MaxX = -1; m.MaxY = -1;
            for (int y = z.Top; y < z.Bottom; y++)
                for (int x = z.Left; x < z.Right; x++)
                    if (masc[y * w + x])
                    {
                        if (x < m.MinX) m.MinX = x;
                        if (x > m.MaxX) m.MaxX = x;
                        if (y < m.MinY) m.MinY = y;
                        if (y > m.MaxY) m.MaxY = y;
                    }
            if (m.MaxX < 0) return m;
            m.Valida = true;

            // Referencia horizontal = centro de la CABEZA (20 % superior del sprite): es lo más estable
            // entre cuadros, así la capa y las piernas se mueven sin que el personaje "patine".
            int alto = m.MaxY - m.MinY + 1;
            int tope = Math.Min(m.MaxY, m.MinY + Math.Max(2, (int)(alto * 0.20f)));
            double sx = 0; int cnt = 0;
            for (int y = m.MinY; y <= tope; y++)
                for (int x = z.Left; x < z.Right; x++)
                    if (masc[y * w + x]) { sx += x; cnt++; }
            m.CabezaX = cnt > 0 ? (float)(sx / cnt) : (m.MinX + m.MaxX) / 2f;
            return m;
        }

        public static HojaCaballero Cargar(Bitmap orig, int alturaPx)
        {
            var hoja = new HojaCaballero();
            // 'orig' viene de Properties.Resources: NO se libera aquí porque el recurso queda en caché.
            using (var limpia = new Bitmap(orig.Width, orig.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g0 = Graphics.FromImage(limpia))
                {
                    g0.CompositingMode = CompositingMode.SourceCopy;
                    g0.DrawImage(orig, 0, 0, orig.Width, orig.Height);
                }
                int w = limpia.Width, h = limpia.Height;

                // 1) Leer píxeles, borrar el ruido casi transparente y crear la máscara de "hay personaje"
                BitmapData bd = limpia.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                int stride = bd.Stride;
                byte[] px = new byte[stride * h];
                Marshal.Copy(bd.Scan0, px, 0, px.Length);
                bool[] masc = new bool[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * stride + x * 4;
                        if (px[i + 3] >= UmbralAlfa) masc[y * w + x] = true;
                        else { px[i] = 0; px[i + 1] = 0; px[i + 2] = 0; px[i + 3] = 0; }
                    }
                Marshal.Copy(px, 0, bd.Scan0, px.Length);
                limpia.UnlockBits(bd);

                // 2) Detectar filas (bandas horizontales) y columnas (bloques) por proyección de píxeles
                int[] perfilFilas = new int[h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        if (masc[y * w + x]) perfilFilas[y]++;
                List<int[]> bandas = Tramos(perfilFilas, 12, 20);

                Rectangle[,] zonas = new Rectangle[Filas, Columnas];
                bool ok = bandas.Count == Filas;
                if (ok)
                {
                    for (int f = 0; f < Filas && ok; f++)
                    {
                        int[] perfilCol = new int[w];
                        for (int y = bandas[f][0]; y <= bandas[f][1]; y++)
                            for (int x = 0; x < w; x++)
                                if (masc[y * w + x]) perfilCol[x]++;
                        List<int[]> cols = Tramos(perfilCol, 10, 10);
                        if (cols.Count != Columnas) { ok = false; break; }
                        for (int c = 0; c < Columnas; c++)
                            zonas[f, c] = new Rectangle(cols[c][0], bandas[f][0],
                                                        cols[c][1] - cols[c][0] + 1, bandas[f][1] - bandas[f][0] + 1);
                    }
                }
                if (!ok)   // plan B: cuadrícula regular
                {
                    for (int f = 0; f < Filas; f++)
                        for (int c = 0; c < Columnas; c++)
                        {
                            int x0 = c * w / Columnas, x1 = (c + 1) * w / Columnas;
                            int y0 = f * h / Filas, y1 = (f + 1) * h / Filas;
                            zonas[f, c] = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                        }
                }

                // 3) Medir cada cuadro (caja ajustada + centro de la cabeza + pies)
                Medida[,] med = new Medida[Filas, Columnas];
                double sumaAlto = 0; int nValidos = 0;
                for (int f = 0; f < Filas; f++)
                    for (int c = 0; c < Columnas; c++)
                    {
                        med[f, c] = Medir(masc, w, zonas[f, c]);
                        if (med[f, c].Valida) { sumaAlto += med[f, c].MaxY - med[f, c].MinY + 1; nValidos++; }
                    }
                if (nValidos == 0) throw new InvalidDataException("La imagen no contiene sprites visibles.");
                float escala = alturaPx / (float)(sumaAlto / nValidos);   // misma escala para TODOS los cuadros

                // 4) Recortar y reducir cada cuadro con alta calidad, guardando su punto de apoyo
                for (int f = 0; f < Filas; f++)
                    for (int c = 0; c < Columnas; c++)
                    {
                        Medida m = med[f, c];
                        if (!m.Valida) continue;
                        int srcX = Math.Max(0, m.MinX - Margen), srcY = Math.Max(0, m.MinY - Margen);
                        int srcR = Math.Min(w, m.MaxX + 1 + Margen), srcB = Math.Min(h, m.MaxY + 1 + Margen);
                        int cw = srcR - srcX, ch = srcB - srcY;
                        int dw = Math.Max(1, (int)Math.Ceiling(cw * escala));
                        int dh = Math.Max(1, (int)Math.Ceiling(ch * escala));

                        var dest = new Bitmap(dw, dh, PixelFormat.Format32bppPArgb);
                        using (Graphics g = Graphics.FromImage(dest))
                        using (var ia = new ImageAttributes())
                        {
                            ia.SetWrapMode(WrapMode.TileFlipXY);
                            g.CompositingMode = CompositingMode.SourceCopy;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.DrawImage(limpia, new Rectangle(0, 0, dw, dh), srcX, srcY, cw, ch, GraphicsUnit.Pixel, ia);
                        }
                        float sxf = dw / (float)cw, syf = dh / (float)ch;
                        hoja.Cuadros[f, c] = new Cuadro
                        {
                            Img = dest,
                            AnclaX = (m.CabezaX - srcX + 0.5f) * sxf,
                            AnclaY = (m.MaxY + 1 - srcY) * syf
                        };
                    }

                // 5) Si algún cuadro estuviera vacío, reutiliza uno vecino para evitar errores
                for (int f = 0; f < Filas; f++)
                    for (int c = 0; c < Columnas; c++)
                        if (hoja.Cuadros[f, c] == null)
                            hoja.Cuadros[f, c] = hoja.Cuadros[f, 1] ?? hoja.Cuadros[f, 0] ?? hoja.Cuadros[0, 1];
            }
            return hoja;
        }

        public void Liberar()
        {
            var vistos = new HashSet<Bitmap>();
            foreach (Cuadro c in Cuadros)
                if (c != null && c.Img != null && vistos.Add(c.Img)) c.Img.Dispose();
        }
    }

    // ===================================================================================
    //  FAUNA, ANOMALÍAS, DIÁLOGOS Y MISIONES (datos)
    // ===================================================================================
    enum Especie
    {
        Zorro = 0, Ciervo = 1, Lobo = 2, Oso = 3,
        ZorroMagico = 4, CiervoLuminoso = 5, Unicornio = 6, Centauro = 7, Minotauro = 8
    }
    enum Estado { Vagar, Huir, Amenaza, Cargar, Domesticando, Domesticado }

    sealed class DatosEspecie
    {
        public string Nombre, Titulo, Riesgo, Clave;
        public bool Domesticable;
        public float VelCam, VelHuir, VelAtaque, Deteccion, Radio, ProbDom, XpDom, Altura, SombraAncho, SombraAlto;
        // índices de cuadro dentro de la hoja de sprites (0 = primero, de izquierda a derecha y de arriba a abajo)
        public int Idle, Idle2, Alerta, Amenaza, Huir, Feliz, Susto;
        public int[] Caminar;
        public int Resplandor;   // 0 = ninguno, 1 = cian, 2 = violeta (luz mágica que emite la criatura)
    }

    static class Fauna
    {
        // El orden coincide con el enum Especie:
        // Zorro, Ciervo, Lobo, Oso, ZorroMagico, CiervoLuminoso, Unicornio, Centauro, Minotauro
        public static readonly DatosEspecie[] D = new DatosEspecie[]
        {
            new DatosEspecie { Nombre = "zorro rojo", Titulo = "Zorro Rojo", Riesgo = "BAJO", Clave = "zorro", Domesticable = true,
                VelCam = 55f, VelHuir = 190f, VelAtaque = 0f, Deteccion = 280f, Radio = 10f, ProbDom = 0.90f, XpDom = 25f,
                Altura = 40f, SombraAncho = 46f, SombraAlto = 14f,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 3, Huir = 5, Feliz = 4, Susto = 2, Caminar = new int[] { 3, 0 } },

            new DatosEspecie { Nombre = "ciervo", Titulo = "Ciervo", Riesgo = "BAJO", Clave = "ciervo", Domesticable = true,
                VelCam = 48f, VelHuir = 230f, VelAtaque = 0f, Deteccion = 300f, Radio = 13f, ProbDom = 0.80f, XpDom = 30f,
                Altura = 88f, SombraAncho = 62f, SombraAlto = 18f,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 2, Huir = 5, Feliz = 4, Susto = 3, Caminar = new int[] { 1, 0 } },

            new DatosEspecie { Nombre = "lobo gris", Titulo = "Lobo Gris", Riesgo = "MEDIO", Clave = "lobo", Domesticable = true,
                VelCam = 60f, VelHuir = 230f, VelAtaque = 250f, Deteccion = 320f, Radio = 13f, ProbDom = 0.60f, XpDom = 70f,
                Altura = 62f, SombraAncho = 66f, SombraAlto = 18f,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 3, Huir = 5, Feliz = 1, Susto = 2, Caminar = new int[] { 4, 0 } },

            new DatosEspecie { Nombre = "oso pardo", Titulo = "Oso Pardo", Riesgo = "ALTO", Clave = "oso", Domesticable = false,
                VelCam = 45f, VelHuir = 0f, VelAtaque = 215f, Deteccion = 340f, Radio = 24f, ProbDom = 0f, XpDom = 0f,
                Altura = 100f, SombraAncho = 100f, SombraAlto = 28f,
                Idle = 2, Idle2 = 0, Alerta = 3, Amenaza = 4, Huir = 5, Feliz = 2, Susto = 3, Caminar = new int[] { 1, 2 } },

            // ---- NUEVAS CRIATURAS (hojas de 2 filas x 4 columnas = 8 cuadros, índices 0..7) ----

            // Zorro de nueve colas: domesticable pero astuto; si falla te lanza un hechizo
            new DatosEspecie { Nombre = "zorro mágico", Titulo = "Zorro Mágico", Riesgo = "MEDIO", Clave = "zorromagico", Domesticable = true,
                VelCam = 55f, VelHuir = 200f, VelAtaque = 0f, Deteccion = 290f, Radio = 11f, ProbDom = 0.55f, XpDom = 75f,
                Altura = 56f, SombraAncho = 66f, SombraAlto = 16f, Resplandor = 2,
                Idle = 0, Idle2 = 1, Alerta = 3, Amenaza = 4, Huir = 7, Feliz = 2, Susto = 3, Caminar = new int[] { 6, 7 } },

            // Espíritu del bosque: domesticable pero tímido en extremo
            new DatosEspecie { Nombre = "ciervo luminoso", Titulo = "Ciervo Luminoso", Riesgo = "BAJO", Clave = "ciervoluminoso", Domesticable = true,
                VelCam = 50f, VelHuir = 250f, VelAtaque = 0f, Deteccion = 330f, Radio = 13f, ProbDom = 0.60f, XpDom = 90f,
                Altura = 96f, SombraAncho = 62f, SombraAlto = 18f, Resplandor = 1,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 3, Huir = 7, Feliz = 5, Susto = 6, Caminar = new int[] { 6, 4 } },

            // Unicornio: NO SE DEJA acercar ni domesticar; solo se avista de lejos
            new DatosEspecie { Nombre = "unicornio", Titulo = "Unicornio", Riesgo = "NINGUNO", Clave = "unicornio", Domesticable = false,
                VelCam = 52f, VelHuir = 280f, VelAtaque = 0f, Deteccion = 360f, Radio = 14f, ProbDom = 0f, XpDom = 0f,
                Altura = 84f, SombraAncho = 62f, SombraAlto = 18f, Resplandor = 1,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 3, Huir = 7, Feliz = 4, Susto = 6, Caminar = new int[] { 5, 6, 7 } },

            // Centauro arquero: no se doma, pero se le puede saludar; si lo provocas corriendo, embiste
            new DatosEspecie { Nombre = "centauro", Titulo = "Centauro", Riesgo = "MEDIO", Clave = "centauro", Domesticable = false,
                VelCam = 50f, VelHuir = 230f, VelAtaque = 235f, Deteccion = 330f, Radio = 22f, ProbDom = 0f, XpDom = 0f,
                Altura = 124f, SombraAncho = 100f, SombraAlto = 26f,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 7, Huir = 6, Feliz = 4, Susto = 3, Caminar = new int[] { 5, 6 } },

            // Minotauro: bestia peligrosa, imposible de domar
            new DatosEspecie { Nombre = "minotauro", Titulo = "Minotauro", Riesgo = "ALTO", Clave = "minotauro", Domesticable = false,
                VelCam = 45f, VelHuir = 0f, VelAtaque = 225f, Deteccion = 340f, Radio = 22f, ProbDom = 0f, XpDom = 0f,
                Altura = 118f, SombraAncho = 92f, SombraAlto = 26f,
                Idle = 0, Idle2 = 1, Alerta = 2, Amenaza = 7, Huir = 6, Feliz = 3, Susto = 4, Caminar = new int[] { 5, 6 } }
        };
    }

    sealed class Ser
    {
        public Especie Esp;
        public float X, Y;
        public bool Der = true, Moviendo;
        public Estado Est = Estado.Vagar;
        public float T, AnimT, Alfa, EsperaT, DestX, DestY, Enfriar, AlertaT, EmoteT;
        public bool TieneDest, Aviso, Objetivo, Hostil;
        public int Emote;      // 1 = "!", 2 = "?", 3 = corazón
        public int Orden;      // posición en la fila de mascotas que siguen al caballero
    }

    sealed class Anomalia
    {
        public float X, Y, Fase, Alfa;
        public int Riesgo;     // 0 = bajo, 1 = medio, 2 = alto
        public bool Aviso, Objetivo, Consumida;
    }

    sealed class Mision
    {
        public string Clave, Texto;
        public int Meta, Prog;
        public float Xp;
        public bool Hecha;
    }

    sealed class Dialogo
    {
        public string[] Paginas;
        public string[] Opciones;          // null = solo texto
        public Action<int> Fin;            // recibe el índice de la opción elegida (-1 si no había opciones)
        public int Pag;
        public float Chars;                // letras visibles (efecto máquina de escribir)
        public List<string> Lineas;
        public int LineasPag = -1;

        public Dialogo(string[] paginas, string[] opciones) { Paginas = paginas; Opciones = opciones; }
        public Dialogo(string texto) : this(new string[] { texto }, null) { }

        public string Actual { get { return Paginas[Pag]; } }
        public bool Completa { get { return Chars >= Actual.Length; } }
        public bool Ultima { get { return Pag >= Paginas.Length - 1; } }
        public bool VerOpciones { get { return Opciones != null && Ultima && Completa; } }
    }

    struct Luciernaga { public float X, Y, Fase, Rumbo, Vel, Tam; public int Tono; }

    // ===================================================================================
    //  HOJA DE ANIMALES / ANOMALÍA: quita el fondo (blanco o transparente), detecta cada
    //  cuadro por proyección de píxeles y lo recorta con un punto de apoyo en el suelo.
    // ===================================================================================
    sealed class HojaAnimal
    {
        public Cuadro[] Der;   // mirando a la derecha (como vienen dibujados)
        public Cuadro[] Izq;   // copia volteada

        static List<int[]> Tramos(int[] perfil, int hueco, int minLargo)
        {
            var res = new List<int[]>();
            int ini = -1, ult = -1;
            for (int i = 0; i < perfil.Length; i++)
            {
                if (perfil[i] <= 0) continue;
                if (ini < 0) { ini = i; ult = i; }
                else if (i - ult - 1 > hueco) { res.Add(new int[] { ini, ult }); ini = i; ult = i; }
                else ult = i;
            }
            if (ini >= 0) res.Add(new int[] { ini, ult });
            res.RemoveAll(t => t[1] - t[0] + 1 < minLargo);
            return res;
        }

        static void Sembrar(bool[] claro, bool[] fondo, int[] pila, ref int sp, int p)
        {
            if (claro[p] && !fondo[p]) { fondo[p] = true; pila[sp++] = p; }
        }

        static void Vecino(bool[] masc, bool[] visto, int[] cola, ref int tail, int q)
        {
            if (masc[q] && !visto[q]) { visto[q] = true; cola[tail++] = q; }
        }

        // Deja transparente el fondo conectado con los bordes (blanco o ya transparente) y las motas sueltas.
        // Devuelve la máscara "aquí hay sprite".
        internal static bool[] Preparar(Bitmap bmp, int minPx)
        {
            int w = bmp.Width, h = bmp.Height, n = w * h;
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bd.Stride;
            byte[] px = new byte[stride * h];
            Marshal.Copy(bd.Scan0, px, 0, px.Length);

            bool[] claro = new bool[n];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * stride + x * 4;
                    int b = px[i], g = px[i + 1], r = px[i + 2], a = px[i + 3];
                    if (a < 48) { claro[y * w + x] = true; continue; }
                    int mn = Math.Min(r, Math.Min(g, b)), mx = Math.Max(r, Math.Max(g, b));
                    claro[y * w + x] = mn >= 226 && mx - mn <= 30;
                }

            bool[] fondo = new bool[n];
            int[] pila = new int[n];
            int sp = 0;
            for (int x = 0; x < w; x++) { Sembrar(claro, fondo, pila, ref sp, x); Sembrar(claro, fondo, pila, ref sp, (h - 1) * w + x); }
            for (int y = 0; y < h; y++) { Sembrar(claro, fondo, pila, ref sp, y * w); Sembrar(claro, fondo, pila, ref sp, y * w + w - 1); }
            while (sp > 0)
            {
                int p = pila[--sp];
                int px0 = p % w, py0 = p / w;
                if (px0 > 0) Sembrar(claro, fondo, pila, ref sp, p - 1);
                if (px0 < w - 1) Sembrar(claro, fondo, pila, ref sp, p + 1);
                if (py0 > 0) Sembrar(claro, fondo, pila, ref sp, p - w);
                if (py0 < h - 1) Sembrar(claro, fondo, pila, ref sp, p + w);
            }

            bool[] masc = new bool[n];
            for (int i = 0; i < n; i++) masc[i] = !fondo[i];

            // eliminar motas (componentes diminutos)
            bool[] visto = new bool[n];
            int[] cola = pila;
            for (int s = 0; s < n; s++)
            {
                if (!masc[s] || visto[s]) continue;
                int head = 0, tail = 0;
                cola[tail++] = s; visto[s] = true;
                while (head < tail)
                {
                    int p = cola[head++];
                    int qx = p % w, qy = p / w;
                    if (qx > 0) Vecino(masc, visto, cola, ref tail, p - 1);
                    if (qx < w - 1) Vecino(masc, visto, cola, ref tail, p + 1);
                    if (qy > 0) Vecino(masc, visto, cola, ref tail, p - w);
                    if (qy < h - 1) Vecino(masc, visto, cola, ref tail, p + w);
                }
                if (tail < minPx) for (int k = 0; k < tail; k++) masc[cola[k]] = false;
            }

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (!masc[y * w + x])
                    {
                        int i = y * stride + x * 4;
                        px[i] = 0; px[i + 1] = 0; px[i + 2] = 0; px[i + 3] = 0;
                    }
            Marshal.Copy(px, 0, bd.Scan0, px.Length);
            bmp.UnlockBits(bd);
            return masc;
        }

        // filas x columnas de cuadros. 'tam' = alto (o ancho si porAncho) promedio en pantalla.
        // centrado = true -> el punto de apoyo es el centro del cuadro (para efectos en el suelo).
        public static HojaAnimal Cargar(Bitmap orig, int filas, int cols, float tam, bool porAncho, bool centrado)
        {
            var hoja = new HojaAnimal();
            int total = filas * cols;
            hoja.Der = new Cuadro[total];
            hoja.Izq = new Cuadro[total];

            // 'orig' viene de Properties.Resources: NO se libera aquí (el recurso queda en caché).
            using (var limpia = new Bitmap(orig.Width, orig.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g0 = Graphics.FromImage(limpia))
                {
                    g0.CompositingMode = CompositingMode.SourceCopy;
                    g0.DrawImage(orig, 0, 0, orig.Width, orig.Height);
                }
                int w = limpia.Width, h = limpia.Height;
                bool[] masc = Preparar(limpia, 150);

                // filas y columnas por proyección
                int[] perfilFilas = new int[h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        if (masc[y * w + x]) perfilFilas[y]++;
                List<int[]> bandas = Tramos(perfilFilas, 8, 20);

                Rectangle[] zonas = new Rectangle[total];
                bool ok = bandas.Count == filas;
                if (ok)
                {
                    for (int f = 0; f < filas && ok; f++)
                    {
                        int[] perfilCol = new int[w];
                        for (int y = bandas[f][0]; y <= bandas[f][1]; y++)
                            for (int x = 0; x < w; x++)
                                if (masc[y * w + x]) perfilCol[x]++;
                        List<int[]> tc = Tramos(perfilCol, 8, 12);
                        if (tc.Count != cols) { ok = false; break; }
                        for (int c = 0; c < cols; c++)
                            zonas[f * cols + c] = new Rectangle(tc[c][0], bandas[f][0],
                                                                tc[c][1] - tc[c][0] + 1, bandas[f][1] - bandas[f][0] + 1);
                    }
                }
                if (!ok)   // plan B: cuadrícula regular
                {
                    for (int f = 0; f < filas; f++)
                        for (int c = 0; c < cols; c++)
                        {
                            int x0 = c * w / cols, x1 = (c + 1) * w / cols;
                            int y0 = f * h / filas, y1 = (f + 1) * h / filas;
                            zonas[f * cols + c] = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                        }
                }

                // medir cada cuadro
                int[] minX = new int[total], minY = new int[total], maxX = new int[total], maxY = new int[total];
                float[] centX = new float[total];
                bool[] valida = new bool[total];
                double sumaAlto = 0, sumaAncho = 0; int nValidos = 0;
                for (int k = 0; k < total; k++)
                {
                    Rectangle z = zonas[k];
                    int mnx = int.MaxValue, mny = int.MaxValue, mxx = -1, mxy = -1;
                    double sx = 0; int cnt = 0;
                    for (int y = z.Top; y < z.Bottom; y++)
                        for (int x = z.Left; x < z.Right; x++)
                            if (masc[y * w + x])
                            {
                                if (x < mnx) mnx = x; if (x > mxx) mxx = x;
                                if (y < mny) mny = y; if (y > mxy) mxy = y;
                                sx += x; cnt++;
                            }
                    if (mxx < 0) continue;
                    valida[k] = true;
                    minX[k] = mnx; minY[k] = mny; maxX[k] = mxx; maxY[k] = mxy;
                    centX[k] = (float)(sx / cnt);
                    sumaAlto += mxy - mny + 1; sumaAncho += mxx - mnx + 1; nValidos++;
                }
                if (nValidos == 0) throw new InvalidDataException("La imagen no contiene sprites visibles.");
                float escala = tam / (float)((porAncho ? sumaAncho : sumaAlto) / nValidos);

                for (int k = 0; k < total; k++)
                {
                    if (!valida[k]) continue;
                    const int Margen = 2;
                    int srcX = Math.Max(0, minX[k] - Margen), srcY = Math.Max(0, minY[k] - Margen);
                    int srcR = Math.Min(w, maxX[k] + 1 + Margen), srcB = Math.Min(h, maxY[k] + 1 + Margen);
                    int cw = srcR - srcX, ch = srcB - srcY;
                    int dw = Math.Max(1, (int)Math.Ceiling(cw * escala));
                    int dh = Math.Max(1, (int)Math.Ceiling(ch * escala));

                    var dest = new Bitmap(dw, dh, PixelFormat.Format32bppPArgb);
                    using (Graphics g = Graphics.FromImage(dest))
                    using (var ia = new ImageAttributes())
                    {
                        ia.SetWrapMode(WrapMode.TileFlipXY);
                        g.CompositingMode = CompositingMode.SourceCopy;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawImage(limpia, new Rectangle(0, 0, dw, dh), srcX, srcY, cw, ch, GraphicsUnit.Pixel, ia);
                    }
                    float sxf = dw / (float)cw, syf = dh / (float)ch;
                    var cuadro = new Cuadro();
                    cuadro.Img = dest;
                    if (centrado) { cuadro.AnclaX = dw / 2f; cuadro.AnclaY = dh / 2f; }
                    else { cuadro.AnclaX = (centX[k] - srcX + 0.5f) * sxf; cuadro.AnclaY = (maxY[k] + 1 - srcY) * syf; }
                    hoja.Der[k] = cuadro;

                    var volteado = (Bitmap)dest.Clone();
                    volteado.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    var cuadroI = new Cuadro();
                    cuadroI.Img = volteado;
                    cuadroI.AnclaX = dw - cuadro.AnclaX;
                    cuadroI.AnclaY = cuadro.AnclaY;
                    hoja.Izq[k] = cuadroI;
                }

                // si algún cuadro quedó vacío, reutiliza el primero válido
                int primero = 0;
                for (int k = 0; k < total; k++) if (valida[k]) { primero = k; break; }
                for (int k = 0; k < total; k++)
                    if (hoja.Der[k] == null) { hoja.Der[k] = hoja.Der[primero]; hoja.Izq[k] = hoja.Izq[primero]; }
            }
            return hoja;
        }

        // Retrato: quita el fondo y lo reduce (respetando proporción) a un cuadrado 'lado' x 'lado'
        public static Bitmap CrearRetrato(Bitmap orig, int lado)
        {
            using (var limpia = new Bitmap(orig.Width, orig.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g0 = Graphics.FromImage(limpia))
                {
                    g0.CompositingMode = CompositingMode.SourceCopy;
                    g0.DrawImage(orig, 0, 0, orig.Width, orig.Height);
                }
                Preparar(limpia, 400);
                float k = lado / (float)Math.Max(limpia.Width, limpia.Height);
                int dw = Math.Max(1, (int)Math.Round(limpia.Width * k)), dh = Math.Max(1, (int)Math.Round(limpia.Height * k));
                var dest = new Bitmap(lado, lado, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(dest))
                using (var ia = new ImageAttributes())
                {
                    ia.SetWrapMode(WrapMode.TileFlipXY);
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(limpia, new Rectangle((lado - dw) / 2, (lado - dh) / 2, dw, dh),
                                0, 0, limpia.Width, limpia.Height, GraphicsUnit.Pixel, ia);
                }
                return dest;
            }
        }

        public void Liberar()
        {
            var vistos = new HashSet<Bitmap>();
            if (Der != null) foreach (Cuadro c in Der) if (c != null && c.Img != null && vistos.Add(c.Img)) c.Img.Dispose();
            if (Izq != null) foreach (Cuadro c in Izq) if (c != null && c.Img != null && vistos.Add(c.Img)) c.Img.Dispose();
        }
    }

    // ===================================================================================
    //  FORMULARIO NivelUno
    // ===================================================================================
    public partial class Juego : Form
    {
        // ------------------------------------------------ AJUSTES (cámbialos a tu gusto)
        const int SemillaMundo = 7341;       // otra semilla = otro mundo
        const int AlturaPersonaje = 104;     // alto del caballero en pantalla (px)
        const float VelCaminar = 200f;       // px por segundo
        const float VelCorrer = 330f;
        const float EnergiaMax = 100f;       // la energía inicia en 100
        const float PxPorXp = 60f;           // 1 punto de experiencia por cada 60 px recorridos
        const float RadioJugador = 14f;

        enum Dir { Abajo = 0, Arriba = 1, Izquierda = 2, Derecha = 3 }
        static readonly int[] SecuenciaCol = { 0, 1, 2, 1 };   // ciclo de paso: pie izq, neutro, pie der, neutro

        struct Particula { public float X, Y, Vx, Vy, Fase, Tam; public int Tono; }

        // Estado del juego
        Mundo _mundo;
        HojaCaballero _hoja;
        PointF _pos;
        float _camX, _camY;
        Dir _dir = Dir.Abajo;
        bool _moviendo;
        float _fase = 1f;
        float _energia = EnergiaMax, _energiaMostrada = EnergiaMax;
        bool _cansado;
        int _nivel = 1;
        float _xp, _xpMostrada;
        string _aviso = "";
        float _avisoT;
        bool _pausa;
        float _t, _tLimpieza;

        // Entrada
        readonly HashSet<Keys> _teclas = new HashSet<Keys>();
        Rectangle _btnMenu;
        readonly Rectangle[] _btnPausa = new Rectangle[3];
        Rectangle _panelPausa;
        bool _hoverMenu;
        int _hoverPausa = -1;

        // Bucle
        readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        readonly Stopwatch _reloj = Stopwatch.StartNew();
        double _ultimo;

        // Recursos de dibujo
        readonly List<Objeto> _visibles = new List<Objeto>();
        static readonly Comparison<Objeto> PorY = delegate (Objeto a, Objeto b) { return a.Y.CompareTo(b.Y); };
        Particula[] _particulas = new Particula[30];
        Bitmap _vineta, _nube;
        Font _fTitulo, _fTexto, _fNivel, _fPeq, _fGrande, _fBoton;
        SolidBrush _brSombra1, _brSombra2, _brAgua, _brHoja;
        StringFormat _sfCentro;

        public Juego()
        {
            // Sin InitializeComponent: todo se crea por código.
            Text = "Nivel Uno";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            KeyPreview = true;
            BackColor = Color.Black;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            _fTitulo = new Font("Segoe UI Semibold", 11f, FontStyle.Regular);
            _fTexto = new Font("Segoe UI", 10f, FontStyle.Regular);
            _fPeq = new Font("Segoe UI Semibold", 8f, FontStyle.Regular);
            _fNivel = new Font("Segoe UI", 20f, FontStyle.Bold);
            _fGrande = new Font("Segoe UI", 30f, FontStyle.Bold);
            _fBoton = new Font("Segoe UI Semibold", 13f, FontStyle.Regular);
            _brSombra1 = new SolidBrush(Color.FromArgb(34, 6, 8, 28));
            _brSombra2 = new SolidBrush(Color.FromArgb(46, 6, 8, 28));
            _brAgua = new SolidBrush(Color.FromArgb(80, 214, 238, 250));
            _brHoja = new SolidBrush(Color.FromArgb(150, 160, 200, 90));
            _sfCentro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            _timer.Interval = 8;
            _timer.Tick += Timer_Tick;
        }

        // ------------------------------------------------------------------ carga
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Pantalla completa sin bordes (cubre también la barra de tareas)
            WindowState = FormWindowState.Normal;
            Bounds = Screen.PrimaryScreen.Bounds;

            try
            {
                Arte.Crear();
                // La hoja de sprites viene de los Resources del proyecto (CaballeroJuego)
                _hoja = HojaCaballero.Cargar(Properties.Resources.CaballeroJuego, AlturaPersonaje);
                CargarHojasMagia();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No pude preparar los sprites: " + ex.Message, "Nivel Uno");
                Close();
                return;
            }

            _mundo = new Mundo(SemillaMundo);
            _pos = _mundo.BuscarInicio();
            _camX = _pos.X; _camY = _pos.Y;

            // Pre-generar el terreno alrededor del jugador
            int pcx = (int)Math.Floor(_pos.X / Mundo.TamParcela), pcy = (int)Math.Floor(_pos.Y / Mundo.TamParcela);
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                    _mundo.Obtener(pcx + dx, pcy + dy);

            _nube = CrearNube(128);
            CrearRecursosMagicos();
            var rnd = new Random(5);
            int W = Math.Max(1, ClientSize.Width), H = Math.Max(1, ClientSize.Height);
            for (int i = 0; i < _particulas.Length; i++)
            {
                Particula p = new Particula();
                p.X = (float)rnd.NextDouble() * W; p.Y = (float)rnd.NextDouble() * H;
                p.Vx = 12f + (float)rnd.NextDouble() * 18f; p.Vy = 6f + (float)rnd.NextDouble() * 12f;
                p.Fase = (float)rnd.NextDouble() * 6.28f; p.Tam = 3f + (float)rnd.NextDouble() * 4f;
                p.Tono = rnd.Next(3);
                _particulas[i] = p;
            }

            _ultimo = _reloj.Elapsed.TotalSeconds;
            IniciarAventura();
            _timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            if (_mundo != null) _mundo.Liberar();
            if (_hoja != null) _hoja.Liberar();
            Arte.Liberar();
            if (_vineta != null) _vineta.Dispose();
            if (_nube != null) _nube.Dispose();
            _fTitulo.Dispose(); _fTexto.Dispose(); _fNivel.Dispose(); _fPeq.Dispose(); _fGrande.Dispose(); _fBoton.Dispose();
            _brSombra1.Dispose(); _brSombra2.Dispose(); _brAgua.Dispose(); _brHoja.Dispose(); _sfCentro.Dispose();
            LiberarMagia();
            base.OnFormClosed(e);
        }

        // ------------------------------------------------------------------ entrada
        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            if (k == Keys.Up || k == Keys.Down || k == Keys.Left || k == Keys.Right) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { AlternarPausa(); e.Handled = true; return; }
            if (_dialogo != null && !_pausa)
            {
                if (e.KeyCode != _teclaDlg) { _teclaDlg = e.KeyCode; TeclaDialogo(e.KeyCode); }
                e.Handled = true;
                return;
            }
            _teclas.Add(e.KeyCode);
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _teclas.Remove(e.KeyCode);
            if (e.KeyCode == _teclaDlg) _teclaDlg = Keys.None;
            base.OnKeyUp(e);
        }
        protected override void OnDeactivate(EventArgs e) { _teclas.Clear(); base.OnDeactivate(e); }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Distribuir();
            _hoverMenu = !_pausa && _btnMenu.Contains(e.Location);
            _hoverPausa = -1;
            if (_pausa)
                for (int i = 0; i < _btnPausa.Length; i++)
                    if (_btnPausa[i].Contains(e.Location)) _hoverPausa = i;
            ActualizarHoverDialogo(e.Location);
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Distribuir();
            if (!_pausa)
            {
                if (_btnMenu.Contains(e.Location)) { AlternarPausa(); return; }
                ClicDialogo(e.Location);
                return;
            }
            for (int i = 0; i < _btnPausa.Length; i++)
                if (_btnPausa[i].Contains(e.Location))
                {
                    if (i == 0) AlternarPausa();
                    else if (i == 1) Reiniciar();
                    else Close();
                    return;
                }
        }

        void AlternarPausa()
        {
            _pausa = !_pausa;
            _teclas.Clear();
            _hoverMenu = false; _hoverPausa = -1;
            _ultimo = _reloj.Elapsed.TotalSeconds;
        }

        void Reiniciar()
        {
            _pos = _mundo.BuscarInicio();
            _camX = _pos.X; _camY = _pos.Y;
            _nivel = 1; _xp = 0; _xpMostrada = 0;
            _energia = EnergiaMax; _energiaMostrada = EnergiaMax; _cansado = false;
            _dir = Dir.Abajo; _fase = 1f; _aviso = ""; _avisoT = 0f;
            ReiniciarAventura();
            _pausa = false;
            _ultimo = _reloj.Elapsed.TotalSeconds;
        }

        float XpParaSiguiente { get { return 100f + (_nivel - 1) * 50f; } }

        // ------------------------------------------------------------------ bucle
        void Timer_Tick(object sender, EventArgs e)
        {
            double ahora = _reloj.Elapsed.TotalSeconds;
            float dt = (float)(ahora - _ultimo);
            _ultimo = ahora;
            if (dt > 0.05f) dt = 0.05f;
            if (dt < 0f) dt = 0f;
            if (!_pausa) Actualizar(dt);
            Invalidate();
        }

        void Actualizar(float dt)
        {
            _t += dt;

            // ---- entrada
            int sx = 0, sy = 0;
            if (_teclas.Contains(Keys.A) || _teclas.Contains(Keys.Left)) sx -= 1;
            if (_teclas.Contains(Keys.D) || _teclas.Contains(Keys.Right)) sx += 1;
            if (_teclas.Contains(Keys.W) || _teclas.Contains(Keys.Up)) sy -= 1;
            if (_teclas.Contains(Keys.S) || _teclas.Contains(Keys.Down)) sy += 1;
            if (_dialogo != null || _accion != 0) { sx = 0; sy = 0; }   // el caballero no se mueve mientras habla o actúa
            bool quiere = sx != 0 || sy != 0;
            bool shift = (ModifierKeys & Keys.Shift) != 0;
            bool correr = quiere && shift && !_cansado && _energia > 0f;

            // ---- energía
            if (correr)
            {
                _energia -= 18f * dt;
                if (_energia <= 0f) { _energia = 0f; _cansado = true; correr = false; }
            }
            else
            {
                _energia = Math.Min(EnergiaMax, _energia + (quiere ? 6f : 14f) * dt);
                if (_cansado && _energia >= 25f) _cansado = false;
            }

            // ---- movimiento con colisiones (por ejes, para poder deslizarse por las paredes)
            float movido = 0f;
            if (quiere)
            {
                float vel = correr ? VelCorrer : VelCaminar;
                if (_mundo.TerrenoEn(_pos.X, _pos.Y) == Terreno.Somera) vel *= 0.6f;
                float len = (float)Math.Sqrt(sx * sx + sy * sy);
                float dx = sx / len * vel * dt, dy = sy / len * vel * dt;
                float x0 = _pos.X, y0 = _pos.Y;
                if (!_mundo.Bloqueado(_pos.X + dx, _pos.Y, RadioJugador)) _pos.X += dx;
                if (!_mundo.Bloqueado(_pos.X, _pos.Y + dy, RadioJugador)) _pos.Y += dy;
                float ddx = _pos.X - x0, ddy = _pos.Y - y0;
                movido = (float)Math.Sqrt(ddx * ddx + ddy * ddy);

                // dirección a la que mira
                if (sx != 0 && sy != 0)
                {
                    bool compatible = (_dir == Dir.Izquierda && sx < 0) || (_dir == Dir.Derecha && sx > 0) ||
                                      (_dir == Dir.Arriba && sy < 0) || (_dir == Dir.Abajo && sy > 0);
                    if (!compatible) _dir = sx < 0 ? Dir.Izquierda : Dir.Derecha;
                }
                else if (sx != 0) _dir = sx < 0 ? Dir.Izquierda : Dir.Derecha;
                else _dir = sy < 0 ? Dir.Arriba : Dir.Abajo;
            }
            _moviendo = movido > 0.05f;
            _corriendo = correr && _moviendo;

            // ---- animación sincronizada con la distancia recorrida (los pies no patinan)
            if (_moviendo) _fase += movido / (correr ? 46f : 36f);
            else _fase = 1f;
            if (_fase > 1000f) _fase -= 1000f;

            // ---- experiencia
            _xp += movido / PxPorXp;
            while (_xp >= XpParaSiguiente)
            {
                _xp -= XpParaSiguiente;
                _nivel++;
                _energia = EnergiaMax; _cansado = false;
                _aviso = "¡Nivel " + _nivel + "!";
                _avisoT = 2.5f;
                _xpMostrada = 0f;
            }
            float suav = Math.Min(1f, dt * 6f);
            _xpMostrada += (_xp - _xpMostrada) * suav;
            _energiaMostrada += (_energia - _energiaMostrada) * suav;
            if (_avisoT > 0f) _avisoT -= dt;

            // ---- cámara
            float camAntX = _camX, camAntY = _camY;
            float k = Math.Min(1f, dt * 10f);
            _camX += (_pos.X - _camX) * k;
            _camY += (_pos.Y - _camY) * k;
            float dcx = _camX - camAntX, dcy = _camY - camAntY;

            // ---- partículas (hojas flotando con paralaje)
            int W = Math.Max(1, ClientSize.Width), H = Math.Max(1, ClientSize.Height);
            for (int i = 0; i < _particulas.Length; i++)
            {
                Particula p = _particulas[i];
                p.Fase += dt * (1.2f + p.Tam * 0.1f);
                p.X += (p.Vx + (float)Math.Sin(p.Fase) * 18f) * dt - dcx * 0.9f;
                p.Y += (p.Vy + (float)Math.Cos(p.Fase * 0.8f) * 8f) * dt - dcy * 0.9f;
                if (p.X < -40f) p.X += W + 80f; else if (p.X > W + 40f) p.X -= W + 80f;
                if (p.Y < -40f) p.Y += H + 80f; else if (p.Y > H + 40f) p.Y -= H + 80f;
                _particulas[i] = p;
            }

            // ---- fauna, anomalías, diálogos, misiones y luciérnagas
            ActualizarMagia(dt);

            // ---- mundo infinito: generar por delante y liberar lo que queda lejos
            int pcx = (int)Math.Floor(_pos.X / Mundo.TamParcela), pcy = (int)Math.Floor(_pos.Y / Mundo.TamParcela);
            int radioVista = (int)Math.Ceiling(Math.Max(W, H) / 2f / Mundo.TamParcela) + 1;
            _mundo.Precargar(pcx, pcy, radioVista - 1);
            _tLimpieza += dt;
            if (_tLimpieza > 1f) { _tLimpieza = 0f; _mundo.Limpiar(pcx, pcy, radioVista); }
        }

        // ------------------------------------------------------------------ dibujo
        static int Piso(int a, int b) { return (int)Math.Floor(a / (double)b); }
        static float Lim(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }

        void Distribuir()
        {
            int W = ClientSize.Width, H = ClientSize.Height;
            _btnMenu = new Rectangle(W - 20 - 150, 20, 150, 46);
            int bw = 320, bh = 56, gap = 16;
            int total = 3 * bh + 2 * gap;
            int y0 = H / 2 - total / 2 + 30;
            for (int i = 0; i < 3; i++) _btnPausa[i] = new Rectangle(W / 2 - bw / 2, y0 + i * (bh + gap), bw, bh);
            _panelPausa = new Rectangle(W / 2 - 220, y0 - 110, 440, total + 170);
            DistribuirDialogo(W, H);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_mundo == null || _hoja == null) { g.Clear(Color.Black); return; }

            int W = ClientSize.Width, H = ClientSize.Height;
            if (W < 2 || H < 2) return;
            Distribuir();
            int ox = (int)Math.Round(_camX - W / 2f), oy = (int)Math.Round(_camY - H / 2f);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.None;

            DibujarSuelo(g, ox, oy, W, H);
            DibujarAgua(g, ox, oy, W, H);
            DibujarAnomalias(g, ox, oy);
            DibujarEntidades(g, ox, oy, W, H);
            DibujarNubes(g, ox, oy, W, H);
            DibujarMagia(g, ox, oy, W, H);
            DibujarParticulas(g);

            if (_vineta == null || _vineta.Width != W || _vineta.Height != H)
            {
                if (_vineta != null) _vineta.Dispose();
                _vineta = CrearVineta(W, H);
            }
            g.DrawImage(_vineta, 0, 0, W, H);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            DibujarHud(g, W, H);
            DibujarMision(g, W, H);
            DibujarDialogo(g, W, H);
            if (_pausa) DibujarMenuPausa(g, W, H);
        }

        void DibujarSuelo(Graphics g, int ox, int oy, int W, int H)
        {
            int c0x = Piso(ox, Mundo.TamParcela), c1x = Piso(ox + W - 1, Mundo.TamParcela);
            int c0y = Piso(oy, Mundo.TamParcela), c1y = Piso(oy + H - 1, Mundo.TamParcela);
            for (int cy = c0y; cy <= c1y; cy++)
                for (int cx = c0x; cx <= c1x; cx++)
                {
                    Parcela p = _mundo.Obtener(cx, cy);
                    g.DrawImage(p.Suelo, cx * Mundo.TamParcela - ox, cy * Mundo.TamParcela - oy, Mundo.TamParcela, Mundo.TamParcela);
                }
        }

        // Destellos animados sobre el agua
        void DibujarAgua(Graphics g, int ox, int oy, int W, int H)
        {
            const int Cel = 56;
            int i0 = Piso(ox, Cel), i1 = Piso(ox + W, Cel), j0 = Piso(oy, Cel), j1 = Piso(oy + H, Cel);
            for (int j = j0; j <= j1; j++)
                for (int i = i0; i <= i1; i++)
                {
                    float h = Ruido.Hash(i, j, 7);
                    if (h < 0.45f) continue;
                    float wx = i * Cel + 8 + Ruido.Hash(i, j, 8) * (Cel - 30);
                    float wy = j * Cel + 8 + Ruido.Hash(i, j, 9) * (Cel - 16);
                    if (_mundo.Elev(wx, wy) >= Mundo.UmbralSomera - 0.012f) continue;
                    float a = ((float)Math.Sin(_t * 1.7f + h * 40f) + 1f) * 0.5f;
                    int alfa = (int)(a * 95f);
                    if (alfa < 8) continue;
                    _brAgua.Color = Color.FromArgb(alfa, 214, 238, 250);
                    int largo = 10 + (int)(h * 18f);
                    int dx = (int)(Math.Sin(_t * 0.8f + h * 10f) * 4f);
                    g.FillRectangle(_brAgua, (int)wx - ox + dx, (int)wy - oy, largo, 2);
                    g.FillRectangle(_brAgua, (int)wx - ox + dx + 4, (int)wy - oy + 4, largo / 2, 2);
                }
        }

        void DibujarSombra(Graphics g, float sx, float sy, float ancho, float alto, float dx)
        {
            float cx = sx + dx, cy = sy;
            g.FillEllipse(_brSombra1, cx - ancho / 2f, cy - alto / 2f, ancho, alto);
            g.FillEllipse(_brSombra2, cx - ancho * 0.38f, cy - alto * 0.34f, ancho * 0.76f, alto * 0.68f);
        }

        void DibujarEntidades(Graphics g, int ox, int oy, int W, int H)
        {
            _visibles.Clear();
            _mundo.ObjetosEnRect(ox - 120, oy - 20, ox + W + 120, oy + H + 230, _visibles);
            _visibles.Sort(PorY);
            PrepararSeres(ox, oy, W, H);

            // 1) todas las sombras primero (se mezclan entre sí y nunca tapan a los objetos)
            g.SmoothingMode = SmoothingMode.AntiAlias;
            for (int i = 0; i < _visibles.Count; i++)
            {
                Objeto o = _visibles[i];
                Sprite s = Arte.De(o);
                DibujarSombra(g, o.X - ox, o.Y - oy, s.SombraAncho, s.SombraAlto, s.SombraDx);
            }
            float jx = _pos.X - ox, jy = _pos.Y - oy;
            DibujarSombra(g, jx, jy - 2f, 54f, 16f, 5f);
            DibujarSombrasSeres(g, ox, oy);

            // 2) cuerpos ordenados por profundidad (Y); el caballero se intercala
            g.SmoothingMode = SmoothingMode.None;
            bool jugadorListo = false;
            int si = 0;
            for (int i = 0; i < _visibles.Count; i++)
            {
                Objeto o = _visibles[i];
                DibujarSeresHasta(g, ox, oy, o.Y, ref si, ref jugadorListo);
                Sprite s = Arte.De(o);
                g.DrawImage(s.Img, (int)Math.Round(o.X - ox - s.Ax), (int)Math.Round(o.Y - oy - s.Ay), s.Img.Width, s.Img.Height);
            }
            DibujarSeresHasta(g, ox, oy, float.MaxValue, ref si, ref jugadorListo);
        }

        void DibujarJugador(Graphics g, int ox, int oy)
        {
            int col = _moviendo ? SecuenciaCol[(int)_fase % 4] : 1;
            Cuadro c = _hoja.Cuadros[(int)_dir, col];
            int sx = (int)Math.Round(_pos.X - ox - c.AnclaX);
            int sy = (int)Math.Round(_pos.Y - oy - c.AnclaY);
            g.DrawImage(c.Img, sx, sy, c.Img.Width, c.Img.Height);

            // ondas al caminar por el agua poco profunda
            if (_mundo.TerrenoEn(_pos.X, _pos.Y) == Terreno.Somera)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                for (int k = 0; k < 2; k++)
                {
                    float ph = (_t * 1.4f + k * 0.5f) % 1f;
                    float rx = 14f + ph * 22f, ry = rx * 0.32f;
                    using (var pen = new Pen(Color.FromArgb((int)((1f - ph) * 150), 220, 240, 250), 2f))
                        g.DrawEllipse(pen, _pos.X - ox - rx, _pos.Y - oy - ry, rx * 2f, ry * 2f);
                }
                g.SmoothingMode = SmoothingMode.None;
            }
        }

        // Sombras de nubes que se desplazan por el mapa
        void DibujarNubes(Graphics g, int ox, int oy, int W, int H)
        {
            const int Cel = 760;
            float dx = _t * 16f, dy = _t * 5f;
            int i0 = (int)Math.Floor((ox - dx - 600f) / Cel), i1 = (int)Math.Floor((ox + W - dx + 200f) / Cel);
            int j0 = (int)Math.Floor((oy - dy - 600f) / Cel), j1 = (int)Math.Floor((oy + H - dy + 200f) / Cel);
            g.InterpolationMode = InterpolationMode.Bilinear;
            for (int j = j0; j <= j1; j++)
                for (int i = i0; i <= i1; i++)
                {
                    if (Ruido.Hash(i, j, 501) < 0.55f) continue;
                    float tam = 520f + Ruido.Hash(i, j, 502) * 380f;
                    float cx = (i + Ruido.Hash(i, j, 503)) * Cel + dx;
                    float cy = (j + Ruido.Hash(i, j, 504)) * Cel + dy;
                    g.DrawImage(_nube, (int)(cx - tam / 2f - ox), (int)(cy - tam * 0.35f - oy), (int)tam, (int)(tam * 0.7f));
                }
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
        }

        void DibujarParticulas(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color[] tonos = { Color.FromArgb(150, 168, 206, 96), Color.FromArgb(150, 236, 214, 120), Color.FromArgb(130, 120, 180, 90) };
            for (int i = 0; i < _particulas.Length; i++)
            {
                Particula p = _particulas[i];
                _brHoja.Color = tonos[p.Tono];
                g.FillEllipse(_brHoja, p.X, p.Y, p.Tam * 1.8f, p.Tam);
            }
            g.SmoothingMode = SmoothingMode.None;
        }

        // ==================================================================================
        //  AMBIENTE MÁGICO · FAUNA · ANOMALÍAS · DIÁLOGOS · MISIONES
        // ==================================================================================

        // Nombre que aparece sobre el mensaje del caballero.
        // TODO (base de datos): asignarlo desde fuera, p. ej.  new NivelUno { NombreJugador = "Fulano" }
        public string NombreJugador = string.IsNullOrEmpty(Sesion.Nombre) ? "Jugador" : Sesion.Nombre;

        const float DistLimite = 860f;          // más lejos que esto, los animales/anomalías salvajes desaparecen
        static readonly int[] AnomSeq = { 0, 1, 2, 3, 2, 1 };
        static readonly Comparison<Ser> PorYSer = delegate (Ser a, Ser b) { return a.Y.CompareTo(b.Y); };

        HojaAnimal[] _animales;
        HojaAnimal _hojaAnom;
        Bitmap _retrato;
        readonly List<Ser> _seres = new List<Ser>();
        readonly List<Ser> _seresVis = new List<Ser>();
        readonly List<Anomalia> _anomalias = new List<Anomalia>();
        readonly List<Mision> _misiones = new List<Mision>();
        readonly Random _rnd = new Random();
        float _tSpawn, _tAnom, _cdDialogo, _dAnim, _flash;
        bool _corriendo;

        Dialogo _dialogo;
        readonly Queue<Dialogo> _cola = new Queue<Dialogo>();
        Rectangle _rDialogo, _rRetrato, _rNombre;
        readonly Rectangle[] _btnOpc = new Rectangle[2];
        int _hoverOpc = -1;
        Keys _teclaDlg = Keys.None;

        Ser _objSer;            // animal que el jugador decidió domesticar
        Anomalia _objAnom;      // anomalía que el jugador decidió investigar
        int _accion;            // 0 = nada, 1 = domesticando, 2 = investigando
        float _accionT, _accionDur;
        Ser _accionSer;
        Anomalia _accionAnom;

        Luciernaga[] _luciernagas = new Luciernaga[46];
        Bitmap _brVerde, _brCian, _brVioleta, _brMagenta, _brCalido;
        readonly ImageAttributes _iaAlfa = new ImageAttributes();
        readonly ColorMatrix _cmAlfa = new ColorMatrix();
        Font _fDialogo, _fNombre, _fEmote, _fMision;
        StringFormat _sfTipo;
        SolidBrush _brNucleo;

        // ------------------------------------------------------------------ inicialización
        void CargarHojasMagia()
        {
            // Resources del proyecto: ZorroRojo, Ciervo, LoboGris, OsoPardo, AnomaliaBosque, RetratoCaballero,
            //                         ZorroMagico, CiervoLuminoso, Unicornio, Centauro, Minotauro
            _animales = new HojaAnimal[9];
            _animales[(int)Especie.Zorro] = HojaAnimal.Cargar(Properties.Resources.ZorroRojo, 1, 6, Fauna.D[(int)Especie.Zorro].Altura, false, false);
            _animales[(int)Especie.Ciervo] = HojaAnimal.Cargar(Properties.Resources.Ciervo, 2, 3, Fauna.D[(int)Especie.Ciervo].Altura, false, false);
            _animales[(int)Especie.Lobo] = HojaAnimal.Cargar(Properties.Resources.LoboGris, 2, 3, Fauna.D[(int)Especie.Lobo].Altura, false, false);
            _animales[(int)Especie.Oso] = HojaAnimal.Cargar(Properties.Resources.OsoPardo, 1, 6, Fauna.D[(int)Especie.Oso].Altura, false, false);

            // Criaturas nuevas: hojas de 2 filas x 4 columnas
            _animales[(int)Especie.ZorroMagico] = HojaAnimal.Cargar(Properties.Resources.ZorroMagico, 2, 4, Fauna.D[(int)Especie.ZorroMagico].Altura, false, false);
            _animales[(int)Especie.CiervoLuminoso] = HojaAnimal.Cargar(Properties.Resources.CiervoLuminoso, 2, 4, Fauna.D[(int)Especie.CiervoLuminoso].Altura, false, false);
            _animales[(int)Especie.Unicornio] = HojaAnimal.Cargar(Properties.Resources.Unicornio, 2, 4, Fauna.D[(int)Especie.Unicornio].Altura, false, false);
            _animales[(int)Especie.Centauro] = HojaAnimal.Cargar(Properties.Resources.Centauro, 2, 4, Fauna.D[(int)Especie.Centauro].Altura, false, false);
            _animales[(int)Especie.Minotauro] = HojaAnimal.Cargar(Properties.Resources.Minotauro, 2, 4, Fauna.D[(int)Especie.Minotauro].Altura, false, false);

            _hojaAnom = HojaAnimal.Cargar(Properties.Resources.AnomaliaBosque, 1, 4, 150f, true, true);
            _retrato = HojaAnimal.CrearRetrato(Properties.Resources.RetratoCaballero, 180);
        }

        void CrearRecursosMagicos()
        {
            _fDialogo = new Font("Segoe UI", 14f, FontStyle.Regular);
            _fNombre = new Font("Segoe UI Semibold", 12f, FontStyle.Regular);
            _fEmote = new Font("Segoe UI", 16f, FontStyle.Bold);
            _fMision = new Font("Segoe UI Semibold", 9.5f, FontStyle.Regular);
            _sfTipo = (StringFormat)StringFormat.GenericTypographic.Clone();
            _sfTipo.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;
            _brNucleo = new SolidBrush(Color.White);

            _brVerde = CrearBrillo(64, 190, 255, 110, 255);
            _brCian = CrearBrillo(96, 90, 220, 255, 190);
            _brVioleta = CrearBrillo(96, 150, 90, 255, 190);
            _brMagenta = CrearBrillo(96, 255, 70, 190, 200);
            _brCalido = CrearBrillo(128, 255, 206, 140, 150);

            var rnd = new Random(23);
            int W = Math.Max(1, ClientSize.Width), H = Math.Max(1, ClientSize.Height);
            for (int i = 0; i < _luciernagas.Length; i++)
            {
                Luciernaga f = new Luciernaga();
                f.X = _camX + ((float)rnd.NextDouble() - 0.5f) * (W + 240f);
                f.Y = _camY + ((float)rnd.NextDouble() - 0.5f) * (H + 240f);
                f.Fase = (float)rnd.NextDouble() * 6.28f;
                f.Rumbo = (float)rnd.NextDouble() * 6.28f;
                f.Vel = 8f + (float)rnd.NextDouble() * 14f;
                f.Tam = (float)rnd.NextDouble();
                f.Tono = rnd.Next(5) == 0 ? 2 : 0;     // la mayoría amarillo-verdosas, alguna azulada y mágica
                _luciernagas[i] = f;
            }
        }

        static Bitmap CrearBrillo(int n, int cr, int cg, int cb, int alfaMax)
        {
            var bmp = new Bitmap(n, n, PixelFormat.Format32bppPArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, n, n), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            int[] fila = new int[n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float t = Lim(1f - d, 0f, 1f);
                    float t2 = t * t;
                    float a = alfaMax * (0.7f * t2 + 0.3f * t2 * t2 * t2);
                    int A = (int)Lim(a, 0f, 255f);
                    int pr = (int)(cr * A / 255f), pg = (int)(cg * A / 255f), pb = (int)(cb * A / 255f);
                    fila[x] = unchecked((A << 24) | (pr << 16) | (pg << 8) | pb);
                }
                Marshal.Copy(fila, 0, IntPtr.Add(bd.Scan0, y * bd.Stride), n);
            }
            bmp.UnlockBits(bd);
            return bmp;
        }

        void LiberarMagia()
        {
            if (_animales != null) foreach (HojaAnimal h in _animales) if (h != null) h.Liberar();
            if (_hojaAnom != null) _hojaAnom.Liberar();
            if (_retrato != null) _retrato.Dispose();
            Bitmap[] brs = { _brVerde, _brCian, _brVioleta, _brMagenta, _brCalido };
            foreach (Bitmap b in brs) if (b != null) b.Dispose();
            _iaAlfa.Dispose();
            if (_fDialogo != null) _fDialogo.Dispose();
            if (_fNombre != null) _fNombre.Dispose();
            if (_fEmote != null) _fEmote.Dispose();
            if (_fMision != null) _fMision.Dispose();
            if (_sfTipo != null) _sfTipo.Dispose();
            if (_brNucleo != null) _brNucleo.Dispose();
        }

        // ------------------------------------------------------------------ inicio de la aventura
        void IniciarAventura()
        {
            _misiones.Clear();
            _misiones.Add(new Mision { Clave = "zorro", Texto = "Domestica un Zorro Rojo", Meta = 1, Xp = 40f });
            _misiones.Add(new Mision { Clave = "ciervo", Texto = "Domestica un Ciervo", Meta = 1, Xp = 40f });
            _misiones.Add(new Mision { Clave = "lobo", Texto = "Domestica un Lobo Gris", Meta = 1, Xp = 100f });
            _misiones.Add(new Mision { Clave = "oso", Texto = "Avista un Oso Pardo (¡de lejos!)", Meta = 1, Xp = 30f });
            _misiones.Add(new Mision { Clave = "anom", Texto = "Investiga 3 anomalías", Meta = 3, Xp = 90f });
            // misiones de las criaturas nuevas
            _misiones.Add(new Mision { Clave = "zorromagico", Texto = "Domestica un Zorro Mágico", Meta = 1, Xp = 120f });
            _misiones.Add(new Mision { Clave = "ciervoluminoso", Texto = "Domestica un Ciervo Luminoso", Meta = 1, Xp = 140f });
            _misiones.Add(new Mision { Clave = "unicornio", Texto = "Avista un Unicornio (¡no se deja!)", Meta = 1, Xp = 60f });
            _misiones.Add(new Mision { Clave = "centauro", Texto = "Saluda a un Centauro con respeto", Meta = 1, Xp = 80f });
            _misiones.Add(new Mision { Clave = "minotauro", Texto = "Avista un Minotauro (¡huye!)", Meta = 1, Xp = 50f });

            _tSpawn = 5f; _tAnom = 11f; _cdDialogo = 0f;

            var intro = new Dialogo(new string[] {
                "¡Salve, viajero! Soy el Caballero del Manto Azul, guardián de este bosque encantado.",
                "Cuando cae la tarde, el bosque despierta. Criaturas salvajes merodean entre los árboles y extrañas anomalías brotan de la tierra.",
                "Tu aventura es simple: gana experiencia cumpliendo misiones. Domestica a un zorro, a un ciervo y a un lobo... y avista al temible oso pardo, ¡pero desde lejos!",
                "Dicen que también habitan aquí criaturas de leyenda: zorros mágicos, ciervos luminosos, un unicornio esquivo, un orgulloso centauro... y un minotauro que es mejor no despertar.",
                "Investiga también las anomalías, aunque con cuidado: cada una guarda un riesgo distinto. Tus misiones están en el panel de la derecha. ¡Que las luciérnagas guíen tu camino!"
            }, null);
            Hablar(intro);
        }

        void ReiniciarAventura()
        {
            _seres.Clear(); _seresVis.Clear(); _anomalias.Clear(); _cola.Clear();
            _dialogo = null; _objSer = null; _objAnom = null;
            _accion = 0; _accionSer = null; _accionAnom = null;
            _flash = 0f; _hoverOpc = -1;
            IniciarAventura();
        }

        // ------------------------------------------------------------------ diálogos
        void Hablar(Dialogo d)
        {
            if (_dialogo != null) { _cola.Enqueue(d); return; }
            _dialogo = d; _dAnim = 0f; _hoverOpc = -1;
            _teclas.Clear();
        }

        void CerrarDialogo(int opcion)
        {
            Dialogo d = _dialogo;
            _dialogo = null; _hoverOpc = -1;
            _cdDialogo = 3f;
            if (d != null && d.Fin != null) d.Fin(opcion);
            if (_dialogo == null && _cola.Count > 0) { _dialogo = _cola.Dequeue(); _dAnim = 0f; }
        }

        void AvanzarDialogo()
        {
            Dialogo d = _dialogo;
            if (d == null) return;
            if (!d.Completa) { d.Chars = d.Actual.Length; return; }
            if (!d.Ultima) { d.Pag++; d.Chars = 0f; d.LineasPag = -1; return; }
            if (d.Opciones == null) CerrarDialogo(-1);
            else if (d.Opciones.Length == 1) CerrarDialogo(0);
        }

        void ElegirOpcion(int i)
        {
            Dialogo d = _dialogo;
            if (d == null || !d.VerOpciones || i < 0 || i >= d.Opciones.Length) return;
            CerrarDialogo(i);
        }

        void TeclaDialogo(Keys k)
        {
            Dialogo d = _dialogo;
            if (d == null) return;
            if (k == Keys.Space || k == Keys.Return) AvanzarDialogo();
            else if (k == Keys.E) { if (d.VerOpciones) ElegirOpcion(0); else AvanzarDialogo(); }
            else if (k == Keys.Q || k == Keys.D2) ElegirOpcion(1);
            else if (k == Keys.D1) ElegirOpcion(0);
        }

        void ActualizarHoverDialogo(Point p)
        {
            _hoverOpc = -1;
            if (_dialogo == null || !_dialogo.VerOpciones || _pausa) return;
            for (int i = 0; i < _dialogo.Opciones.Length && i < _btnOpc.Length; i++)
                if (_btnOpc[i].Contains(p)) _hoverOpc = i;
        }

        void ClicDialogo(Point p)
        {
            Dialogo d = _dialogo;
            if (d == null) return;
            if (d.VerOpciones)
            {
                for (int i = 0; i < d.Opciones.Length && i < _btnOpc.Length; i++)
                    if (_btnOpc[i].Contains(p)) { ElegirOpcion(i); return; }
                if (d.Opciones.Length == 1 && _rDialogo.Contains(p)) AvanzarDialogo();
                return;
            }
            if (_rDialogo.Contains(p) || _rRetrato.Contains(p)) AvanzarDialogo();
        }

        void DistribuirDialogo(int W, int H)
        {
            int tw = Math.Min(W - 80, 1080);
            int x0 = (W - tw) / 2;
            const int bh = 176;
            int baseY = H - 36;
            _rRetrato = new Rectangle(x0, baseY - 196, 196, 196);
            _rDialogo = new Rectangle(x0 + 212, baseY - bh, tw - 212, bh);
            _rNombre = new Rectangle(_rDialogo.X + 22, _rDialogo.Y - 22, 250, 40);

            int n = (_dialogo != null && _dialogo.Opciones != null) ? Math.Min(_dialogo.Opciones.Length, _btnOpc.Length) : 0;
            const int bw = 230, bhh = 36, gap = 14;
            int total = n * bw + Math.Max(0, n - 1) * gap;
            int start = _rDialogo.Right - 22 - total;
            for (int i = 0; i < n; i++)
                _btnOpc[i] = new Rectangle(start + i * (bw + gap), _rDialogo.Bottom - 18 - bhh, bw, bhh);
        }

        List<string> Envolver(Graphics g, string texto, Font f, float ancho)
        {
            var res = new List<string>();
            string[] pal = texto.Split(' ');
            string linea = "";
            for (int i = 0; i < pal.Length; i++)
            {
                string prueba = linea.Length == 0 ? pal[i] : linea + " " + pal[i];
                if (linea.Length > 0 && g.MeasureString(prueba, f, 100000, _sfTipo).Width > ancho)
                {
                    res.Add(linea);
                    linea = pal[i];
                }
                else linea = prueba;
            }
            if (linea.Length > 0) res.Add(linea);
            return res;
        }

        void DibujarDialogo(Graphics g, int W, int H)
        {
            Dialogo d = _dialogo;
            if (d == null) return;
            float e = Lim(_dAnim, 0f, 1f);
            g.TranslateTransform(0f, (1f - e) * (1f - e) * 70f);

            // ---- marco del retrato
            using (GraphicsPath pr = Redondo(_rRetrato, 16f))
            using (var gr = new LinearGradientBrush(_rRetrato, Color.FromArgb(245, 44, 52, 92), Color.FromArgb(245, 14, 16, 34), 90f))
            using (var pen = new Pen(Color.FromArgb(240, 196, 156, 70), 3f))
            {
                g.FillPath(gr, pr);
                g.SetClip(pr, CombineMode.Replace);
                if (_retrato != null)
                    g.DrawImage(_retrato, _rRetrato.X + 8, _rRetrato.Y + 8, _rRetrato.Width - 16, _rRetrato.Height - 16);
                g.ResetClip();
                g.DrawPath(pen, pr);
            }

            // ---- caja del mensaje
            using (GraphicsPath pb = Redondo(_rDialogo, 16f))
            using (var gr = new LinearGradientBrush(_rDialogo, Color.FromArgb(240, 28, 32, 58), Color.FromArgb(240, 12, 14, 28), 90f))
            using (var pen = new Pen(Color.FromArgb(240, 196, 156, 70), 3f))
            {
                g.FillPath(gr, pb);
                g.DrawPath(pen, pb);
            }

            // ---- pestaña con el nombre del jugador
            using (GraphicsPath pn = Redondo(_rNombre, 10f))
            using (var gr = new LinearGradientBrush(_rNombre, Color.FromArgb(255, 62, 52, 104), Color.FromArgb(255, 26, 28, 56), 90f))
            using (var pen = new Pen(Color.FromArgb(255, 226, 180, 80), 2.5f))
            {
                g.FillPath(gr, pn);
                g.DrawPath(pen, pn);
            }
            using (var bt = new SolidBrush(Color.FromArgb(255, 232, 130)))
                g.DrawString(NombreJugador ?? "", _fNombre, bt, _rNombre, _sfCentro);

            // ---- texto (máquina de escribir)
            if (d.LineasPag != d.Pag)
            {
                d.Lineas = Envolver(g, d.Actual, _fDialogo, _rDialogo.Width - 60);
                d.LineasPag = d.Pag;
            }
            float ty = _rDialogo.Y + 30f;
            int restante = (int)d.Chars;
            using (var bt = new SolidBrush(Color.FromArgb(244, 244, 238, 224)))
                for (int i = 0; i < d.Lineas.Count && restante > 0; i++)
                {
                    string ln = d.Lineas[i];
                    int n = Math.Min(ln.Length, restante);
                    g.DrawString(ln.Substring(0, n), _fDialogo, bt, _rDialogo.X + 28f, ty, _sfTipo);
                    restante -= ln.Length + 1;
                    ty += 27f;
                }

            // ---- opciones o indicador de "continuar"
            if (d.VerOpciones)
            {
                string[] teclas = { "[E] ", "[Q] " };
                for (int i = 0; i < d.Opciones.Length && i < _btnOpc.Length; i++)
                    DibujarBoton(g, _btnOpc[i], teclas[i] + d.Opciones[i], _hoverOpc == i, false);
            }
            else if (d.Completa)
            {
                float pulso = ((float)Math.Sin(_t * 6f) + 1f) * 0.5f;
                string cont = "Espacio";
                SizeF sc = g.MeasureString(cont, _fPeq);
                float cx = _rDialogo.Right - 30f - sc.Width, cy = _rDialogo.Bottom - 30f;
                using (var bt = new SolidBrush(Color.FromArgb(120 + (int)(pulso * 120f), 255, 226, 130)))
                {
                    g.DrawString(cont, _fPeq, bt, cx - 4f, cy - 1f);
                    PointF[] tri = { new PointF(cx + sc.Width + 2f, cy + 3f), new PointF(cx + sc.Width + 14f, cy + 3f), new PointF(cx + sc.Width + 8f, cy + 12f) };
                    g.FillPolygon(bt, tri);
                }
            }
            g.ResetTransform();
        }

        // ------------------------------------------------------------------ misiones, experiencia y daño
        void Recompensa(float xp)
        {
            _xp += xp;       // el bucle de Actualizar se encarga de subir de nivel
            _aviso = "+" + (int)xp + " XP";
            _avisoT = 2f;
        }

        void Progreso(string clave, int n)
        {
            for (int i = 0; i < _misiones.Count; i++)
            {
                Mision m = _misiones[i];
                if (m.Clave != clave || m.Hecha) continue;
                m.Prog += n;
                if (m.Prog >= m.Meta)
                {
                    m.Prog = m.Meta;
                    m.Hecha = true;
                    _xp += m.Xp;
                    _aviso = "¡Misión cumplida! +" + (int)m.Xp + " XP";
                    _avisoT = 3.2f;
                }
            }
        }

        void Dano(float cant, string aviso)
        {
            _energia = Math.Max(0f, _energia - cant);
            if (_energia <= 0f) _cansado = true;
            _flash = 1f;
            if (!string.IsNullOrEmpty(aviso)) { _aviso = aviso; _avisoT = 2.5f; }
            CancelarAccion();
        }

        void Empujar(float dx, float dy, float dist, float cuanto)
        {
            if (dist < 1f) return;
            float nx = _pos.X + dx / dist * cuanto, ny = _pos.Y + dy / dist * cuanto;
            if (!_mundo.Bloqueado(nx, ny, RadioJugador)) { _pos.X = nx; _pos.Y = ny; }
        }

        float Dist(float x, float y)
        {
            float dx = x - _pos.X, dy = y - _pos.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // ------------------------------------------------------------------ actualización general
        void ActualizarMagia(float dt)
        {
            int W = Math.Max(1, ClientSize.Width), H = Math.Max(1, ClientSize.Height);
            bool hablando = _dialogo != null;

            if (hablando)
            {
                _dAnim = Math.Min(1f, _dAnim + dt * 4f);
                Dialogo d = _dialogo;
                d.Chars = Math.Min(d.Actual.Length, d.Chars + 55f * dt);
            }
            if (_cdDialogo > 0f) _cdDialogo -= dt;
            if (_flash > 0f) _flash -= dt * 1.6f;

            ActualizarLuciernagas(dt, W, H);

            if (_accion != 0 && !hablando)
            {
                _accionT += dt;
                if (_accionT >= _accionDur) FinalizarAccion();
            }

            ActualizarSpawns(dt, hablando);
            ActualizarSeres(dt, hablando);
            ActualizarAnomalias(dt, hablando);
            ActualizarObjetivo();
        }

        void ActualizarLuciernagas(float dt, int W, int H)
        {
            float mw = W / 2f + 120f, mh = H / 2f + 120f;
            for (int i = 0; i < _luciernagas.Length; i++)
            {
                Luciernaga f = _luciernagas[i];
                f.Fase += dt * (1.6f + f.Tam * 1.4f);
                f.Rumbo += (float)Math.Sin(_t * 0.7f + i * 1.7f) * 1.8f * dt;
                f.X += (float)Math.Cos(f.Rumbo) * f.Vel * dt;
                f.Y += (float)Math.Sin(f.Rumbo) * f.Vel * 0.7f * dt;
                if (f.X < _camX - mw) f.X += 2f * mw; else if (f.X > _camX + mw) f.X -= 2f * mw;
                if (f.Y < _camY - mh) f.Y += 2f * mh; else if (f.Y > _camY + mh) f.Y -= 2f * mh;
                _luciernagas[i] = f;
            }
        }

        // ------------------------------------------------------------------ aparición aleatoria
        void ActualizarSpawns(float dt, bool hablando)
        {
            if (hablando) return;
            _tSpawn -= dt;
            if (_tSpawn <= 0f)
            {
                _tSpawn = 7f + (float)_rnd.NextDouble() * 9f;
                int salvajes = 0; bool hayOso = false;   // "hayOso" = ya hay una bestia grande (oso o minotauro)
                for (int i = 0; i < _seres.Count; i++)
                    if (_seres[i].Est != Estado.Domesticado)
                    {
                        salvajes++;
                        if (_seres[i].Esp == Especie.Oso || _seres[i].Esp == Especie.Minotauro) hayOso = true;
                    }
                if (salvajes < 4) GenerarSer(hayOso);
            }
            _tAnom -= dt;
            if (_tAnom <= 0f)
            {
                _tAnom = 14f + (float)_rnd.NextDouble() * 14f;
                if (_anomalias.Count < 2) GenerarAnomalia();
            }
        }

        bool BuscarPuntoLibre(float dmin, float dmax, float radio, out PointF res)
        {
            for (int i = 0; i < 14; i++)
            {
                double a = _rnd.NextDouble() * Math.PI * 2.0;
                float d = dmin + (float)_rnd.NextDouble() * (dmax - dmin);
                float x = _pos.X + (float)Math.Cos(a) * d, y = _pos.Y + (float)Math.Sin(a) * d;
                if (_mundo.TerrenoEn(x, y) != Terreno.Pasto) continue;
                if (_mundo.Bloqueado(x, y, radio)) continue;
                bool cerca = false;
                for (int k = 0; k < _seres.Count && !cerca; k++)
                {
                    float dx = _seres[k].X - x, dy = _seres[k].Y - y;
                    if (dx * dx + dy * dy < 90f * 90f) cerca = true;
                }
                for (int k = 0; k < _anomalias.Count && !cerca; k++)
                {
                    float dx = _anomalias[k].X - x, dy = _anomalias[k].Y - y;
                    if (dx * dx + dy * dy < 140f * 140f) cerca = true;
                }
                if (cerca) continue;
                res = new PointF(x, y);
                return true;
            }
            res = PointF.Empty;
            return false;
        }

        void GenerarSer(bool hayOso)
        {
            // Probabilidades (%): zorro 20, ciervo 20, lobo 16, oso 8,
            //                     zorro mágico 10, ciervo luminoso 8, unicornio 6, centauro 6, minotauro 6
            float r = (float)_rnd.NextDouble() * 100f;
            Especie e;
            if (r < 20f) e = Especie.Zorro;
            else if (r < 40f) e = Especie.Ciervo;
            else if (r < 56f) e = Especie.Lobo;
            else if (r < 64f) e = Especie.Oso;
            else if (r < 74f) e = Especie.ZorroMagico;
            else if (r < 82f) e = Especie.CiervoLuminoso;
            else if (r < 88f) e = Especie.Unicornio;
            else if (r < 94f) e = Especie.Centauro;
            else e = Especie.Minotauro;
            if ((e == Especie.Oso || e == Especie.Minotauro) && hayOso) e = Especie.Ciervo;
            PointF p;
            if (!BuscarPuntoLibre(430f, 760f, 26f, out p)) return;
            var s = new Ser();
            s.Esp = e; s.X = p.X; s.Y = p.Y; s.Alfa = 0f;
            s.Der = _rnd.Next(2) == 0;
            s.AnimT = (float)_rnd.NextDouble() * 6f;
            s.EsperaT = 1f + (float)_rnd.NextDouble() * 2f;
            _seres.Add(s);
        }

        void GenerarAnomalia()
        {
            PointF p;
            if (!BuscarPuntoLibre(380f, 700f, 40f, out p)) return;
            float r = (float)_rnd.NextDouble();
            var a = new Anomalia();
            a.X = p.X; a.Y = p.Y; a.Alfa = 0f;
            a.Riesgo = r < 0.45f ? 0 : (r < 0.80f ? 1 : 2);
            a.Fase = (float)_rnd.NextDouble() * 5f;
            _anomalias.Add(a);
        }

        // ------------------------------------------------------------------ comportamiento de los animales
        bool MoverSer(Ser s, float vx, float vy, float dt)
        {
            float r = Fauna.D[(int)s.Esp].Radio;
            float nx = s.X + vx * dt, ny = s.Y + vy * dt;
            bool mov = false;
            if (!_mundo.Bloqueado(nx, s.Y, r)) { s.X = nx; mov = true; }
            if (!_mundo.Bloqueado(s.X, ny, r)) { s.Y = ny; mov = true; }
            if (Math.Abs(vx) > 1f) s.Der = vx > 0f;
            return mov;
        }

        void Asustar(Ser s)
        {
            s.Est = Estado.Huir; s.T = 0f; s.Objetivo = false; s.TieneDest = false;
            s.Emote = 1; s.EmoteT = 1.2f;
            if (_objSer == s) _objSer = null;
        }

        void Pasear(Ser s, float dt)
        {
            DatosEspecie d = Fauna.D[(int)s.Esp];
            if (!s.TieneDest)
            {
                s.EsperaT -= dt;
                if (s.EsperaT <= 0f)
                {
                    double a = _rnd.NextDouble() * Math.PI * 2.0;
                    float r = 50f + (float)_rnd.NextDouble() * 120f;
                    s.DestX = s.X + (float)Math.Cos(a) * r;
                    s.DestY = s.Y + (float)Math.Sin(a) * r;
                    s.TieneDest = true; s.T = 0f;
                }
                return;
            }
            float dx = s.DestX - s.X, dy = s.DestY - s.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 8f || s.T > 6f)
            {
                s.TieneDest = false;
                s.EsperaT = 1.5f + (float)_rnd.NextDouble() * 3.5f;
                return;
            }
            bool mov = MoverSer(s, dx / dist * d.VelCam, dy / dist * d.VelCam, dt);
            s.Moviendo = mov;
            if (!mov) { s.TieneDest = false; s.EsperaT = 0.8f; }
        }

        void TeleportarCerca(Ser s)
        {
            float r0 = Fauna.D[(int)s.Esp].Radio;
            for (int i = 0; i < 12; i++)
            {
                double a = _rnd.NextDouble() * Math.PI * 2.0;
                float r = 70f + (float)_rnd.NextDouble() * 50f;
                float x = _pos.X + (float)Math.Cos(a) * r, y = _pos.Y + (float)Math.Sin(a) * r;
                if (!_mundo.Bloqueado(x, y, r0)) { s.X = x; s.Y = y; return; }
            }
            s.X = _pos.X; s.Y = _pos.Y + 20f;
        }

        int ContarMascotas()
        {
            int n = 0;
            for (int i = 0; i < _seres.Count; i++) if (_seres[i].Est == Estado.Domesticado) n++;
            return n;
        }

        void ActualizarSeres(float dt, bool congelar)
        {
            for (int i = _seres.Count - 1; i >= 0; i--)
            {
                Ser s = _seres[i];
                DatosEspecie d = Fauna.D[(int)s.Esp];
                s.AnimT += dt;
                if (s.Alfa < 1f) s.Alfa = Math.Min(1f, s.Alfa + dt * 1.2f);
                if (s.EmoteT > 0f) s.EmoteT -= dt;
                if (s.AlertaT > 0f) s.AlertaT -= dt;
                if (s.Enfriar > 0f) s.Enfriar -= dt;

                float dx = _pos.X - s.X, dy = _pos.Y - s.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (s.Est != Estado.Domesticado && s.Est != Estado.Domesticando && !s.Objetivo && dist > DistLimite)
                {
                    _seres.RemoveAt(i);
                    continue;
                }

                s.Moviendo = false;
                if (congelar) continue;
                s.T += dt;

                switch (s.Est)
                {
                    case Estado.Vagar:
                        {
                            if (s.Objetivo) { s.TieneDest = false; s.Der = dx > 0f; break; }   // espera quieto, mirando al caballero
                            if (s.Esp == Especie.Zorro || s.Esp == Especie.Ciervo)
                            {
                                if (dist < 105f || (dist < 220f && _corriendo)) { Asustar(s); break; }
                            }
                            else if (s.Esp == Especie.ZorroMagico || s.Esp == Especie.CiervoLuminoso)
                            {
                                // criaturas mágicas domesticables: más recelosas (el ciervo luminoso, muchísimo)
                                float lim = s.Esp == Especie.CiervoLuminoso ? 160f : 120f;
                                if (dist < lim || (dist < lim + 130f && _corriendo)) { Asustar(s); break; }
                            }
                            else if (s.Esp == Especie.Unicornio)
                            {
                                // el unicornio NO se deja acercar: huye desde muy lejos
                                if (dist < 320f || (dist < 460f && _corriendo)) { Asustar(s); break; }
                            }
                            else if (s.Esp == Especie.Centauro)
                            {
                                // el centauro es pacífico... salvo que lo provoques corriendo hacia él
                                if (s.Aviso && s.Enfriar <= 0f && _corriendo && dist < 190f)
                                {
                                    s.Hostil = true;
                                    s.Est = Estado.Amenaza; s.T = 0f; s.Emote = 1; s.EmoteT = 1.4f;
                                    break;
                                }
                            }
                            else if (s.Esp == Especie.Lobo)
                            {
                                if (s.Aviso && s.Enfriar <= 0f && dist < 150f)
                                {
                                    s.Est = Estado.Amenaza; s.T = 0f; s.Emote = 1; s.EmoteT = 1.2f;
                                    break;
                                }
                            }
                            else
                            {
                                // oso pardo y minotauro
                                if (s.Aviso && s.Enfriar <= 0f && dist < 240f)
                                {
                                    s.Est = Estado.Amenaza; s.T = 0f; s.Emote = 1; s.EmoteT = 1.5f;
                                    break;
                                }
                            }
                            Pasear(s, dt);
                            break;
                        }

                    case Estado.Amenaza:
                        {
                            s.Der = dx > 0f;
                            if (s.Esp == Especie.Oso || s.Esp == Especie.Minotauro)
                            {
                                if (dist > 380f) { s.Est = Estado.Vagar; s.Enfriar = 2f; }
                                else if (s.T > 1.1f) { s.Est = Estado.Cargar; s.T = 0f; }
                            }
                            else
                            {
                                if (dist > 220f) { s.Est = Estado.Vagar; s.Enfriar = 2f; s.Hostil = false; }
                                else if (s.Hostil || (s.T > 3f && dist < 120f)) { s.Est = Estado.Cargar; s.T = 0f; }
                            }
                            break;
                        }

                    case Estado.Cargar:
                        {
                            if (dist > 1f) MoverSer(s, dx / dist * d.VelAtaque, dy / dist * d.VelAtaque, dt);
                            s.Moviendo = true;
                            if (dist < d.Radio + RadioJugador + 12f)
                            {
                                if (s.Esp == Especie.Oso)
                                {
                                    Dano(40f, "¡El oso pardo te golpeó!  -40");
                                    Empujar(dx, dy, dist, 60f);
                                    s.Est = Estado.Vagar; s.Enfriar = 5f; s.TieneDest = false; s.EsperaT = 2f;
                                }
                                else if (s.Esp == Especie.Minotauro)
                                {
                                    Dano(55f, "¡El minotauro te embistió!  -55");
                                    Empujar(dx, dy, dist, 70f);
                                    s.Est = Estado.Vagar; s.Enfriar = 5f; s.TieneDest = false; s.EsperaT = 2f;
                                }
                                else if (s.Esp == Especie.Centauro)
                                {
                                    Dano(35f, "¡El centauro te embistió!  -35");
                                    Empujar(dx, dy, dist, 50f);
                                    s.Hostil = false;
                                    Asustar(s);
                                }
                                else
                                {
                                    Dano(30f, "¡El lobo te mordió!  -30");
                                    Empujar(dx, dy, dist, 40f);
                                    s.Hostil = false;
                                    Asustar(s);
                                }
                                break;
                            }
                            if (s.T > 6f || dist > 560f) { s.Est = Estado.Vagar; s.Enfriar = 4f; s.Hostil = false; }
                            break;
                        }

                    case Estado.Huir:
                        {
                            float ax = -dx, ay = -dy;
                            float len = (float)Math.Sqrt(ax * ax + ay * ay);
                            if (len < 1f) { ax = 1f; ay = 0f; len = 1f; }
                            ax /= len; ay /= len;
                            if (!MoverSer(s, ax * d.VelHuir, ay * d.VelHuir, dt))
                                MoverSer(s, -ay * d.VelHuir, ax * d.VelHuir, dt);    // rodea el obstáculo
                            s.Moviendo = true;
                            if (s.T > 2.6f || dist > 480f) { s.Est = Estado.Vagar; s.TieneDest = false; s.EsperaT = 1.5f; }
                            break;
                        }

                    case Estado.Domesticando:
                        break;

                    case Estado.Domesticado:
                        {
                            float sep = 80f + s.Orden * 26f;
                            if (dist > 700f) TeleportarCerca(s);
                            else if (dist > sep)
                            {
                                float vel = Math.Min(250f, 70f + (dist - sep) * 1.6f);
                                MoverSer(s, dx / dist * vel, dy / dist * vel, dt);
                                s.Moviendo = true;
                            }
                            else s.Der = dx > 0f;
                            if (s.EmoteT <= 0f && (s.AnimT % 9f) < dt) { s.Emote = 3; s.EmoteT = 1.6f; }
                            break;
                        }
                }
            }

            // ¿algún animal nuevo ha sido avistado?
            if (congelar || _cdDialogo > 0f || _accion != 0 || _dialogo != null) return;
            for (int i = 0; i < _seres.Count; i++)
            {
                Ser s = _seres[i];
                if (s.Aviso || s.Est != Estado.Vagar) continue;
                if (Dist(s.X, s.Y) < Fauna.D[(int)s.Esp].Deteccion) { DialogoAvistamiento(s); return; }
            }
        }

        void ActualizarAnomalias(float dt, bool congelar)
        {
            for (int i = _anomalias.Count - 1; i >= 0; i--)
            {
                Anomalia a = _anomalias[i];
                a.Fase += dt;
                if (a.Consumida)
                {
                    a.Alfa -= dt * 0.7f;
                    if (a.Alfa <= 0f) _anomalias.RemoveAt(i);
                    continue;
                }
                if (a.Alfa < 1f) a.Alfa = Math.Min(1f, a.Alfa + dt * 0.8f);
                float dist = Dist(a.X, a.Y);
                if (dist > DistLimite && !a.Objetivo) { _anomalias.RemoveAt(i); continue; }
                if (!congelar && !a.Aviso && _cdDialogo <= 0f && _accion == 0 && _dialogo == null && dist < 270f)
                    DialogoAnomalia(a);
            }
        }

        // ------------------------------------------------------------------ objetivo elegido por el jugador
        void ActualizarObjetivo()
        {
            if (_dialogo != null || _accion != 0) return;

            if (_objSer != null)
            {
                Ser s = _objSer;
                float dist = Dist(s.X, s.Y);
                if (!_seres.Contains(s) || !s.Objetivo || dist > 760f)
                {
                    s.Objetivo = false; _objSer = null;
                    return;
                }
                if (_corriendo && dist < 230f)
                {
                    string nombre = Fauna.D[(int)s.Esp].Nombre;
                    Asustar(s);
                    Hablar(new Dialogo("¡Corrimos hacia el " + nombre + " y se asustó! Hay que acercarse despacio, sin correr."));
                    return;
                }
                if (dist < 82f) IniciarAccion(1, s, null, 3f);
            }
            else if (_objAnom != null)
            {
                Anomalia a = _objAnom;
                float dist = Dist(a.X, a.Y);
                if (a.Consumida || dist > 900f) { a.Objetivo = false; _objAnom = null; return; }
                if (dist < 90f) IniciarAccion(2, null, a, 2.6f);
            }
        }

        void IniciarAccion(int tipo, Ser s, Anomalia a, float dur)
        {
            _accion = tipo; _accionT = 0f; _accionDur = dur; _accionSer = s; _accionAnom = a;
            float tx = s != null ? s.X : a.X, ty = s != null ? s.Y : a.Y;
            float dx = tx - _pos.X, dy = ty - _pos.Y;
            if (Math.Abs(dx) > Math.Abs(dy)) _dir = dx > 0f ? Dir.Derecha : Dir.Izquierda;
            else _dir = dy > 0f ? Dir.Abajo : Dir.Arriba;
            if (s != null) { s.Est = Estado.Domesticando; s.T = 0f; s.TieneDest = false; s.Der = dx < 0f; }
        }

        void CancelarAccion()
        {
            if (_accion == 0) return;
            if (_accionSer != null && _accionSer.Est == Estado.Domesticando)
            {
                _accionSer.Est = Estado.Vagar; _accionSer.Objetivo = false;
                if (_objSer == _accionSer) _objSer = null;
            }
            _accion = 0; _accionSer = null; _accionAnom = null;
        }

        void FinalizarAccion()
        {
            int tipo = _accion;
            Ser s = _accionSer; Anomalia a = _accionAnom;
            _accion = 0; _accionSer = null; _accionAnom = null;

            if (tipo == 1 && s != null)
            {
                DatosEspecie d = Fauna.D[(int)s.Esp];
                _objSer = null; s.Objetivo = false;
                if (_rnd.NextDouble() < d.ProbDom)
                {
                    s.Orden = ContarMascotas();
                    s.Est = Estado.Domesticado; s.Emote = 3; s.EmoteT = 3f;
                    Recompensa(d.XpDom);
                    Progreso(d.Clave, 1);
                    Hablar(new Dialogo("¡Lo logramos! El " + d.Nombre + " confía en nosotros y nos seguirá a partir de ahora. (+" + (int)d.XpDom + " XP)"));
                }
                else if (s.Esp == Especie.Lobo)
                {
                    s.Hostil = true; s.Est = Estado.Cargar; s.T = 0f; s.Emote = 1; s.EmoteT = 1.5f;
                    Hablar(new Dialogo("¡Cuidado! El lobo gris no confió en nosotros y se abalanza... ¡Corre con Shift o prepárate para el golpe!"));
                }
                else if (s.Esp == Especie.ZorroMagico)
                {
                    Dano(15f, "");
                    Asustar(s);
                    Hablar(new Dialogo("¡El zorro mágico nos lanzó un hechizo de llamas violetas y huyó entre risas! (-15 energía) Quizá con otro ejemplar tengamos más suerte."));
                }
                else
                {
                    Asustar(s);
                    Hablar(new Dialogo("Se asustó y huyó... Quizá con otro ejemplar tengamos más suerte."));
                }
            }
            else if (tipo == 2 && a != null)
            {
                _objAnom = null; a.Objetivo = false; a.Consumida = true;
                float xp; string msg;
                if (a.Riesgo == 0)
                {
                    xp = 30f;
                    msg = "Es una grieta de energía antigua y apacible. Absorbimos un poco de su luz... (+30 XP)";
                }
                else if (a.Riesgo == 1)
                {
                    xp = 55f;
                    if (_rnd.NextDouble() < 0.4)
                    {
                        Dano(25f, "");
                        msg = "¡Una descarga! Nos sacudió, pero aprendimos mucho de ella. (-25 energía, +55 XP)";
                    }
                    else msg = "Su energía violeta se calmó al estudiarla. Hemos recopilado valiosos datos. (+55 XP)";
                }
                else
                {
                    xp = 110f;
                    if (_rnd.NextDouble() < 0.6)
                    {
                        Dano(60f, "");
                        msg = "¡Una explosión arcana! Casi no lo contamos... (-60 energía, +110 XP)";
                    }
                    else msg = "¡Increíble, salimos ilesos! Esa anomalía era un portal inestable. (+110 XP)";
                }
                Recompensa(xp);
                Progreso("anom", 1);
                Hablar(new Dialogo(msg));
            }
        }

        // El caballero saluda con respeto al centauro: recompensa y el centauro se marcha al galope
        void SaludarCentauro(Ser s)
        {
            if (s == null || !_seres.Contains(s)) return;
            s.Hostil = false;
            s.Est = Estado.Huir; s.T = 0f; s.TieneDest = false;
            s.Emote = 3; s.EmoteT = 2.2f;
            Recompensa(40f);
            Progreso("centauro", 1);
            Hablar(new Dialogo("El centauro inclina la cabeza con respeto, alza su arco en señal de saludo y se marcha al galope. (+40 XP)"));
        }

        // ------------------------------------------------------------------ mensajes del caballero
        void DialogoAvistamiento(Ser s)
        {
            DatosEspecie d = Fauna.D[(int)s.Esp];
            s.Aviso = true; s.AlertaT = 2f; s.Emote = 1; s.EmoteT = 1.6f; s.Der = _pos.X > s.X;

            string[] frases; string pregunta; string[] ops;
            switch (s.Esp)
            {
                case Especie.Zorro:
                    frases = new string[] {
                        "Un zorro rojo se asoma entre los arbustos. Parece curioso y bastante manso. Riesgo: BAJO.",
                        "¿Lo ves? Un zorro rojo nos observa con ojos curiosos. No parece peligroso. Riesgo: BAJO." };
                    pregunta = "Si nos acercamos despacio, quizá podamos domesticarlo. ¿Lo intentamos?";
                    ops = new string[] { "Domesticar", "Dejarlo en paz" };
                    break;
                case Especie.Ciervo:
                    frases = new string[] {
                        "Un ciervo majestuoso pasta cerca. Es muy tímido: un movimiento brusco lo hará huir. Riesgo: BAJO.",
                        "Qué belleza de ciervo... sus astas parecen ramas del propio bosque. Riesgo: BAJO." };
                    pregunta = "Con paciencia podríamos ganarnos su confianza. ¿Lo intentamos?";
                    ops = new string[] { "Domesticar", "Dejarlo en paz" };
                    break;
                case Especie.Lobo:
                    frases = new string[] {
                        "¡Un lobo gris! Sus ojos ámbar no se apartan de nosotros. Riesgo: MEDIO.",
                        "Un lobo gris merodea entre las sombras. Si fallamos al domesticarlo, nos atacará. Riesgo: MEDIO." };
                    pregunta = "Podría ser un gran compañero... o un gran problema. ¿Nos arriesgamos?";
                    ops = new string[] { "Intentarlo", "Alejarme" };
                    break;

                // ---------------------------------------------------- criaturas nuevas
                case Especie.ZorroMagico:
                    frases = new string[] {
                        "¡Un zorro de nueve colas! Llamas violetas danzan a su alrededor. Es astuto y puede embrujarnos si falla la confianza. Riesgo: MEDIO.",
                        "Sus nueve colas se mecen con llamas fantasmales... Es un zorro mágico, listo y juguetón. Riesgo: MEDIO." };
                    pregunta = "Su lealtad sería un tesoro, pero su magia es traviesa. ¿Intentamos domesticarlo?";
                    ops = new string[] { "Domesticar", "Dejarlo en paz" };
                    break;
                case Especie.CiervoLuminoso:
                    frases = new string[] {
                        "Un ciervo de astas llameantes azules... es un espíritu del bosque. Es timidísimo: cualquier prisa lo espantará. Riesgo: BAJO.",
                        "¡Qué visión! Un ciervo luminoso, con runas brillantes en el pelaje. Casi nadie ha logrado acercarse a uno. Riesgo: BAJO." };
                    pregunta = "Solo con mucha paciencia (¡sin correr!) confiará en nosotros. ¿Lo intentamos?";
                    ops = new string[] { "Domesticar", "Dejarlo en paz" };
                    break;
                case Especie.Unicornio:
                    frases = new string[] {
                        "¡Un unicornio! Su cuerno dorado y su crin azul brillan como el agua al amanecer. Riesgo: NINGUNO.",
                        "Dicen que ningún mortal ha tocado jamás a un unicornio. Este nos observa con ojos violeta, sereno pero alerta." };
                    pregunta = "No se deja domesticar ni acercar: huirá en cuanto nos aproximemos. Disfrutemos de verlo mientras dure.";
                    ops = new string[] { "Admirarlo" };
                    Progreso("unicornio", 1);
                    break;
                case Especie.Centauro:
                    frases = new string[] {
                        "Un centauro arquero patrulla con orgullo entre los árboles. Es noble y jamás ha sido domado. Riesgo: MEDIO (si lo provocas).",
                        "Un guerrero mitad hombre, mitad caballo, con carcaj y capa azul. No tolera que corran hacia él. Riesgo: MEDIO." };
                    pregunta = "No se puede domesticar, pero un saludo respetuoso podría ganarnos su aprecio. ¿Lo saludamos?";
                    ops = new string[] { "Saludar", "Alejarme" };
                    break;
                case Especie.Minotauro:
                    frases = new string[] {
                        "¡Por todos los cielos, un minotauro! Esos cuernos de marfil y esa mirada ámbar prometen problemas. Imposible de domar. Riesgo: ALTO.",
                        "Una bestia de músculo, armadura y furia: un minotauro. Si nos ve, embestirá sin piedad. Riesgo: ALTO." };
                    pregunta = "Retrocede sin hacer ruido. Si te embiste, ¡corre con Shift y no mires atrás!";
                    ops = new string[] { "Entendido" };
                    Progreso("minotauro", 1);
                    break;

                default:   // oso pardo
                    frases = new string[] {
                        "¡Por todos los cielos, un oso pardo! Es enorme y feroz. Domesticarlo es imposible. Riesgo: ALTO.",
                        "Ese rugido... ¡un oso pardo! Se ve muy peligroso, no hay forma de domarlo. Riesgo: ALTO." };
                    pregunta = "Aléjate despacio. Si te persigue, ¡corre con Shift y no mires atrás!";
                    ops = new string[] { "Entendido" };
                    Progreso("oso", 1);
                    break;
            }

            var dlg = new Dialogo(new string[] { frases[_rnd.Next(frases.Length)], pregunta }, ops);
            if (s.Esp == Especie.Centauro)
            {
                dlg.Fin = delegate (int op)
                {
                    if (op == 0) SaludarCentauro(s);
                };
            }
            else
            {
                dlg.Fin = delegate (int op)
                {
                    if (d.Domesticable && op == 0)
                    {
                        if (_objAnom != null) { _objAnom.Objetivo = false; _objAnom = null; }
                        if (_objSer != null && _objSer != s) _objSer.Objetivo = false;
                        _objSer = s; s.Objetivo = true;
                    }
                };
            }
            Hablar(dlg);
        }

        void DialogoAnomalia(Anomalia a)
        {
            a.Aviso = true;
            string[] frases; string pregunta; string[] ops;
            if (a.Riesgo == 0)
            {
                frases = new string[] {
                    "Una grieta luminosa brota entre las raíces. Su brillo es débil y estable. Riesgo: BAJO.",
                    "Esta anomalía late con calma entre las piedras... no parece amenazante. Riesgo: BAJO." };
                pregunta = "Podría enseñarnos algo valioso. ¿La investigamos?";
                ops = new string[] { "Investigar", "Alejarme" };
            }
            else if (a.Riesgo == 1)
            {
                frases = new string[] {
                    "Esta anomalía palpita con energía violeta... podría ser inestable. Riesgo: MEDIO.",
                    "Siento un cosquilleo en la armadura. La anomalía vibra de forma irregular. Riesgo: MEDIO." };
                pregunta = "Con cuidado podríamos estudiarla, aunque quizá nos dé una descarga. ¿La investigamos?";
                ops = new string[] { "Investigar", "Alejarme" };
            }
            else
            {
                frases = new string[] {
                    "¡Cuidado! Esta anomalía arde con un brillo cegador. Se ve muy peligrosa. Riesgo: ALTO.",
                    "El aire cruje alrededor de esa grieta... Lo prudente sería alejarnos. Riesgo: ALTO." };
                pregunta = "Si la investigamos, podríamos salir muy mal parados... pero la recompensa sería enorme. ¿Qué hacemos?";
                ops = new string[] { "Investigar igual", "Alejarme" };
            }

            var dlg = new Dialogo(new string[] { frases[_rnd.Next(frases.Length)], pregunta }, ops);
            dlg.Fin = delegate (int op)
            {
                if (op == 0)
                {
                    if (_objSer != null) { _objSer.Objetivo = false; _objSer = null; }
                    _objAnom = a; a.Objetivo = true;
                }
            };
            Hablar(dlg);
        }

        // ------------------------------------------------------------------ dibujo: animales y anomalías
        void DibujarImagenAlfa(Graphics g, Bitmap img, int x, int y, float alfa)
        {
            if (alfa >= 0.98f) { g.DrawImage(img, x, y, img.Width, img.Height); return; }
            if (alfa <= 0.01f) return;
            _cmAlfa.Matrix33 = alfa;
            _iaAlfa.SetColorMatrix(_cmAlfa, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(img, new Rectangle(x, y, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, _iaAlfa);
        }

        void DibujarBrillo(Graphics g, Bitmap b, float cx, float cy, float w, float h, float alfa)
        {
            if (b == null || alfa <= 0.02f) return;
            _cmAlfa.Matrix33 = Lim(alfa, 0f, 1f);
            _iaAlfa.SetColorMatrix(_cmAlfa, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(b, new Rectangle((int)(cx - w / 2f), (int)(cy - h / 2f), (int)w, (int)h), 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, _iaAlfa);
        }

        Cuadro FrameSer(Ser s)
        {
            DatosEspecie d = Fauna.D[(int)s.Esp];
            HojaAnimal h = _animales[(int)s.Esp];
            int idx;
            if (s.Est == Estado.Huir) idx = s.T < 0.35f ? d.Susto : d.Huir;
            else if (s.Est == Estado.Amenaza || s.Est == Estado.Cargar) idx = d.Amenaza;
            else if (s.Est == Estado.Domesticando) idx = ((int)(s.AnimT * 1.5f) % 2 == 0) ? d.Feliz : d.Idle2;
            else if (s.Moviendo) idx = d.Caminar[(int)(s.AnimT * 4.5f) % d.Caminar.Length];
            else if (s.AlertaT > 0f || s.Objetivo) idx = d.Alerta;
            else if (s.Est == Estado.Domesticado) idx = (s.AnimT % 6f) > 4f ? d.Feliz : d.Idle;
            else idx = (s.AnimT % 6f) > 5f ? d.Idle2 : d.Idle;
            return s.Der ? h.Der[idx] : h.Izq[idx];
        }

        void PrepararSeres(int ox, int oy, int W, int H)
        {
            _seresVis.Clear();
            for (int i = 0; i < _seres.Count; i++)
            {
                Ser s = _seres[i];
                if (s.X > ox - 160 && s.X < ox + W + 160 && s.Y > oy - 60 && s.Y < oy + H + 260) _seresVis.Add(s);
            }
            _seresVis.Sort(PorYSer);
        }

        void DibujarSombrasSeres(Graphics g, int ox, int oy)
        {
            for (int i = 0; i < _seresVis.Count; i++)
            {
                Ser s = _seresVis[i];
                DatosEspecie d = Fauna.D[(int)s.Esp];
                DibujarSombra(g, s.X - ox, s.Y - oy - 1f, d.SombraAncho, d.SombraAlto, 4f);
            }
        }

        void DibujarSer(Graphics g, Ser s, int ox, int oy)
        {
            Cuadro c = FrameSer(s);
            float bob = 0f;
            if (s.Moviendo)
            {
                bool rapido = s.Est == Estado.Huir || s.Est == Estado.Cargar;
                bob = Math.Abs((float)Math.Sin(s.AnimT * (rapido ? 14f : 8f))) * (rapido ? 4f : 2f);
            }
            int sx = (int)Math.Round(s.X - ox - c.AnclaX), sy = (int)Math.Round(s.Y - oy - c.AnclaY - bob);
            DibujarImagenAlfa(g, c.Img, sx, sy, s.Alfa);
        }

        // Dibuja (por orden de profundidad) a los animales y al caballero que estén por detrás de 'y'
        void DibujarSeresHasta(Graphics g, int ox, int oy, float y, ref int si, ref bool jugadorListo)
        {
            while (true)
            {
                float ys = si < _seresVis.Count ? _seresVis[si].Y : float.MaxValue;
                float yj = jugadorListo ? float.MaxValue : _pos.Y;
                if (ys >= y && yj >= y) break;
                if (yj <= ys) { DibujarJugador(g, ox, oy); jugadorListo = true; }
                else { DibujarSer(g, _seresVis[si], ox, oy); si++; }
            }
        }

        void DibujarAnomalias(Graphics g, int ox, int oy)
        {
            int W = ClientSize.Width, H = ClientSize.Height;
            for (int i = 0; i < _anomalias.Count; i++)
            {
                Anomalia a = _anomalias[i];
                float sx = a.X - ox, sy = a.Y - oy;
                if (sx < -220 || sx > W + 220 || sy < -220 || sy > H + 220) continue;
                int idx = AnomSeq[(int)(a.Fase * (a.Riesgo == 2 ? 5f : 3.5f)) % 6];
                Cuadro c = _hojaAnom.Der[idx];
                DibujarImagenAlfa(g, c.Img, (int)Math.Round(sx - c.AnclaX), (int)Math.Round(sy - c.AnclaY), a.Alfa);
            }
        }

        // ------------------------------------------------------------------ dibujo: ambiente de atardecer mágico
        static void Corazon(Graphics g, Brush b, float cx, float cy, float s)
        {
            g.FillEllipse(b, cx - s, cy - s * 0.9f, s, s);
            g.FillEllipse(b, cx, cy - s * 0.9f, s, s);
            PointF[] tri = { new PointF(cx - s * 0.97f, cy - s * 0.25f), new PointF(cx + s * 0.97f, cy - s * 0.25f), new PointF(cx, cy + s * 1.1f) };
            g.FillPolygon(b, tri);
        }

        void DibujarMagia(Graphics g, int ox, int oy, int W, int H)
        {
            // 1) tinte de atardecer: violeta arriba, ámbar tenue abajo
            var full = new Rectangle(0, 0, W, H);
            using (var gr = new LinearGradientBrush(full, Color.FromArgb(124, 42, 26, 108), Color.FromArgb(78, 120, 54, 80), 90f))
                g.FillRectangle(gr, full);

            // 2) luces: brillo de las anomalías, farol del caballero y luciérnagas
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.Bilinear;

            for (int i = 0; i < _anomalias.Count; i++)
            {
                Anomalia a = _anomalias[i];
                float sx = a.X - ox, sy = a.Y - oy;
                if (sx < -300 || sx > W + 300 || sy < -300 || sy > H + 300) continue;
                Bitmap br = a.Riesgo == 0 ? _brCian : (a.Riesgo == 1 ? _brVioleta : _brMagenta);
                float pulso = 0.78f + 0.22f * (float)Math.Sin(a.Fase * 2.6f);
                DibujarBrillo(g, br, sx, sy, 360f, 250f, a.Alfa * pulso);
            }

            // resplandor de las criaturas mágicas (zorro mágico, ciervo luminoso, unicornio)
            for (int i = 0; i < _seresVis.Count; i++)
            {
                Ser s = _seresVis[i];
                DatosEspecie d = Fauna.D[(int)s.Esp];
                if (d.Resplandor == 0) continue;
                float pulso = 0.70f + 0.30f * (float)Math.Sin(_t * 3f + i * 1.3f);
                DibujarBrillo(g, d.Resplandor == 2 ? _brVioleta : _brCian,
                              s.X - ox, s.Y - oy - d.Altura * 0.55f, d.Altura * 2.6f, d.Altura * 2.2f, s.Alfa * pulso * 0.8f);
            }

            DibujarBrillo(g, _brCalido, _pos.X - ox, _pos.Y - oy - 34f, 340f, 340f, 0.55f);

            for (int i = 0; i < _luciernagas.Length; i++)
            {
                Luciernaga f = _luciernagas[i];
                float sx = f.X - ox, sy = f.Y - oy;
                if (sx < -40f || sx > W + 40f || sy < -40f || sy > H + 40f) continue;
                float brillo = 0.5f + 0.5f * (float)Math.Sin(f.Fase);
                float al = brillo * brillo;
                Bitmap b = f.Tono == 2 ? _brCian : _brVerde;
                float tam = 22f + 16f * brillo;
                DibujarBrillo(g, b, sx, sy, tam, tam, al * 0.95f);
                _brNucleo.Color = Color.FromArgb((int)(Lim(al, 0f, 1f) * 235f), 255, 255, 200);
                g.FillEllipse(_brNucleo, sx - 1.6f, sy - 1.6f, 3.2f, 3.2f);
            }

            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            // 3) globos de emoción y progreso de las acciones
            DibujarEmotes(g, ox, oy);

            // 4) destello rojo al recibir daño
            if (_flash > 0f)
                using (var bf = new SolidBrush(Color.FromArgb((int)(Lim(_flash, 0f, 1f) * 110f), 200, 20, 20)))
                    g.FillRectangle(bf, 0, 0, W, H);

            g.SmoothingMode = SmoothingMode.None;
        }

        void DibujarEmotes(Graphics g, int ox, int oy)
        {
            for (int i = 0; i < _seresVis.Count; i++)
            {
                Ser s = _seresVis[i];
                if (s.EmoteT <= 0f || s.Emote == 0) continue;
                Cuadro c = FrameSer(s);
                float x = s.X - ox, y = s.Y - oy - c.Img.Height - 18f - (float)Math.Sin(_t * 6f) * 3f;
                float a = Lim(s.EmoteT / 0.35f, 0f, 1f);
                int A = (int)(a * 255f);
                using (var bf = new SolidBrush(Color.FromArgb((int)(a * 225f), 250, 248, 240)))
                using (var pen = new Pen(Color.FromArgb(A, 40, 36, 60), 2f))
                {
                    g.FillEllipse(bf, x - 15f, y - 15f, 30f, 30f);
                    g.DrawEllipse(pen, x - 15f, y - 15f, 30f, 30f);
                }
                if (s.Emote == 3)
                {
                    using (var hb = new SolidBrush(Color.FromArgb(A, 236, 70, 110))) Corazon(g, hb, x, y - 1f, 8f);
                }
                else
                {
                    string t = s.Emote == 1 ? "!" : "?";
                    Color tc = s.Emote == 1 ? Color.FromArgb(A, 220, 50, 40) : Color.FromArgb(A, 200, 140, 20);
                    using (var tb = new SolidBrush(tc))
                        g.DrawString(t, _fEmote, tb, new RectangleF(x - 15f, y - 17f, 30f, 32f), _sfCentro);
                }
            }

            if (_accion == 0) return;
            float tx, ty; string txt; Color col;
            if (_accionSer != null)
            {
                Cuadro c = FrameSer(_accionSer);
                tx = _accionSer.X - ox; ty = _accionSer.Y - oy - c.Img.Height * 0.5f;
                txt = "Domesticando..."; col = Color.FromArgb(255, 150, 200);
            }
            else
            {
                tx = _accionAnom.X - ox; ty = _accionAnom.Y - oy;
                txt = "Investigando..."; col = Color.FromArgb(110, 225, 255);
                for (int k = 0; k < 2; k++)
                {
                    float ph = (_t * 1.1f + k * 0.5f) % 1f;
                    float rx = 30f + ph * 90f, ry = rx * 0.6f;
                    using (var pr = new Pen(Color.FromArgb((int)((1f - ph) * 170f), col), 2.5f))
                        g.DrawEllipse(pr, tx - rx, ty - ry, rx * 2f, ry * 2f);
                }
            }
            float frac = Lim(_accionT / _accionDur, 0f, 1f);
            using (var pen0 = new Pen(Color.FromArgb(90, 255, 255, 255), 5f)) g.DrawEllipse(pen0, tx - 28f, ty - 28f, 56f, 56f);
            if (frac > 0.01f)
                using (var pen = new Pen(col, 5f)) g.DrawArc(pen, tx - 28f, ty - 28f, 56f, 56f, -90f, 360f * frac);
            SizeF sz = g.MeasureString(txt, _fTexto);
            TextoSombra(g, txt, _fTexto, Color.White, tx - sz.Width / 2f, ty - 60f);
        }

        // ------------------------------------------------------------------ HUD: misiones y pista
        void DibujarMision(Graphics g, int W, int H)
        {
            const int pw = 310, rowH = 40;
            var r = new RectangleF(W - 20 - pw, 78, pw, 44 + _misiones.Count * rowH);
            using (GraphicsPath pp = Redondo(r, 14f))
            using (var gr = new LinearGradientBrush(r, Color.FromArgb(215, 26, 30, 48), Color.FromArgb(215, 12, 14, 26), 90f))
            using (var pen = new Pen(Color.FromArgb(220, 196, 156, 70), 2f))
            {
                g.FillPath(gr, pp);
                g.DrawPath(pen, pp);
            }
            TextoSombra(g, "Misiones", _fTitulo, Color.FromArgb(255, 226, 124), r.X + 16, r.Y + 8);

            for (int i = 0; i < _misiones.Count; i++)
            {
                Mision m = _misiones[i];
                float y = r.Y + 42 + i * rowH;
                var cas = new RectangleF(r.X + 16, y + 6, 16, 16);
                if (m.Hecha)
                    using (var bv = new SolidBrush(Color.FromArgb(70, 190, 100))) g.FillRectangle(bv, cas);
                using (var pen = new Pen(Color.FromArgb(220, 200, 200, 220), 2f)) g.DrawRectangle(pen, cas.X, cas.Y, cas.Width, cas.Height);
                if (m.Hecha)
                    using (var pt = new Pen(Color.White, 2.5f))
                    {
                        g.DrawLine(pt, cas.X + 3, cas.Y + 8, cas.X + 7, cas.Y + 12);
                        g.DrawLine(pt, cas.X + 7, cas.Y + 12, cas.X + 13, cas.Y + 4);
                    }
                Color c = m.Hecha ? Color.FromArgb(150, 225, 150) : Color.FromArgb(236, 232, 250);
                using (var b = new SolidBrush(c)) g.DrawString(m.Texto, _fMision, b, r.X + 42, y + 2);
                string det = m.Prog + "/" + m.Meta + "     +" + (int)m.Xp + " XP";
                using (var b = new SolidBrush(Color.FromArgb(190, 190, 200, 235))) g.DrawString(det, _fPeq, b, r.X + 42, y + 22);
            }

            string pista = null;
            if (_dialogo == null && _accion == 0)
            {
                if (_objSer != null) pista = "Acércate despacio al " + Fauna.D[(int)_objSer.Esp].Nombre + " para domesticarlo (sin correr)";
                else if (_objAnom != null) pista = "Acércate a la anomalía para investigarla";
            }
            if (pista != null)
            {
                SizeF sz = g.MeasureString(pista, _fTexto);
                var pr = new RectangleF((W - sz.Width) / 2f - 18f, 18f, sz.Width + 36f, 38f);
                using (GraphicsPath pp = Redondo(pr, 12f))
                using (var bf = new SolidBrush(Color.FromArgb(200, 14, 16, 30)))
                using (var pen = new Pen(Color.FromArgb(220, 196, 156, 70), 2f))
                {
                    g.FillPath(bf, pp);
                    g.DrawPath(pen, pp);
                }
                TextoSombra(g, pista, _fTexto, Color.FromArgb(255, 236, 170), pr.X + 18f, pr.Y + 8f);
            }
        }

        // ------------------------------------------------------------------ recursos procedurales
        static Bitmap CrearVineta(int W, int H)
        {
            var bmp = new Bitmap(W, H, PixelFormat.Format32bppPArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            int[] fila = new int[W];
            for (int y = 0; y < H; y++)
            {
                float ny = (y + 0.5f) / H * 2f - 1f;
                for (int x = 0; x < W; x++)
                {
                    float nx = (x + 0.5f) / W * 2f - 1f;
                    float d = (float)Math.Sqrt(nx * nx + ny * ny);
                    float t = Lim((d - 0.55f) / 0.70f, 0f, 1f);
                    float a = 175f * t * t;
                    int A = (int)a;
                    int pr = (int)(6f * a / 255f), pg = (int)(10f * a / 255f), pb = (int)(26f * a / 255f);
                    fila[x] = unchecked((A << 24) | (pr << 16) | (pg << 8) | pb);
                }
                Marshal.Copy(fila, 0, IntPtr.Add(bd.Scan0, y * bd.Stride), W);
            }
            bmp.UnlockBits(bd);
            return bmp;
        }

        static Bitmap CrearNube(int n)
        {
            var bmp = new Bitmap(n, n, PixelFormat.Format32bppPArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, n, n), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            int[] fila = new int[n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float t = Lim(1f - d, 0f, 1f);
                    int A = (int)(62f * t * t);
                    fila[x] = unchecked(A << 24);   // negro con alfa (premultiplicado)
                }
                Marshal.Copy(fila, 0, IntPtr.Add(bd.Scan0, y * bd.Stride), n);
            }
            bmp.UnlockBits(bd);
            return bmp;
        }

        // ------------------------------------------------------------------ HUD
        static GraphicsPath Redondo(RectangleF r, float rad)
        {
            float d = rad * 2f;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        void TextoSombra(Graphics g, string txt, Font f, Color c, float x, float y)
        {
            using (var bs = new SolidBrush(Color.FromArgb(170, 0, 0, 0))) g.DrawString(txt, f, bs, x + 1.5f, y + 1.5f);
            using (var b = new SolidBrush(c)) g.DrawString(txt, f, b, x, y);
        }

        void DibujarBarra(Graphics g, RectangleF r, float frac, Color c1, Color c2)
        {
            frac = Lim(frac, 0f, 1f);
            using (GraphicsPath fondo = Redondo(r, r.Height / 2f))
            using (var bf = new SolidBrush(Color.FromArgb(220, 10, 12, 22)))
            {
                g.FillPath(bf, fondo);
                float wRel = r.Width * frac;
                if (wRel >= 3f)
                {
                    var rf = new RectangleF(r.X + 1.5f, r.Y + 1.5f, wRel - 3f, r.Height - 3f);
                    if (rf.Width > 2f)
                        using (GraphicsPath pf = Redondo(rf, rf.Height / 2f))
                        using (var gr = new LinearGradientBrush(rf, c1, c2, 0f))
                        {
                            g.FillPath(gr, pf);
                            var brillo = new RectangleF(rf.X + 2f, rf.Y + 1.5f, Math.Max(1f, rf.Width - 4f), rf.Height * 0.35f);
                            using (var bb = new SolidBrush(Color.FromArgb(70, 255, 255, 255))) g.FillRectangle(bb, brillo);
                        }
                }
                using (var pen = new Pen(Color.FromArgb(90, 255, 255, 255), 1f)) g.DrawPath(pen, fondo);
            }
        }

        void DibujarHud(Graphics g, int W, int H)
        {
            // ---- panel superior izquierdo
            var panel = new RectangleF(16, 16, 336, 128);
            using (GraphicsPath pp = Redondo(panel, 14f))
            using (var gr = new LinearGradientBrush(panel, Color.FromArgb(225, 26, 30, 48), Color.FromArgb(225, 12, 14, 26), 90f))
            using (var pen = new Pen(Color.FromArgb(230, 196, 156, 70), 2.5f))
            {
                g.FillPath(gr, pp);
                g.DrawPath(pen, pp);
            }

            // insignia de nivel
            var ins = new RectangleF(30, 49, 62, 62);
            using (var gr = new LinearGradientBrush(ins, Color.FromArgb(255, 226, 124), Color.FromArgb(176, 118, 32), 90f))
                g.FillEllipse(gr, ins);
            var ins2 = new RectangleF(ins.X + 5, ins.Y + 5, ins.Width - 10, ins.Height - 10);
            using (var b = new SolidBrush(Color.FromArgb(24, 28, 54))) g.FillEllipse(b, ins2);
            using (var bt = new SolidBrush(Color.FromArgb(230, 196, 70)))
                g.DrawString("NIVEL", _fPeq, bt, new RectangleF(ins.X, ins.Y + 10, ins.Width, 14), _sfCentro);
            using (var bt = new SolidBrush(Color.White))
                g.DrawString(_nivel.ToString(), _fNivel, bt, new RectangleF(ins.X, ins.Y + 22, ins.Width, 34), _sfCentro);

            // experiencia
            float xpMax = XpParaSiguiente;
            TextoSombra(g, "Experiencia", _fTitulo, Color.FromArgb(232, 226, 255), 110, 26);
            string txtXp = (int)_xp + " / " + (int)xpMax;
            SizeF sz = g.MeasureString(txtXp, _fTexto);
            TextoSombra(g, txtXp, _fTexto, Color.FromArgb(210, 205, 240), 332 - sz.Width, 27);
            DibujarBarra(g, new RectangleF(110, 52, 222, 16), _xpMostrada / xpMax, Color.FromArgb(104, 88, 255), Color.FromArgb(176, 148, 255));

            // energía
            bool baja = _cansado || _energia < 25f;
            TextoSombra(g, "Energía", _fTitulo, Color.FromArgb(222, 250, 220), 110, 76);
            string txtEn = (int)Math.Ceiling(_energia) + " / " + (int)EnergiaMax;
            sz = g.MeasureString(txtEn, _fTexto);
            TextoSombra(g, txtEn, _fTexto, Color.FromArgb(214, 240, 210), 332 - sz.Width, 77);
            Color e1 = baja ? Color.FromArgb(220, 72, 52) : Color.FromArgb(70, 196, 92);
            Color e2 = baja ? Color.FromArgb(255, 160, 60) : Color.FromArgb(186, 232, 84);
            DibujarBarra(g, new RectangleF(110, 102, 222, 16), _energiaMostrada / EnergiaMax, e1, e2);

            // ---- botón de menú (arriba a la derecha)
            DibujarBoton(g, _btnMenu, "Menú", _hoverMenu, true);

            // ---- ayuda inferior
            string ayuda = "WASD / Flechas: mover     ·     Shift: correr     ·     Esc: menú";
            SizeF sa = g.MeasureString(ayuda, _fTexto);
            if (_dialogo == null)
                TextoSombra(g, ayuda, _fTexto, Color.FromArgb(215, 235, 235, 245), (W - sa.Width) / 2f, H - 40);

            // ---- aviso de subida de nivel
            if (_avisoT > 0f)
            {
                float a = Lim(_avisoT / 1.0f, 0f, 1f);
                float yOff = (2.5f - _avisoT) * 14f;
                SizeF sl = g.MeasureString(_aviso, _fGrande);
                TextoSombra(g, _aviso, _fGrande, Color.FromArgb((int)(255 * a), 255, 224, 110), (W - sl.Width) / 2f, 120 - yOff);
            }
        }

        void DibujarBoton(Graphics g, Rectangle r, string texto, bool hover, bool hamburguesa)
        {
            Color a = hover ? Color.FromArgb(240, 58, 64, 96) : Color.FromArgb(225, 30, 34, 54);
            Color b = hover ? Color.FromArgb(240, 36, 40, 64) : Color.FromArgb(225, 14, 16, 28);
            using (GraphicsPath p = Redondo(r, 12f))
            using (var gr = new LinearGradientBrush(r, a, b, 90f))
            using (var pen = new Pen(hover ? Color.FromArgb(255, 255, 214, 110) : Color.FromArgb(230, 196, 156, 70), 2.5f))
            {
                g.FillPath(gr, p);
                g.DrawPath(pen, p);
            }
            Color tc = hover ? Color.FromArgb(255, 244, 200) : Color.FromArgb(240, 232, 214);
            if (hamburguesa)
            {
                using (var bl = new SolidBrush(tc))
                    for (int i = 0; i < 3; i++) g.FillRectangle(bl, r.X + 22, r.Y + 14 + i * 7, 20, 3);
                using (var bt = new SolidBrush(tc))
                    g.DrawString(texto, _fBoton, bt, new RectangleF(r.X + 40, r.Y, r.Width - 40, r.Height), _sfCentro);
            }
            else
            {
                using (var bt = new SolidBrush(tc))
                    g.DrawString(texto, _fBoton, bt, r, _sfCentro);
            }
        }

        void DibujarMenuPausa(Graphics g, int W, int H)
        {
            using (var b = new SolidBrush(Color.FromArgb(150, 0, 0, 8))) g.FillRectangle(b, 0, 0, W, H);
            using (GraphicsPath pp = Redondo(_panelPausa, 18f))
            using (var gr = new LinearGradientBrush(_panelPausa, Color.FromArgb(240, 30, 34, 56), Color.FromArgb(240, 14, 16, 30), 90f))
            using (var pen = new Pen(Color.FromArgb(240, 196, 156, 70), 3f))
            {
                g.FillPath(gr, pp);
                g.DrawPath(pen, pp);
            }
            using (var bt = new SolidBrush(Color.FromArgb(255, 224, 120)))
                g.DrawString("PAUSA", _fGrande, bt, new RectangleF(_panelPausa.X, _panelPausa.Y + 18, _panelPausa.Width, 60), _sfCentro);
            string[] textos = { "Continuar", "Reiniciar", "Salir" };
            for (int i = 0; i < 3; i++) DibujarBoton(g, _btnPausa[i], textos[i], _hoverPausa == i, false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Nexuss
{
    /// <summary>Jugador que ha iniciado partida. Úsalo desde cualquier formulario (ej: lblJugador.Text = Sesion.Nombre).</summary>
    public static class Sesion
    {
        public static int IdJugador { get; set; }
        public static string Nombre { get; set; }
    }

    /// <summary>Una fila de la lista de jugadores del menú.</summary>
    public class JugadorResumen
    {
        public int IdJugador { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public int NivelCadete { get; set; }
        public int Experiencia { get; set; }
        public int NivelEnergia { get; set; }
        public int NivelConexion { get; set; }
        public string RealidadActual { get; set; }
        public int CriaturasDescubiertas { get; set; }
        public int CriaturasDomesticadas { get; set; }
        public int Anomalias { get; set; }
        public DateTime FechaUltimoGuardado { get; set; }
    }

    /// <summary>CRUD de jugadores. Cada método llama a un procedimiento almacenado de NexusDB.</summary>
    public static class BaseDatos
    {
        // Servidor de tu SSMS: FABICITA (instancia por defecto).
        // Si no conecta, prueba cambiando FABICITA por  .   |   (local)   |   localhost
        public const string CadenaConexion =
            @"Server=FABICITA;Database=NexusDB;Integrated Security=True;Connect Timeout=5;";

        private static SqlCommand Sp(SqlConnection cn, string nombre)
        {
            return new SqlCommand(nombre, cn) { CommandType = CommandType.StoredProcedure };
        }

        private static JugadorResumen Leer(SqlDataReader r)
        {
            int oReal = r.GetOrdinal("RealidadActual");

            return new JugadorResumen
            {
                IdJugador = r.GetInt32(r.GetOrdinal("IdJugador")),
                Nombre = r.GetString(r.GetOrdinal("Nombre")),
                Edad = r.GetInt32(r.GetOrdinal("Edad")),
                NivelCadete = r.GetInt32(r.GetOrdinal("NivelCadete")),
                Experiencia = r.GetInt32(r.GetOrdinal("Experiencia")),
                NivelEnergia = r.GetInt32(r.GetOrdinal("NivelEnergia")),
                NivelConexion = r.GetInt32(r.GetOrdinal("NivelConexion")),
                RealidadActual = r.IsDBNull(oReal) ? "-" : r.GetString(oReal),
                CriaturasDescubiertas = r.GetInt32(r.GetOrdinal("CriaturasDescubiertas")),
                CriaturasDomesticadas = r.GetInt32(r.GetOrdinal("CriaturasDomesticadas")),
                Anomalias = r.GetInt32(r.GetOrdinal("Anomalias")),
                FechaUltimoGuardado = r.GetDateTime(r.GetOrdinal("FechaUltimoGuardado"))
            };
        }

        // ---------- READ ----------
        public static List<JugadorResumen> Listar()
        {
            var lista = new List<JugadorResumen>();

            using (var cn = new SqlConnection(CadenaConexion))
            using (var cmd = Sp(cn, "dbo.usp_Jugador_Listar"))
            {
                cn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lista.Add(Leer(r));
                }
            }

            return lista;
        }

        public static JugadorResumen Obtener(int idJugador)
        {
            using (var cn = new SqlConnection(CadenaConexion))
            using (var cmd = Sp(cn, "dbo.usp_Jugador_Obtener"))
            {
                cmd.Parameters.AddWithValue("@IdJugador", idJugador);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    return r.Read() ? Leer(r) : null;
                }
            }
        }

        // ---------- CREATE (jugador + partida nueva; devuelve su Id) ----------
        public static int Crear(string nombre, int edad, int energia, int conexion, int idRealidad)
        {
            using (var cn = new SqlConnection(CadenaConexion))
            using (var cmd = Sp(cn, "dbo.usp_Jugador_Crear"))
            {
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@Edad", edad);
                cmd.Parameters.AddWithValue("@NivelEnergia", energia);
                cmd.Parameters.AddWithValue("@NivelConexion", conexion);
                cmd.Parameters.AddWithValue("@IdRealidad", idRealidad);

                var salida = new SqlParameter("@IdJugador", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(salida);

                cn.Open();
                cmd.ExecuteNonQuery();
                return (int)salida.Value;
            }
        }

        // ---------- UPDATE ----------
        public static void Actualizar(int idJugador, string nombre, int edad)
        {
            using (var cn = new SqlConnection(CadenaConexion))
            using (var cmd = Sp(cn, "dbo.usp_Jugador_Actualizar"))
            {
                cmd.Parameters.AddWithValue("@IdJugador", idJugador);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@Edad", edad);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------- DELETE (borra también todo su progreso) ----------
        public static void Eliminar(int idJugador)
        {
            using (var cn = new SqlConnection(CadenaConexion))
            using (var cmd = Sp(cn, "dbo.usp_Jugador_Eliminar"))
            {
                cmd.Parameters.AddWithValue("@IdJugador", idJugador);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------- REINICIAR PARTIDA (mismo jugador, progreso desde cero) ----------
        public static void Reiniciar(int idJugador, int energia, int conexion, int idRealidad)
        {
            using (var cn = new SqlConnection(CadenaConexion))
            using (var cmd = Sp(cn, "dbo.usp_Partida_Reiniciar"))
            {
                cmd.Parameters.AddWithValue("@IdJugador", idJugador);
                cmd.Parameters.AddWithValue("@NivelEnergia", energia);
                cmd.Parameters.AddWithValue("@NivelConexion", conexion);
                cmd.Parameters.AddWithValue("@IdRealidad", idRealidad);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

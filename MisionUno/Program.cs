using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;


class Personaje
{
    public string Nombre { get; set; }
    public string Tipo { get; set; }
    public int Salud { get; protected set; }
    public int Defensa { get; protected set; }
    public bool Hostil { get; protected set; }

    public Personaje(string nombre, string tipo, int salud, int defensa, bool hostil)
    {
        Nombre = nombre;
        Tipo = tipo;
        Salud = salud;
        Defensa = defensa;
        Hostil = hostil;
    }

    public virtual string Analizar()
    {
        return "Entidad clasificada como " + Tipo + ".";
    }

    public virtual void RecibirDanio(int danio)
    {
        int danioReal = danio - Defensa;

        if (danioReal < 1)
            danioReal = 1;

        Salud -= danioReal;

        if (Salud < 0)
            Salud = 0;
    }

    public bool EstaDerrotado()
    {
        return Salud <= 0;
    }
}

class Robot : Personaje
{
    public int NivelDefensa { get; private set; }

    public Robot()
        : base("R-07 Guardian", "Robot", 70, 8, false)
    {
        NivelDefensa = 25;
        Defensa = NivelDefensa;  /*Pregunta 6 - Corregir Defensa... Localizar Personaje.RecibirDanio y Robot.ActivarSistemaDefensivo*/
    }

    public string ActivarSistemaDefensivo()
    {
        NivelDefensa += 20;
        Defensa += 20;

        return "El robot activa su sistema defensivo. Defensa efectiva: " + Defensa + " (núcleo: " + NivelDefensa + ")";
    }

    public override string Analizar() /*Pregunta 8 - Crea una interfaz pequeña y usarla mediante polimorfismo para el método analizar() 
                                       * haciendo que el robot y el enemigo la implementen usando sus comportamientos existentes.*/
    {
        return "Robot de vigilancia. Su núcleo defensivo responde a cualquier amenaza directa.";
    }
}

class Enemigo : Personaje
{
    public int PoderAtaque { get; private set; }

    public Enemigo()
        : base("Depredador NEX-4", "Enemigo", 60, 4, true)
    {
        PoderAtaque = 12;
    }

    public override string Analizar()
    {
        return "Entidad hostil. El análisis revela una vulnerabilidad en su núcleo.";
    }

    public int Atacar()
    {
        return PoderAtaque + new Random().Next(0, 6);
    }
}

class ObjetoInventario
{
    public string Nombre { get; set; }
    public string Tipo { get; set; }
    public int Cantidad { get; set; }
    public int Efecto { get; set; }

    public ObjetoInventario(string nombre, string tipo, int cantidad, int efecto)
    {
        Nombre = nombre;
        Tipo = tipo;
        Cantidad = cantidad;
        Efecto = efecto;
    }
}

class Inventario
{
    private List<ObjetoInventario> objetos = new List<ObjetoInventario>();

    public int Count
    {
        get { return objetos.Count; }
    }

    public ObjetoInventario this[int indice]
    {
        get { return objetos[indice]; }
    }

    public void Agregar(string nombre, string tipo, int efecto)
    {
        for (int i = 0; i < objetos.Count; i++)
        {
            if (objetos[i].Nombre == nombre)
            {
                objetos[i].Cantidad++;
                return;
            }
        }

        objetos.Add(new ObjetoInventario(nombre, tipo, 1, efecto));
    }

    public bool Contiene(string nombre)
    {
        for (int i = 0; i < objetos.Count; i++)
        {
            if (objetos[i].Nombre == nombre && objetos[i].Cantidad > 0)
                return true;
        }

        return false;
    }

    public bool Usar(int indice)
    {
        if (indice < 0 || indice >= objetos.Count)
            return false;

        if (objetos[indice].Cantidad <= 0)
            return false;

        objetos[indice].Cantidad--;

        if (objetos[indice].Cantidad == 0)
            objetos.RemoveAt(indice);

        return true;
    }

    public bool Eliminar(int indice)
    {
        if (indice < 0 || indice >= objetos.Count)
            return false;

        objetos.RemoveAt(indice);
        return true;
    }
}

class Criatura
{
    private string nombre = "";
    private string categoria = "";
    private string entorno = "";
    private string peligro = "";
    private string domesticable = "";
    private string descripcion = "";
    private string recomendacion = "";
    private string grafico = "";
    private bool legendaria;
    private bool domesticada;

    public string Nombre
    {
        get { return nombre; }
        private set { nombre = value; }
    }

    public string Categoria
    {
        get { return categoria; }
        private set { categoria = value; }
    }

    public string Entorno
    {
        get { return entorno; }
        private set { entorno = value; }
    }

    public string Peligro
    {
        get { return peligro; }
        private set { peligro = value; }
    }

    public string Domesticable
    {
        get { return domesticable; }
        private set { domesticable = value; }
    }

    public string Descripcion
    {
        get { return descripcion; }
        private set { descripcion = value; }
    }

    public string Recomendacion
    {
        get { return recomendacion; }
        private set { recomendacion = value; }
    }

    public string Grafico
    {
        get { return grafico; }
        private set { grafico = value; }
    }

    public bool Legendaria
    {
        get { return legendaria; }
        private set { legendaria = value; }
    }

    public bool Domesticada
    {
        get { return domesticada; }
        private set { domesticada = value; }
    }

    public Criatura(
        string nombre,
        string categoria,
        string entorno,
        string peligro,
        string domesticable,
        string descripcion,
        string recomendacion,
        string grafico,
        bool legendaria = false)
    {
        Nombre = nombre;
        Categoria = categoria;
        Entorno = entorno;
        Peligro = peligro;
        Domesticable = domesticable;
        Descripcion = descripcion;
        Recomendacion = recomendacion;
        Grafico = grafico;
        Legendaria = legendaria;
        Domesticada = false;
    }

    public bool EsDomesticable()
    {
        return Domesticable == "Sí";
    }

    public bool RequierePrecaucion()
    {
        return Peligro == "Media" || Peligro == "Alta" || Peligro == "Crítica";
    }

    public void MarcarComoLegendaria()
    {
        Legendaria = true;
    }

    public void MarcarComoDomesticada()
    {
        Domesticada = true;
    }
}

class Cadete : Personaje
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public int NivelEnergia { get; set; }
    public int NivelConexion { get; set; }

    public Cadete()
        : base("Cadete", "Cadete", (0-100), 5, false) /*Pregunta 7 - Incorporar algo que valide el nivel de energia para que tenga un limite de 0-100*/
    {
        Nombre = "";
        Edad = 0;
        NivelEnergia = 0;
        NivelConexion = 0;
    }
}

class Realidad
{
    public string Nombre { get; set; }
    public string Entorno { get; set; }
    public string Actividad { get; set; }
    public string Senal { get; set; }
    public string Riesgo { get; set; }
    public int Sector { get; set; }

    public Realidad()
    {
        Nombre = "";
        Entorno = "";
        Actividad = "";
        Senal = "";
        Riesgo = "";
        Sector = 0;
    }

    public Realidad(string nombre, string entorno)
    {
        Nombre = nombre;
        Entorno = entorno;
        Actividad = "Cartográfica";
        Senal = "Estable";
        Riesgo = "Bajo";
        Sector = 0;
    }

    public Realidad(string nombre, string entorno, string actividad, string senal, string riesgo, int sector)
    {
        Nombre = nombre;
        Entorno = entorno;
        Actividad = actividad;
        Senal = senal;
        Riesgo = riesgo;
        Sector = sector;
    }

    public Realidad Copiar()
    {
        return new Realidad(Nombre, Entorno, Actividad, Senal, Riesgo, Sector);
    }
}

class Anomalia
{
    public string Tipo { get; set; }
    public int Intensidad { get; set; }
    public string Origen { get; set; }
    public string InformacionAdicional { get; set; }

    public Anomalia()
    {
        Tipo = "";
        Intensidad = 0;
        Origen = "";
        InformacionAdicional = "";
    }
}

class MapaSector
{
    public char[,] Datos { get; private set; } = new char[0, 0];
    public int Filas { get; private set; }
    public int Columnas { get; private set; }
    public int JugadorX { get; set; }
    public int JugadorY { get; set; }
    public int ObjetivoX { get; set; }
    public int ObjetivoY { get; set; }
    public bool TieneAnomalia { get; set; }
    public bool TieneCriatura { get; set; }
    public bool TieneRecurso { get; set; }
    public bool TieneEstructura { get; set; }
    public int Semilla { get; set; }

    public MapaSector(int filas, int columnas)
    {
        Filas = filas;
        Columnas = columnas;
        Reiniciar();
    }

    public void Reiniciar()
    {
        Datos = new char[Filas, Columnas];
        JugadorX = 0;
        JugadorY = 0;
        ObjetivoX = -1;
        ObjetivoY = -1;
        TieneAnomalia = false;
        TieneCriatura = false;
        TieneRecurso = false;
        TieneEstructura = false;
        Semilla = 0;
    }
}

class RegistroDescubrimientos
{
    private List<string> registros = new List<string>();

    public int Count
    {
        get { return registros.Count; }
    }

    public string this[int indice]
    {
        get { return registros[indice]; }
    }

    public void Agregar(string descubrimiento)
    {
        if (registros.Contains(descubrimiento))
            return;

        if (registros.Count < 100)
            registros.Add(descubrimiento);
    }

    public bool Contiene(string descubrimiento)
    {
        return registros.Contains(descubrimiento);
    }
}

class EstadoMision
{
    public int NivelCadete { get; set; }
    public int Experiencia { get; set; }
    public int SectoresExplorados { get; set; }
    public int AnomaliasAnalizadas { get; set; }
    public int Descubrimientos { get; set; }
    public int DecisionesTomadas { get; set; }
    public int EventosEncontrados { get; set; }
    public int TiempoMision { get; set; }
    public bool SesionActiva { get; set; }
    public bool RealidadExplorada { get; set; }
    public bool ObjetivoExploracion { get; set; }
    public bool ObjetivoAnomalia { get; set; }
    public bool ObjetivoEstabilidad { get; set; }
    public bool HabilidadEscaneo { get; set; }
    public bool HabilidadEstabilizacion { get; set; }
    public bool HabilidadAnalisis { get; set; }
    public bool ArchivoSecreto { get; set; }
    public bool EntidadEncontrada { get; set; }
    public int CantidadDomesticadas { get; set; }
    public string CriaturaActual { get; set; } = "";
    public bool ZonaAnomaliaAlcanzada { get; set; }
    public bool NucleoEnergeticoEncontrado { get; set; }
    public bool EntidadSuperada { get; set; }
    public bool PuntoExtraccionAlcanzado { get; set; }
    public bool MisionCompletada { get; set; }
    public bool RecompensaEntregada { get; set; }

    public EstadoMision()
    {
        NivelCadete = 1;
        Experiencia = 0;
        SectoresExplorados = 0;
        AnomaliasAnalizadas = 0;
        Descubrimientos = 0;
        DecisionesTomadas = 0;
        EventosEncontrados = 0;
        TiempoMision = 0;
        SesionActiva = true;
        RealidadExplorada = false;
        ObjetivoExploracion = false;
        ObjetivoAnomalia = false;
        ObjetivoEstabilidad = false;
        HabilidadEscaneo = false;
        HabilidadEstabilizacion = false;
        HabilidadAnalisis = false;
        ArchivoSecreto = false;
        EntidadEncontrada = false;
        CantidadDomesticadas = 0;
        CriaturaActual = "";
        ZonaAnomaliaAlcanzada = false;
        NucleoEnergeticoEncontrado = false;
        EntidadSuperada = false;
        PuntoExtraccionAlcanzado = false;
        MisionCompletada = false;
        RecompensaEntregada = false;
    }
}

class Program
{

    const int FILA_INICIO_MAPA = 6;
    const int ANCHO_PANEL_MAPA = 79;
    const char SIMBOLO_CADETE = '♟';

    static Cadete cadete = new Cadete();
    static Realidad realidadActual = new Realidad();
    static Anomalia anomaliaActual = new Anomalia();
    static MapaSector mapaActual = new MapaSector(31, 75);
    static EstadoMision mision = new EstadoMision();
    static RegistroDescubrimientos registroDescubrimientos = new RegistroDescubrimientos();
    static Inventario inventario = new Inventario();
    static Personaje entidadActual = null;
    static int puntoExtraccionX = -1;
    static int puntoExtraccionY = -1;
    static List<Realidad> realidades = new List<Realidad>
    {
        new Realidad("NEXUS-07", "Boscoso"),
        new Realidad("ECHO-13", "Desértico"),
        new Realidad("AURORA-21", "Subterráneo"),
        new Realidad("VOID-42", "Urbano"),
        new Realidad("IRIS-55", "Acuático"),
        new Realidad("OMEGA-99", "Desconocido")
    };
    static List<Criatura> criaturas = new List<Criatura>(50);
    static Random random = new Random();

    static void Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        Console.InputEncoding = new UTF8Encoding(false);
        Console.Title = "NEXUS TRAINING SYSTEM";

        MostrarInicio();
        MostrarBienvenida();

        if (cadete.NivelEnergia < 20 || cadete.NivelConexion < 20)
        {
            Console.Clear();
            Color(ConsoleColor.Red);
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║ ACCESO DENEGADO                                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Color(ConsoleColor.Gray);
            Console.WriteLine();
            Console.WriteLine("El cadete no cumple los requisitos mínimos.");
            Console.WriteLine();
            Console.WriteLine("Energía mínima : 20");
            Console.WriteLine("Estabilidad mínima: 20");
            Console.WriteLine();
            Console.WriteLine("Entrenamiento cancelado.");
            return;
        }

        CargarSistema();
        InicializarInventario();
        MostrarObjetivos();

        while (mision.SesionActiva)
        {
            MostrarMenu();

            int opcion;
            if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
            {
                MensajeError("Debe ingresar un número.");
                PausaMenu();
                continue;
            }

            Console.Clear();

            switch (opcion)
            {
                case 1:
                    ExplorarRealidad();
                    PausaMenu();
                    break;
                case 2:
                    AnalizarAnomalias();
                    PausaMenu();
                    break;
                case 3:
                    ConsultarEstado();
                    PausaMenu();
                    break;
                case 4:
                    RecuperarEnergia();
                    PausaMenu();
                    break;
                case 5:
                    MostrarDescubrimientos();
                    PausaMenu();
                    break;
                case 6:
                    MostrarHabilidades();
                    PausaMenu();
                    break;
                case 7:
                    MostrarBestiario();
                    PausaMenu();
                    break;
                case 8:
                    MostrarMapasRealidades();
                    PausaMenu();
                    break;
                case 9:
                    IntentarDesconexion();
                    if (mision.SesionActiva)
                        PausaMenu();
                    break;
                case 10:
                    MostrarInventario();
                    PausaMenu();
                    break;
                case 11:
                    MostrarMisionActual();
                    PausaMenu();
                    break;
                default:
                    MensajeError("Opción inválida.");
                    PausaMenu();
                    break;
            }
        }

        MostrarFinal();
    }

    static void MostrarInicio()
    {
        Console.Clear();
        Color(ConsoleColor.Cyan);
        Console.WriteLine();
        Console.WriteLine("★ ★ ★ ★ ★ ★");
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                    ║");
        Console.WriteLine("║                     N E X U S                      ║");
        Console.WriteLine("║                                                    ║");
        Console.WriteLine("║             DIMENSIONAL TRAINING SYSTEM            ║");
        Console.WriteLine("║                                                    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("★ ★ ★ ★ ★ ★");
        Color(ConsoleColor.Gray);
        Console.WriteLine();

        EscribirAnimado("Inicializando núcleo dimensional...", 20);
        Console.WriteLine();
        CargarBarra("Estableciendo conexión", 20);
        EscribirAnimado("Verificando estabilidad neuronal...", 20);
        Console.WriteLine();
        CargarBarra("Sincronizando realidad", 20);

        Console.Clear();
        Color(ConsoleColor.Green);
        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════");
        Console.WriteLine(" CONEXIÓN ESTABLECIDA");
        Console.WriteLine("══════════════════════════════════════════════════════");
        Color(ConsoleColor.Gray);
        Thread.Sleep(500);
    }

    static void MostrarBienvenida()
    {
        Console.Clear();
        Color(ConsoleColor.Cyan);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ REGISTRO DE CADETE                                 ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("Antes de comenzar el entrenamiento necesitamos");
        Console.WriteLine("registrar los datos del nuevo cadete.");
        Console.WriteLine();

        do
        {
            Console.Write("Nombre: ");
            cadete.Nombre = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(cadete.Nombre))
                MensajeError("El nombre no puede estar vacío.");
        }
        while (string.IsNullOrWhiteSpace(cadete.Nombre));

        do
        {
            Console.Write("Edad: ");

            int edadIngresada;
            if (!int.TryParse(Console.ReadLine() ?? "", out edadIngresada))
                edadIngresada = 0;

            cadete.Edad = edadIngresada;

            if (cadete.Edad < 18 || cadete.Edad > 50)
                MensajeError("La edad debe estar entre 18 y 50 años.");
        }
        while (cadete.Edad < 18 || cadete.Edad > 50);

        Console.WriteLine();
        Console.WriteLine("Realidades disponibles:");
        Console.WriteLine("1. NEXUS-07");
        Console.WriteLine("2. ECHO-13");
        Console.WriteLine("3. AURORA-21");
        Console.WriteLine("4. VOID-42");

        int opcionRealidad;

        do
        {
            Console.Write("Seleccione una realidad: ");

            if (!int.TryParse(Console.ReadLine() ?? "", out opcionRealidad))
                opcionRealidad = 0;

            switch (opcionRealidad)
            {
                case 1:
                    realidadActual.Nombre = "NEXUS-07";
                    break;
                case 2:
                    realidadActual.Nombre = "ECHO-13";
                    break;
                case 3:
                    realidadActual.Nombre = "AURORA-21";
                    break;
                case 4:
                    realidadActual.Nombre = "VOID-42";
                    break;
                default:
                    MensajeError("Seleccione una realidad válida.");
                    break;
            }
        }
        while (opcionRealidad < 1 || opcionRealidad > 4);

        do
        {
            Console.Write("Nivel de Energía inicial: ");

            int energiaIngresada;
            if (!int.TryParse(Console.ReadLine() ?? "", out energiaIngresada))
                energiaIngresada = -1;

            cadete.NivelEnergia = energiaIngresada;

            if (cadete.NivelEnergia < 0 || cadete.NivelEnergia > 100)
                MensajeError("El nivel debe estar entre 0 y 100.");
        }
        while (cadete.NivelEnergia < 0 || cadete.NivelEnergia > 100);

        do
        {
            Console.Write("Nivel de Conexión inicial: ");

            int conexionIngresada;
            if (!int.TryParse(Console.ReadLine() ?? "", out conexionIngresada))
                conexionIngresada = -1;

            cadete.NivelConexion = conexionIngresada;

            if (cadete.NivelConexion < 0 || cadete.NivelConexion > 100)
                MensajeError("El nivel debe estar entre 0 y 100.");
        }
        while (cadete.NivelConexion < 0 || cadete.NivelConexion > 100);

        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════");

        if (cadete.NivelEnergia < 20 || cadete.NivelConexion < 20)
        {
            Color(ConsoleColor.Red);
            Console.WriteLine("DATOS REGISTRADOS.");
            Console.WriteLine("REQUISITOS DE ACCESO NO CUMPLIDOS.");
            Color(ConsoleColor.Gray);
        }
        else
        {
            Color(ConsoleColor.Green);
            Console.WriteLine("DATOS REGISTRADOS CORRECTAMENTE.");
            Color(ConsoleColor.Gray);
        }

        Thread.Sleep(800);
    }

    static void CargarSistema()
    {
        Console.Clear();
        Color(ConsoleColor.Cyan);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ NEXUS TRAINING SYSTEM                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        CargarBarra("Cargando sistema", 25);
        Console.WriteLine();
        Color(ConsoleColor.Green);
        Console.WriteLine("SISTEMA LISTO.");
        Color(ConsoleColor.Gray);
        Thread.Sleep(500);
    }

    static void MostrarObjetivos()
    {
        Console.Clear();
        Color(ConsoleColor.Yellow);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ OBJETIVOS DE MISIÓN                                ║");
        Console.WriteLine("╠════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                    ║");
        Console.WriteLine("║ • Explorar 3 sectores                              ║");
        Console.WriteLine("║ • Llegar a una zona anómala                        ║");
        Console.WriteLine("║ • Encontrar el núcleo energético                   ║");
        Console.WriteLine("║ • Superar un encuentro con una entidad             ║");
        Console.WriteLine("║ • Regresar al punto de extracción                  ║");
        Console.WriteLine("║ • Mantener estabilidad superior al 30%             ║");
        Console.WriteLine("║                                                    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Thread.Sleep(800);
    }

    static void MostrarMenu()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ NEXUS CONTROL CENTER                               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════╣");
        Console.WriteLine("║ CADETE : " + Ajustar(cadete.Nombre, 43) + "║");
        Console.WriteLine("║ Nivel : " + mision.NivelCadete.ToString().PadRight(43) + "║");
        Console.WriteLine("╠════════════════════════════════════════════════════╣");
        MostrarBarraMenu("ENERGÍA", cadete.NivelEnergia);
        MostrarBarraMenu("ESTABILIDAD", cadete.NivelConexion);
        Console.WriteLine("╠════════════════════════════════════════════════════╣");
        Console.WriteLine("║ [1] EXPLORAR REALIDAD                              ║");
        Console.WriteLine("║ [2] ANALIZAR ANOMALÍA                              ║");
        Console.WriteLine("║ [3] CONSULTAR ESTADO                               ║");
        Console.WriteLine("║ [4] RECUPERAR ENERGÍA                              ║");
        Console.WriteLine("║ [5] REGISTRO DE DESCUBRIMIENTOS                   ║");
        Console.WriteLine("║ [6] HABILIDADES                                    ║");
        Console.WriteLine("║ [7] BESTIARIO                                      ║");
        Console.WriteLine("║ [8] MAPAS DE REALIDADES                            ║");
        Console.WriteLine("║ [9] DESCONEXIÓN                                    ║");
        Console.WriteLine("║ [10] INVENTARIO                                    ║");
        Console.WriteLine("║ [11] MISIÓN ACTUAL                                 ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.Write("Seleccione una operación: ");
    }

    static void MostrarMapasRealidades()
    {
        bool seleccionando = true;

        while (seleccionando)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║ NEXUS // MAPAS DE REALIDADES                       ║");
            Console.WriteLine("╠════════════════════════════════════════════════════╣");
            Console.ResetColor();

            for (int i = 0; i < realidades.Count; i++)
            {
                Console.WriteLine("║ [" + (i + 1) + "] " + realidades[i].Nombre.PadRight(14) + " // " + realidades[i].Entorno.PadRight(21) + "║");
            }

            Console.WriteLine("║ [0] REGRESAR                                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.Write("Seleccione una realidad: ");

            int opcion;
            if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
                opcion = -1;

            if (opcion == 0)
            {
                seleccionando = false;
                continue;
            }

            if (opcion >= 1 && opcion <= realidades.Count)
            {
                Realidad realidadAnterior = realidadActual.Copiar();
                realidadActual = realidades[opcion - 1].Copiar();

                realidadActual.Actividad = "Cartográfica";
                realidadActual.Senal = opcion == 4 ? "Anómala" : "Estable";
                realidadActual.Riesgo = opcion == 4 ? "Alto" : "Bajo";
                realidadActual.Sector = opcion;

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("CARGANDO CARTOGRAFÍA DE " + realidadActual.Nombre + "...");
                Console.ResetColor();
                Thread.Sleep(500);

                GenerarMapaSector();
                ExplorarMapaSector();

                realidadActual = realidadAnterior;
            }
            else
            {
                MensajeError("Seleccione una realidad válida.");
                PausaMenu();
            }
        }
    }

    static void ExplorarRealidad()
    {
        Console.Clear();

        if (cadete.NivelEnergia < 10 || cadete.NivelConexion < 10)
        {
            Color(ConsoleColor.Red);
            Console.WriteLine("No es seguro realizar otra exploración.");
            Color(ConsoleColor.Gray);
            Console.WriteLine("Se requiere al menos 10 de energía y estabilidad.");
            return;
        }

        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ ESCANEO DIMENSIONAL                                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        mision.CriaturaActual = "";

        CargarBarra("Buscando sectores", 20);
        CargarBarra("Analizando actividad", 20);
        CargarBarra("Buscando señales", 20);
        CargarBarra("Evaluando riesgo", 20);

        Console.Clear();

        realidadActual.Sector = random.Next(1, 51);

        string[] entornos = { "Desértico", "Subterráneo", "Boscoso", "Urbano", "Acuático", "Desconocido" };
        string[] actividades = { "Alta", "Media", "Baja", "Nula" };
        string[] senales = { "Nula", "Débil", "Desconocida", "Inestable" };

        realidadActual.Entorno = entornos[random.Next(entornos.Length)];
        realidadActual.Actividad = actividades[random.Next(actividades.Length)];

        int probabilidadAnomalia = 20 + mision.SectoresExplorados * 5;

        if (realidadActual.Nombre == "VOID-42")
            probabilidadAnomalia += 15;

        if (probabilidadAnomalia > 70)
            probabilidadAnomalia = 70;

        if (random.Next(1, 101) <= probabilidadAnomalia)
            realidadActual.Senal = "Anómala";
        else
            realidadActual.Senal = senales[random.Next(senales.Length)];

        int riesgoNumero = random.Next(1, 101);

        if (realidadActual.Senal == "Anómala")
        {
            if (riesgoNumero <= 20)
                realidadActual.Riesgo = "Medio";
            else if (riesgoNumero <= 60)
                realidadActual.Riesgo = "Alto";
            else
                realidadActual.Riesgo = "Crítico";
        }
        else
        {
            if (riesgoNumero <= 55)
                realidadActual.Riesgo = "Bajo";
            else if (riesgoNumero <= 85)
                realidadActual.Riesgo = "Medio";
            else
                realidadActual.Riesgo = "Alto";
        }

        MostrarResultadoExploracion();

        mision.RealidadExplorada = true;
        mision.SectoresExplorados++;
        cadete.NivelEnergia -= 8;
        cadete.NivelConexion -= 5;
        mision.TiempoMision += 30;

        LimitarValores();
        AgregarXP(20);

        if (mision.SectoresExplorados >= 3 && !mision.ObjetivoExploracion)
        {
            mision.ObjetivoExploracion = true;
            CompletarObjetivo("Explorar 3 sectores");
        }

        if (cadete.NivelConexion > 30 && !mision.ObjetivoEstabilidad)
        {
            mision.ObjetivoEstabilidad = true;
            CompletarObjetivo("Mantener estabilidad superior al 30%");
        }

        MostrarEstadoRapido();

        if (realidadActual.Senal == "Anómala")
            EventoAnomalia();
        else
            EventoAleatorio();

        DetectarCriaturaPorEntorno();

        Console.Clear();
        GenerarMapaSector();
        ExplorarMapaSector();
        VerificarHabilidades();
    }

    static void GenerarMapaSector()
    {
        mapaActual.Reiniciar();

        mapaActual.Semilla = realidadActual.Sector * 7919 + mision.SectoresExplorados * 104729 + random.Next(1, 100000);

        mapaActual.TieneAnomalia = realidadActual.Senal == "Anómala";
        mapaActual.TieneCriatura = mision.CriaturaActual != "";
        mapaActual.TieneRecurso = random.Next(1, 101) <= 65;
        mapaActual.TieneEstructura = random.Next(1, 101) <= 55;

        if (realidadActual.Entorno == "Boscoso")
            GenerarMapaBosque();
        else if (realidadActual.Entorno == "Desértico")
            GenerarMapaDesierto();
        else if (realidadActual.Entorno == "Subterráneo")
            GenerarMapaSubterraneo();
        else if (realidadActual.Entorno == "Urbano")
            GenerarMapaUrbano();
        else if (realidadActual.Entorno == "Acuático")
            GenerarMapaAcuatico();
        else
            GenerarMapaDesconocido();

        ColocarElementosMapa();
        BuscarObjetivoMapa();
        ColocarJugadorMapa();
    }

    static void LlenarMapaSector(char caracter)
    {
        for (int fila = 0; fila < mapaActual.Filas; fila++)
        {
            for (int columna = 0; columna < mapaActual.Columnas; columna++)
                mapaActual.Datos[fila, columna] = caracter;
        }
    }

    static void CrearBordeMapa()
    {
        for (int columna = 0; columna < mapaActual.Columnas; columna++)
        {
            mapaActual.Datos[0, columna] = '#';
            mapaActual.Datos[mapaActual.Filas - 1, columna] = '#';
        }

        for (int fila = 0; fila < mapaActual.Filas; fila++)
        {
            mapaActual.Datos[fila, 0] = '#';
            mapaActual.Datos[fila, mapaActual.Columnas - 1] = '#';
        }
    }

    static void GenerarMapaBosque()
    {
        LlenarMapaSector('^');
        Random mapaRandom = new Random(mapaActual.Semilla);

        for (int i = 0; i < 230; i++)
        {
            int fila = mapaRandom.Next(1, mapaActual.Filas - 1);
            int columna = mapaRandom.Next(1, mapaActual.Columnas - 1);

            if (mapaActual.Datos[fila, columna] == '^')
                mapaActual.Datos[fila, columna] = '*';
        }

        for (int i = 0; i < 48; i++)
        {
            int fila = mapaRandom.Next(2, mapaActual.Filas - 2);
            int columna = mapaRandom.Next(2, mapaActual.Columnas - 2);

            if (mapaActual.Datos[fila, columna] == '^')
                mapaActual.Datos[fila, columna] = 'T';
        }

        for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
        {
            mapaActual.Datos[fila, 36] = '.';
            mapaActual.Datos[fila, 37] = '.';
            mapaActual.Datos[fila, 38] = '.';
        }

        for (int columna = 4; columna < mapaActual.Columnas - 4; columna++)
        {
            mapaActual.Datos[14, columna] = '.';
            mapaActual.Datos[15, columna] = '.';
        }

        for (int fila = 21; fila <= 25; fila++)
        {
            for (int columna = 1; columna < mapaActual.Columnas - 1; columna++)
                mapaActual.Datos[fila, columna] = '~';
        }

        for (int columna = 32; columna <= 42; columna++)
            mapaActual.Datos[23, columna] = '=';

        CrearBordeMapa();
    }

    static void GenerarMapaDesierto()
    {
        LlenarMapaSector(':');
        Random mapaRandom = new Random(mapaActual.Semilla);

        for (int i = 0; i < 200; i++)
        {
            int fila = mapaRandom.Next(1, mapaActual.Filas - 1);
            int columna = mapaRandom.Next(1, mapaActual.Columnas - 1);

            if (mapaActual.Datos[fila, columna] == ':')
                mapaActual.Datos[fila, columna] = '.';
        }

        for (int i = 0; i < 55; i++)
        {
            int fila = mapaRandom.Next(2, mapaActual.Filas - 2);
            int columna = mapaRandom.Next(2, mapaActual.Columnas - 2);
            mapaActual.Datos[fila, columna] = 'O';
        }

        for (int fila = 6; fila <= 10; fila++)
        {
            for (int columna = 8; columna <= 18; columna++)
                mapaActual.Datos[fila, columna] = '~';
        }

        for (int i = 0; i < 18; i++)
        {
            int fila = mapaRandom.Next(3, mapaActual.Filas - 3);
            int columna = mapaRandom.Next(3, mapaActual.Columnas - 3);

            if (mapaActual.Datos[fila, columna] == ':')
                mapaActual.Datos[fila, columna] = 'T';
        }

        for (int columna = 3; columna < mapaActual.Columnas - 3; columna++)
        {
            mapaActual.Datos[16, columna] = '.';

            if (columna % 4 == 0)
                mapaActual.Datos[17, columna] = '.';
        }

        CrearBordeMapa();
    }

    static void GenerarMapaSubterraneo()
    {
        LlenarMapaSector('#');
        Random mapaRandom = new Random(mapaActual.Semilla);

        for (int fila = 2; fila < mapaActual.Filas - 2; fila++)
        {
            for (int columna = 2; columna < mapaActual.Columnas - 2; columna++)
            {
                if (mapaRandom.Next(1, 101) <= 70)
                    mapaActual.Datos[fila, columna] = '.';
            }
        }

        for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
        {
            mapaActual.Datos[fila, 36] = '.';
            mapaActual.Datos[fila, 37] = '.';
            mapaActual.Datos[fila, 38] = '.';
        }

        for (int i = 0; i < 60; i++)
        {
            int fila = mapaRandom.Next(2, mapaActual.Filas - 2);
            int columna = mapaRandom.Next(2, mapaActual.Columnas - 2);

            if (mapaActual.Datos[fila, columna] == '.')
                mapaActual.Datos[fila, columna] = '*';
        }

        for (int fila = 21; fila <= 25; fila++)
        {
            for (int columna = 8; columna <= 27; columna++)
                mapaActual.Datos[fila, columna] = '~';
        }

        for (int i = 45; i > 0; i--)
        {
            int fila = mapaRandom.Next(2, mapaActual.Filas - 2);
            int columna = mapaRandom.Next(2, mapaActual.Columnas - 2);

            if (mapaActual.Datos[fila, columna] == '#')
                mapaActual.Datos[fila, columna] = 'O';
        }

        CrearBordeMapa();
    }

    static void GenerarMapaUrbano()
    {
        LlenarMapaSector('.');

        for (int columna = 10; columna < mapaActual.Columnas - 5; columna += 15)
        {
            for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
            {
                mapaActual.Datos[fila, columna] = '=';
                mapaActual.Datos[fila, columna + 1] = '=';
            }
        }

        for (int fila = 7; fila < mapaActual.Filas - 5; fila += 8)
        {
            for (int columna = 1; columna < mapaActual.Columnas - 1; columna++)
            {
                mapaActual.Datos[fila, columna] = '=';
                mapaActual.Datos[fila + 1, columna] = '=';
            }
        }

        for (int fila = 2; fila < mapaActual.Filas - 5; fila += 8)
        {
            for (int columna = 3; columna < mapaActual.Columnas - 10; columna += 15)
                CrearEdificioMapa(fila, columna);
        }

        for (int fila = 12; fila <= 18; fila++)
        {
            for (int columna = 30; columna <= 44; columna++)
                mapaActual.Datos[fila, columna] = '.';
        }

        mapaActual.Datos[15, 36] = '~';
        mapaActual.Datos[15, 37] = '~';
        mapaActual.Datos[16, 36] = '~';
        mapaActual.Datos[16, 37] = '~';

        CrearBordeMapa();
    }

    static void CrearEdificioMapa(int filaInicio, int columnaInicio)
    {
        for (int fila = filaInicio; fila < filaInicio + 5; fila++)
        {
            for (int columna = columnaInicio; columna < columnaInicio + 9; columna++)
            {
                if (fila >= 1 && fila < mapaActual.Filas - 1 && columna >= 1 && columna < mapaActual.Columnas - 1)
                    mapaActual.Datos[fila, columna] = '#';
            }
        }

        mapaActual.Datos[filaInicio + 4, columnaInicio + 4] = 'H';
        mapaActual.Datos[filaInicio + 2, columnaInicio + 2] = 'W';
        mapaActual.Datos[filaInicio + 2, columnaInicio + 6] = 'W';
    }

    static void GenerarMapaAcuatico()
    {
        LlenarMapaSector('~');
        Random mapaRandom = new Random(mapaActual.Semilla);

        for (int isla = 0; isla < 9; isla++)
        {
            int centroY = mapaRandom.Next(4, mapaActual.Filas - 4);
            int centroX = mapaRandom.Next(5, mapaActual.Columnas - 5);

            for (int fila = centroY - 2; fila <= centroY + 2; fila++)
            {
                for (int columna = centroX - 4; columna <= centroX + 4; columna++)
                {
                    if (fila >= 1 && fila < mapaActual.Filas - 1 && columna >= 1 && columna < mapaActual.Columnas - 1)
                    {
                        if (mapaRandom.Next(1, 101) <= 75)
                            mapaActual.Datos[fila, columna] = '.';
                    }
                }
            }
        }

        for (int i = 0; i < 40; i++)
        {
            int fila = mapaRandom.Next(2, mapaActual.Filas - 2);
            int columna = mapaRandom.Next(2, mapaActual.Columnas - 2);

            if (mapaActual.Datos[fila, columna] == '.')
                mapaActual.Datos[fila, columna] = '*';
        }

        for (int i = 0; i < 45; i++)
        {
            int fila = mapaRandom.Next(1, mapaActual.Filas - 1);
            int columna = mapaRandom.Next(1, mapaActual.Columnas - 1);

            if (mapaActual.Datos[fila, columna] == '~')
                mapaActual.Datos[fila, columna] = 'O';
        }

        CrearBordeMapa();
    }

    static void GenerarMapaDesconocido()
    {
        char[] elementos = { '.', ':', '?', '#', '*', 'O' };
        Random mapaRandom = new Random(mapaActual.Semilla);

        for (int fila = 0; fila < mapaActual.Filas; fila++)
        {
            for (int columna = 0; columna < mapaActual.Columnas; columna++)
                mapaActual.Datos[fila, columna] = elementos[mapaRandom.Next(elementos.Length)];
        }

        for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
        {
            mapaActual.Datos[fila, 36] = '.';
            mapaActual.Datos[fila, 37] = '.';
            mapaActual.Datos[fila, 38] = '.';
        }

        for (int fila = 9; fila <= 21; fila++)
        {
            for (int columna = 27; columna <= 48; columna++)
            {
                if (mapaRandom.Next(1, 101) <= 65)
                    mapaActual.Datos[fila, columna] = '?';
            }
        }

        CrearBordeMapa();
    }

    static void ColocarElementosMapa()
    {
        if (mapaActual.TieneAnomalia)
            ColocarAnomaliaMapa();

        if (mapaActual.TieneEstructura)
            ColocarEstructuraMapa();

        if (mapaActual.TieneRecurso)
            ColocarRecursoMapa();

        if (mapaActual.TieneCriatura)
            ColocarCriaturaMapa();

        ColocarPuntoInteresMapa();
    }

    static bool EsTransitableMapa(char elemento)
    {
        if (elemento == '^' || elemento == 'T' || elemento == '#' ||
            elemento == '~' || elemento == 'O' || elemento == 'W')
            return false;

        return true;
    }

    static void ColocarAnomaliaMapa()
    {
        int centroY;
        int centroX;

        do
        {
            centroY = random.Next(5, mapaActual.Filas - 5);
            centroX = random.Next(9, mapaActual.Columnas - 9);
        }
        while (mapaActual.Datos[centroY, centroX] == '~');

        for (int fila = centroY - 6; fila <= centroY + 6; fila++)
        {
            for (int columna = centroX - 9; columna <= centroX + 9; columna++)
            {
                if (fila >= 1 && fila < mapaActual.Filas - 1 && columna >= 1 && columna < mapaActual.Columnas - 1)
                {
                    if (random.Next(1, 101) <= 28)
                        mapaActual.Datos[fila, columna] = '?';
                }
            }
        }

        for (int fila = centroY - 3; fila <= centroY + 3; fila++)
        {
            for (int columna = centroX - 5; columna <= centroX + 5; columna++)
            {
                if (fila >= 1 && fila < mapaActual.Filas - 1 && columna >= 1 && columna < mapaActual.Columnas - 1)
                {
                    if (random.Next(1, 101) <= 70)
                        mapaActual.Datos[fila, columna] = 'a';
                }
            }
        }

        mapaActual.Datos[centroY, centroX] = 'A';

        if (centroY - 4 >= 1)
            mapaActual.Datos[centroY - 4, centroX] = 'A';

        if (centroY + 4 < mapaActual.Filas - 1)
            mapaActual.Datos[centroY + 4, centroX] = 'A';

        if (centroX - 6 >= 1)
            mapaActual.Datos[centroY, centroX - 6] = 'A';

        if (centroX + 6 < mapaActual.Columnas - 1)
            mapaActual.Datos[centroY, centroX + 6] = 'A';
    }

    static void ColocarEstructuraMapa()
    {
        for (int intento = 0; intento < 40; intento++)
        {
            int fila = random.Next(3, mapaActual.Filas - 7);
            int columna = random.Next(4, mapaActual.Columnas - 14);

            if (mapaActual.Datos[fila, columna] != '.')
                continue;

            for (int y = fila; y < fila + 4; y++)
            {
                for (int x = columna; x < columna + 9; x++)
                {
                    if (y >= 1 && y < mapaActual.Filas - 1 && x >= 1 && x < mapaActual.Columnas - 1)
                        mapaActual.Datos[y, x] = '#';
                }
            }

            mapaActual.Datos[fila + 3, columna + 4] = 'H';
            return;
        }
    }

    static void ColocarRecursoMapa()
    {
        int fila;
        int columna;

        do
        {
            fila = random.Next(2, mapaActual.Filas - 2);
            columna = random.Next(2, mapaActual.Columnas - 2);
        }
        while (!EsTransitableMapa(mapaActual.Datos[fila, columna]));

        mapaActual.Datos[fila, columna] = '$';
    }

    static void ColocarCriaturaMapa()
    {
        int fila;
        int columna;

        do
        {
            fila = random.Next(3, mapaActual.Filas - 3);
            columna = random.Next(3, mapaActual.Columnas - 3);
        }
        while (!EsTransitableMapa(mapaActual.Datos[fila, columna]));

        mapaActual.Datos[fila, columna] = 'C';
    }

    static void ColocarPuntoInteresMapa()
    {
        for (int intento = 0; intento < 40; intento++)
        {
            int fila = random.Next(2, mapaActual.Filas - 2);
            int columna = random.Next(2, mapaActual.Columnas - 2);

            if (EsTransitableMapa(mapaActual.Datos[fila, columna]))
            {
                mapaActual.Datos[fila, columna] = 'P';
                return;
            }
        }
    }

    static void ColocarJugadorMapa()
    {
        mapaActual.JugadorY = mapaActual.Filas - 3;
        mapaActual.JugadorX = mapaActual.Columnas / 2;

        if (!EsTransitableMapa(mapaActual.Datos[mapaActual.JugadorY, mapaActual.JugadorX]))
        {
            for (int distancia = 1; distancia < 20; distancia++)
            {
                bool encontrado = false;

                for (int desplazamiento = -distancia; desplazamiento <= distancia; desplazamiento++)
                {
                    int x = mapaActual.JugadorX + desplazamiento;

                    if (x >= 1 && x < mapaActual.Columnas - 1 && EsTransitableMapa(mapaActual.Datos[mapaActual.JugadorY, x]))
                    {
                        mapaActual.JugadorX = x;
                        encontrado = true;
                        break;
                    }
                }

                if (encontrado)
                    break;
            }
        }

        puntoExtraccionX = mapaActual.JugadorX;
        puntoExtraccionY = mapaActual.JugadorY;
    }

    static void BuscarObjetivoMapa()
    {
        mapaActual.ObjetivoX = -1;
        mapaActual.ObjetivoY = -1;

        for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
        {
            for (int columna = 1; columna < mapaActual.Columnas - 1; columna++)
            {
                if (mapaActual.Datos[fila, columna] == 'A')
                {
                    mapaActual.ObjetivoY = fila;
                    mapaActual.ObjetivoX = columna;
                    return;
                }
            }
        }

        for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
        {
            for (int columna = 1; columna < mapaActual.Columnas - 1; columna++)
            {
                if (mapaActual.Datos[fila, columna] == '$' || mapaActual.Datos[fila, columna] == 'C' || mapaActual.Datos[fila, columna] == 'P')
                {
                    mapaActual.ObjetivoY = fila;
                    mapaActual.ObjetivoX = columna;
                    return;
                }
            }
        }
    }

    static void ExplorarMapaSector()
    {
        bool mostrandoMapa = true;



        DibujarMapaSector();
        Console.CursorVisible = false;

        try
        {
            while (mostrandoMapa && mision.SesionActiva)
            {
                ConsoleKey tecla = Console.ReadKey(true).Key;

                if (tecla == ConsoleKey.I)
                {
                    MostrarInfoMapa();
                    DibujarMapaSector();
                    Console.CursorVisible = false;
                }
                else if (tecla == ConsoleKey.Q || tecla == ConsoleKey.Escape)
                {
                    mostrandoMapa = false;
                }
                else if (tecla == ConsoleKey.UpArrow || tecla == ConsoleKey.W)
                {
                    MoverJugadorMapa(0, -1);
                }
                else if (tecla == ConsoleKey.DownArrow || tecla == ConsoleKey.S)
                {
                    MoverJugadorMapa(0, 1);
                }
                else if (tecla == ConsoleKey.LeftArrow || tecla == ConsoleKey.A)
                {
                    MoverJugadorMapa(-1, 0);
                }
                else if (tecla == ConsoleKey.RightArrow || tecla == ConsoleKey.D)
                {
                    MoverJugadorMapa(1, 0);
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
    }

    static bool EsPosicionDentroMapa(int x, int y)
    {


        return x > 0 && x < mapaActual.Columnas - 1 &&
               y > 0 && y < mapaActual.Filas - 1;
    }

    static void MoverJugadorMapa(int dx, int dy)
    {
        int viejoX = mapaActual.JugadorX;
        int viejoY = mapaActual.JugadorY;
        int nuevoX = viejoX + dx;
        int nuevoY = viejoY + dy;
        if (!EsPosicionDentroMapa(nuevoX, nuevoY))
        {
            MostrarAvisoFlotante("⚠ Límite del mapa. No puedes salir de la zona.");
            return;
        }

        char destino = mapaActual.Datos[nuevoY, nuevoX];

        if (destino == '#')
        {
            MostrarAvisoFlotante("⚠ Has llegado al borde del mapa.");
            return;
        }

        if (!EsTransitableMapa(destino))
        {
            MostrarAvisoFlotante("⚠ El terreno impide el desplazamiento.");
            return;
        }
        mapaActual.JugadorX = nuevoX;
        mapaActual.JugadorY = nuevoY;

        cadete.NivelEnergia -= 1;
        mision.TiempoMision += 2;
        LimitarValores();


        ActualizarCeldaMapa(viejoX, viejoY, mapaActual.Datos[viejoY, viejoX], false);
        ActualizarCeldaMapa(nuevoX, nuevoY, destino, true);
        ActualizarPanelMapaEnVivo();

        if (mapaActual.JugadorY == puntoExtraccionY &&
            mapaActual.JugadorX == puntoExtraccionX &&
            mision.ZonaAnomaliaAlcanzada &&
            mision.NucleoEnergeticoEncontrado &&
            mision.EntidadSuperada)
        {
            mision.PuntoExtraccionAlcanzado = true;
            CompletarMisionV2();
            return;
        }

        bool huboEvento = destino == '$' || destino == 'A' || destino == 'C' || destino == 'P';
        ResolverCasillaMapa(destino);



        if (huboEvento && mision.SesionActiva)
        {
            DibujarMapaSector();
            Console.CursorVisible = false;
        }
    }

    static void ActualizarCeldaMapa(int x, int y, char simbolo, bool esJugador)
    {
        if (x < 0 || y < 0 || x >= mapaActual.Columnas || y >= mapaActual.Filas)
            return;

        int filaPantalla = FILA_INICIO_MAPA + y;

        try
        {
            Console.SetCursorPosition(x, filaPantalla);

            if (esJugador)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(SIMBOLO_CADETE);
            }
            else
            {
                char visual = ObtenerSimboloVisualMapa(simbolo);
                AplicarColorMapa(simbolo);
                Console.Write(visual);
            }
        }
        catch (ArgumentOutOfRangeException)
        {

        }
        finally
        {
            Console.ResetColor();
        }
    }

    static char ObtenerSimboloVisualMapa(char simbolo)
    {

        if (simbolo == 'T') return '♣';
        if (simbolo == '^') return '▲';
        if (simbolo == '*') return '✦';
        if (simbolo == '$') return '◆';
        if (simbolo == 'C') return '☠';
        if (simbolo == 'A') return '⚡';
        if (simbolo == 'P') return 'ℹ';
        return simbolo;
    }

    static void ActualizarPanelMapaEnVivo()
    {
        int lineaEnergia = FILA_INICIO_MAPA + mapaActual.Filas + 2;
        int lineaCoordenadas = FILA_INICIO_MAPA + mapaActual.Filas + 5;

        try
        {

            Console.SetCursorPosition(0, lineaEnergia);
            Console.Write(new string(' ', ANCHO_PANEL_MAPA));
            Console.SetCursorPosition(0, lineaEnergia);
            Console.Write(" ENERGÍA ");
            MostrarBarraMapa(cadete.NivelEnergia, ConsoleColor.Green);
            Console.Write(" ESTABILIDAD ");
            MostrarBarraMapa(cadete.NivelConexion, ConsoleColor.Cyan);


            Console.SetCursorPosition(0, lineaCoordenadas);
            Console.Write(new string(' ', ANCHO_PANEL_MAPA));
            Console.SetCursorPosition(0, lineaCoordenadas);
            Color(ConsoleColor.White);
            Console.Write(" CADETE ");
            Color(ConsoleColor.Cyan);
            Console.Write(SIMBOLO_CADETE + " ");
            Color(ConsoleColor.Gray);
            Console.Write("POSICIÓN ACTUAL: [" + mapaActual.JugadorX + "," + mapaActual.JugadorY + "]");
        }
        catch (ArgumentOutOfRangeException)
        {

        }
        finally
        {
            Console.ResetColor();
        }
    }

    static void MostrarAvisoFlotante(string mensaje)
    {
        int linea = FILA_INICIO_MAPA + mapaActual.Filas + 9;

        try
        {
            Console.SetCursorPosition(0, linea);
            Console.Write(new string(' ', ANCHO_PANEL_MAPA));
            Console.SetCursorPosition(0, linea);
            Color(ConsoleColor.Red);
            Console.Write(mensaje);
            Console.ResetColor();
            Thread.Sleep(350);
            Console.SetCursorPosition(0, linea);
            Console.Write(new string(' ', ANCHO_PANEL_MAPA));
        }
        catch (ArgumentOutOfRangeException)
        {

        }
        finally
        {
            Console.ResetColor();
        }
    }

    static void ResolverCasillaMapa(char destino)
    {
        if (destino == '$')
        {
            Console.Clear();
            Color(ConsoleColor.Yellow);
            Console.WriteLine("🎁 OBJETO ENCONTRADO");
            Color(ConsoleColor.Gray);
            Console.WriteLine();
            Console.WriteLine("El escáner identifica una célula de energía.");
            inventario.Agregar("Célula de energía", "Energía", 25);
            mapaActual.Datos[mapaActual.JugadorY, mapaActual.JugadorX] = '.';
            mision.Descubrimientos++;
            AgregarDescubrimiento("Célula de energía recuperada en sector " + realidadActual.Sector);
            AgregarXP(20);
            Console.WriteLine("Añadido al inventario.");
            PausaMapa();
        }
        else if (destino == 'A')
        {
            mision.ZonaAnomaliaAlcanzada = true;
            Console.Clear();
            Color(ConsoleColor.Magenta);
            Console.WriteLine("⚡ ANOMALÍA DETECTADA");
            Color(ConsoleColor.Gray);
            Console.WriteLine();
            Console.WriteLine("Has llegado al núcleo de la zona anómala.");
            Console.WriteLine("La señal coincide con la misión: \"La anomalía perdida\".");
            AgregarXP(25);

            if (!mision.NucleoEnergeticoEncontrado && mision.AnomaliasAnalizadas > 0)
            {
                inventario.Agregar("Núcleo energético", "Misión", 0);
                mision.NucleoEnergeticoEncontrado = true;
                Color(ConsoleColor.Green);
                Console.WriteLine("★ NÚCLEO ENERGÉTICO RECUPERADO ★");
                Color(ConsoleColor.Gray);
                CompletarObjetivo("Encontrar el núcleo energético");
            }
            else if (!mision.NucleoEnergeticoEncontrado)
            {
                Console.WriteLine("Analiza la anomalía desde el menú para recuperar el núcleo energético.");
            }

            if (!mision.EntidadSuperada)
            {
                PausaMapa();
                IniciarEncuentroPersonaje();
            }
            else
            {
                PausaMapa();
            }
        }
        else if (destino == 'C')
        {
            mapaActual.Datos[mapaActual.JugadorY, mapaActual.JugadorX] = '.';
            IniciarEncuentroPersonaje();
        }
        else if (destino == 'P')
        {
            Console.Clear();
            Color(ConsoleColor.Cyan);
            Console.WriteLine("ℹ INFORMACIÓN ENCONTRADA");
            Color(ConsoleColor.Gray);
            Console.WriteLine();
            Console.WriteLine("El punto de interés contiene registros de una ruta de extracción.");
            mision.Descubrimientos++;
            AgregarDescubrimiento("Ruta de extracción localizada");
            AgregarXP(15);
            mapaActual.Datos[mapaActual.JugadorY, mapaActual.JugadorX] = '.';
            PausaMapa();
        }

        VerificarMisionV2();
    }

    static void PausaMapa()
    {
        Console.WriteLine();
        Console.WriteLine("Presione una tecla para continuar...");
        Console.ReadKey(true);
    }

    static void ColorMapa(char simbolo)
    {
        if (simbolo == 'C')
            Console.ForegroundColor = ConsoleColor.Red;
        else if (simbolo == 'A')
            Console.ForegroundColor = ConsoleColor.Magenta;
        else if (simbolo == 'E')
            Console.ForegroundColor = ConsoleColor.Yellow;
        else if (simbolo == 'R')
            Console.ForegroundColor = ConsoleColor.Green;
        else if (simbolo == '^' || simbolo == 'T')
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        else if (simbolo == '~')
            Console.ForegroundColor = ConsoleColor.Cyan;
        else if (simbolo == '#')
            Console.ForegroundColor = ConsoleColor.DarkGray;
        else if (simbolo == '.')
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        else
            Console.ForegroundColor = ConsoleColor.Gray;
    }

    static void DibujarAnimalGrande(int tipo)
    {
        Console.WriteLine();

        if (tipo == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("        /\\_/\\\\");
            Console.WriteLine("       ( o.o )");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("        > ^ <");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("      /|     |\\\\");
            Console.WriteLine("     /_|_____|_\\\\");
            Console.WriteLine("       /     \\\\");
        }
        else if (tipo == 1)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("         /\\");
            Console.WriteLine("        /  \\\\");
            Console.WriteLine("       / /\\ \\\\");
            Console.WriteLine("      / /  \\ \\\\");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("     (  o  o  )");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("      \\  --  /");
            Console.WriteLine("       \\____/");
            Console.WriteLine("        /||\\\\");
            Console.WriteLine("       /_||_\\\\");
        }
        else if (tipo == 2)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("        .-\"\"\"-.");
            Console.WriteLine("       /  _ _  \\\\");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("      |  (o o)  |");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("      |  \\_/   |");
            Console.WriteLine("       \\  ___  /");
            Console.WriteLine("        '-.__.-'");
            Console.WriteLine("       /|     |\\\\");
            Console.WriteLine("      /_|_____|_\\\\");
        }
        else if (tipo == 3)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("          /\\_/\\\\");
            Console.WriteLine("     ____/ o o \\\\____");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("    /      >^<      \\\\");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("   /  /|         |\\\\  \\\\");
            Console.WriteLine("  /__/ |_________| \\__\\\\");
            Console.WriteLine("      /_/       \\_\\\\");
        }
        else if (tipo == 4)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("            ___");
            Console.WriteLine("         __(o o)__");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("       /   \\_/   \\\\");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("     /___________\\\\");
            Console.WriteLine("    /  /       \\  \\\\");
            Console.WriteLine("   (_/          \\_ )");
        }

        Console.ResetColor();
    }

    static void DibujarMapaSector()
    {
        Console.Clear();
        DibujarCabeceraMapa();

        for (int fila = 0; fila < mapaActual.Filas; fila++)
        {
            for (int columna = 0; columna < mapaActual.Columnas; columna++)
            {
                char elemento = mapaActual.Datos[fila, columna];

                if (fila == mapaActual.JugadorY && columna == mapaActual.JugadorX)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(SIMBOLO_CADETE);
                }
                else
                {
                    AplicarColorMapa(elemento);
                    Console.Write(ObtenerSimboloVisualMapa(elemento));
                }
            }

            Console.WriteLine();
        }

        Console.ResetColor();
        DibujarPanelMapa();
    }

    static void DibujarCabeceraMapa()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ N E X U S // CARTOGRAFÍA                                                     ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════════════════════╣");
        Console.ResetColor();

        Console.Write("║ REALIDAD: ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(Ajustar(realidadActual.Nombre, 12));
        Console.ResetColor();

        Console.Write(" SECTOR: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(realidadActual.Sector.ToString().PadRight(4));
        Console.ResetColor();

        Console.Write(" ENTORNO: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(Ajustar(realidadActual.Entorno, 12));
        Console.ResetColor();
        Console.WriteLine("║");

        Console.Write("║ ACTIVIDAD: ");
        Console.Write(Ajustar(realidadActual.Actividad, 9));
        Console.Write(" SEÑAL: ");

        if (realidadActual.Senal == "Anómala")
            Console.ForegroundColor = ConsoleColor.Magenta;
        else
            Console.ForegroundColor = ConsoleColor.Cyan;

        Console.Write(Ajustar(realidadActual.Senal, 10));
        Console.ResetColor();

        Console.Write(" RIESGO: ");

        if (realidadActual.Riesgo == "Crítico")
            Console.ForegroundColor = ConsoleColor.Red;
        else if (realidadActual.Riesgo == "Alto")
            Console.ForegroundColor = ConsoleColor.Yellow;
        else
            Console.ForegroundColor = ConsoleColor.Green;

        Console.Write(Ajustar(realidadActual.Riesgo, 8));
        Console.ResetColor();
        Console.WriteLine("║");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    static void AplicarColorMapa(char elemento)
    {
        switch (elemento)
        {
            case '^':
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                break;
            case 'T':
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case '*':
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case '.':
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                break;
            case ':':
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case '~':
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case '=':
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case '#':
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
            case 'H':
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case 'W':
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case 'O':
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case '$':
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case 'P':
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case 'C':
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case 'A':
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            case 'a':
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                break;
            case '?':
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
        }
    }

    static void DibujarPanelMapa()
    {
        Console.WriteLine();
        Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");
        Console.Write(" ENERGÍA ");
        MostrarBarraMapa(cadete.NivelEnergia, ConsoleColor.Green);
        Console.Write(" ESTABILIDAD ");
        MostrarBarraMapa(cadete.NivelConexion, ConsoleColor.Cyan);
        Console.WriteLine();
        Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");
        Console.WriteLine(" CARTOGRAFÍA REGISTRADA: " + CalcularCartografia() + "%");
        MostrarCoordenadasMapa();
        Console.WriteLine(" [WASD/FLECHAS] Moverse [I] Escáner [Q] Salir del mapa");
        Console.WriteLine();
        MostrarLeyendaMapa();

        if (mapaActual.TieneAnomalia)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("⚠ DISTORSIÓN DIMENSIONAL ACTIVA EN ESTE SECTOR");
            Console.ResetColor();
        }
    }

    static void MostrarBarraMapa(int valor, ConsoleColor color)
    {
        int llenos = valor / 10;

        if (llenos > 10)
            llenos = 10;

        if (llenos < 0)
            llenos = 0;

        Console.ForegroundColor = color;
        Console.Write("[");

        for (int i = 0; i < 10; i++)
        {
            if (i < llenos)
                Console.Write("█");
            else
                Console.Write("░");
        }

        Console.Write("]");
        Console.ResetColor();
        Console.Write(" " + valor.ToString().PadLeft(3) + "% ");
    }

    static void MostrarLeyendaMapa()
    {
        Console.Write(" ");

        Console.Write(SIMBOLO_CADETE + " Cadete ");
        Console.Write("♣ Árbol ");
        Console.Write(". Zona registrada ");
        Console.Write("~ Agua ");
        Console.Write("◆ Recurso ");
        Console.Write("☠ Amenaza ");
        Console.Write("⚡ Anomalía ");
        Console.Write("ℹ Info");
        Console.Write("H Base/origen ⌂ ");
        Console.Write("P Punto de interés ● ");
        Console.WriteLine("? Zona desconocida");
    }

    static int CalcularCartografia()
    {
        int conocidas = 0;
        int total = 0;

        for (int fila = 1; fila < mapaActual.Filas - 1; fila++)
        {
            for (int columna = 1; columna < mapaActual.Columnas - 1; columna++)
            {
                total++;

                if (mapaActual.Datos[fila, columna] != '?')
                    conocidas++;
            }
        }

        if (total == 0)
            return 0;

        return conocidas * 100 / total;
    }

    static void MostrarCoordenadasMapa()
    {
        if (mapaActual.ObjetivoX >= 0 && mapaActual.ObjetivoY >= 0)
        {
            Console.WriteLine(" COORDENADAS DEL PUNTO DE INTERÉS: [" + mapaActual.ObjetivoX + "," + mapaActual.ObjetivoY + "]");
        }
        else
        {
            Console.WriteLine(" COORDENADAS DEL PUNTO DE INTERÉS: [--,--]");
        }
    }

    static void MostrarInfoMapa()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ NEXUS // ESCÁNER                                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("REALIDAD : " + realidadActual.Nombre);
        Console.WriteLine("SECTOR : " + realidadActual.Sector);
        Console.WriteLine("ENTORNO : " + realidadActual.Entorno);
        Console.WriteLine("ACTIVIDAD : " + realidadActual.Actividad);
        Console.WriteLine("SEÑAL : " + realidadActual.Senal);
        Console.WriteLine("RIESGO : " + realidadActual.Riesgo);
        Console.WriteLine();

        Console.WriteLine("ANOMALÍA : " + (mapaActual.TieneAnomalia ? "DETECTADA" : "NO DETECTADA"));
        Console.WriteLine("CRIATURA : " + (mapaActual.TieneCriatura ? "DETECTADA" : "NO DETECTADA"));
        Console.WriteLine("RECURSO : " + (mapaActual.TieneRecurso ? "POSIBLE" : "NO DETECTADO"));
        Console.WriteLine("ESTRUCTURA : " + (mapaActual.TieneEstructura ? "DETECTADA" : "NO DETECTADA"));
        Console.WriteLine();

        int distancia = 0;

        if (mapaActual.ObjetivoX >= 0 && mapaActual.ObjetivoY >= 0)
            distancia = Math.Abs(mapaActual.JugadorX - mapaActual.ObjetivoX) + Math.Abs(mapaActual.JugadorY - mapaActual.ObjetivoY);

        Console.WriteLine("DISTANCIA AL OBJETIVO: " + distancia);
        Console.WriteLine("COORDENADAS DEL OBJETIVO: [" + (mapaActual.ObjetivoX >= 0 ? mapaActual.ObjetivoX.ToString() : "--") + "," + (mapaActual.ObjetivoY >= 0 ? mapaActual.ObjetivoY.ToString() : "--") + "]");
        Console.WriteLine("CARTOGRAFÍA REGISTRADA: " + CalcularCartografia() + "%");
        Console.WriteLine();

        if (mapaActual.TieneAnomalia)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("LECTURA: El sector presenta una alteración dimensional.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("LECTURA: No se detecta una distorsión dimensional.");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Presione una tecla para regresar al mapa...");
        Console.ReadKey(true);
    }

    static void MostrarResultadoExploracion()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ RESULTADO DE EXPLORACIÓN                           ║");
        Console.WriteLine("╠════════════════════════════════════════════════════╣");
        Console.WriteLine("║ REALIDAD : " + Ajustar(realidadActual.Nombre, 39) + "║");
        Console.WriteLine("║ SECTOR : " + realidadActual.Sector.ToString().PadRight(41) + "║");
        Console.WriteLine("║ ENTORNO : " + Ajustar(realidadActual.Entorno, 39) + "║");
        Console.WriteLine("║ ACTIVIDAD : " + Ajustar(realidadActual.Actividad, 37) + "║");
        Console.WriteLine("║ SEÑAL : " + Ajustar(realidadActual.Senal, 41) + "║");
        Console.WriteLine("║ RIESGO : " + Ajustar(realidadActual.Riesgo, 40) + "║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        if (realidadActual.Riesgo == "Crítico")
        {
            Color(ConsoleColor.Red);
            Console.WriteLine("⚠ NIVEL DE RIESGO CRÍTICO");
            Color(ConsoleColor.Gray);
        }
        else if (realidadActual.Riesgo == "Alto")
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine("⚠ Se recomienda precaución.");
            Color(ConsoleColor.Gray);
        }
        else
        {
            Color(ConsoleColor.Green);
            Console.WriteLine("✓ El sector parece estable.");
            Color(ConsoleColor.Gray);
        }
    }

    static void DetectarCriaturaPorEntorno()
    {
        int posibilidad;

        if (realidadActual.Actividad == "Alta")
            posibilidad = 65;
        else if (realidadActual.Actividad == "Media")
            posibilidad = 45;
        else if (realidadActual.Actividad == "Baja")
            posibilidad = 25;
        else
            posibilidad = 10;

        if (realidadActual.Riesgo == "Alto")
            posibilidad += 10;

        if (realidadActual.Riesgo == "Crítico")
            posibilidad += 15;

        if (random.Next(1, 101) > posibilidad)
            return;

        int[] posibles = new int[20];
        int cantidadPosibles = ObtenerCriaturasDelEntorno(posibles);

        if (cantidadPosibles == 0)
            return;

        int indice = posibles[random.Next(cantidadPosibles)];
        RegistrarCriatura(indice, false);
    }

    static int ObtenerCriaturasDelEntorno(int[] indices)
    {
        int cantidad = 0;

        for (int i = 0; i < criaturas.Count; i++)
        {
            if (criaturas[i].Entorno == realidadActual.Entorno && !criaturas[i].Legendaria)
            {
                indices[cantidad] = i;
                cantidad++;
            }
        }

        if (cantidad == 0)
            CrearCriaturas();

        cantidad = 0;

        for (int i = 0; i < criaturas.Count; i++)
        {
            if (criaturas[i].Entorno == realidadActual.Entorno && !criaturas[i].Legendaria)
            {
                indices[cantidad] = i;
                cantidad++;
            }
        }

        return cantidad;
    }

    static void CrearCriaturas()
    {
        if (criaturas.Count > 0)
            return;

        AgregarCriatura("Lobo Gris", "Animal", "Boscoso", "Media", "No",
            "Manada de lobos adaptada a zonas boscosas.",
            "Mantener distancia y evitar movimientos bruscos.",
            "      / \\__\n     (    o\\___\n     /         O\n    /   (_____)\n/_____/   U");

        AgregarCriatura("Ciervo", "Animal", "Boscoso", "Baja", "Sí",
            "Mamífero herbívoro de comportamiento tranquilo.",
            "Puede observarse sin acercarse demasiado.",
            "        /\\\n       /  \\__\n      /      \\\n     /  o  o  \\\n    /    __    \\\n   /____/  \\____\\\n      /|  |\\\n     /_|__|_\\");

        AgregarCriatura("Oso Pardo", "Animal", "Boscoso", "Alta", "No",
            "Gran depredador territorial.",
            "No acercarse. Evitar provocar contacto directo.",
            "       /\\_/\\\n      ( o.o )\n       > ^ <\n      /|   |\\\n     (_|   |_)\n        / \\\n       /___\\");

        AgregarCriatura("Zorro Rojo", "Animal", "Boscoso", "Baja", "Sí",
            "Pequeño mamífero inteligente y cauteloso.",
            "Observar desde distancia. Puede acostumbrarse al cadete.",
            "        /\\___/\\\n       /  o o  \\\n      (    ^    )\n       \\  ---  /\n      /|       |\\\n     /_|_______|_\\\n       /       \\\n      /_/       \\_\\");

        AgregarCriatura("Zorro del Desierto", "Animal", "Desértico", "Baja", "Sí",
            "Pequeño depredador nocturno adaptado al calor.",
            "No perseguir. El animal evitará al cadete.",
            "        ____\n      /      \\__\n     /  o        \\\n    |      __     |\n    |_____/  \\____|\n       /  ||  \\\n      /___||___\\");

        AgregarCriatura("Camello", "Animal", "Desértico", "Media", "Sí",
            "Mamífero resistente a ambientes extremadamente secos.",
            "Mantener distancia de la cabeza y extremidades.",
            "        /\\       /\\\n       /  \\_____/  \\\n      /   ( o o )   \\\n     |      \\_/      |\n      \\   .---.     /\n       \\_/     \\___/\n          \\  /\n        __/  \\__");

        AgregarCriatura("Escorpión Gigante", "Criatura", "Desértico", "Alta", "No",
            "Artrópodo de tamaño anormal.",
            "No intentar acercamiento. Presenta veneno.",
            "             __\n        _____/  \\__\n     __/            \\__\n   _/     o            \\\n  /     __      __       \\\n <_____/  \\____/  \\_______>\n        \\          /\n         \\________/");

        AgregarCriatura("Delfín", "Animal", "Acuático", "Baja", "Sí",
            "Mamífero acuático altamente inteligente.",
            "La interacción pasiva es segura.",
            "      __________________\n  ___/  o  o  o  o       \\__\n /     ______________        \\\n|_____/  _   _   _  \\_________|\n      \\_/ \\_/ \\_/ \\_/");

        AgregarCriatura("Cocodrilo", "Animal", "Acuático", "Alta", "No",
            "Depredador semiacuático territorial.",
            "Nunca acercarse al borde del agua.",
            "          /\\\n     _____/  \\_____\n   /              __\\\n  /   /\\        __/  \\\n <___/  \\______/      )\n      \\______________/");

        AgregarCriatura("Tiburón", "Animal", "Acuático", "Alta", "No",
            "Depredador marino.",
            "Evitar el contacto y abandonar el área.",
            "          /\\_/\\\n     ____/ o o \\____\n    /      \\_/       \\\n   /   __       __    \\\n  /___/  \\_____/  \\____\\\n      \\        /\n       \\______/");

        AgregarCriatura("Murciélago", "Animal", "Subterráneo", "Baja", "No",
            "Mamífero nocturno adaptado a cavernas.",
            "No representa amenaza si no se le molesta.",
            "        /\\        /\\\n       /  \\______/  \\\n      /              \\\n     (   o        o   )\n      \\      __      /\n       \\____/  \\____/\n          /|  |\\\n         /_|__|_\\");

        AgregarCriatura("Araña de Caverna", "Criatura", "Subterráneo", "Media", "No",
            "Arácnido de comportamiento territorial.",
            "Evitar las zonas con telarañas.",
            "        .-.\n       /   \\\n      / o o \\\n     (   ^   )\n      \\ \\_/ /\n       |   |\n     __|___|__\n    /   / \\   \\\n   /___/   \\___\\");

        AgregarCriatura("Topo Gigante", "Criatura", "Subterráneo", "Media", "Sí",
            "Criatura excavadora de gran tamaño.",
            "Puede domesticarse con alimento adecuado.",
            "        .-.\n      _(   )_\n     /  o o  \\\n    /    ^    \\\n    \\  .---.  /\n     \\/_____\\/\n      /|   |\\\n     /_|___|_\\");

        AgregarCriatura("Cuervo", "Animal", "Urbano", "Baja", "Sí",
            "Ave inteligente adaptada a zonas urbanas.",
            "Puede observarse y atraer mediante alimento.",
            "       __\n   ___/  \\___\n  /  o      o \\\n |      __     |\n |  ___/  \\___ |\n  \\____________/");

        AgregarCriatura("Perro Errante", "Animal", "Urbano", "Media", "Sí",
            "Canino doméstico sin propietario identificado.",
            "Puede aproximarse lentamente y ofrecer alimento.",
            "        /\\_/\\\n       ( o.o )\n        \\ ^ /\n       /|   |\\\n      /_|___|_\\\n        /   \\\n       /_____\\");

        AgregarCriatura("Gato Nocturno", "Animal", "Urbano", "Baja", "Sí",
            "Felino doméstico adaptado a zonas urbanas.",
            "La aproximación debe ser lenta.",
            "       .-.\n      (o o)\n       |^|\n    ___/ \\___\n   /   ???   \\\n  /___________\\\n      / | \\\n     /__|__\\");

        AgregarCriatura("Entidad Faunística Desconocida", "Criatura", "Desconocido", "Alta", "No",
            "Forma de vida biológica no clasificada.",
            "No acercarse hasta obtener más datos.",
            "          /\\___/\\\n       ___/  o o  \\___\n      /     \\ ^ /     \\\n     /  /\\   \\_/   /\\  \\\n    /__/  \\_______/  \\__\\\n       \\      |      /\n        \\_____|_____/\n          /   \\\n         /_____\\");

        AgregarCriatura("Minotauro", "Mítica", "Laberinto dimensional", "Crítica", "No",
            "Entidad humanoide con rasgos bovinos. Su origen parece estar vinculado a estructuras dimensionales cerradas.",
            "NO INTERACTUAR. Abandonar el sector si detecta actividad hostil.",
            "            /\\\n       _____/  \\_____\n      /  _  o o  _   \\\n     /__/ \\  ^  / \\___\\\n        \\  \\___/  /\n         \\_/   \\_/\n       __/       \\__\n      /___/     \\___\\");

        AgregarCriatura("Grifo", "Mítica", "Zona anómala", "Alta", "No",
            "Criatura híbrida con características de águila y felino. Presenta comportamiento territorial.",
            "No aproximarse. Mantener perfil bajo y evitar contacto visual prolongado.",
            "          /\\\n      ____/  \\____\n     /   o    o   \\\n    /      ^       \\\n   |    .------.    |\n   |   /  / \\   \\   |\n    \\  \\_/___\\_/  /\n     \\_____|_____/\n       /   |   \\\n      /____|____\\");

        AgregarCriatura("Basilisco", "Mítica", "Zona anómala", "Crítica", "No",
            "Entidad reptiliana asociada con efectos neurotóxicos y alteraciones perceptivas.",
            "NO OBSERVAR DIRECTAMENTE. Retirarse inmediatamente.",
            "             .-.\n          .-(   )-.\n         /  o   o  \\\n        |     ^     |\n        |   \\___/   |\n         \\  / | \\  /\n       ___\\/  |  \\/___\n      /     \\ | /     \\\n     /_______\\|/_______\\\n        /    / \\    \\\n       /____/   \\____\\");

        AgregarCriatura("Dragón", "Mítica", "Zona anómala extrema", "Crítica", "No",
            "Entidad de escala gigantesca con capacidad energética desconocida.",
            "EVACUACIÓN INMEDIATA. No intentar combatir ni establecer contacto.",
            "            /\\\n          _/  \\_\n       __/  o o  \\__\n      /      ^      \\\n     /   .-------.   \\\n    |   /  /\\_/\\  \\   |\n    |   \\_/     \\_/   |\n     \\       |       /\n      \\_____/ \\_____/\n         /   |   \\\n        /____|____\\");

        AgregarCriatura("Fénix", "Mítica", "Anomalía energética", "Alta", "No",
            "Entidad aviar compuesta por una concentración energética de naturaleza desconocida.",
            "No tocar. Mantener distancia y registrar la actividad energética.",
            "             /\\\n          __/  \\__\n       __/  o  o  \\__\n      /      ^      \\\n     /   .-------.   \\\n    /___/  /| |\\  \\___\\\n       /__/ | | \\__\\\n          \\_|_/\n        __/   \\__\n       /_________\\");
    }

    static void AgregarCriatura(string nombreC, string categoria, string entornoC,
        string peligro, string domesticable, string descripcion, string recomendacion,
        string grafico, bool legendaria = false)
    {
        Criatura criatura = new Criatura(
            nombreC, categoria, entornoC, peligro, domesticable,
            descripcion, recomendacion, grafico, legendaria);

        criaturas.Add(criatura);
    }

    static int BuscarCriatura(string nombreBuscado)
    {
        for (int i = 0; i < criaturas.Count; i++)
        {
            if (criaturas[i].Nombre == nombreBuscado)
                return i;
        }

        return -1;
    }

    static void RegistrarCriatura(int indice, bool legendaria)
    {
        CrearCriaturas();

        if (ExisteCriatura(criaturas[indice].Nombre))
            return;

        if (legendaria)
            criaturas[indice].MarcarComoLegendaria();

        mision.CriaturaActual = criaturas[indice].Nombre;
        mision.Descubrimientos++;

        AgregarDescubrimiento("Criatura descubierta: " + criaturas[indice].Nombre);

        Console.WriteLine();

        if (legendaria)
            MostrarEncuentroLegendario(indice);
        else
            MostrarEncuentroCriatura(indice);
    }

    static bool ExisteCriatura(string nombreBuscado)
    {
        for (int i = 0; i < criaturas.Count; i++)
        {
            if (criaturas[i].Nombre == nombreBuscado && criaturas[i].Legendaria)
                return true;
        }

        return false;
    }

    static void MostrarEncuentroCriatura(int indice)
    {
        Criatura criatura = criaturas[indice];

        Console.Clear();
        Color(ConsoleColor.Green);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ VIDA DETECTADA                                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("El escáner ha detectado una forma de vida.");
        Console.WriteLine();
        Color(ConsoleColor.Cyan);
        Console.WriteLine(criatura.Grafico);
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("ESPECIE : " + criatura.Nombre);
        Console.WriteLine("CATEGORÍA : " + criatura.Categoria);
        Console.WriteLine("PELIGROSIDAD : " + criatura.Peligro);
        Console.WriteLine("DOMESTICABLE : " + criatura.Domesticable);
        Console.WriteLine();
        Console.WriteLine(criatura.Descripcion);
        Console.WriteLine();
        MostrarRecomendacion(indice);

        if (criatura.EsDomesticable())
            IntentarDomesticar(indice);

        AgregarXP(15);
    }

    static void MostrarEncuentroLegendario(int indice)
    {
        Criatura criatura = criaturas[indice];

        Console.WriteLine();
        Color(ConsoleColor.Magenta);
        Console.WriteLine("★ ★ ★ ★ ★ ★");
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ ★ ENTIDAD LEGENDARIA ★                             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("★ ★ ★ ★ ★ ★");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("El sistema ha registrado una presencia");
        Console.WriteLine("que no pertenece al catálogo biológico común.");
        Console.WriteLine();
        Color(ConsoleColor.Yellow);
        Console.WriteLine(criatura.Grafico);
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("ENTIDAD : " + criatura.Nombre);
        Console.WriteLine("CATEGORÍA : " + criatura.Categoria);
        Console.WriteLine("PELIGROSIDAD : " + criatura.Peligro);
        Console.WriteLine();
        Console.WriteLine(criatura.Descripcion);
        Console.WriteLine();
        MostrarRecomendacion(indice);
        mision.EntidadEncontrada = true;
        mision.ArchivoSecreto = true;
        AgregarXP(60);
        AgregarDescubrimiento("Entidad legendaria registrada: " + criatura.Nombre);
    }

    static void MostrarRecomendacion(int indice)
    {
        Criatura criatura = criaturas[indice];

        Console.WriteLine("RECOMENDACIÓN NEXUS");
        Console.WriteLine("──────────────────────────────────────────────────────");

        if (criatura.Peligro == "Crítica")
        {
            Color(ConsoleColor.Red);
            Console.WriteLine("⚠ PELIGRO CRÍTICO");
        }
        else if (criatura.Peligro == "Alta")
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine("⚠ PELIGRO ALTO");
        }
        else if (criatura.RequierePrecaucion())
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine("PRECAUCIÓN");
        }
        else
        {
            Color(ConsoleColor.Green);
            Console.WriteLine("✓ NIVEL DE RIESGO BAJO");
        }

        Console.WriteLine(criatura.Recomendacion);
        Color(ConsoleColor.Gray);
    }

    static void IntentarDomesticar(int indice)
    {
        Console.WriteLine();
        Console.WriteLine("El sistema ha detectado potencial de domesticación.");
        Console.WriteLine();
        Console.WriteLine("[1] Intentar domesticar");
        Console.WriteLine("[2] Dejarla tranquila");

        int opcion;

        if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
            opcion = 2;

        if (opcion != 1)
        {
            Console.WriteLine();
            Console.WriteLine("El cadete decide no intervenir.");
            return;
        }

        Console.WriteLine();
        CargarBarra("Analizando comportamiento", 12);

        int probabilidad = 50;

        Criatura criatura = criaturas[indice];

        if (criatura.Peligro == "Baja")
            probabilidad += 25;

        if (criatura.Peligro == "Media")
            probabilidad += 5;

        if (mision.HabilidadEstabilizacion)
            probabilidad += 10;

        int resultado = random.Next(1, 101);

        if (resultado <= probabilidad)
        {
            Color(ConsoleColor.Green);
            Console.WriteLine();
            Console.WriteLine("★ DOMESTICACIÓN EXITOSA ★");
            Color(ConsoleColor.Gray);
            Console.WriteLine(criatura.Nombre + " ha aceptado al cadete.");

            if (!criatura.Domesticada)
            {
                criatura.MarcarComoDomesticada();
                mision.CantidadDomesticadas++;
            }

            AgregarXP(30);
        }
        else
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine();
            Console.WriteLine("La criatura no confía en el cadete.");
            Color(ConsoleColor.Gray);
            Console.WriteLine("El sistema recomienda no insistir.");
            cadete.NivelConexion -= 3;
            LimitarValores();
        }
    }

    static bool EstaDomesticada(string nombreBuscado)
    {
        int indice = BuscarCriatura(nombreBuscado);

        if (indice < 0)
            return false;

        return criaturas[indice].Domesticada;
    }

    static void EventoAnomalia()
    {
        mision.EventosEncontrados++;

        Console.WriteLine();
        Color(ConsoleColor.Magenta);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ ⚠ ANOMALÍA DETECTADA ⚠                            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("La realidad ha producido una lectura irregular.");
        Console.WriteLine();

        if (anomaliaActual.Intensidad == 0)
            anomaliaActual.Intensidad = random.Next(20, 101);

        DetectarCriaturaLegendaria();

        Console.WriteLine("El sistema espera una decisión.");
        Console.WriteLine();
        Console.WriteLine("[1] Analizar");
        Console.WriteLine("[2] Evitar");
        Console.WriteLine("[3] Interactuar");
        Console.WriteLine("[4] Registrar y retirarse");

        int opcion;

        if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
            opcion = 4;

        mision.DecisionesTomadas++;

        switch (opcion)
        {
            case 1:
                AnalizarAnomalias();
                break;
            case 2:
                Console.WriteLine();
                Console.WriteLine("El cadete evita la anomalía.");
                cadete.NivelConexion += 3;
                mision.TiempoMision += 10;
                AgregarXP(10);
                break;
            case 3:
                InteractuarAnomalia();
                break;
            case 4:
                Console.WriteLine();
                Console.WriteLine("La anomalía ha sido registrada.");
                AgregarDescubrimiento("Anomalía registrada en sector " + realidadActual.Sector);
                mision.Descubrimientos++;
                AgregarXP(15);
                break;
            default:
                Console.WriteLine();
                Console.WriteLine("Decisión no reconocida.");
                Console.WriteLine("El sistema recomienda retirarse.");
                cadete.NivelConexion -= 5;
                break;
        }

        LimitarValores();
    }

    static void DetectarCriaturaLegendaria()
    {
        int posibilidad;

        if (anomaliaActual.Intensidad >= 80)
            posibilidad = 65;
        else if (anomaliaActual.Intensidad >= 60)
            posibilidad = 45;
        else if (anomaliaActual.Intensidad >= 40)
            posibilidad = 25;
        else
            posibilidad = 10;

        if (realidadActual.Nombre == "VOID-42")
            posibilidad += 15;

        if (mision.HabilidadEscaneo)
            posibilidad += 5;

        if (random.Next(1, 101) > posibilidad)
            return;

        CrearCriaturas();

        int[] legendarias = new int[10];
        int cantidad = 0;

        for (int i = 0; i < criaturas.Count; i++)
        {
            if (criaturas[i].Legendaria)
            {
                legendarias[cantidad] = i;
                cantidad++;
            }
        }

        if (cantidad > 0)
        {
            int indice = legendarias[random.Next(cantidad)];
            MostrarEncuentroLegendario(indice);
        }
    }

    static void AnalizarAnomalias()
    {
        if (!mision.RealidadExplorada)
        {
            MensajeError("Primero debe explorar una realidad.");
            return;
        }

        if (realidadActual.Senal != "Anómala")
        {
            Console.WriteLine("No se ha detectado ninguna anomalía.");
            Console.WriteLine("Señal actual: " + realidadActual.Senal);
            return;
        }

        if (cadete.NivelEnergia < 10)
        {
            MensajeError("Energía insuficiente para realizar el análisis.");
            return;
        }

        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ ANÁLISIS DE ANOMALÍA                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        CargarBarra("Procesando patrones", 20);
        CargarBarra("Comparando registros", 20);
        CargarBarra("Determinando origen", 20);

        string[] tipos =
        {
            "Interferencia dimensional",
            "Distorsión temporal",
            "Alteración gravitacional",
            "Señal desconocida",
            "Fragmentación de realidad"
        };

        string[] origenes =
        {
            "Desconocido",
            "Sector externo",
            "Fuente energética",
            "Entidad no identificada",
            "Realidad desconocida"
        };

        string[] informaciones =
        {
            "Presenta una frecuencia repetitiva.",
            "Su patrón cambia constantemente.",
            "Se detecta una fuente energética cercana.",
            "La señal no coincide con registros anteriores.",
            "Se ha detectado una alteración temporal."
        };

        anomaliaActual.Tipo = tipos[random.Next(tipos.Length)];
        anomaliaActual.Intensidad = random.Next(1, 101);
        anomaliaActual.Origen = origenes[random.Next(origenes.Length)];
        anomaliaActual.InformacionAdicional = informaciones[random.Next(informaciones.Length)];

        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ RESULTADO DEL ANÁLISIS                             ║");
        Console.WriteLine("╠════════════════════════════════════════════════════╣");
        Console.WriteLine("║ TIPO : " + Ajustar(anomaliaActual.Tipo, 43) + "║");
        Console.WriteLine("║ INTENSIDAD : " + anomaliaActual.Intensidad.ToString().PadRight(37) + "║");
        Console.WriteLine("║ ORIGEN : " + Ajustar(anomaliaActual.Origen, 41) + "║");
        Console.WriteLine("║ INFORMACIÓN: " + Ajustar(anomaliaActual.InformacionAdicional, 36) + "║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");

        cadete.NivelEnergia -= 10;
        cadete.NivelConexion -= 7;
        mision.AnomaliasAnalizadas++;
        mision.TiempoMision += 20;

        AgregarXP(40);

        if (anomaliaActual.Intensidad >= 80)
        {
            Color(ConsoleColor.Red);
            Console.WriteLine();
            Console.WriteLine("⚠ INTENSIDAD EXTREMA DETECTADA");
            Console.WriteLine("La anomalía está reaccionando al análisis.");
            Color(ConsoleColor.Gray);
            cadete.NivelConexion -= 10;
            DetectarCriaturaLegendaria();
        }

        if (mision.AnomaliasAnalizadas >= 1 && !mision.ObjetivoAnomalia)
        {
            mision.ObjetivoAnomalia = true;
            CompletarObjetivo("Analizar 1 anomalía");
        }

        if (mision.ZonaAnomaliaAlcanzada && !mision.NucleoEnergeticoEncontrado)
        {
            inventario.Agregar("Núcleo energético", "Misión", 0);
            mision.NucleoEnergeticoEncontrado = true;
            Console.WriteLine();
            Color(ConsoleColor.Green);
            Console.WriteLine("★ NÚCLEO ENERGÉTICO RECUPERADO ★");
            Color(ConsoleColor.Gray);
            Console.WriteLine("El objetivo principal de la anomalía ha sido localizado.");
            CompletarObjetivo("Encontrar el núcleo energético");
        }

        AgregarDescubrimiento(anomaliaActual.Tipo + " detectada en sector " + realidadActual.Sector);
        mision.Descubrimientos++;

        LimitarValores();
        VerificarHabilidades();
    }

    static void InteractuarAnomalia()
    {
        Console.WriteLine();
        Console.WriteLine("Estableciendo enlace con la anomalía...");
        CargarBarra("Sincronizando frecuencia", 15);

        int resultado = random.Next(1, 101);

        if (resultado <= 35)
        {
            Color(ConsoleColor.Green);
            Console.WriteLine();
            Console.WriteLine("La interacción fue exitosa.");
            Console.WriteLine("Se ha obtenido información desconocida.");
            Color(ConsoleColor.Gray);

            cadete.NivelConexion += 5;
            cadete.NivelEnergia -= 5;
            mision.Descubrimientos++;
            AgregarXP(50);
            AgregarDescubrimiento("Información desconocida obtenida en sector " + realidadActual.Sector);
        }
        else if (resultado <= 75)
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine();
            Console.WriteLine("La anomalía reaccionó violentamente.");
            Color(ConsoleColor.Gray);

            cadete.NivelConexion -= 18;
            cadete.NivelEnergia -= 8;
            AgregarXP(20);
        }
        else
        {
            Color(ConsoleColor.Red);
            Console.WriteLine();
            Console.WriteLine("⚠ LA ANOMALÍA HA DETECTADO AL CADETE.");
            Console.WriteLine("El enlace ha sido interrumpido.");
            Color(ConsoleColor.Gray);

            cadete.NivelConexion -= 25;
            cadete.NivelEnergia -= 15;
            AgregarXP(10);
        }

        LimitarValores();
    }

    static void EventoAleatorio()
    {
        int evento = random.Next(1, 101);

        if (evento <= 20)
        {
            Console.WriteLine();
            Color(ConsoleColor.Cyan);
            Console.WriteLine("EVENTO: FUENTE ENERGÉTICA ENCONTRADA");
            Color(ConsoleColor.Gray);

            cadete.NivelEnergia += 15;
            mision.Descubrimientos++;
            AgregarDescubrimiento("Fuente energética encontrada en sector " + realidadActual.Sector);
            AgregarXP(20);
        }
        else if (evento <= 35)
        {
            Console.WriteLine();
            Color(ConsoleColor.Yellow);
            Console.WriteLine("EVENTO: DISTORSIÓN TEMPORAL");
            Color(ConsoleColor.Gray);

            cadete.NivelConexion -= 12;
            mision.TiempoMision += 15;
            AgregarXP(15);
        }
        else if (evento <= 45)
        {
            Console.WriteLine();
            Color(ConsoleColor.Magenta);
            Console.WriteLine("EVENTO: SEÑAL DESCONOCIDA");
            Color(ConsoleColor.Gray);

            mision.Descubrimientos++;
            AgregarDescubrimiento("Señal desconocida detectada en sector " + realidadActual.Sector);
            AgregarXP(25);
        }
        else if (evento <= 50)
        {
            EntidadEspecial();
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("No se detectaron eventos adicionales.");
        }

        LimitarValores();
    }

    static void EntidadEspecial()
    {
        mision.EntidadEncontrada = true;

        Console.WriteLine();
        Color(ConsoleColor.Magenta);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ ⚠ CONTACTO DESCONOCIDO                             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("El sistema no reconoce la forma de vida.");
        Console.WriteLine();
        Console.WriteLine("[1] Investigar");
        Console.WriteLine("[2] Ocultarse");
        Console.WriteLine("[3] Abandonar sector");

        int opcion;

        if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
            opcion = 3;

        if (opcion == 1)
        {
            Console.WriteLine();
            Console.WriteLine("La entidad permanece inmóvil.");

            int resultado = random.Next(1, 101);

            if (resultado <= 50)
            {
                Console.WriteLine("La entidad transmite una secuencia de datos.");
                mision.Descubrimientos++;
                cadete.NivelConexion += 8;
                AgregarXP(60);
                AgregarDescubrimiento("Contacto con entidad desconocida");
            }
            else
            {
                Console.WriteLine("La entidad desaparece.");
                cadete.NivelConexion -= 15;
            }
        }
        else if (opcion == 2)
        {
            Console.WriteLine();
            Console.WriteLine("El cadete permanece oculto.");
            cadete.NivelConexion += 3;
            AgregarXP(15);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("El sector ha sido abandonado.");
            cadete.NivelEnergia -= 5;
            AgregarXP(10);
        }

        LimitarValores();
    }

    static void CompletarMisionV2()
    {
        if (mision.MisionCompletada)
            return;

        mision.MisionCompletada = true;
        mision.RecompensaEntregada = true;

        Color(ConsoleColor.Green);
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ 🟢 MISIÓN COMPLETADA                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("La anomalía fue investigada y el cadete regresó con vida.");
        Console.WriteLine("+100 XP");
        Console.WriteLine("+1 módulo desconocido");
        inventario.Agregar("Módulo desconocido", "Recompensa", 0);
        AgregarXP(100);
        AgregarDescubrimiento("Misión completada: La anomalía perdida");
    }

    static void InicializarInventario()
    {
        inventario.Agregar("Célula de energía", "Energía", 25);
        inventario.Agregar("Escudo", "Defensa", 20);
        inventario.Agregar("Módulo de ataque", "Ataque", 18);
        inventario.Agregar("Analizador", "Análisis", 0);
    }

    static void MostrarInventario()
    {
        Console.Clear();
        Color(ConsoleColor.Cyan);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ NEXUS // INVENTARIO                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();

        if (inventario.Count == 0)
        {
            Console.WriteLine("El inventario está vacío.");
            return;
        }

        for (int i = 0; i < inventario.Count; i++)
        {
            ObjetoInventario objeto = inventario[i];
            Console.WriteLine("[" + (i + 1) + "] " + objeto.Nombre +
                " | Tipo: " + objeto.Tipo +
                " | Cantidad: " + objeto.Cantidad);
        }

        Console.WriteLine();
        Console.WriteLine("[U] Utilizar objeto");
        Console.WriteLine("[E] Eliminar objeto");
        Console.WriteLine("[Q] Regresar");

        ConsoleKey tecla = Console.ReadKey(true).Key;

        if (tecla == ConsoleKey.U)
            UtilizarObjetoInventario();
        else if (tecla == ConsoleKey.E)
            EliminarObjetoInventario();
    }

    static void UtilizarObjetoInventario()
    {
        if (inventario.Count == 0)
        {
            MensajeError("No hay objetos para utilizar.");
            return;
        }

        Console.Write("Número del objeto: ");
        int opcion;

        if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
        {
            MensajeError("Opción inválida.");
            return;
        }

        opcion--;

        if (opcion < 0 || opcion >= inventario.Count)
        {
            MensajeError("Ese objeto no existe en el inventario.");
            return;
        }

        ObjetoInventario objeto = inventario[opcion];

        if (objeto.Tipo == "Energía")
        {
            cadete.NivelEnergia += objeto.Efecto;
            LimitarValores();
            inventario.Usar(opcion);
            Console.WriteLine("Célula utilizada. Energía recuperada.");
        }
        else if (objeto.Tipo == "Defensa")
        {
            cadete.NivelConexion += objeto.Efecto;
            LimitarValores();
            inventario.Usar(opcion);
            Console.WriteLine("Escudo utilizado. Estabilidad recuperada.");
        }
        else if (objeto.Tipo == "Ataque")
        {
            if (entidadActual != null && entidadActual.Hostil)
            {
                entidadActual.RecibirDanio(objeto.Efecto);
                inventario.Usar(opcion);
                Console.WriteLine("Módulo de ataque utilizado contra " + entidadActual.Nombre + ".");
            }
            else
            {
                MensajeError("No existe un enemigo activo al que atacar.");
            }
        }
        else if (objeto.Tipo == "Análisis")
        {
            if (entidadActual != null)
            {
                Console.WriteLine(entidadActual.Analizar());
                inventario.Usar(opcion);
            }
            else
            {
                MensajeError("No existe una entidad activa para analizar.");
            }
        }

        LimitarValores();
    }

    static void EliminarObjetoInventario()
    {
        if (inventario.Count == 0)
        {
            MensajeError("No hay objetos para eliminar.");
            return;
        }

        Console.Write("Número del objeto a eliminar: ");
        int opcion;

        if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
        {
            MensajeError("Opción inválida.");
            return;
        }

        opcion--;

        if (!inventario.Eliminar(opcion))
        {
            MensajeError("No existe ese objeto.");
            return;
        }

        Console.WriteLine("Objeto eliminado del inventario.");
    }

    static void MostrarMisionActual()
    {
        Console.Clear();
        Color(ConsoleColor.Yellow);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ MISIÓN: LA ANOMALÍA PERDIDA                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();

        Console.WriteLine("[ " + (mision.RealidadExplorada ? "✓" : " ") + " ] Explorar la realidad");
        Console.WriteLine("[ " + (mision.ZonaAnomaliaAlcanzada ? "✓" : " ") + " ] Llegar a una zona anómala");
        Console.WriteLine("[ " + (mision.NucleoEnergeticoEncontrado ? "✓" : " ") + " ] Encontrar el núcleo energético");
        Console.WriteLine("[ " + (mision.EntidadSuperada ? "✓" : " ") + " ] Superar el encuentro con la entidad");
        Console.WriteLine("[ " + (mision.PuntoExtraccionAlcanzado ? "✓" : " ") + " ] Regresar al punto de extracción");
        Console.WriteLine();

        if (mision.MisionCompletada)
        {
            Color(ConsoleColor.Green);
            Console.WriteLine("MISIÓN COMPLETADA");
            Color(ConsoleColor.Gray);
            Console.WriteLine("+100 XP");
            Console.WriteLine("+1 módulo desconocido");
        }
        else
        {
            Console.WriteLine("Estado: EN CURSO");
        }
    }

    static void IniciarEncuentroPersonaje()
    {
        bool usarRobot = random.Next(0, 2) == 0;

        if (usarRobot)
            entidadActual = new Robot();
        else
            entidadActual = new Enemigo();

        mision.EntidadEncontrada = true;
        mision.DecisionesTomadas++;
        mision.EventosEncontrados++;

        Console.Clear();
        Color(ConsoleColor.Magenta);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ 👾 ENTIDAD DETECTADA                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("Entidad : " + entidadActual.Nombre);
        Console.WriteLine("Tipo : " + entidadActual.Tipo);
        Console.WriteLine("Salud : " + entidadActual.Salud);
        Console.WriteLine();
        Console.WriteLine("[1] Atacar");
        Console.WriteLine("[2] Escapar");
        Console.WriteLine("[3] Utilizar objeto");
        Console.WriteLine("[4] Analizar entidad");

        bool encuentroActivo = true;

        while (encuentroActivo && mision.SesionActiva)
        {
            Console.Write("Seleccione una acción: ");
            int opcion;

            if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
            {
                MensajeError("Opción inválida.");
                continue;
            }

            switch (opcion)
            {
                case 1:
                    ResolverAtaque();
                    break;
                case 2:
                    Console.WriteLine("El cadete escapa del encuentro y sobrevive.");
                    cadete.NivelEnergia -= 5;
                    mision.EntidadSuperada = true;
                    AgregarDescubrimiento("Encuentro resuelto mediante escape");
                    AgregarXP(10);
                    LimitarValores();
                    encuentroActivo = false;
                    break;
                case 3:
                    UtilizarObjetoInventario();
                    break;
                case 4:
                    Console.WriteLine(entidadActual.Analizar());

                    if (entidadActual.Tipo == "Robot")
                    {
                        Robot robot = entidadActual as Robot;
                        Console.WriteLine(robot.ActivarSistemaDefensivo());
                    }

                    mision.DecisionesTomadas++;
                    AgregarXP(15);
                    break;
                default:
                    MensajeError("Opción inválida. El encuentro continúa.");
                    break;
            }

            if (entidadActual != null && entidadActual.EstaDerrotado())
            {
                Console.WriteLine();
                Color(ConsoleColor.Green);
                Console.WriteLine("✓ ENTIDAD SUPERADA");
                Color(ConsoleColor.Gray);
                mision.EntidadSuperada = true;
                AgregarDescubrimiento("Entidad superada: " + entidadActual.Nombre);
                AgregarXP(40);
                encuentroActivo = false;
            }

            if (cadete.NivelEnergia <= 0)
            {
                Console.WriteLine();
                Color(ConsoleColor.Red);
                Console.WriteLine("El cadete ya no puede continuar.");
                Color(ConsoleColor.Gray);
                mision.SesionActiva = false;
                encuentroActivo = false;
            }
        }

        entidadActual = null;
        LimitarValores();
        VerificarMisionV2();
    }

    static void ResolverAtaque()
    {
        if (entidadActual == null)
        {
            MensajeError("No existe una entidad activa.");
            return;
        }

        if (!entidadActual.Hostil)
        {
            Robot robot = entidadActual as Robot;

            if (robot != null)
            {
                Console.WriteLine(robot.ActivarSistemaDefensivo());
                cadete.NivelConexion -= 8;
            }

            LimitarValores();
            return;
        }

        int danio = inventario.Contiene("Módulo de ataque") ? 18 : 8;
        entidadActual.RecibirDanio(danio);

        if (!entidadActual.EstaDerrotado())
        {
            Enemigo enemigo = entidadActual as Enemigo;

            if (enemigo != null)
                cadete.NivelEnergia -= enemigo.Atacar();
        }

        LimitarValores();
        Console.WriteLine("Daño aplicado. Salud de la entidad: " + entidadActual.Salud);
    }

    static void VerificarMisionV2()
    {
        if (mision.ZonaAnomaliaAlcanzada &&
            mision.NucleoEnergeticoEncontrado &&
            mision.EntidadSuperada)
        {
            if (!mision.PuntoExtraccionAlcanzado)
            {
                Console.WriteLine();
                Color(ConsoleColor.Cyan);
                Console.WriteLine("La ruta de extracción ha sido desbloqueada.");
                Color(ConsoleColor.Gray);
                Console.WriteLine("Regresa a la zona de origen para cerrar la misión.");
            }
        }
    }

    static void ConsultarEstado()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ DIAGNÓSTICO DEL CADETE                             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine("Cadete : " + cadete.Nombre);
        Console.WriteLine("Edad : " + cadete.Edad);
        Console.WriteLine("Realidad : " + realidadActual.Nombre);
        Console.WriteLine("Nivel : " + mision.NivelCadete);
        Console.WriteLine("XP : " + mision.Experiencia + "/100");
        Console.WriteLine();

        MostrarBarra("ENERGÍA", cadete.NivelEnergia);
        MostrarBarra("ESTABILIDAD", cadete.NivelConexion);
        Console.WriteLine();

        if (mision.RealidadExplorada)
        {
            Console.WriteLine("Sector actual : " + realidadActual.Sector);
            Console.WriteLine("Entorno : " + realidadActual.Entorno);
            Console.WriteLine("Actividad : " + realidadActual.Actividad);
            Console.WriteLine("Señal : " + realidadActual.Senal);
            Console.WriteLine("Riesgo : " + realidadActual.Riesgo);
        }
        else
        {
            Console.WriteLine("Sector actual : No explorado");
        }

        Console.WriteLine();
        Console.WriteLine("Sectores explorados : " + mision.SectoresExplorados);
        Console.WriteLine("Anomalías analizadas: " + mision.AnomaliasAnalizadas);
        Console.WriteLine("Descubrimientos : " + mision.Descubrimientos);
        Console.WriteLine("Eventos encontrados : " + mision.EventosEncontrados);
        Console.WriteLine("Decisiones tomadas : " + mision.DecisionesTomadas);
        Console.WriteLine("Tiempo de misión : " + mision.TiempoMision + " minutos");
        Console.WriteLine("Criaturas registradas: " + cantidadCriaturasRegistradas());
        Console.WriteLine("Criaturas domesticadas: " + mision.CantidadDomesticadas);
        Console.WriteLine();

        MostrarDiagnostico();
    }

    static int cantidadCriaturasRegistradas()
    {
        int cantidad = 0;

        for (int i = 0; i < criaturas.Count; i++)
        {
            if (criaturas[i].Legendaria || ExisteRegistroNormal(i))
                cantidad++;
        }

        return cantidad;
    }

    static bool ExisteRegistroNormal(int indice)
    {
        for (int i = 0; i < registroDescubrimientos.Count; i++)
        {
            if (registroDescubrimientos[i] == "Criatura descubierta: " + criaturas[indice].Nombre)
                return true;
        }

        return false;
    }

    static void MostrarDiagnostico()
    {
        Console.WriteLine("══════════════════════════════════════════════════════");
        Console.WriteLine("DIAGNÓSTICO");

        if (cadete.NivelEnergia < 30 && cadete.NivelConexion < 30)
        {
            Color(ConsoleColor.Red);
            Console.WriteLine("ESTADO GENERAL: CRÍTICO");
            Console.WriteLine("Energía críticamente baja.");
            Console.WriteLine("Estabilidad críticamente baja.");
            Color(ConsoleColor.Gray);
            Console.WriteLine("Recomendación:");
            Console.WriteLine("Recuperar energía o desconectarse inmediatamente.");
            return;
        }

        if (realidadActual.Riesgo == "Crítico" && (cadete.NivelEnergia < 50 || cadete.NivelConexion < 50))
        {
            Color(ConsoleColor.Red);
            Console.WriteLine("ESTADO GENERAL: INESTABLE");
            Color(ConsoleColor.Gray);
            Console.WriteLine("El sector presenta un nivel de riesgo crítico.");
            Console.WriteLine("Se recomienda iniciar la desconexión.");
            return;
        }

        if (cadete.NivelConexion < 50)
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine("ESTADO GENERAL: INESTABLE");
            Color(ConsoleColor.Gray);
            Console.WriteLine("La estabilidad se encuentra por debajo del nivel recomendado.");
            Console.WriteLine("Proceder con precaución.");
            return;
        }

        if (cadete.NivelEnergia < 50)
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine("ESTADO GENERAL: ADVERTENCIA");
            Color(ConsoleColor.Gray);
            Console.WriteLine("La energía se encuentra por debajo del nivel recomendado.");
            Console.WriteLine("Se recomienda recuperar energía.");
            return;
        }

        Color(ConsoleColor.Green);
        Console.WriteLine("ESTADO GENERAL: ESTABLE");
        Color(ConsoleColor.Gray);

        if (realidadActual.Riesgo == "Alto" || realidadActual.Riesgo == "Crítico")
            Console.WriteLine("El entorno presenta riesgo elevado.");
        else
            Console.WriteLine("El cadete puede continuar con la misión.");
    }

    static void RecuperarEnergia()
    {
        if (cadete.NivelEnergia >= 100)
        {
            Color(ConsoleColor.Yellow);
            Console.WriteLine("La energía ya se encuentra al máximo.");
            Color(ConsoleColor.Gray);
            return;
        }

        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ RECUPERACIÓN ENERGÉTICA                            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        CargarBarra("Buscando fuentes", 15);

        string[] fuentes =
        {
            "Fuente Solar",
            "Fuente Térmica",
            "Fuente Desconocida",
            "Núcleo Energético",
            "Cristal Energético"
        };

        string fuente = fuentes[random.Next(fuentes.Length)];
        int energia = 0;

        switch (fuente)
        {
            case "Fuente Solar":
                energia = 10;
                break;
            case "Fuente Térmica":
                energia = 15;
                break;
            case "Fuente Desconocida":
                energia = 5;
                break;
            case "Núcleo Energético":
                energia = 20;
                break;
            default:
                energia = 25;
                break;
        }

        Console.WriteLine();
        Console.WriteLine("Fuente localizada: " + fuente);
        CargarBarra("Extrayendo energía", 15);

        cadete.NivelEnergia += energia;
        mision.TiempoMision += 10;

        if (cadete.NivelEnergia > 100)
            cadete.NivelEnergia = 100;

        Console.WriteLine();
        Color(ConsoleColor.Green);
        Console.WriteLine("Energía recuperada: +" + energia);
        Color(ConsoleColor.Gray);
        Console.WriteLine("Nivel actual: " + cadete.NivelEnergia + "/100");

        AgregarXP(10);
    }

    static void MostrarDescubrimientos()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ REGISTRO DE DESCUBRIMIENTOS                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        if (registroDescubrimientos.Count == 0)
        {
            Console.WriteLine("No existen descubrimientos registrados.");
            return;
        }

        for (int i = 0; i < registroDescubrimientos.Count; i++)
        {
            Color(ConsoleColor.Cyan);
            Console.Write("[" + (i + 1) + "] ");
            Color(ConsoleColor.Gray);
            Console.WriteLine(registroDescubrimientos[i]);
        }

        Console.WriteLine();
        Console.WriteLine("Total: " + registroDescubrimientos.Count);
    }

    static void MostrarBestiario()
    {
        Console.Clear();
        Color(ConsoleColor.Magenta);
        Console.WriteLine("★ ★ ★ ★ ★ ★");
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ NEXUS BESTIARIO                                    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();
        Console.WriteLine("REGISTRO EXCLUSIVO DE FORMAS DE VIDA");
        Console.WriteLine();

        CrearCriaturas();

        int registros = 0;

        for (int i = 0; i < criaturas.Count; i++)
        {
            if (ExisteRegistroNormal(i) || criaturas[i].Legendaria)
                registros++;
        }

        if (registros == 0)
        {
            Color(ConsoleColor.DarkGray);
            Console.WriteLine("EL BESTIARIO AÚN NO CONTIENE REGISTROS");
            Console.WriteLine();
            Console.WriteLine("Explora sectores para encontrar formas de vida.");
            Console.WriteLine("Las criaturas legendarias requieren anomalías.");
            Color(ConsoleColor.Gray);
            return;
        }

        Color(ConsoleColor.Cyan);
        Console.WriteLine("BESTIAS REGISTRADAS: " + registros);
        Color(ConsoleColor.Gray);
        Console.WriteLine();

        int numero = 1;

        for (int i = 0; i < criaturas.Count; i++)
        {
            if (ExisteRegistroNormal(i) || criaturas[i].Legendaria)
            {
                MostrarFichaBestiario(i, numero);
                numero++;
            }
        }

        Console.WriteLine();
        Color(ConsoleColor.Green);
        Console.WriteLine("══════════════════════════════════════════════════════");
        Console.WriteLine("CRIATURAS DOMESTICADAS: " + mision.CantidadDomesticadas);
        Color(ConsoleColor.Gray);
    }

    static void MostrarFichaBestiario(int indice, int numero)
    {
        Console.WriteLine();

        if (criaturas[indice].Legendaria)
            Color(ConsoleColor.Magenta);
        else
            Color(ConsoleColor.Cyan);

        Console.WriteLine("┌──────────────────────────────────────────────────────┐");
        Console.WriteLine("│ REGISTRO " + numero.ToString().PadRight(44) + "│");
        Console.WriteLine("├──────────────────────────────────────────────────────┤");

        DibujarCriaturaDetallada(indice);

        Color(ConsoleColor.Yellow);
        Console.WriteLine("│ ESPECIE : " + Ajustar(criaturas[indice].Nombre, 37) + "│");
        Color(ConsoleColor.Gray);
        Console.WriteLine("│ CATEGORÍA : " + Ajustar(criaturas[indice].Categoria, 37) + "│");
        Console.WriteLine("│ ENTORNO : " + Ajustar(criaturas[indice].Entorno, 37) + "│");
        Console.WriteLine("│ PELIGROSIDAD : " + Ajustar(criaturas[indice].Peligro, 37) + "│");
        Console.WriteLine("│ DOMESTICABLE : " + Ajustar(criaturas[indice].Domesticable, 37) + "│");
        Console.WriteLine("├──────────────────────────────────────────────────────┤");
        Console.WriteLine("│ DESCRIPCIÓN │");
        Console.WriteLine("│ " + Ajustar(criaturas[indice].Descripcion, 53) + "│");
        Console.WriteLine();
        Console.WriteLine("│ RECOMENDACIÓN │");
        Console.WriteLine("│ " + Ajustar(criaturas[indice].Recomendacion, 53) + "│");

        if (EstaDomesticada(criaturas[indice].Nombre))
        {
            Color(ConsoleColor.Green);
            Console.WriteLine("│ ★ ESTADO: DOMESTICADA                             │");
        }

        if (criaturas[indice].Legendaria)
        {
            Color(ConsoleColor.Magenta);
            Console.WriteLine("│ ★ REGISTRO LEGENDARIO                             │");
        }

        Color(ConsoleColor.Gray);
        Console.WriteLine("└──────────────────────────────────────────────────────┘");
    }

    static void DibujarCriaturaDetallada(int indice)
    {
        string dibujo = criaturas[indice].Grafico;
        string[] lineas = dibujo.Split('\n');

        if (criaturas[indice].Legendaria)
            Console.ForegroundColor = ConsoleColor.Magenta;
        else
            Console.ForegroundColor = ConsoleColor.Gray;

        for (int fila = 0; fila < lineas.Length; fila++)
        {
            string linea = lineas[fila];

            for (int columna = 0; columna < linea.Length; columna++)
            {
                char caracter = linea[columna];

                if (caracter == 'o' || caracter == 'O')
                    Console.ForegroundColor = criaturas[indice].Legendaria ? ConsoleColor.Yellow : ConsoleColor.Red;
                else if (caracter == '^' || caracter == 'v' || caracter == 'V')
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (caracter == '/' || caracter == '\\' || caracter == '|')
                    Console.ForegroundColor = criaturas[indice].Legendaria ? ConsoleColor.Magenta : ConsoleColor.DarkGray;
                else
                    Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write(caracter);
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    static void MostrarHabilidades()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ HABILIDADES                                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();

        MostrarHabilidad("Escaneo profundo", mision.HabilidadEscaneo);
        MostrarHabilidad("Estabilización", mision.HabilidadEstabilizacion);
        MostrarHabilidad("Análisis avanzado", mision.HabilidadAnalisis);

        Console.WriteLine();
        Console.WriteLine("Las habilidades se desbloquean al aumentar de nivel.");
    }

    static void MostrarHabilidad(string nombreHabilidad, bool desbloqueada)
    {
        Console.Write(nombreHabilidad.PadRight(25));

        if (desbloqueada)
        {
            Color(ConsoleColor.Green);
            Console.WriteLine("✓ DESBLOQUEADA");
        }
        else
        {
            Color(ConsoleColor.DarkGray);
            Console.WriteLine("🔒 BLOQUEADA");
        }

        Color(ConsoleColor.Gray);
    }

    static void VerificarHabilidades()
    {
        if (mision.NivelCadete >= 2 && !mision.HabilidadEscaneo)
        {
            mision.HabilidadEscaneo = true;
            Console.WriteLine();
            Color(ConsoleColor.Cyan);
            Console.WriteLine("★ NUEVA HABILIDAD: ESCANEO PROFUNDO");
            Color(ConsoleColor.Gray);
        }

        if (mision.NivelCadete >= 3 && !mision.HabilidadEstabilizacion)
        {
            mision.HabilidadEstabilizacion = true;
            Console.WriteLine();
            Color(ConsoleColor.Cyan);
            Console.WriteLine("★ NUEVA HABILIDAD: ESTABILIZACIÓN");
            Color(ConsoleColor.Gray);
        }

        if (mision.NivelCadete >= 4 && !mision.HabilidadAnalisis)
        {
            mision.HabilidadAnalisis = true;
            Console.WriteLine();
            Color(ConsoleColor.Cyan);
            Console.WriteLine("★ NUEVA HABILIDAD: ANÁLISIS AVANZADO");
            Color(ConsoleColor.Gray);
        }
    }

    static void AgregarXP(int cantidad)
    {
        mision.Experiencia += cantidad;

        Console.WriteLine();
        Color(ConsoleColor.Cyan);
        Console.WriteLine("XP obtenida: +" + cantidad);
        Color(ConsoleColor.Gray);

        while (mision.Experiencia >= 100)
        {
            mision.Experiencia -= 100;
            mision.NivelCadete++;

            Console.WriteLine();
            Color(ConsoleColor.Yellow);
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║ ★ NIVEL AUMENTADO ★                                ║");
            Console.WriteLine("╠════════════════════════════════════════════════════╣");
            Console.WriteLine("║ CADETE NIVEL " + mision.NivelCadete.ToString().PadRight(33) + "║");
            Console.WriteLine("║ Nuevas capacidades disponibles                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Color(ConsoleColor.Gray);

            VerificarHabilidades();
        }
    }

    static void CompletarObjetivo(string objetivo)
    {
        Console.WriteLine();
        Color(ConsoleColor.Green);
        Console.WriteLine("✓ OBJETIVO COMPLETADO");
        Console.WriteLine(objetivo);
        Color(ConsoleColor.Gray);
        AgregarXP(30);
    }

    static void AgregarDescubrimiento(string descubrimiento)
    {
        registroDescubrimientos.Agregar(descubrimiento);
    }

    static void MostrarEstadoRapido()
    {
        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════");
        MostrarBarra("ENERGÍA", cadete.NivelEnergia);
        MostrarBarra("ESTABILIDAD", cadete.NivelConexion);
        Console.WriteLine("══════════════════════════════════════════════════════");
    }

    static void IntentarDesconexion()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ DESCONEXIÓN NEXUS                                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("¿Desea abandonar la realidad?");
        Console.WriteLine();
        Console.WriteLine("[1] Sí");
        Console.WriteLine("[2] No");

        int opcion;

        if (!int.TryParse(Console.ReadLine() ?? "", out opcion))
        {
            MensajeError("Opción inválida.");
            return;
        }

        if (opcion == 1)
        {
            if (!mision.MisionCompletada)
            {
                Console.WriteLine();
                Color(ConsoleColor.Yellow);
                Console.WriteLine("La misión aún no está completada.");
                Console.WriteLine("La desconexión anticipada finalizará la misión sin recompensa.");
                Color(ConsoleColor.Gray);
                Console.WriteLine();
                Console.WriteLine("[1] Desconectarse de todos modos");
                Console.WriteLine("[2] Continuar misión");

                int confirmar;
                if (!int.TryParse(Console.ReadLine() ?? "", out confirmar) || confirmar != 1)
                {
                    Console.WriteLine("Desconexión cancelada.");
                    return;
                }
            }

            Console.WriteLine();
            CargarBarra("Cerrando enlace dimensional", 20);
            CargarBarra("Desconectando conciencia", 20);
            CargarBarra("Sellando realidad", 20);
            mision.SesionActiva = false;
        }
        else if (opcion == 2)
        {
            Console.WriteLine();
            Console.WriteLine("Desconexión cancelada.");
        }
        else
        {
            MensajeError("Opción inválida.");
        }
    }

    static void MostrarFinal()
    {
        Console.Clear();
        Color(ConsoleColor.Cyan);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ EVALUACIÓN FINAL                                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Color(ConsoleColor.Gray);
        Console.WriteLine();

        Console.WriteLine("CADETE: " + cadete.Nombre);
        Console.WriteLine("NIVEL : " + mision.NivelCadete);
        Console.WriteLine();

        MostrarBarra("ENERGÍA FINAL", cadete.NivelEnergia);
        MostrarBarra("ESTABILIDAD FINAL", cadete.NivelConexion);

        Console.WriteLine();
        Console.WriteLine("Sectores explorados : " + mision.SectoresExplorados);
        Console.WriteLine("Anomalías analizadas: " + mision.AnomaliasAnalizadas);
        Console.WriteLine("Descubrimientos : " + mision.Descubrimientos);
        Console.WriteLine("Eventos encontrados : " + mision.EventosEncontrados);
        Console.WriteLine("Decisiones tomadas : " + mision.DecisionesTomadas);
        Console.WriteLine("Criaturas registradas: " + cantidadCriaturasRegistradas());
        Console.WriteLine("Objetos en inventario : " + inventario.Count);
        Console.WriteLine("Misión V2 completada : " + (mision.MisionCompletada ? "SÍ" : "NO"));
        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════");

        if (cadete.NivelEnergia <= 0 || cadete.NivelConexion <= 0)
            FinalCritico();
        else if (mision.MisionCompletada)
            FinalSuperviviente();
        else if (cadete.NivelConexion <= 15)
            FinalCritico();
        else if (mision.AnomaliasAnalizadas >= 3 && mision.Descubrimientos >= 3)
            FinalInvestigador();
        else if (mision.SectoresExplorados >= 5 && cadete.NivelConexion > 40)
            FinalExplorador();
        else if (mision.EntidadEncontrada && mision.Descubrimientos >= 2)
            FinalSecreto();
        else
            FinalSuperviviente();

        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════");
        Console.WriteLine(" NEXUS SYSTEM OFFLINE");
        Console.WriteLine("══════════════════════════════════════════════════════");
    }

    static void FinalCritico()
    {
        Color(ConsoleColor.Red);
        Console.WriteLine("MISIÓN INTERRUMPIDA");
        Console.WriteLine();
        Console.WriteLine("La estabilidad alcanzó niveles críticos.");
        Console.WriteLine("Clasificación: CADETE DE RIESGO");
        Color(ConsoleColor.Gray);
    }

    static void FinalInvestigador()
    {
        Color(ConsoleColor.Magenta);
        Console.WriteLine("MISIÓN COMPLETADA");
        Console.WriteLine();
        Console.WriteLine("Se registraron múltiples anomalías.");
        Console.WriteLine("Clasificación: INVESTIGADOR DIMENSIONAL");
        Color(ConsoleColor.Gray);
    }

    static void FinalExplorador()
    {
        Color(ConsoleColor.Green);
        Console.WriteLine("MISIÓN COMPLETADA");
        Console.WriteLine();
        Console.WriteLine("El cadete exploró múltiples sectores.");
        Console.WriteLine("Clasificación: EXPLORADOR NEXUS");
        Color(ConsoleColor.Gray);
    }

    static void FinalSecreto()
    {
        Color(ConsoleColor.Yellow);
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║ ★ ARCHIVO CLASIFICADO DESBLOQUEADO ★              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("La entidad no pertenecía a la realidad explorada.");
        Console.WriteLine();
        Console.WriteLine("Los registros indican algo todavía más extraño:");
        Console.WriteLine();
        Console.WriteLine("La realidad podría estar observando al cadete.");
        Console.WriteLine();
        Console.WriteLine("CLASIFICACIÓN: CONTACTO DIMENSIONAL");
        Color(ConsoleColor.Gray);
    }

    static void FinalSuperviviente()
    {
        Color(ConsoleColor.Cyan);
        Console.WriteLine("MISIÓN COMPLETADA");
        Console.WriteLine();
        Console.WriteLine("El cadete abandonó la realidad con éxito.");
        Console.WriteLine("Clasificación: SUPERVIVIENTE");
        Color(ConsoleColor.Gray);
    }

    static void MostrarBarra(string nombreBarra, int valor)
    {
        int cantidad = valor / 5;

        if (cantidad > 20)
            cantidad = 20;

        if (cantidad < 0)
            cantidad = 0;

        string barra = "";

        for (int i = 0; i < cantidad; i++)
            barra += "█";

        for (int i = cantidad; i < 20; i++)
            barra += "░";

        Console.Write(nombreBarra.PadRight(18) + " ");

        if (valor < 30)
            Color(ConsoleColor.Red);
        else if (valor < 50)
            Color(ConsoleColor.Yellow);
        else
            Color(ConsoleColor.Green);

        Console.WriteLine("[" + barra + "] " + valor + "%");
        Color(ConsoleColor.Gray);
    }

    static void MostrarBarraMenu(string nombreBarra, int valor)
    {
        int cantidad = valor / 5;

        if (cantidad > 20)
            cantidad = 20;

        if (cantidad < 0)
            cantidad = 0;

        string barra = "";

        for (int i = 0; i < cantidad; i++)
            barra += "█";

        for (int i = cantidad; i < 20; i++)
            barra += "░";

        string texto = "║ " + nombreBarra.PadRight(12) + "[" + barra + "] " + valor + "%";
        Console.WriteLine(texto.PadRight(55) + "║");
    }

    static void CargarBarra(string texto, int pasos)
    {
        Console.Write(texto + " ");

        for (int i = 0; i < pasos; i++)
        {
            Console.Write("█");
            Thread.Sleep(20);
        }

        Console.WriteLine(" 100%");
        Console.WriteLine();
    }

    static void EscribirAnimado(string texto, int velocidad)
    {
        for (int i = 0; i < texto.Length; i++)
        {
            Console.Write(texto[i]);
            Thread.Sleep(velocidad);
        }
    }

    static void LimitarValores()
    {
        if (cadete.NivelEnergia < 0)
            cadete.NivelEnergia = 0;

        if (cadete.NivelEnergia > 100)
            cadete.NivelEnergia = 100;

        if (cadete.NivelConexion < 0)
            cadete.NivelConexion = 0;

        if (cadete.NivelConexion > 100)
            cadete.NivelConexion = 100;
    }

    static string Ajustar(string texto, int longitud)
    {
        if (texto == null)
            texto = "";

        if (texto.Length > longitud)
            return texto.Substring(0, longitud);

        return texto.PadRight(longitud);
    }

    static void MensajeError(string mensaje)
    {
        Color(ConsoleColor.Red);
        Console.WriteLine("⚠ " + mensaje);
        Color(ConsoleColor.Gray);
    }

    static void Color(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    static void PausaMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Presione Enter para regresar al menú...");
        Console.ReadLine();
    }
}

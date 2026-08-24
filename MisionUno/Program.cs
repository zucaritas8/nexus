using System;
using System.Threading;

class Program
{
    public string nombre;
    public int edad;
    public string realidad;
    public int nivelEnergia;
    public int nivelConexion;
    public int sector;
    public string señal;
    public string riesgo;
    public bool sesionActiva = true;
    public bool realidadExplorada = false;

    static void Main(string[] args)
    {
        Program program = new Program();

        MostrarBienvenida(program);

        if (program.nivelEnergia < 20 || program.nivelConexion < 20)
        {
            Console.WriteLine();
            Console.WriteLine("No puede ingresar al entrenamiento.");
            Console.WriteLine("El nivel de energía o estabilidad es insuficiente.");
            Console.WriteLine("Energía mínima: 20");
            Console.WriteLine("Estabilidad mínima: 20");
            return;
        }

        Thread.Sleep(3000);
        Console.Clear();

        Console.WriteLine("NEXUS TRAINING SYSTEM" +
            "\r\n-------------------------------------------------------------------" +
            "\r\nCADETE: " + program.nombre +
            "\r\nREALIDAD: " + program.realidad +
            "\r\nENERGÍA: " + program.nivelEnergia +
            "\r\nESTABILIDAD: " + program.nivelConexion);

        Console.WriteLine("-------------------------------------------------------------------");

        while (program.sesionActiva)
        {
            Console.WriteLine("1. Explorar realidad");
            Console.WriteLine("2. Analizar anomalía");
            Console.WriteLine("3. Consultar estado");
            Console.WriteLine("4. Recuperar energía");
            Console.WriteLine("5. Intentar Desconexión");
            Console.WriteLine("-------------------------------------------------------------------");

            int opcion = int.Parse(Console.ReadLine());

            switch (opcion)
            {
                case 1:
                    Console.WriteLine("Explorando la realidad asignada...");
                    Thread.Sleep(2000);
                    Console.Clear();
                    ExplorarRealidad(program);
                    PausaMenu();
                    break;

                case 2:
                    if (!program.realidadExplorada)
                    {
                        Console.WriteLine("No se puede analizar una anomalía.");
                        Console.WriteLine("Primero debe explorar una realidad.");
                        PausaMenu();
                        break;
                    }

                    if (program.señal != "Anómala")
                    {
                        Console.WriteLine("No se ha detectado ninguna anomalía.");
                        Console.WriteLine("La señal registrada es: " + program.señal);
                        PausaMenu();
                        break;
                    }

                    Console.WriteLine("Sector a analizar: " + program.sector);
                    Console.WriteLine("Señal: " + program.señal);
                    Console.WriteLine("Riesgo registrado: " + program.riesgo);
                    Console.WriteLine("");
                    Console.WriteLine("Procesando patrones...");
                    Thread.Sleep(2000);
                    Console.Clear();
                    AnalizarAnomalias(program);
                    PausaMenu();
                    break;

                case 3:
                    Console.WriteLine("Consultando estado...");
                    Thread.Sleep(2000);
                    Console.Clear();
                    consultarEstado(program);
                    PausaMenu();
                    break;

                case 4:
                    Console.WriteLine("Cargando recuperación de energía...");
                    Thread.Sleep(2000);
                    Console.Clear();
                    RecuperarEnergia(program);
                    PausaMenu();
                    break;

                case 5:
                    Console.WriteLine("Intentando desconexión...");
                    Thread.Sleep(2000);
                    Console.Clear();
                    IntentarDesconexion(program);

                    if (program.sesionActiva)
                    {
                        PausaMenu();
                    }
                    break;

                default:
                    Console.WriteLine("Opcion Invalida");
                    PausaMenu();
                    break;
            }
        }
    }

    static void MostrarBienvenida(Program program)
    {
        Console.WriteLine("NEXUS...");
        Thread.Sleep(3000);
        Console.Clear();

        Console.WriteLine("¡Bienvenido, Cadete! " +
            "\r\n-------------------------------------------------------------------" +
            "\r\nAntes de comenzar tu entrenamiento, necesitamos conocerte mejor. " +
            "\r\nIngrese sus datos para continuar con el proceso de incorporación." +
            "\r\n-------------------------------------------------------------------");

        Console.WriteLine("Nombre: ");
        program.nombre = Console.ReadLine();

        do
        {
            Console.WriteLine("Edad: ");
            program.edad = int.Parse(Console.ReadLine());

            if (program.edad < 18 || program.edad > 50)
            {
                Console.WriteLine("Edad no válida.");
                Console.WriteLine("La edad debe estar entre 18 y 50 años.");
            }
        } while (program.edad < 18 || program.edad > 50);

        Console.WriteLine("Realidad Asignada: ");
        program.realidad = Console.ReadLine();

        do
        {
            Console.WriteLine("Nivel de Energía: ");
            program.nivelEnergia = int.Parse(Console.ReadLine());

            if (program.nivelEnergia < 0 || program.nivelEnergia > 100)
            {
                Console.WriteLine("El nivel debe estar entre 0 y 100.");
            }
        } while (program.nivelEnergia < 0 || program.nivelEnergia > 100);

        do
        {
            Console.WriteLine("Nivel de Conexión: ");
            program.nivelConexion = int.Parse(Console.ReadLine());

            if (program.nivelConexion < 0 || program.nivelConexion > 100)
            {
                Console.WriteLine("El nivel debe estar entre 0 y 100.");
            }
        } while (program.nivelConexion < 0 || program.nivelConexion > 100);

        Console.WriteLine("-------------------------------------------------------------------");

        if (program.nivelEnergia < 20 || program.nivelConexion < 20)
        {
            Console.WriteLine("Sus datos fueron registrados.");
            Console.WriteLine("Sin embargo, no cumple con los requisitos para ingresar al entrenamiento.");
        }
        else
        {
            Console.WriteLine("Sus datos fueron registrados correctamente.");
            Console.WriteLine("Espere un momento por favor...");
        }
    }

    static void ExplorarRealidad(Program program)
    {
        Random random = new Random();

        Console.WriteLine("Resultados de la exploración");
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("Realidad " + program.realidad);
        Console.WriteLine("");

        program.sector = random.Next(1, 51);
        Console.WriteLine("Sector localizado: " + program.sector);

        string[] entorno = { "Desertico", "Subterraneo", "Boscoso", "Urbano", "Acuático", "Desconocido" };
        Console.WriteLine("Entorno: " + entorno[random.Next(entorno.Length)]);

        string[] actividad = { "Alta", "Media", "Baja", "Nula" };
        Console.WriteLine("Actividad: " + actividad[random.Next(actividad.Length)]);

        string[] señal = { "Nula", "Débil", "Desconocida", "Inestable", "Anómala" };
        program.señal = señal[random.Next(señal.Length)];
        Console.WriteLine("Señal: " + program.señal);

        string[] riesgo = { "Bajo", "Medio", "Alto", "Crítico" };
        program.riesgo = riesgo[random.Next(riesgo.Length)];
        Console.WriteLine("Riesgo: " + program.riesgo);

        program.realidadExplorada = true;
        program.nivelEnergia -= 8;
        program.nivelConexion -= 5;

        if (program.nivelEnergia < 0)
        {
            program.nivelEnergia = 0;
        }

        if (program.nivelConexion < 0)
        {
            program.nivelConexion = 0;
        }

        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("ESTADO DE CADETE");
        Console.WriteLine("");
        Console.WriteLine("Energía: " + program.nivelEnergia);
        Console.WriteLine("Estabilidad: " + program.nivelConexion);
        Console.WriteLine("-------------------------------------------------------------------");
    }

    static void AnalizarAnomalias(Program program)
    {
        Random random = new Random();

        Console.WriteLine("Análisis de Anomalía");
        Console.WriteLine("-------------------------------------------------------------------");

        string[] tipoAnomalia = {
            "Interferencia dimencional",
            "Distorción temporal",
            "Alteración gravitacional",
            "Señal desconocida",
            "fragmentación desconocida"
        };

        Console.WriteLine("Tipo de Anomalía: " + tipoAnomalia[random.Next(tipoAnomalia.Length)]);

        int intensidad = random.Next(1, 100);
        Console.WriteLine("Intensidad: " + intensidad);

        string[] origen = {
            "Desconocido",
            "Sector externo",
            "Fuente energética",
            "Entidad no identificada",
            "Realidad desconocida"
        };

        Console.WriteLine("Origen: " + origen[random.Next(origen.Length)]);

        string[] infoAdicional = {
            "Presenta un frcuencia répetitiva",
            "Su patrón cambia constantemente",
            "Se detecta una fuente energética cercana",
            "La señal no coincide con registros anteriores",
            "Se ha detectado una alteración temporal"
        };

        Console.WriteLine("Información Adicional: " + infoAdicional[random.Next(infoAdicional.Length)]);
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("ESTADO DE CADETE");
        Console.WriteLine("");
        Console.WriteLine("Energía: " + program.nivelEnergia);
        Console.WriteLine("Estabilidad: " + program.nivelConexion);
    }

    static void consultarEstado(Program program)
    {
        Console.WriteLine("Consulta del Estado");
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("Cadete: " + program.nombre);
        Console.WriteLine("Realidad: " + program.realidad);
        Console.WriteLine("Energía: " + program.nivelEnergia + "/100");
        Console.WriteLine("Estabilidad: " + program.nivelConexion + "/100");
        Console.WriteLine("Sector Actual: " + program.sector);
        Console.WriteLine("Nivel de Riesgo: " + program.riesgo);
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("Diagnostico");

        if (program.nivelEnergia < 50 && program.nivelConexion >= 50)
        {
            if (program.riesgo == "Bajo" || program.riesgo == "Medio")
            {
                Console.WriteLine("Energía Baja");
                Console.WriteLine("Conexión Estable");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: ESTABLE");
                Console.WriteLine("Recomendación: \r\nEvitar nuevas exploraciones.\r\nSe recomienda recuperar energía o iniciar desconexión");
            }

            if (program.riesgo == "Alto" || program.riesgo == "Crítico")
            {
                Console.WriteLine("Energía Baja");
                Console.WriteLine("Conexión Estable");
                Console.WriteLine("El nivel de riesgo requiere precaución");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: INESTABLE");
                Console.WriteLine("Recomendación: \r\nDesconexión inmediata!");
            }
        }

        if (program.nivelEnergia < 50 && program.nivelConexion < 50)
        {
            if (program.riesgo == "Bajo" || program.riesgo == "Medio")
            {
                Console.WriteLine("Energía Baja");
                Console.WriteLine("Estabilidad Crítica");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: INESTABLE");
                Console.WriteLine("Recomendación: \r\nEvitar nuevas exploraciones.\r\nSe recomienda recuperar energía o iniciar desconexión");
            }

            if (program.riesgo == "Alto" || program.riesgo == "Crítico")
            {
                Console.WriteLine("Energía Baja");
                Console.WriteLine("Estabilidad Crítica");
                Console.WriteLine("El nivel de riesgo requiere precaución");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: INESTABLE");
                Console.WriteLine("Recomendación: \r\nDesconexión inmediata!");
            }
        }

        if (program.nivelEnergia >= 50 && program.nivelConexion >= 50)
        {
            if (program.riesgo == "Bajo" || program.riesgo == "Medio")
            {
                Console.WriteLine("Energía Suficiente");
                Console.WriteLine("Conexión Estable");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: ESTABLE");
                Console.WriteLine("Recomendación: \r\nPuede continuar con la exploración");
            }

            if (program.riesgo == "Alto" || program.riesgo == "Crítico")
            {
                Console.WriteLine("Energía Suficiente");
                Console.WriteLine("Conexión Estable");
                Console.WriteLine("El nivel de riesgo requiere precaución");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: ESTABLE");
                Console.WriteLine("Recomendación: \r\nPuede continuar con la exploración");
            }
        }

        if (program.nivelEnergia >= 50 && program.nivelConexion < 50)
        {
            if (program.riesgo == "Bajo" || program.riesgo == "Medio")
            {
                Console.WriteLine("Energía Suficiente");
                Console.WriteLine("Estabilidad Crítica");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: ESTABLE");
                Console.WriteLine("Recomendación: \r\nPuede continuar con la exploración");
            }

            if (program.riesgo == "Alto" || program.riesgo == "Crítico")
            {
                Console.WriteLine("Energía Suficiente");
                Console.WriteLine("Estabilidad Crítica");
                Console.WriteLine("El nivel de riesgo requiere precaución");
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("ESTADO GENRAL: INESTABLE");
                Console.WriteLine("Recomendación: \r\nSe recomienda proceder con precaución");
            }
        }
    }

    static void RecuperarEnergia(Program program)
    {
        Random random = new Random();

        if (program.nivelEnergia >= 100)
        {
            program.nivelEnergia = 100;
            Console.WriteLine("No es necesario recuperar energía. \r\nNivel actual: " + program.nivelEnergia + "/100");
        }
        else
        {
            Console.WriteLine("Buscando fuentes de energía cercanas...");
            Thread.Sleep(3000);
            Console.Clear();

            string[] fuente = {
                "Fuente Solar",
                "Fuente Térmica",
                "Fuente Desconocida",
                "Núcleo Energético",
                "Cristal Energético"
            };

            string fuenteSeleccionada = fuente[random.Next(fuente.Length)];

            Console.WriteLine("Fuente de energía localizada \r\n" + fuenteSeleccionada);
            Thread.Sleep(3000);

            Console.WriteLine("Extrayendo energía...");
            Thread.Sleep(3000);
            int energiaRecuperada = 0;

            if (fuenteSeleccionada == "Fuente Solar")
            {
                energiaRecuperada = 10;
            }

            if (fuenteSeleccionada == "Fuente Térmica")
            {
                energiaRecuperada = 15;
            }

            if (fuenteSeleccionada == "Fuente Desconocida")
            {
                energiaRecuperada = 5;
            }

            if (fuenteSeleccionada == "Núcleo Energético")
            {
                energiaRecuperada = 20;
            }

            if (fuenteSeleccionada == "Cristal Energético")
            {
                energiaRecuperada = 25;
            }

            program.nivelEnergia += energiaRecuperada;

            if (program.nivelEnergia > 100)
            {
                program.nivelEnergia = 100;
            }

            Console.WriteLine("Energía recuperada. \r\nNivel actual: "
                + program.nivelEnergia + "/100");
        }
    }


    static void IntentarDesconexion(Program program)
    {
        Console.WriteLine("¿Desea abandonar la realidad?");
        Console.WriteLine("1. Sí");
        Console.WriteLine("2. No");
        Console.WriteLine("-------------------------------------------------------------------");

        int opcion = int.Parse(Console.ReadLine());

        if (opcion == 1)
        {
            Console.WriteLine("Desconexión exitosa.");
            Console.WriteLine("Gracias por participar en el entrenamiento.");
            program.sesionActiva = false;
        }
        else if (opcion == 2)
        {
            Console.WriteLine("Desconexión cancelada.");
            Console.WriteLine("Regresando al entrenamiento...");
        }
        else
        {
            Console.WriteLine("Opcion Invalida");
        }
    }

    static void PausaMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Presione Enter para regresar al menú...");
        Console.ReadLine();
        Console.Clear();
    }
}
# Arquitectura actual de NEXUS H3

## 1. Base examinada

El análisis se basa en el archivo de código C# de NEXUS compartido para H3, identificado como `NEXUS_V2_0_Program_sin_comentarios_actual.cs`. La implementación observada es de consola. Este documento describe lo visible en ese código, no funcionalidades futuras.

## 2. Clases y responsabilidades actuales

| Clase | Responsabilidad observada | Uso en la evolución |
|---|---|---|
| `Personaje` | Clase base con nombre, tipo, salud, defensa, hostilidad, análisis y recepción de daño. | Puede seguir como base de entidades con estado de combate; evitar que controle la interfaz. |
| `Robot` | Hereda de `Personaje`; contiene nivel defensivo, activación de sistema defensivo y análisis propio. | Puede ser asistente de análisis/soporte. |
| `Enemigo` | Hereda de `Personaje`; tiene poder de ataque, análisis y método `Atacar()`. | Conservar encuentros peligrosos, sin convertirlos en el centro de todas las misiones. |
| `ObjetoInventario` | Guarda nombre, tipo, cantidad y efecto de un objeto. | Convertir sus datos en registros de inventario persistibles. |
| `Inventario` | Mantiene una `List<ObjetoInventario>` y operaciones para agregar, consultar, usar y eliminar. | Reutilizar la lógica y añadir validaciones; la UI solo la invoca. |
| `Criatura` | Guarda nombre, categoría, entorno, peligro, domesticabilidad, descripción, recomendación, gráfico, rareza legendaria y estado domesticado. | Es el eje de la nueva experiencia; ampliar con identificador, datos de vínculo y comportamiento si se justifica. |
| `Cadete` | Hereda de `Personaje`; contiene edad, energía y nivel de conexión. | Representa al explorador. Revisar la duplicación de `Nombre` heredado de `Personaje`. |
| `Realidad` | Describe nombre, entorno, actividad, señal, riesgo y sector; puede crear una copia. | Fuente de datos para selección de misión y mapa. |
| `Anomalia` | Guarda tipo, intensidad, origen e información adicional. | Relacionarla con misiones, realidades y efectos sobre criaturas. |
| `MapaSector` | Representa el mapa con `char[,]`, dimensiones, coordenadas, objetivos y marcadores de criatura, recurso, anomalía y estructura. | Mantener la matriz como lógica del mapa y desacoplarla de su dibujo en consola. |
| `RegistroDescubrimientos` | Mantiene una lista de descubrimientos y permite agregarlos y consultarlos. | Persistir descubrimientos y evitar duplicados por jugador/partida. |
| `EstadoMision` | Guarda nivel, experiencia, sectores explorados, análisis, decisiones, eventos, habilidades y objetivos/estados de misión. | Separar definición de misión del progreso del jugador. |
| `Program` | Contiene el punto de entrada y gran parte del flujo de juego, listas, estados globales, menús, mapas, encuentros, inventario, misiones, finales y presentación de consola. | Dividir gradualmente en servicios de juego, repositorios y formularios. |

## 3. Funcionalidades que sí aparecen en H3

- Inicio, bienvenida, validación de requisitos mínimos y carga del sistema.
- Menú de consola con 11 operaciones: explorar realidad, analizar anomalía, consultar estado, recuperar energía, ver descubrimientos, habilidades, bestiario, mapas de realidades, desconexión, inventario y misión actual.
- Selección entre seis realidades/entornos: boscoso, desértico, subterráneo, urbano, acuático y desconocido.
- Generación de mapas con matriz, bordes, estructuras, recursos, criaturas, anomalías, puntos de interés y movimiento del jugador.
- Encuentros y análisis de criaturas; recomendaciones de precaución; registro de criaturas y criaturas legendarias; método para intentar domesticarlas.
- Gestión de energía, nivel de conexión/estabilidad, salud, inventario y objetos.
- Experiencia, habilidades, objetivos, registro de descubrimientos y bestiario.
- Eventos, encuentros con entidades y diferentes métodos de finalización.

La existencia de un método o campo en el código no garantiza que todas las rutas estén equilibradas o que los datos se conserven después de cerrar el programa.

## 4. Limitaciones reales observables

1. **Interfaz acoplada a consola.** `Program` usa `Console.WriteLine`, `Console.ReadLine`, `Console.Clear`, `ConsoleColor`, escritura animada y pausas. Eso impide presentar directamente la experiencia como una interfaz gráfica.
2. **Responsabilidades concentradas.** La clase `Program` reúne gran parte del flujo, generación de mapas, encuentros, presentación, reglas y progreso. Es difícil probar o modificar una función sin afectar otras.
3. **Estado global estático.** Se observan estados compartidos como cadete, realidad actual, anomalía, mapa, misión, inventario y listas. Para una versión mantenible conviene agrupar el estado de sesión en un objeto y definir servicios.
4. **Persistencia no observada.** El archivo analizado no muestra conexión a SQL Server, comandos SQL ni repositorios. El progreso vive en objetos y listas en memoria durante la ejecución.
5. **Datos narrativos integrados en código.** La creación de criaturas y sus descripciones se realiza mediante llamadas de inicialización en el propio código.
6. **Representación visual ligada a texto.** El mapa usa caracteres y hay dibujos ASCII de criaturas. La interfaz gráfica necesitará un renderizador de celdas/recursos visuales separado de la matriz.
7. **Diseño de criaturas orientado parcialmente a domesticación.** La clase contiene `Domesticable` y `Domesticada`; la nueva visión propone un sistema de vínculo más flexible. Debe migrarse gradualmente, sin borrar el comportamiento existente antes de contar con pruebas.
8. **Posible duplicación de propiedad.** `Cadete` declara `Nombre`, aunque `Personaje` ya contiene `Nombre`. Conviene decidir si se elimina la propiedad duplicada y adaptar los usos con cuidado.
9. **Pruebas no evidenciadas en el archivo.** No se identifican aquí pruebas automatizadas. Se recomienda añadir pruebas para movimiento, energía, inventario, vínculo y finalización de misión al separar la lógica.

## 5. Estrategia de evolución desde el código existente

1. Mantener una copia funcional de H3 y establecer un commit base.
2. Extraer primero la lógica de mapa, inventario, criaturas y misión a clases/servicios que no escriban directamente en consola.
3. Crear una aplicación Windows Forms y construir una pantalla por etapa.
4. Conectar la persistencia mediante repositorios ADO.NET y SQL Server.
5. Sustituir almacenamiento solo después de que el caso de uso equivalente funcione en memoria y tenga pruebas.
6. Implementar una misión vertical completa antes de ampliar las seis realidades.

La meta es refactorizar de manera incremental. No se recomienda eliminar el proyecto de consola ni reescribirlo íntegramente sin una versión de referencia.

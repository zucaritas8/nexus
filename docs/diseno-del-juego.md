# Diseño del juego: NEXUS V1.0, Ecos de los olvidados

## 1. Visión del juego

**NEXUS: Ecos de los olvidados** será una aventura de exploración y misterio para Windows, desarrollada en C# con Windows Forms. La experiencia gira alrededor de las criaturas que habitan distintas realidades: observarlas, analizar su comportamiento, descubrir su relación con el entorno y decidir cómo actuar cuando una anomalía altera su hábitat.

La historia comienza cuando una expedición anterior desaparece en la realidad OMEGA-99. El jugador controla a un cadete de NEXUS que participa en una misión de reconocimiento. En la primera realidad encuentra señales de que varias criaturas están huyendo de zonas afectadas por anomalías. Los registros de esas criaturas contienen pistas sobre la expedición desaparecida y sobre decisiones que la organización no ha explicado por completo. La relación personal del cadete con la expedición podrá definirse en una etapa posterior; para la primera versión se mantiene como un misterio narrativo sin fijar todavía un parentesco concreto.

El objetivo del jugador no será derrotar a todo lo que encuentre. Deberá explorar, identificar especies, interpretar pistas, administrar energía, completar objetivos y elegir si conviene acercarse, ayudar, evitar o defenderse. La motivación para continuar será descubrir nuevas especies, ampliar el bestiario, desbloquear realidades y reconstruir gradualmente lo ocurrido en OMEGA-99.

### Relación con el código H3

La propuesta no parte de cero. El código existente ya incluye `Criatura`, `Cadete`, `Realidad`, `Anomalia`, `MapaSector`, `Inventario`, `ObjetoInventario`, `RegistroDescubrimientos` y `EstadoMision`. También contiene seis entornos, análisis, encuentros, energía, habilidades, experiencia, objetivos y finales. La evolución reorganiza estas funciones y les da una dirección narrativa común; no afirma que la interfaz gráfica o la base de datos ya estén implementadas.

## 2. Ciclo principal de juego

```text
CENTRO NEXUS
    ↓ seleccionar misión y revisar equipo
ENTRAR A UNA REALIDAD
    ↓ explorar el mapa y administrar energía
OBSERVAR UNA CRIATURA / RECURSO / ANOMALÍA
    ↓ elegir: analizar, investigar, acercarse, evitar o defenderse
RESOLVER LA SITUACIÓN
    ↓ registrar descubrimiento, pista o consecuencia
COMPLETAR OBJETIVOS DE MISIÓN
    ↓ verificar energía y llegar al punto de extracción
REGRESAR AL CENTRO
    ↓ guardar progreso, revisar bestiario y desbloquear contenido
SIGUIENTE MISIÓN
```

La partida se organiza en misiones breves. Una misión contiene una realidad, una zona delimitada, objetivos claros y al menos un descubrimiento relacionado con criaturas o anomalías. Las decisiones no necesitan generar finales múltiples en cada pantalla; basta con que cambien el estado de una criatura, el registro del bestiario o la información disponible.

## 3. Mecánicas y reglas

### 3.1 Explorador y estado de la partida

El jugador controla un cadete. En H3, `Cadete` hereda de `Personaje` y tiene atributos como edad, energía y nivel de conexión. La evolución conservará salud, energía y estabilidad como valores de juego, pero mostrará sus estados en indicadores gráficos. El perfil del explorador y el progreso de campaña se guardarán por separado para facilitar su mantenimiento.

### 3.2 Mapas y exploración

`MapaSector` usa una matriz `char[,]`, con filas, columnas, coordenadas del jugador y marcadores para anomalía, criatura, recurso y estructura. Esta representación puede conservarse inicialmente. En Windows Forms, cada carácter se dibujará como una celda o imagen; la lógica de movimiento no deberá depender de la forma en que se dibuje la pantalla.

Los seis entornos definidos por el proyecto son: boscoso, desértico, subterráneo, urbano, acuático y desconocido. Para una entrega viable, se implementará primero una zona de bosque y se ampliará el resto después de validar el ciclo principal.

### 3.3 Criaturas como centro de la experiencia

La clase `Criatura` ya contempla nombre, categoría, entorno, peligro, domesticabilidad, descripción, recomendación, representación gráfica, condición legendaria y condición domesticada. Estas propiedades servirán como punto de partida para un bestiario gráfico.

La evolución propone reemplazar gradualmente la idea de “domesticar” por un **sistema de vínculo**. El vínculo aumentará al observar sin molestar, analizar correctamente, ayudar a una criatura o resolver un problema de su entorno. No todas las especies serán vinculables. Una criatura hostil o asustada puede requerir distancia, protección o una ruta alternativa, no un combate obligatorio.

Categorías narrativas propuestas:
- **Silvestres:** forman parte del ecosistema y pueden observarse o evitarse.
- **Alteradas:** muestran cambios asociados a anomalías.
- **Vinculables:** pueden aceptar una interacción de confianza si se cumplen condiciones.
- **Ancestrales:** especies raras que aportan pistas principales de la historia.

Estas categorías son una propuesta de diseño y no deben confundirse con una clasificación ya implementada en H3.

### 3.4 Energía, estabilidad y recursos

`Cadete` incluye `NivelEnergia` y `NivelConexion`; el juego también verifica estabilidad y tiene una opción de recuperación de energía. La energía limitará acciones como escanear, estabilizar una anomalía o usar un dispositivo. Los recursos encontrados en el mapa podrán recuperar energía o aportar datos. La estabilidad representará el estado de la realidad y estará conectada con los objetivos de la misión.

La interfaz debe advertir antes de una acción costosa y mostrar claramente el resultado después de realizarla. No se recomienda castigar al jugador con pérdida irreversible de progreso por un solo error.

### 3.5 Misiones, experiencia y descubrimientos

`EstadoMision` registra nivel, experiencia, sectores explorados, anomalías analizadas, descubrimientos, decisiones, habilidades y varios objetivos de misión. `RegistroDescubrimientos` almacena descubrimientos en una lista. Se mantendrán estas responsabilidades, separando en una etapa posterior los datos persistentes de la lógica de la misión.

Cada misión mostrará:
- objetivo principal;
- objetivos secundarios opcionales;
- realidad y riesgo;
- estado de energía/estabilidad;
- condición de extracción;
- recompensa narrativa, registro de criatura o desbloqueo.

La experiencia desbloqueará herramientas o interacciones; no será el propósito principal del juego. Las recompensas narrativas y el bestiario tendrán más importancia que acumular niveles.

### 3.6 Inventario

`Inventario` mantiene una lista de `ObjetoInventario`, con nombre, tipo, cantidad y efecto. La interfaz propuesta mostrará solo objetos útiles para la misión, cantidad y efecto. Se priorizarán recursos de energía, herramientas de análisis y dispositivos de estabilización. La interfaz impedirá usar objetos inexistentes o cantidades no disponibles.

### 3.7 Encuentros y anomalías

El código actual incluye `Enemigo`, `Personaje.RecibirDanio`, análisis de anomalías y eventos. Se conservará el riesgo cuando contribuya a la tensión, pero no todo encuentro será una pelea. Las anomalías afectarán el comportamiento de las criaturas, el mapa o la estabilidad; resolver una anomalía puede exigir analizar señales, encontrar un recurso o ayudar a una especie.

### 3.8 Condiciones de misión y final

La misión se completa cuando se alcanzan los objetivos obligatorios y se llega al punto de extracción. El juego debe guardar los descubrimientos y el resultado antes de volver al centro. La historia completa culminará en OMEGA-99, donde se investigará la relación entre una especie ancestral, las anomalías y la expedición perdida. Los detalles del final se implementarán después de validar la primera misión.

## 4. Diseño de pantallas

Los wireframes están en [`evidencias/wireframes.md`](evidencias/wireframes.md). Se definen cuatro pantallas obligatorias:

1. **Menú principal / Centro NEXUS:** Continuar, Nueva partida, Misiones, Bestiario, Opciones y Salir.
2. **Exploración / mapa:** mapa central, estado del cadete, objetivo activo y acciones contextuales.
3. **Misiones:** misiones disponibles, estado, objetivos, riesgo y recompensa.
4. **Exploradores / inventario:** perfil, energía, estabilidad, objetos y herramientas.

La navegación debe mantener un botón claro para volver al centro. Durante una misión no se debe abrir una pantalla totalmente distinta para cada acción; un panel contextual mostrará el detalle de la criatura, el recurso o la anomalía seleccionada.

## 5. Experiencia de usuario

- **Legibilidad:** texto principal de al menos 12–14 pt en pantallas de escritorio, contraste suficiente y nombres acompañados por iconos o etiquetas; no depender solo del color.
- **Jerarquía visual:** objetivo y estado del jugador en zonas fijas; el mapa ocupa el área principal; acciones secundarias permanecen agrupadas.
- **Consistencia:** botones equivalentes mantienen el mismo nombre, icono y ubicación. “Volver al centro” no cambia de función entre pantallas.
- **Navegación sencilla:** las acciones principales deben estar disponibles en uno o dos pasos desde el centro.
- **Retroalimentación:** cada acción muestra resultado, coste de energía y cambio en misión/bestiario. Los errores deben indicar cómo corregirlos.
- **Prevención de errores:** confirmar acciones que puedan abandonar una misión; advertir cuando no hay energía suficiente; no permitir completar un objetivo sin validar sus condiciones.
- **Accesibilidad básica:** permitir operar con ratón y teclado, tener foco visible, evitar texto decorativo excesivo y ofrecer etiquetas legibles en los botones.

## 6. Alcance y restricciones

### Prioridad alta (primera versión gráfica)
- Ventana principal y navegación.
- Una misión vertical completa en la realidad boscosa.
- Mapa basado inicialmente en `MapaSector`.
- Encuentro con criatura: observar, analizar y elegir respuesta.
- Energía, inventario mínimo, objetivos y extracción.
- Bestiario y registro de descubrimientos.
- Guardar y cargar perfil/progreso en SQL Server.
- Validaciones y mensajes de error claros.

### Prioridad media
- Segunda y tercera realidad.
- Sistema de vínculo para criaturas seleccionadas.
- Más tipos de recursos, anomalías y herramientas.
- Variaciones de resultado según decisiones.
- Sonido y animaciones sencillas.

### Opcional o posterior
- Las seis realidades completas con mapas únicos.
- Criaturas legendarias complejas.
- Varias rutas narrativas y finales.
- Animaciones elaboradas, cinemáticas, voz y efectos avanzados.

### Restricciones técnicas y de alcance
- La aplicación objetivo será de escritorio para Windows, en C# con Windows Forms y SQL Server.
- La versión H3 es de consola y usa `Console.ReadLine`, `Console.WriteLine`, `ConsoleColor`, `Console.Clear` y pausas con `Thread.Sleep`. Estas llamadas no se deben trasladar directamente a los formularios: deben sustituirse por eventos y controles gráficos.
- No se debe afirmar que SQL Server ya guarda el progreso; la persistencia es una etapa propuesta.
- No se debe rehacer todo al mismo tiempo. Primero se valida una misión vertical completa y después se amplía el contenido.
- El repositorio conservará el código H3 y documentará los cambios mediante commits descriptivos.

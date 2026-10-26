# Análisis técnico: alternativas y decisiones para NEXUS

## 1. Criterio de evaluación

La tecnología debe respetar las restricciones de la actividad: C#, aplicación de escritorio con Windows Forms y SQL Server. Las decisiones deben ser viables para un proyecto académico y partir del código de H3, que hoy es una aplicación de consola con clases de dominio y estado en memoria.

## 2. Interfaz: consola frente a Windows Forms

### Alternativa A: conservar consola

**Ventajas:** casi no requiere migración; sirve para probar reglas de juego; mantiene el código actual ejecutable con pocos cambios.

**Desventajas:** no cumple el objetivo de experiencia gráfica; los mapas y las criaturas se representan con caracteres; navegación por números y `Console.ReadLine` no ofrece la interacción visual esperada.

### Alternativa B: Windows Forms

**Ventajas:** está dentro de las tecnologías solicitadas; permite ventanas, controles, eventos, imágenes, paneles y enlace de datos; Visual Studio incluye diseñador visual.

**Desventajas:** está orientado a Windows; una interfaz de calidad requiere diseñar formularios, gestionar eventos y separar la presentación de la lógica. Las llamadas a consola no pueden trasladarse literalmente.

**Decisión:** usar Windows Forms. Mantener el programa de consola como referencia temporal para comparar reglas durante la migración. Crear una primera versión gráfica con cuatro pantallas principales y una misión vertical.

## 3. Persistencia: memoria frente a SQL Server

### Alternativa A: mantener listas en memoria

**Ventajas:** simple para prototipos y pruebas; no requiere configuración de servidor.

**Desventajas:** al cerrar el programa, el progreso no se conserva; no permite validar guardado/carga ni consultar el historial entre sesiones.

### Alternativa B: SQL Server con ADO.NET

**Ventajas:** cumple el requisito; permite definir claves, relaciones y restricciones; `SqlConnection`, `SqlCommand`, `SqlParameter` y `SqlDataReader` ofrecen acceso explícito desde C#; facilita persistir partida, misiones, inventario y descubrimientos.

**Desventajas:** requiere instalar/configurar SQL Server o una instancia compatible; hay que gestionar errores, permisos, conexiones y cambios del esquema; el código de acceso a datos debe probarse.

**Decisión:** SQL Server mediante ADO.NET, con repositorios y consultas parametrizadas. No ejecutar consultas directamente desde los formularios. Usar transacciones para operaciones que actualicen varias tablas como una unidad.

## 4. Alternativas de organización del código

| Alternativa | Ventaja | Riesgo | Decisión |
|---|---|---|---|
| Mantener todo en `Program` | Cambio inicial pequeño. | La clase concentra interfaz, reglas y estado; cada cambio puede afectar muchas funciones. | No como arquitectura final. |
| Reescritura completa | Permite empezar con una estructura limpia. | Riesgo alto de perder funcionalidades de H3 y consumir el tiempo en reimplementar. | No recomendada. |
| Refactorización incremental | Conserva clases y reglas; permite probar cada paso. | Requiere disciplina para separar responsabilidades gradualmente. | Recomendada. |

Se propone extraer lógica de exploración, criaturas, misiones e inventario a servicios, y mantener las clases existentes cuando sigan representando conceptos válidos. `MapaSector` puede conservar la matriz `char[,]`; el formulario gráfico la dibuja, sin modificar las reglas de movimiento.

## 5. Modelo de criaturas y narrativa

La implementación actual tiene `Criatura` con peligro, entorno, domesticabilidad, descripción, recomendación, estado legendario y domesticado. La propuesta conserva estos datos como base, pero la historia debe hacer que observar y comprender a las criaturas sea tan importante como combatir. El “vínculo” se plantea como evolución de la domesticación, no como una función ya disponible.

Para evitar un alcance excesivo:
1. Primera entrega: catálogo de criaturas, análisis, recomendaciones y bestiario.
2. Segunda entrega: vínculo para especies seleccionadas.
3. Después: efectos narrativos más complejos según decisiones y entorno.

## 6. Priorización y riesgos

| Riesgo | Consecuencia | Mitigación |
|---|---|---|
| Migrar todos los formularios y realidades a la vez | Retrasos y errores difíciles de localizar. | Construir una misión vertical con una realidad primero. |
| Mantener reglas dentro de los eventos de UI | Dificultad de prueba y reutilización. | Usar servicios independientes de Windows Forms. |
| Guardar cada movimiento en SQL | Consultas innecesarias y complejidad. | Guardar en hitos de misión y puntos definidos. |
| Cadenas SQL concatenadas | Riesgo de inyección SQL y errores de formato. | Usar parámetros de `SqlCommand`. |
| Exponer secretos en GitHub | Acceso no autorizado a datos. | Excluir configuraciones locales y no versionar credenciales. |
| Cambiar la lógica y la interfaz al mismo tiempo | Difícil comparar resultados con H3. | Separar cambios y usar commits pequeños. |
| Añadir demasiadas mecánicas narrativas | El prototipo no llega a completarse. | Priorizar exploración, criatura, anomalía, extracción y guardado. |

## 7. Plan de implementación sugerido

1. **Base y documentación:** conservar H3, verificar compilación y registrar un commit base.
2. **Separación de lógica:** extraer estado y servicios para mapa, criaturas, inventario y misiones.
3. **Prototipo gráfico:** construir `FrmInicio` y `FrmExploracion`, dibujar una zona boscosa y mover al cadete.
4. **Bucle jugable:** agregar encuentro, análisis, energía, objetivos y extracción.
5. **Persistencia:** crear esquema SQL Server y repositorios para partida, progreso de misión, inventario y descubrimientos.
6. **Pantallas de progreso:** bestiario, inventario y selección de misión.
7. **Pruebas y expansión:** validar reglas, manejo de errores y guardado/carga; después agregar otras realidades.

## 8. Fuentes técnicas consultadas

Las siguientes fuentes respaldan las decisiones técnicas. Se debe registrar en el documento final la fecha en que se consultaron y comprobar que siguen aplicando a la versión de .NET usada por el proyecto.

1. Microsoft Learn, **Descripción general de Windows Forms**: explica que Windows Forms permite crear aplicaciones de escritorio para Windows con controles, eventos, entrada de usuario y diseñador visual.  
   https://learn.microsoft.com/es-es/dotnet/desktop/winforms/overview/
2. Microsoft Learn, **Configurar parámetros con Microsoft.Data.SqlClient**: explica el uso de parámetros tipados para enviar valores a SQL Server sin concatenarlos en el texto SQL.  
   https://learn.microsoft.com/es-es/sql/connect/ado-net/configure-parameters?view=sql-server-ver17
3. Microsoft Learn, **Claves principales y externas**: explica cómo las claves primarias identifican registros y las claves foráneas mantienen la integridad referencial.  
   https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver15
4. GitHub Docs, **Ignorar archivos**: guía para excluir archivos que no deben formar parte del repositorio.  
   https://docs.github.com/en/get-started/git-basics/ignoring-files

## 9. Verificación de información obtenida con IA

La IA se utilizó como apoyo para organizar alternativas, proponer la separación en capas y redactar un modelo inicial. Sus propuestas no se consideran prueba de que una función ya exista ni sustituyen la verificación técnica.

Verificaciones realizadas o pendientes para la entrega:
- **Contrastado con el código H3:** existencia de `Criatura`, `Cadete`, `Realidad`, `Anomalia`, `MapaSector`, `Inventario`, `ObjetoInventario`, `RegistroDescubrimientos`, `EstadoMision` y flujo principal en `Program`.
- **Contrastado con documentación oficial:** capacidades generales de Windows Forms, parámetros de ADO.NET y finalidad de claves primarias/foráneas.
- **Pendiente de verificar en el entorno del proyecto:** versión exacta de .NET, tipo de solución/proyecto, instancia SQL Server disponible, configuración de `Microsoft.Data.SqlClient`, compilación después de crear el proyecto gráfico y pruebas de guardado/carga.
- **Decisiones propuestas, no funcionalidades actuales:** formularios Windows Forms, servicios de aplicación, repositorios SQL, sistema de vínculo y persistencia.

Antes de entregar, ejecutar el código real, comprobar las versiones instaladas y ajustar cualquier afirmación que no coincida con el repositorio final. La IA puede equivocarse o proponer nombres de clases nuevos; las clases nuevas de este documento son una arquitectura recomendada, no evidencia de que ya estén programadas.

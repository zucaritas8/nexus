# Evidencias de diseño: wireframes de NEXUS

Los siguientes bocetos son esquemáticos y representan la distribución propuesta para Windows Forms. No son capturas de una interfaz implementada. El objetivo es explicar qué verá el jugador, qué acciones puede realizar y cómo navegará.

## 1. Menú principal / Centro NEXUS

```text
┌──────────────────────────────────────────────────────────────────┐
│ NEXUS // ECOS DE LOS OLVIDADOS                                   │
│ Centro de exploración dimensional                                │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│                   [ LOGO / IMAGEN DE NEXUS ]                     │
│                                                                  │
│  [ CONTINUAR PARTIDA ]        [ NUEVA PARTIDA ]                  │
│  [ MISIONES ]                 [ BESTIARIO ]                      │
│  [ EXPLORADOR E INVENTARIO ]  [ OPCIONES ]                       │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│ Último registro: señal biológica detectada en el sector boscoso  │
│ Estado del sistema: estable                                      │
└──────────────────────────────────────────────────────────────────┘
```

**Elementos:** nombre del juego, botones principales, estado resumido y último registro narrativo.

**Navegación:** Continuar abre la última partida guardada; Nueva partida inicia perfil/expedición; Misiones abre la lista; Bestiario abre las especies conocidas; Explorador e inventario abre el estado y equipo; Opciones permite ajustar volumen y accesibilidad; Salir cierra la aplicación con confirmación si hay progreso sin guardar.

## 2. Exploración / mapa

```text
┌──────────────────────────────────────────────────────────────────┐
│ NEXUS / REALIDAD: BOSQUE // MISIÓN 01           [PAUSA] [CENTRO] │
├──────────────────────────────────────────────┬───────────────────┤
│                                              │ CADETE            │
│   MAPA DE CELDAS / SPRITES                   │ Energía: 72/100   │
│                                              │ Estabilidad: 85%  │
│   ▒ ▒ ▒ ▒ ▒ ▒ ▒ ▒ ▒ ▒                        │ Salud: 100/100    │
│   ▒ · · 🌿 · · · ▒ ▒ ▒                        ├───────────────────┤
│   ▒ · C · · ? · · ▒ ▒                        │ OBJETIVO ACTUAL   │
│   ▒ · · · R · · · ▒ ▒                        │ Analizar la señal │
│   ▒ ▒ ▒ ▒ ▒ ▒ ▒ ▒ ▒ ▒                        ├───────────────────┤
│                                              │ ACCIONES          │
│                                              │ [Analizar]        │
│                                              │ [Interactuar]     │
│                                              │ [Investigar]      │
│                                              │ [Evitar]          │
│                                              │ [Inventario]      │
├──────────────────────────────────────────────┴───────────────────┤
│ Mensaje: se detectaron huellas junto a una anomalía.             │
│ [WASD / Flechas para moverse]    [Misión] [Bestiario]             │
└──────────────────────────────────────────────────────────────────┘
```

**Elementos:** mapa central, energía/estabilidad/salud, objetivo activo, acciones contextuales y registro de mensajes.

**Interacción:** el jugador selecciona una celda o se mueve con teclado. Al encontrar una criatura, recurso o anomalía, se habilitan las acciones aplicables. Los iconos son ilustrativos; la versión inicial puede usar imágenes simples o símbolos claramente etiquetados.

## 3. Misiones

```text
┌──────────────────────────────────────────────────────────────────┐
│ CENTRO NEXUS / MISIONES                              [VOLVER]    │
├───────────────────────┬──────────────────────────────────────────┤
│ DISPONIBLES            │ MISIÓN SELECCIONADA                     │
│                       │                                          │
│ ● 01 La señal del     │ La señal del bosque                     │
│      bosque           │ Realidad: Boscoso                        │
│ ○ 02 Huellas de       │ Riesgo: Bajo                             │
│      ceniza           │                                          │
│ 🔒 03 Ecos bajo tierra│ Objetivo principal: analizar la criatura │
│ 🔒 04 Sector OMEGA-99 │ Objetivos secundarios: registrar pistas  │
│                       │                                          │
│                       │ Recompensa: nuevo registro de bestiario  │
│                       │                                          │
│                       │ [INICIAR MISIÓN] [VER OBJETIVOS]          │
└───────────────────────┴──────────────────────────────────────────┘
```

**Elementos:** lista de misiones y detalle de la seleccionada. Las misiones bloqueadas muestran el requisito de desbloqueo cuando el jugador las selecciona.

**Navegación:** iniciar misión abre la pantalla de exploración. Volver regresa al Centro NEXUS. Los nombres y el orden son propuestas narrativas y podrán cambiar tras probar la primera misión.

## 4. Explorador / inventario

```text
┌──────────────────────────────────────────────────────────────────┐
│ CENTRO NEXUS / EXPLORADOR Y EQUIPO                   [VOLVER]    │
├───────────────────────────────┬──────────────────────────────────┤
│ PERFIL DEL CADETE             │ INVENTARIO                       │
│                               │                                  │
│ Nombre: [Cadete              ]│ Objeto             Cant. Efecto │
│ Nivel: 1                     │ Célula energética     2  +20      │
│ Experiencia: 0 / 100         │ Escáner de campo      1  Análisis │
│ Energía: 100/100             │ Estabilizador         1  Anomalía │
│ Estabilidad: 100%            │                                  │
│                               │ [USAR] [DETALLES]                 │
├───────────────────────────────┴──────────────────────────────────┤
│ HERRAMIENTAS DESBLOQUEADAS: escáner básico                       │
│ CONSEJO: analizar antes de acercarse puede revelar el riesgo.    │
│ [GUARDAR PROGRESO]                                               │
└──────────────────────────────────────────────────────────────────┘
```

**Elementos:** perfil, progreso, energía, estabilidad, lista de objetos y herramientas.

**Interacción:** seleccionar un objeto muestra su efecto y permite usarlo si la regla lo permite. Guardar progreso solicita confirmación y muestra si el guardado se completó.

## 5. Criterios compartidos por las cuatro pantallas

- Encabezado y navegación consistentes.
- Botones con verbos claros.
- El estado del jugador se muestra siempre durante una misión.
- El color no es la única forma de comunicar riesgo o disponibilidad.
- Las acciones inválidas se deshabilitan o explican su requisito.
- El contenido narrativo aparece en mensajes breves y puede ampliarse en una ficha.

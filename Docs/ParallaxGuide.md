# Guía de Parallax del Proyecto

Esta guía documenta el sistema de parallax usado en el proyecto, basado en el script `Assets/Scripts/Level/ParallaxController.cs`. Incluye conceptos, configuración, valores recomendados y resolución de problemas.

## Índice

- [Conceptos clave](#conceptos-clave)
- [Componente ParallaxController](#componente-parallaxcontroller)
  - [Campos y qué hacen](#campos-y-qué-hacen)
  - [Ciclo de actualización y suavidad](#ciclo-de-actualización-y-suavidad)
- [Configuración paso a paso](#configuración-paso-a-paso)
- [Ejemplo: 5 capas solapadas](#ejemplo-5-capas-solapadas)
- [Ajustes verticales avanzados](#ajustes-verticales-avanzados)
- [Tiling infinito y sprites](#tiling-infinito-y-sprites)
- [Solución de problemas](#solución-de-problemas)
- [Checklist rápido](#checklist-rápido)

---

## Conceptos clave

- Parallax: mover capas del fondo a distinta velocidad que la cámara o el jugador, simulando profundidad.
- Delta relativo: el parallax se calcula desde la posición inicial del target; esto evita saltos al iniciar.
- Tiling: repetir el sprite cuando la cámara se desplaza más allá del tamaño del sprite, para que el fondo nunca “se pierda”.

---

## Componente ParallaxController

Archivo: `Assets/Scripts/Level/ParallaxController.cs`

### Campos y qué hacen

- Target de referencia
  - `target` (Transform): referencia para el parallax (cámara o jugador). Por defecto usa `Camera.main`.

- Parallax
  - `parallaxEffectX` (float): factor horizontal. 0 = fijo, 1 = sigue igual que el target.
  - `enableVerticalParallax` (bool): activar parallax vertical.
  - `parallaxEffectY` (float): factor vertical.

- Ajustes verticales avanzados
  - `enableVerticalBias` (bool): sesgo hacia abajo al ascender. Útil en torres para que capas cercanas queden visualmente más bajas al subir.
  - `verticalBiasOnAscent` (float): magnitud del sesgo. Fórmula: `yOffset -= max(0, deltaY) * verticalBiasOnAscent`.
  - `enableVerticalClamp` (bool): limitar el desplazamiento vertical relativo a la posición inicial.
  - `minYOffset`, `maxYOffset` (float): límites en unidades de mundo.

- Tiling infinito
  - `infiniteHorizontal` (bool): repetir sprite en X.
  - `infiniteVertical` (bool): repetir sprite en Y (usar solo con texturas tileables verticalmente).

- Actualización y nitidez
  - `updatePhase` (enum): cuándo aplicar el parallax. Opciones:
    - Update: para targets que se mueven en Update.
    - LateUpdate (recomendado para cámaras): tras mover la cámara.
    - FixedUpdate: cuando sigues un Rigidbody sin interpolación.
  - `enablePixelSnap` (bool): redondear posición a rejilla de píxeles (pixel art).
  - `pixelsPerUnit` (float): PPU para el redondeo.

Notas internas
- Se usa `DefaultExecutionOrder(1000)` para ejecutarse tarde por defecto y reducir jitter con cámaras.
- El cálculo de tiling no muta el origen, evitando saltos al iniciar.

### Ciclo de actualización y suavidad

- Target = Cámara (e.g., Cinemachine): usar `updatePhase = LateUpdate`.
- Target = Jugador (Rigidbody2D):
  - Si `Interpolate = Interpolate`: LateUpdate suele dar mejor resultado.
  - Si `Interpolate = None`: usar FixedUpdate.
- `enablePixelSnap`: útil si hay “shimmer” o subpíxeles.

---

## Configuración paso a paso

1) Crea un GameObject por capa de fondo con un `SpriteRenderer`.
2) Añade `ParallaxController` en cada capa.
3) Define el `target`:
   - Cámara si quieres parallax típico de scroll.
   - Jugador si quieres respuesta directa al movimiento del player.
4) Ajusta `parallaxEffectX` (0.05–0.85 según profundidad). Más lejano ⇒ menor valor.
5) Activa `enableVerticalParallax` solo si quieres parallax vertical; pon `parallaxEffectY` bajo (0.02–0.35).
6) Activa `infiniteHorizontal` (recomendado) y `infiniteVertical` solo si tus sprites tilean en Y.
7) Elige `updatePhase`:
   - LateUpdate para cámaras.
   - FixedUpdate para Rigidbodies sin interpolar.
8) Activa `enablePixelSnap` y define `pixelsPerUnit` si ves jitter en pixel art.

---

## Ejemplo: 5 capas solapadas

Objetivo: Capa 1 = cielo (fondo plano), Capa 3 = montañas, Capa 5 = pasto (foreground). target = Cámara.

- Capa 1 (Cielo)
  - parallaxEffectX: 0.03
  - enableVerticalParallax: true
  - parallaxEffectY: 0.01
  - infiniteHorizontal: true
  - infiniteVertical: false
  - updatePhase: LateUpdate

- Capa 2 (Nubes lejanas)
  - parallaxEffectX: 0.10
  - enableVerticalParallax: true
  - parallaxEffectY: 0.04
  - infiniteHorizontal: true
  - infiniteVertical: false
  - updatePhase: LateUpdate

- Capa 3 (Montañas)
  - parallaxEffectX: 0.22
  - enableVerticalParallax: true
  - parallaxEffectY: 0.08
  - infiniteHorizontal: true
  - infiniteVertical: false
  - updatePhase: LateUpdate

- Capa 4 (Colinas/árboles medios)
  - parallaxEffectX: 0.40
  - enableVerticalParallax: true
  - parallaxEffectY: 0.15
  - infiniteHorizontal: true
  - infiniteVertical: false
  - updatePhase: LateUpdate

- Capa 5 (Pasto / foreground)
  - parallaxEffectX: 0.75
  - enableVerticalParallax: true
  - parallaxEffectY: 0.30
  - infiniteHorizontal: true
  - infiniteVertical: false
  - updatePhase: LateUpdate

Ajusta los valores según la velocidad de tu juego y el tamaño de sprites.

---

## Ajustes verticales avanzados

Para escenas de torre (ascenso vertical) donde las capas frontales deben quedarse “más abajo” al subir:

- En capas cercanas (4 y 5):
  - `enableVerticalParallax = true`
  - `enableVerticalBias = true`
  - `verticalBiasOnAscent`: 0.10–0.20 (4 más suave, 5 más evidente)
  - `enableVerticalClamp = true` para limitar desplazamiento hacia abajo:
    - Capa 4: `minYOffset = -1.2`, `maxYOffset = 0.4`
    - Capa 5: `minYOffset = -1.8`, `maxYOffset = 0.2`
- En montañas (Capa 3):
  - Mantén `enableVerticalBias = false`
  - Si quieres máxima estabilidad, activa clamp con un rango estrecho (p. ej. `minYOffset = -0.2`, `maxYOffset = 0.6`).

El sesgo solo afecta al ascenso (`deltaY > 0`), así las capas cercanas quedan visualmente más bajas cuando subes, sin invadir la zona de las montañas.

---

## Tiling infinito y sprites

- El tiling usa el tamaño de `SpriteRenderer.bounds.size`.
- Asegura que el sprite tilea perfecto (sin bordes) para evitar “seams”.
- Import Settings recomendados cuando hay líneas visibles:
  - Filter Mode: según arte (Point o Bilinear), con Wrap Mode = Repeat si usas materiales con tiling.
  - Añade padding o usa sprites ligeramente solapados si hace falta.
- Para fondos muy grandes, considera un modo por material (offset UV) en vez de duplicar sprites (no implementado por defecto, se puede añadir).

---

## Solución de problemas

- Salto al iniciar
  - Asegura que el parallax usa delta relativo (esta guía ya lo contempla) y que el target está asignado antes del primer frame.
  - Evita mutar el origen de la capa en tiempo de ejecución.

- Tartamudeo/Jitter
  - Usa `updatePhase = LateUpdate` si sigues cámara, `FixedUpdate` con RB sin interpolar.
  - Activa `enablePixelSnap` y revisa `pixelsPerUnit` acorde a tus sprites (ej. 16, 32, 100).
  - Revisa que la cámara y el objeto seguido no se interpole de formas opuestas.

- El fondo “desaparece”
  - Activa `infiniteHorizontal` y verifica el tamaño del sprite.
  - Si usas vertical, solo activa `infiniteVertical` con texturas tileables en Y.

- “Seams” o líneas entre tiles
  - Revisa import/settings, wrap, filtros y padding. Considera un leve solapamiento.

---

## Checklist rápido

- [ ] target asignado (Cámara o Jugador)
- [ ] parallaxEffectX/Y configurados por profundidad
- [ ] enableVerticalParallax solo donde aporta (Y moderado)
- [ ] updatePhase adecuado (LateUpdate cámara / FixedUpdate RB sin interpolar)
- [ ] enablePixelSnap para pixel art
- [ ] infiniteHorizontal activo, infiniteVertical solo si aplica
- [ ] En torres: verticalBias y clamp configurados en capas cercanas (no en montañas)

---

¿Dudas o quieres un preset/prefabs por capa con estos valores? Se puede añadir una escena de ejemplo o prefabs con los parámetros ya puestos.

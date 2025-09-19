# EnemyPatrol2D – Patrullas estilo Mario

Este componente añade movimiento de patrulla a tus enemigos en 2D con tres modos:

- BetweenPoints: se mueve entre dos puntos A y B y gira al llegar.
- Range: patrulla entre un rango horizontal alrededor de un centro.
- GroundWalker: camina y gira al detectar borde sin suelo o una pared delante ("Goomba").

## Requisitos

- Rigidbody2D (congelar rotación Z recomendado).
- Collider2D adecuado para colisionar con el suelo/paredes.
- Capas configuradas para `groundMask` (suelo/pared).

## Parámetros principales

- speed: velocidad horizontal.
- startFacingRight: orientación inicial.
- waitAtTurn: pausa (segundos) cuando gira en los límites.

## Modos

### BetweenPoints
- pointA, pointB: transforms que marcan los límites.
- arriveThreshold: distancia para considerar que llegó al punto.

Tips:
- Usa los botones del inspector para crear pointA/pointB como hijos del enemigo.
- Mueve los puntos con el gizmo en escena.

### Range
- center: transform como centro del rango (si es null, usa la X actual al arrancar).
- leftOffset, rightOffset: desplazamientos a izquierda/derecha.

Tips:
- Botón “Set center = current X” en el inspector para fijarlo rápidamente.

### GroundWalker
- groundCheck: punto desde el cual se lanza un raycast hacia abajo para detectar suelo delante.
- wallCheck: punto desde el cual se lanza un raycast hacia delante para detectar paredes.
- groundMask: capa(s) que cuentan como suelo/pared.
- groundCheckDistance, wallCheckDistance: longitudes de los rayos.

Tips:
- Usa los botones del inspector para crear groundCheck/wallCheck con posiciones sugeridas.

## Animator (opcional)

- animator: referencia al Animator si quieres actualizar un bool de “running”.
- runningId: nombre del parámetro bool (por defecto `isRunningRigth`).

El componente lo pondrá en true cuando esté moviéndose y false cuando pare.

## Gizmos

- gizmoPatrolColor: color de líneas principales (rango/segmento).
- gizmoLimitColor: color de los puntos límite.
- gizmoRayColor: color de raycasts (suelo/pared).

Se dibujan al seleccionar el enemigo en la escena.

## Integración con Enemigo.cs

Para evitar conflictos (doble giro/animación):

- Deja que `EnemyPatrol2D` controle el giro (flip de localScale.x) y la animación de correr.
- En `Enemigo.cs`, elimina o condiciona la lógica de `GestionarMovimiento(...)` y las escrituras del bool de correr. Por ejemplo:

```csharp
void Update()
{
    var patrulla = GetComponent<EnemyPatrol2D>();
    if (patrulla == null)
    {
        // Lógica antigua de movimiento/animación si no hay patrulla
    }
}
```

### Opciones A/B/C para integrarlo

- Opción A (simple): quitar en `Enemigo.cs` toda la lógica de giro y escritura del parámetro de correr, dejando a `EnemyPatrol2D` el control de movimiento/flip. Pros: simple y limpio. Contras: `Enemigo` deja de controlar movimiento.
- Opción B (condicional): en `Update()`, si existe `EnemyPatrol2D` no ejecutas la lógica de `Enemigo` (compatibilidad con enemigos antiguos sin patrulla).
- Opción C (mixto): dejar que `EnemyPatrol2D` gestione el bool de correr (`runningId`) y el flip, y que `Enemigo` se encargue del resto (ataques/vida). Evita que ambos scripts escriban el mismo parámetro del Animator.

## Notas del inspector

- En este repo usamos el inspector estándar para mantener simplicidad. Puedes reorganizar por headers.
- Alternativa sin CustomEditor: crear un atributo + drawer (p. ej. `ConditionalFieldAttribute`) para ocultar/mostrar campos por modo (requiere código en `Assets/Scripts/Editor/`).
- Alternativa con CustomEditor: construir un inspector guiado que solo muestre lo relevante y ofrezca botones (más UX, más mantenimiento).

## Valores sugeridos

- GroundWalker: speed 1.5–2.5, waitAtTurn 0–0.15, groundCheckDistance 0.25, wallCheckDistance 0.1–0.2.
- BetweenPoints/Range: speed 1.5–3, arriveThreshold 0.05–0.1, waitAtTurn 0–0.2.

## Problemas comunes

- No gira al borde: revisa groundCheck y groundMask, y que el raycast baje lo suficiente.
- Se queda encallado en paredes: aumenta wallCheckDistance o coloca mejor `wallCheck`.
- Corre en dirección contraria visualmente: invierte `startFacingRight` o revisa el flip del sprite.

## Cómo guardar esta información y las conversaciones

- Esta guía vive en `Docs/EnemyPatrol2D.md` para que quede versionada.
- El chat de Copilot en VS Code no tiene exportador directo: copia/pega lo relevante en un `.md` bajo `Docs/`, o haz capturas.
- Podemos crear y mantener un `Docs/ChatNotes.md` con resúmenes por sesión si lo prefieres.

---

© Proyecto PlataformaGame. Este archivo documenta el uso del componente `EnemyPatrol2D` en este repo.

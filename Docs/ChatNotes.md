# Chat Notes – PlataformaGame

Archivo de apuntes offline. Aquí se centraliza la información clave compartida por el asistente sobre el sistema de patrullas, editores, atributos y opciones de integración.

---
## 1. Sistema de Patrulla `EnemyPatrol2D`
Modos disponibles:
- BetweenPoints: va de A a B y vuelve.
- Range: se desplaza entre (centerX - leftOffset) y (centerX + rightOffset).
- GroundWalker: camina y gira al detectar borde sin suelo (raycast hacia abajo) o pared (raycast frontal).

Campos relevantes:
- speed: velocidad horizontal.
- waitAtTurn: pausa antes de girar.
- startFacingRight: orientación inicial.
- Animator opcional para parámetro de "correr".
- Colores de gizmos configurables.

---
## 2. Opciones de integración con `Enemigo.cs`
Tres enfoques para evitar conflicto de giro/animación:
- Opción A (simple): eliminar lógica de movimiento/flip/animación en `Enemigo.cs` y dejar todo al patrullero.
- Opción B (condicional): si existe `EnemyPatrol2D` no ejecutar la parte de movimiento de `Enemigo`.
- Opción C (mixto): `EnemyPatrol2D` controla flip + bool de correr; `Enemigo` conserva daño, ataques y otros triggers.

Ejemplo Opción B:
```csharp
void Update() {
    var patrol = GetComponent<EnemyPatrol2D>();
    if (patrol == null) {
        // Lógica antigua de animación/giro
    }
}
```

---
## 3. Enfoques de Inspector
### 3.1 Inspector Estándar
- Ventaja: cero mantenimiento, siempre compatible.
- Inconveniente: muestra todos los campos incluso si no se usan en cierto modo.

### 3.2 CustomEditor (Editor Personalizado)
- Archivo en carpeta `Editor/` que hereda de `UnityEditor.Editor`.
- Control absoluto: puedes agrupar, añadir botones (crear pointA, pointB, etc.), colorear labels.
- Ventajas: UX clara, menos confusión.
- Contras: más código que mantener, hay que actualizar si agregas campos nuevos.

Activación: Restaurar `EnemyPatrol2DEditor.cs` (si se borró) o crear uno nuevo. Unity detecta automáticamente scripts en carpeta `Editor`.

### 3.3 Atributos + PropertyDrawer (Enfoque Ligero)
- Se crea un atributo (ej. `ConditionalFieldAttribute`) + un `PropertyDrawer` que decide si dibuja o no el campo.
- Decoras campos con `[ConditionalField("mode", 0)]` para mostrarlos solo cuando el enum vale 0.
- Ventajas: Reutilizable en muchos componentes, menos código específico por clase.
- Contras: Debug algo más difícil; si cambias nombres de campos o el enum, debes sincronizar.
- Nota: Para evitar errores en builds o si el atributo no se compila, puedes envolver con `#if UNITY_EDITOR`, pero esto impide la serialización del valor si se esconde del runtime.

Recomendación: usar CustomEditor si deseas botones y experiencia guiada; usar atributos si solo quieres ocultar campos sin más.

---
## 4. Gizmos
Los gizmos se dibujan con `OnDrawGizmosSelected()`:
- `Gizmos.color = gizmoPatrolColor` para la línea del recorrido.
- `Gizmos.color = gizmoLimitColor` para puntos límite (rangos o A/B).
- `Gizmos.color = gizmoRayColor` para rayos de detección (suelo/pared).

Consejos:
- Usa alfa <= 1 para no saturar la vista.
- Solo dibujar rayos cuando existan referencias (groundCheck/wallCheck no null).
- Para depurar siempre visible usar `OnDrawGizmos()`; para limpiar la escena, `OnDrawGizmosSelected()` es mejor.

---
## 5. Alternar entre enfoques
1. Inspector estándar (actual): No necesitas nada adicional.
2. Para probar CustomEditor: recrea `Assets/Scripts/Editor/EnemyPatrol2DEditor.cs` con el contenido (puedes copiar de historial si lo tenías) y guarda; Unity recompila y verás el inspector nuevo al seleccionar el objeto.
3. Para probar Atributos:
   - Asegúrate de tener `ConditionalFieldAttribute` y su Drawer en `Assets/Scripts/Attributes` y `Assets/Scripts/Editor` respectivamente.
   - Añade el atributo encima de los campos dependiendo de `mode`.
   - Ejemplo:
```csharp
[ConditionalField("mode", 0)] [SerializeField] Transform pointA;
[ConditionalField("mode", 1)] [SerializeField] float leftOffset;
```
   - Compila y observa el inspector.

Rollback rápido:
- Quitar atributos: elimina las líneas del atributo y deja solo `[SerializeField]`.
- Quitar CustomEditor: borra el archivo en carpeta `Editor/`.

---
## 6. Ventajas comparativas
| Enfoque | Código extra | Botones utilitarios | Oculta campos | Riesgo errores | Escalabilidad |
|---------|--------------|--------------------|---------------|---------------|---------------|
| Estándar | Ninguno | No | No | Muy bajo | Alta (simple) |
| CustomEditor | Medio | Sí | Sí (manual) | Medio | Alta (control total) |
| Atributos+Drawer | Bajo-Medio | No (a menos que combines) | Sí (automático) | Bajo-Medio | Buena (reutilizable) |

---
## 7. Siguientes pasos sugeridos (Opcionales)
- Estado "Chase": cambiar a persecución si jugador entra en un radio (OverlapCircle / distancia).
- Pausas aleatorias: variar `waitAtTurn` para que no todos giren igual.
- Velocidad escalonada: acelerar tras N ciclos (contador interno).
- Detección de hueco largo: si borde + sin pared frontal, intentar salto (requiere Animator y fuerza vertical).
- Sincronizar con plataformas móviles: recalcular centerX o puntos A/B en LateUpdate si son hijos de una plataforma.
- Pool de enemigos: despawn/respawn fuera de cámara usando un gestor.

### 7.1 Comportamiento de Persecución (Implementado)
Archivo: `EnemyChaseBehaviour.cs`
Funcionamiento:
- Escucha distancia al jugador cada frame (`Update`).
- Si jugador entra en `detectionRadius` (y diferencia vertical dentro de `verticalTolerance`) => pausa patrulla (`patrol.Pause()`) y entra a estado de persecución.
- Mientras persigue ajusta la velocidad horizontal hacia la posición X del jugador usando un multiplicador (`chaseSpeedMultiplier`).
- Si distancia supera `detectionRadius + loseRadiusExtra` => retoma patrulla (`patrol.Resume()`).
- Ataque: cuando distancia <= `attackRange` y cooldown listo => dispara trigger `Attack` (o el definido en `attackTrigger`). Un evento de animación debe llamar a `Enemigo.AplicarDanoJugadorEnAtaque()`.

Campos clave:
- detectionRadius / loseRadiusExtra: hysteresis para evitar parpadeos de estado.
- chaseSpeedMultiplier: reutiliza velocidad base (reflexión sobre campo privado speed) si existe `EnemyPatrol2D`.
- attackRange / attackCooldown: control de timing de ataques.
- player: opcional; si null busca por tag "Player".

Gizmos: círculos concéntricos (detección, perder persecución, rango de ataque).

### 7.2 Fix Trigger Animación Daño del Jugador
Problema detectado: trigger `isAttacked` solo reproducía la animación la primera vez.
Causas comunes:
- El Animator permanece en el mismo estado y subsecuentes SetTrigger no causan transición.
- Falta un ResetTrigger previo.
Solución aplicada en `CharacterController.PerderVidaPJ()`:
```csharp
animator.ResetTrigger(isAttackedId);
animator.SetTrigger(isAttackedId);
StartCoroutine(ResetAttackedFlagSafeguard());
```
La corrutina limpia tras un pequeño delay para permitir re-disparo. Recomendaciones extra:
- Asegurar que la transición de salida del estado de daño tenga Has Exit Time o condiciones distintas.
- Evitar usar simultáneamente bool + trigger con el mismo propósito.

---
## 8. Cómo guardar más notas
### Nota (Reversión a CustomEditor)
Se revirtió el uso de atributos condicionales en `EnemyPatrol2D` porque al combinar la lógica de persecución y los atributos algunas referencias (ej. `center`) podían no inicializarse/verse claramente al cambiar de modo, lo que daba la impresión de que los gizmos de rango desaparecían. Al volver al Inspector clásico + `EnemyPatrol2DEditor`:
- Se garantizan siempre los campos visibles según modo.
- Los gizmos de rango dependen de `center` y offsets; si `center` es null usa la posición actual. Asegurarse de que no se destruye ni se mueve inadvertidamente.
- Para estudiar ambos enfoques se conserva el CustomEditor en `Assets/Scripts/Editor/Custom/`.
- Añade bloques nuevos aquí con fecha:
```
### [2025-09-16] Tema tratado
Resumen...
```
- Commits frecuentes para no perder estado.

---
## 9. FAQ Rápido
**P: Por qué no se gira al borde?**  groundCheck mal posicionado o distancia corta.
**P: Por qué atraviesa paredes?** Falta Layer en `groundMask` o collider no está en capa correcta.
**P: Animator no cambia a running?** Revisar nombre del parámetro `runningId`.
**P: Se ve invertido el sprite?** Ajustar `startFacingRight` o invertir escala inicial en el prefab.

---
## 10. [2025-09-18] Mejoras posteriores no listadas previamente

### 10.1 Refinamiento modo Range
- Nuevo toggle `useOwnCenter`: ancla el rango a la X inicial del enemigo (ignora `center`). Simplifica y evita crear un objeto `PatrolCenter` cuando no hace falta.
- Nuevo toggle `useColliderWidth`: el giro en los límites considera el borde frontal del enemigo usando `Collider2D.bounds.extents.x`, ofreciendo precisión visual (el sprite gira cuando su borde toca el límite, no su pivote).
- Fallback elegante: si no hay collider en el root busca en hijos (`GetComponentInChildren<Collider2D>`).
- Gizmos: marcas verdes adicionales muestran límites y ancho si `useColliderWidth` está activo.

### 10.2 Hitbox de arma del enemigo
Sistema añadido para daño sólo durante la ventana real del ataque:
- `EnemyWeaponHitbox`: colocado en un hijo con `Collider2D` (isTrigger). Se desactiva por defecto y se activa con eventos de animación. Aplica daño filtrando capa/tag del Player.
- `EnemyAttackAnimatorBridge`: recibe eventos de animación `Enemy_AttackHitbox_On()` y `Enemy_AttackHitbox_Off()` para activar/desactivar arrays de hitboxes.
- Beneficio: evita daño permanente por contacto y mejora sincronización con la animación.

### 10.3 Comparativa métodos de daño
| Método | Precisión temporal | Configuración | Uso típico |
|--------|--------------------|---------------|-----------|
| Daño por contacto (OnCollision/Trigger en cuerpo) | Baja | Muy simple | Enemigos tipo espina / lava |
| Trigger de Animator + lógica interna (sin hitbox separada) | Media | Simple | Golpes básicos |
| Hitbox activada por animación (actual) | Alta | Moderada (eventos + hijo) | Ataques cuerpo a cuerpo precisos |
| Raycast/Overlap manual en frame clave | Muy alta | Mayor código | Proyectiles, ataques direccionales |

### 10.4 Recomendación de Layers
Sugeridas (ajusta a tu proyecto): `Player`, `PlayerWeapon`, `Enemy`, `EnemyWeapon`, `Ground`.
Matriz clave:
- PlayerWeapon ↔ Enemy (sí).
- EnemyWeapon ↔ Player (sí).
- EnemyWeapon ✕ Enemy (no) para evitar auto-golpes.
- PlayerWeapon ✕ Player (no) salvo que quieras auto-daño.
- Ajustar `Physics2D > Layer Collision Matrix` y recordar que `isTrigger` sigue necesitando que la pareja esté habilitada en la matriz.

### 10.5 Oportunidades futuras (pendientes)
- Recalcular `centerX` dinámicamente (plataformas móviles). Opción propuesta: flag `recalculateCenterOnMove` o método público `RecomputeCenter()`.
- Propiedad pública `BaseSpeed` en `EnemyPatrol2D` para que `EnemyChaseBehaviour` deje de usar reflexión.
- Interfaz `IDamageable` para desacoplar daño del jugador y enemigos.
- Ventana de debug en editor que muestre estado actual (patrullando / persiguiendo / atacando).

### 10.6 Resumen rápido de la sesión
- Añadidos toggles `useOwnCenter` y `useColliderWidth` al modo Range.
- Editor personalizado actualizado para mostrar los nuevos toggles con ayuda contextual.
- Ajustes de gizmos para reflejar centro propio y ancho real del collider.
- Incorporado sistema de hitbox activada por animación (arma enemigo) + puente de eventos.
- Documentadas recomendaciones de layers y comparativa de métodos de daño.

---
© Proyecto PlataformaGame – Notas técnicas (actualizado 2025-09-18).
 
---
## 11. [2025-09-23] Nuevas mejoras (detección cápsula, capas armas, bloqueo por animación)

### 11.1 Chase con detección en cápsula
`EnemyChaseBehaviour` ahora soporta área de detección tipo cápsula vertical configurable (altura, radio, offsets, X/Y):
- Campos nuevos: `useCapsuleDetection`, `capsuleHeight`, `capsuleRadius`, `detectOffsetX`, `detectOffsetY`.
- Permite al enemigo “ver” en una banda vertical (útil para plataformas) en vez de usar solo un círculo.
- Si `capsuleRadius` = 0 usa `detectionRadius` como radio lateral.
- Offsets permiten adelantar la cápsula hacia donde mira el enemigo.
- Gizmos: se dibujan dos semicírculos y líneas laterales; el área de pérdida (lose) se visualiza más tenue.

### 11.2 Ventana vertical de ataque
Campo nuevo: `attackVerticalWindow` – el enemigo solo ataca si la diferencia vertical |ΔY| <= ventana. Evita golpes cuando el jugador está claramente arriba/abajo.

### 11.3 Integración con velocidad base sin reflexión
`EnemyPatrol2D` expone `public float BaseSpeed => speed;` y `EnemyChaseBehaviour` deja de usar reflexión para leer la velocidad.

### 11.4 Bloqueo de movimiento durante animaciones críticas
Se añadió `SetExternalMovementLock(bool)` en `EnemyPatrol2D` y en `Enemigo` se usa en la secuencia de muerte:
- Al morir: se bloquea patrulla, se desactiva chase, se pone velocidad a 0 y se lanza el trigger `deathTrigger` antes de `Destroy`.
- Evita que el enemigo siga deslizándose mientras muere o ataca (puedes también llamar a `SetExternalMovementLock(true)` desde un evento de animación de ataque si quieres inmovilizar completamente durante el golpe).

### 11.5 Hitbox de arma enemigo refinada
`EnemyWeaponHitbox` ahora:
- Usa `LayerMask playerMask` + tag fallback.
- Opción `autoDisableAfterHit` para desactivar el GO tras un impacto (controlado de nuevo por evento de animación para reactivarlo).
- Reproduce sonido solo si está asignado.

### 11.6 Balas usando capas
`Bullet.cs` incorpora `LayerMask enemyLayers` además del tag; la colisión valida `layerMatch || tag` para flexibilidad de transición.

### 11.7 Detección y ataque ordenados
Secuencia: Detección (cápsula/círculo) -> Chasing -> Ataque (verifica distancia + ventana vertical + cooldown) -> Activación de hitbox por animación.

### 11.8 Recomendación adicional (pendiente opcional)
- Exponer un evento C# (Action) en `Enemigo` para notificar UI al morir.
- Reemplazar búsqueda de Player (`FindGameObjectWithTag`) por cache central (ya se usa `GameManager.Instance.player`).
- Ajustar duración `DeathSequence` al clip real: sustituir 0.6f por `GetAnimationLength(deathTrigger)` si se implementa un helper.

### 11.9 Checklist de configuración tras cambios
1. Asignar `playerMask` en `EnemyChaseBehaviour` y `EnemyWeaponHitbox` (capa Player).
2. Ajustar `capsuleHeight` para cubrir salto medio del jugador, pero no pisos arriba.
3. `detectOffsetX` positivo para mirar hacia delante (0.5–1.0 recomendado) si el pivot está al centro.
4. Ajustar `attackVerticalWindow` a tolerancia (1.0–1.5 inicial).
5. Revisar que la animación de muerte tenga el trigger definido en `deathTrigger` (por defecto "Die").
6. Asegurar que los eventos de animación de ataque vuelven a activar/desactivar hitbox.

### 11.10 Resumen rápido
- Detección más precisa (cápsula vertical con offsets).
- Ataques restringidos por ventana vertical.
- Patrulla/chase se integran mejor con muerte y animaciones.
- Hitboxes y balas usan capas configurables.
- Código más limpio (sin reflexión).

### 11.11 Espera dinámica de animación de muerte (nuevo) Comentado pq no me cuadra
Archivo: `Enemigo.cs`

- Nuevos campos:
    - `waitForDeathAnimation` (bool): si está activo, la corrutina de muerte espera a que el Animator entre y finalice un estado con tag `deathStateTag`.
    - `deathStateTag` (string): etiqueta del estado de muerte. Configura tu estado de muerte en el Animator con esta Tag (por defecto "Death").
    - `deathTime` (float): tiempo de respaldo si no se puede detectar la animación.
- Protección: `isDying` evita arrancar múltiples veces la secuencia.
- Integración: se dispara el trigger `isDead` y se bloquea movimiento (patrulla/chase) antes de destruir el GO cuando termina el clip.

Checklist para usarlo:
1) En tu Animator, abre el state de muerte y en la esquina superior izquierda asigna la Tag "Death" (o el valor de `deathStateTag`).
2) Asegúrate de que la transición al estado de muerte se activa con el trigger configurado (por defecto `isDead`).
3) Si no deseas esperar al final del clip, desactiva `waitForDeathAnimation` y se usará `deathTime` como fallback.

---
© Proyecto PlataformaGame – Notas técnicas (actualizado 2025-09-23).

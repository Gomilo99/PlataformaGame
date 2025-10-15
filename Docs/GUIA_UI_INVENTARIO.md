## Guía de UI, Pausa, Menú e Inventario (Unity 2D)

Esta guía resume cómo montar la interfaz del juego (menú principal, pausa, botones con hover/click) y un inventario básico reutilizable. Se asume Unity con Input clásico y EventSystem.

### 1) Estructura de escenas
- MainMenu: pantalla principal con botones Jugar, Seleccionar Nivel (opcional), Salir.
- Level_01 (tu nivel actual): contiene UI in-game (HUD/Inventario) y Panel de Pausa.

### 2) Menú principal
1. Crea Canvas (Screen Space - Overlay), agrega Panel de fondo.
2. Añade botones (UI > Button) y agrega `UIButtonHover` a cada uno para hover/click visual.
3. Añade `MainMenuController` a un GO vacío y conecta:
   - Botón Jugar → OnClick → MainMenuController.Play()
   - O usa `LoadSceneByName("Level_01")` si cambias el nombre.
   - Botón Salir → OnClick → MainMenuController.Quit()
4. Asegúrate de incluir las escenas en Build Settings.

### 3) Pausa en nivel (GameManager unificado)
1. En la escena del nivel, asegúrate de tener un `GameManager` (único) con:
   - `pausePanel` asignado (Panel de pausa en Canvas) y `winPanel` si lo usas.
   - Metas opcionales: `targetCoins`, `targetEnemies`.
2. Crea un Panel de Pausa dentro de un Canvas (Screen Space - Overlay):
   - Botones: Reanudar, Reiniciar, Menú.
   - Añade `UIButtonHover` a los botones para feedback visual.
   - Conecta sus OnClick directamente a métodos del HUD (que internamente llaman al GameManager):
     - Reanudar → `HUD.UI_Resume()`
     - Reiniciar → `HUD.UI_RestartLevel()`
     - Menú → `HUD.UI_ExitToMainMenu()`
3. Pulsa ESC para alternar pausa: el `GameManager` captura ESC en `Update()` y llama Pause/Resume.

### 4) Botones de Unity: cómo funcionan y parámetros
Los `Button` de Unity incluyen transición visual integrada y eventos:
- Transition:
   - Color Tint: define Normal, Highlighted, Pressed, Selected, Disabled. Úsalo si solo necesitas cambiar color/alpha.
   - Sprite Swap: reemplaza el sprite según estado.
   - Animation: anima propiedades con un Animator.
- Interactable: habilita/deshabilita el botón.
- Navigation: navegación por teclado/gamepad (Automatic/Explicit/None).
- Target Graphic: el Graphic (Image/Text) que se colorea en Color Tint.
- OnClick(): lista de callbacks. Aquí conectas métodos públicos (por ejemplo `HUD.UI_Resume()`).

¿Cuándo usar `UIButtonHover`?
- Si necesitas lógica adicional en hover/click (sonido, escala, efectos que no cubre Color Tint) sin crear un Animator.
- Implementa `IPointerEnter/Exit/Down/Up/Click` para reaccionar con precisión (p.ej., reproducir sonido al entrar/salir, escalar levemente el botón).

### 5) Reacciones de mouse (interfaces Unity) y casos de uso
Interfaces útiles del EventSystems:
- IPointerEnterHandler / IPointerExitHandler: hover in/out (tooltips, resaltar)
- IPointerDownHandler / IPointerUpHandler: pulsación mantenida (botones “hold”, sliders custom)
- IPointerClickHandler: click final (usar, equipar)
- IBeginDragHandler / IDragHandler / IEndDragHandler: drag & drop (reordenar slots, mover ítems, arrastrar ventanas)
- IDropHandler: receptor de drop (inventario, equipamiento)
- IScrollHandler: zoom o scroll de listas
- ISelectHandler / IDeselectHandler: enfoque de navegación (gamepad/teclado)

Ideas prácticas:
- Inventario: arrastrar un ítem entre slots (begin/drag/end + drop), click derecho para usar/descartar.
- Barra rápida: arrastrar desde inventario a hotbar; click para consumir.
- Tooltips: mostrar descripción al hacer hover en slots o botones.
- Ventanas: hacer drag del título para mover paneles.
- Reasignación de teclas: botones que cambian de estado y capturan próxima tecla (IPointerDown + escucha de input).

### 6) Inventario básico
Modelo:
- `InventoryModel` (ScriptableObject) con slots con stack. Crea uno: Right Click en Project → Create → PlataformaGame → Inventory → InventoryModel.
- Define capacidad (p.ej. 12).

Datos de ítems:
- `ItemData` (clase serializable simple). Puedes convertir a ScriptableObject si necesitas catálogos grandes.

UI:
1. Canvas In-Game → Panel Inventario (puede estar oculto y abrirse con una tecla o botón).
2. Dentro, un GameObject con GridLayoutGroup (por ejemplo 4 cols x 3 filas).
3. Crea un prefab `Slot` con:
   - Image de fondo (para hover),
   - Image de icono (hijo),
   - Text (cantidad) (u TextMeshProUGUI si usas TMP).
4. Añade `InventoryUI` al Panel, asigna:
   - model: tu `InventoryModel` creado,
   - gridRoot: el contenedor con GridLayout,
   - slotPrefab: tu prefab Slot.

Poblar/Probar:
- Desde cualquier script de juego, llama `model.Add(itemData, cantidad)` para ver aparecer los ítems en la UI.
- `InventoryUI` re-renderiza cuando el modelo emite `OnChanged`.

### 7) Variaciones útiles
- Mostrar/ocultar el inventario con una tecla: activa/desactiva el Panel y pausa o no el juego según prefieras.
- Usar TextMeshPro: cambia los componentes `Text` por `TextMeshProUGUI` y ajusta `InventoryUI` para encontrarlos (o referencia explícita por script).
- Drag & drop: añade interfaces `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler` a `InventoryUISlot` y una capa visual para el icono arrastrado.

### 8) Consejos de organización
- Capas: Menús (Screen Space Overlay), HUD (Overlay), Barras de vida de enemigos (World Space) para claridad.
- Sonidos de UI: añade `AudioSource` y dispara clips en hover/click desde `UIButtonHover`.
- Navegación por gamepad/teclado: usa Navigation de los Button si lo necesitas.

### 9) Integración con GameManager (unificado)
- `GameManager` ahora maneja Pause/Resume/Reset, metas y win panel.
- El HUD expone `UI_Resume`, `UI_RestartLevel`, `UI_ExitToMainMenu` para conectar botones.
- HUD escucha eventos (pausa/resume/level reset/coin/enemy) y actualiza textos.

### 10) Checklist rápido
- [ ] Scenes en Build Settings (MainMenu, Level_01)
- [ ] Canvas MainMenu con botones y `MainMenuController`
- [ ] Canvas Pausa con `PauseMenuController` y `PauseInput` en escena
- [ ] InventoryModel asset, `InventoryUI` configurado y slotPrefab hecho
- [ ] EventSystem presente en escenas con UI

---
Con esto tienes lo esencial: pausa con ESC y hover/click con interfaces, menú principal funcional y un inventario visual básico que puedes expandir.

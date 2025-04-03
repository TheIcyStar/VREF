# EQUIManager

This class manages the Equation UI, including its positioning and visibility, in a VR environment.

## Public Fields

### `playerHead : Transform`

the players main camera

### `equiCanvasGroup : CanvasGroup`

canvas group of the equation UI used for toggling visibility

## Private Fields

### `distance : float`

distance away from the player the UI relocates to

### `height : float`

the exact height the UI relocates to

### `inputActions : XRInput`

### `isUIActive : bool`

## Private Methods

### `Awake() : void`

### `OnEnable() : void`

### `OnDisable() : void`

### `MoveUIInFrontOfPlayer() : void`

### `ToggleUIVisibility() : void`
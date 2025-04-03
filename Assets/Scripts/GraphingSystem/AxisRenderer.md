# AxisRenderer

This class manages the rendering of the coordinate axes in the graph.

## Public Fields

### `xAxis : LineRenderer`

### `yAxis : LineRenderer`

### `zAxis : LineRenderer`

### `xStartArrow : Transform`

### `xEndArrow : Transform`

### `yStartArrow : Transform`

### `yEndArrow : Transform`

### `zStartArrow : Transform`

### `zEndArrow : Transform`

## Public Methods

### `InitializeAxes() : void`

does nothing for now

### `UpdateAxes(GraphSettings graphSettings) : void`

updates all three axes based on new graph settings

## Private Methods

### `UpdateAxis(LineRenderer line, Vector3 direction, float length, Transform startArrow, Transform endArrow) : void`

updates a specific axis's length
# SurfaceGraphRenderer

This class handles the rendering of surface graphs.

## Public Methods

### `SurfaceGraphRenderer(Transform parent, Material meshColor) : void`

### `RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar) : void`

## Private Fields

### `graphParent : Transform`

### `segmentSurfaces : List<(MeshFilter filter, MeshRenderer renderer)>`

gameobject contains the MeshRenderer and the MeshFilter

### `meshColor : Material`

## Private Methods

### `AssignPoint(float inputVar1Val, float inputVar2Val, float outputVarVal, GraphVariable inputVar1, GraphVariable inputVar2, GraphVariable outputVar) : Vector3`

x is z, z is y, and y is x

### `GenerateMesh(List<List<Vector3>> currentGrid) : (List<Vector3>, List<int>)`

generates a mesh based on a grid of points
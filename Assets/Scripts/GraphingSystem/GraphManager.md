# GraphManager

This class manages the creation and storage of graph objects in the scene.

## Public Fields

### `graphPrefab : GameObject`

graph prefab

### `equationUITransform : Transform`

transform of the equation UI

### `globalGraphSettings : GraphSettings`

global graph settings from options menu
storing default values for now

### `defaultLineColor : Material`

default material of graph lines

### `defaultMeshColor : Material`

default material of meshes

### `instance : GraphManager`

## Public Methods

### `Awake() : void`

Ensures there's only one ServerConnection object

### `Start() : void`

### `GetGraphs() : ParseTreeNode[]`

### `BulkOverwriteGraphs(ParseTreeNode[] equations) : void`

### `CreateNewGraph(ParseTreeNode equationTree) : void`

creates the graph object

## Private Fields

### `graphDisplacement : float`

distance from the UI that all graphs will spawn in from

### `graphHeight : float`

distance from the ground that the origin of the graph spawns above

### `graphs : Dictionary<GameObject, GraphInstance>`

dictionary that maps prefabs to their manager script

### `graphCount : int`

keep track of graph count solely for naming (for now)
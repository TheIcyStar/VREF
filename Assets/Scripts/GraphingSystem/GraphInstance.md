# GraphInstance

This class represents a single instance of a graph, managing its data, rendering, and settings.

## Public Fields

### `equationTree : ParseTreeNode`

the root node of the equation tree

### `axisRenderer : AxisRenderer`

### `graphVisualsObj : GameObject`

### `graphObj : GameObject`

### `xAxisMinUI : TMP_InputField`

all graph settings UI elements

### `xAxisMaxUI : TMP_InputField`

### `yAxisMinUI : TMP_InputField`

### `yAxisMaxUI : TMP_InputField`

### `zAxisMinUI : TMP_InputField`

### `zAxisMaxUI : TMP_InputField`

### `xRotationUI : TMP_InputField`

### `yRotationUI : TMP_InputField`

### `zRotationUI : TMP_InputField`

### `stepUI : TMP_InputField`

## Public Methods

### `InitializeGraph(ParseTreeNode equationTree, Material defaultLineColor, Material defaultMeshColor, GraphSettings settings) : void`

does nothing w/ def lin color for now

### `UpdateGraphSettings() : void`

updates the graph settings and re-renders the graph

### `ResetToDefault() : void`

for now it just sets rotation to 0, 0, 0

## Private Fields

### `graphRenderer : IGraphRenderer`

renderer interface set to the correct renderer type

### `graphSettings : GraphSettings`

graph settings passed in

### `originalSettings : GraphSettings`

store original settings

### `inputVars : HashSet<GraphVariable>`

set of input variables (independent variables)

### `outputVar : GraphVariable`

single output var computed by the function (dependent variable)

### `TYPE_LINE : int`

equation renderer types
change to enums later

### `TYPE_SURFACE : int`

## Private Methods

### `RefreshGraph() : void`

### `ScaleGraph() : void`

scale the graph based on the largest range

### `VisualizeGraph() : void`

renders the graph using the respective renderer and settings

### `RefreshAllUIText() : void`

updates the entire UI to have the current graph settings displayed

### `RefreshUIText(TMP_InputField textField, float value) : void`

updates a specific UI element's text to a value

### `DetermineEquationType(ParseTreeNode equationTree) : (int, HashSet<GraphVariable>, GraphVariable)`

determines the type of equation by analyzing the parse tree
also returns the input and output variables
ONLY WORKS WITH [output var] = [f(input var)] FOR NOW

### `DetermineVariables(ParseTreeNode equationTree, HashSet<GraphVariable> variables) : void`

finds all the variables in the given tree and adds them to a set
potentially could create the set during tree creation if this
full tree traversal ends up being too costly

### `ConvertToGraphVariable(string stringVar) : GraphVariable`

converts a string to a GraphVariable
if we really want to optimize everything down the line
we can change the tree to use an enum for every possible
token to avoid using strings entirely
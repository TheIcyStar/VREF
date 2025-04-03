# LineGraphRenderer

This class handles the rendering of line graphs.

## Public Methods

### `LineGraphRenderer(Transform parent, Material lineColor) : void`

### `RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar) : void`

right now this function uses a very basic approach:
go from inputMin -> inputMax and plot all points
the problem is the output values can be outside the outputRange
one solution was binary search at the edge once the value left the range
you would search between the point in range and the point out of range
to find the closest point to the range value, but that did not end up
working for graphs with huge jumps from -inf to inf, like y = 1/sin(x)
the next solution to solve this would be to adaptively sample the sections
where the graph jumps too much, and give them a higher step count
this requires much more implementation and will be done later on
for now, endcaps are not touched, and all the points are simply plotted

## Private Fields

### `graphParent : Transform`

### `segmentRenderers : List<LineRenderer>`

### `lineColor : Material`

## Private Methods

### `AssignPoint(float inputVarVal, float outputVarVal, GraphVariable inputVar, GraphVariable outputVar) : Vector3`

assigns the point to the correct axis
this goes against the default unity positions
x is z, z is y, and y is x
very annoying
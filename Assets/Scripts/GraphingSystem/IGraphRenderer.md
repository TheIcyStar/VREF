# IGraphRenderer

Interface for all graph renderers.

## Public Methods

-   `void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar)`
    -   Renders the graph based on the equation tree, graph settings, input variables, and output variable.

## Protected Internal Static Methods

-   `float GetAxisMin(GraphSettings settings, GraphVariable variable)`
    -   Gets the minimum value of the specified variable axis from the graph settings.
-   `float GetAxisMax(GraphSettings settings, GraphVariable variable)`
    -   Gets the maximum value of the specified variable axis from the graph settings.
-   `float EvaluateEquation(ParseTreeNode node, Dictionary<string, float> vars)`
    -   Evaluates the equation represented by the parse tree node with the given variable values.
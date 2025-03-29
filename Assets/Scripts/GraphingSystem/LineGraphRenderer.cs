using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// this renderer will always assume the unused variable is 0
// and therefore can only graph on a 2d plane (xy, yz, xz)
public class LineGraphRenderer : IGraphRenderer
{
    private Transform graphParent;
    private List<LineRenderer> segmentRenderers;
    private Material lineColor;

    public LineGraphRenderer(Transform parent, Material lineColor)
    {
        this.graphParent = parent;
        this.segmentRenderers = new();
        this.lineColor = lineColor;
    }

    // right now this function uses a very basic approach:
    // go from inputMin -> inputMax and plot all points
    // the problem is the output values can be outside the outputRange
    // one solution was binary search at the edge once the value left the range
    // you would search between the point in range and the point out of range
    // to find the closest point to the range value, but that did not end up 
    // working for graphs with huge jumps from -inf to inf, like y = 1/sin(x)
    // the next solution to solve this would be to adaptively sample the sections
    // where the graph jumps too much, and give them a higher step count
    // this requires much more implementation and will be done later on
    // for now, endcaps are not touched, and all the points are simply plotted
    public void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar)
    {
        // extract the input var
        GraphVariable inputVar = inputVars.First();

        // initialize the variable dictionary (only one var for line graph)
        Dictionary<string, float> variables = new(){ { inputVar.ToString().ToLower(), 0f } };

        // since its explicit, dont need LHS of equal sign
        equationTree = equationTree.right;

        // initialize list of graph segments
        List<List<Vector3>> segments = new();

        // deal with constants later, as this would require multiple variable mappings at the same time
        // i.e. x = 5, z = 0 to get a constant line on the xz plane, as just x = 5 would have to decide which var to set to 0

        // find the mins and maxes of the graph for the input and output
        float inputMin = IGraphRenderer.GetAxisMin(settings, inputVar);
        float inputMax = IGraphRenderer.GetAxisMax(settings, inputVar);
        float outputMin = IGraphRenderer.GetAxisMin(settings, outputVar);
        float outputMax = IGraphRenderer.GetAxisMax(settings, outputVar);   

        // track when the graph is in range to only graph valid points
        bool previousInRange = false;

        // go through each point of the indepedent variable and calculate the value of the RHS, then plot
        for (float inputVal = inputMin; inputVal < inputMax; inputVal += settings.step)
        {
            // set the variable dictionary to store the current input val
            variables[inputVar.ToString().ToLower()] = inputVal;
            // find what the output evaluates to when given the input at this point
            float outputVal = IGraphRenderer.EvaluateEquation(equationTree, variables);
            
            // check if current function value is in the output range
            bool currentInRange = !float.IsNaN(outputVal) && outputVal >= outputMin && outputVal <= outputMax;

            // only add points when in range
            if (currentInRange) {
                // just came back in range, start a new segment
                if(!previousInRange) segments.Add(new List<Vector3>());

                // determine the correct axis of the graph to add the point to
                Vector3 currentPoint = AssignPoint(inputVal, outputVal, inputVar, outputVar);

                // add the point to the current segment
                segments.Last().Add(currentPoint);
            }
            
            previousInRange = currentInRange;
        }

        // basic object pooling, only add new line renderers when needed, and prioritize using old ones
        // this will only matter for graph editing (later)
        for (int i = 0; i < segments.Count; i++)
        {
            if (i >= segmentRenderers.Count)
            {
                GameObject segmentObj = new GameObject($"Line Segment {i + 1}");
                segmentObj.transform.SetParent(graphParent, false);

                LineRenderer newRenderer = segmentObj.gameObject.AddComponent<LineRenderer>();

                newRenderer.material = lineColor;

                newRenderer.useWorldSpace = false;
                newRenderer.startWidth = newRenderer.endWidth = 0.02f;
                segmentRenderers.Add(newRenderer);
            }

            LineRenderer renderer = segmentRenderers[i];
            List<Vector3> segment = segments[i];
            renderer.positionCount = segment.Count;
            renderer.SetPositions(segment.ToArray());
            renderer.enabled = true;
        }

        // disable any unused renderers
        for (int i = segments.Count; i < segmentRenderers.Count; i++)
        {
            segmentRenderers[i].enabled = false;
        }
    }

    // assigns the point to the correct axis
    // this goes against the default unity positions
    // x is z, z is y, and y is x
    // very annoying
    private Vector3 AssignPoint(float inputVarVal, float outputVarVal, GraphVariable inputVar, GraphVariable outputVar)
    {
        return (inputVar, outputVar) switch
        {
                                                            //     x       y        z
            (GraphVariable.X, GraphVariable.Y) => new Vector3(outputVarVal, 0, inputVarVal),
            (GraphVariable.X, GraphVariable.Z) => new Vector3(0, outputVarVal, inputVarVal),
            (GraphVariable.Y, GraphVariable.X) => new Vector3(inputVarVal, 0, outputVarVal),
            (GraphVariable.Y, GraphVariable.Z) => new Vector3(inputVarVal, outputVarVal, 0),
            (GraphVariable.Z, GraphVariable.X) => new Vector3(0, inputVarVal, outputVarVal),
            (GraphVariable.Z, GraphVariable.Y) => new Vector3(outputVarVal, inputVarVal, 0),
            // this should already be stopped
            _ => throw new GraphEvaluationException("Point lies on unknown plane (only XY, XZ, ZY planes supported).")
        };
    }
}
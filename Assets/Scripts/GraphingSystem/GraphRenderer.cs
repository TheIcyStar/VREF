using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;


// TODO: Do the following when refactoring all scripts
// -move the graph variable enum to its own file
// -change token type to an enum
// -store token type in its own file
// -store the interface in a separate file
// -store LineGraphRenderer in a separate file

// graph variable enum to avoid string comparison
public enum GraphVariable {
    X,
    Y,
    Z,
    Constant
}

// interface for all graph renderers
public interface IGraphRenderer
{
    void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar);

    // get min value of inputted variable axis
    protected internal static float GetAxisMin(GraphSettings settings, GraphVariable variable)
    {
        return variable switch
        {
            GraphVariable.X => settings.xMin,
            GraphVariable.Y => settings.yMin,
            GraphVariable.Z => settings.zMin,
            _ => throw new GraphEvaluationException("Unknown variable attempting to be graphed.")
        };
    }

    // get max value of inputted variable axis
    protected internal static float GetAxisMax(GraphSettings settings, GraphVariable variable)
    {
        return variable switch
        {
            GraphVariable.X => settings.xMax,
            GraphVariable.Y => settings.yMax,
            GraphVariable.Z => settings.zMax,
            _ => throw new GraphEvaluationException("Unknown variable attempting to be graphed.")
        };
    }

    // evaluates the equation to solve for the LHS value at a certain point
    protected internal static float EvaluateEquation(ParseTreeNode node, Dictionary<string, float> vars)
    {
        switch (node.token.type)
        {
            case EquationParser.TYPE_NUMBER:
                return float.TryParse(node.token.text, out float num) ? num : throw new GraphEvaluationException($"Incorrect number format for '{node.token.text}'.");
            case EquationParser.TYPE_VARIABLE:
                if (!vars.TryGetValue(node.token.text, out float val))
                    throw new GraphEvaluationException($"Variable '{node.token.text}' not found.");
                return val;
            case EquationParser.TYPE_OPERATOR:
                // treat missing left operand as 0 so that unary minus works without left operand (negation)
                // every missing operand error should be stopped in the parser before it gets here
                float left = node.left != null ? EvaluateEquation(node.left, vars) : 0;
                float right = EvaluateEquation(node.right, vars);
                return node.token.text switch
                {
                    "+" => left + right,
                    "-" => left - right,
                    "*" => left * right,
                    // something like 1 / x will have ungraphable values set to NaN
                    // but something like 1 / 0 will throw an error
                    "/" => right != 0 ? left / right : (node.right.token.type == EquationParser.TYPE_NUMBER ? throw new GraphEvaluationException("Cannot divide by zero.") : float.NaN),
                    "^" => Mathf.Pow(left, right),
                    // should never happen, should be caught in parser
                    _ => throw new GraphEvaluationException($"Unsupported operator '{node.token.text}'.")
                };
            case EquationParser.TYPE_FUNCTION:
                // functions have their expression on the right
                float arg = EvaluateEquation(node.right, vars);
                return node.token.text switch
                {
                    "sin" => Mathf.Sin(arg),
                    "log" => Mathf.Log(arg),
                    "sqrt" => Mathf.Sqrt(arg),
                    // should never happen, should be caught in parser
                    _ => throw new GraphEvaluationException($"Unsupported function type '{node.token.text}'.")
                };

            default:
                throw new GraphEvaluationException("Unknown token type in evaluation.");
        }
    }
}

// this renderer will always assume the unused variable is 0
// and therefore can only graph on a 2d plane (xy, yz, xz)
public class LineGraphRenderer : IGraphRenderer
{
    private Transform graphParent;
    private List<LineRenderer> segmentRenderers;

    public LineGraphRenderer(Transform parent)
    {
        this.graphParent = parent;
        this.segmentRenderers = new();
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
                GameObject segmentObj = new GameObject($"Segment {i + 1}");
                segmentObj.transform.SetParent(graphParent, false);

                LineRenderer newRenderer = segmentObj.gameObject.AddComponent<LineRenderer>();
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

public class SurfaceGraphRenderer : IGraphRenderer
{
    private Transform graphParent;
    // gameobject contains the MeshRenderer and the MeshFilter
    private List<GameObject> surfaceSegments;

    public SurfaceGraphRenderer(Transform parent) 
    {
        this.graphParent = parent;
        this.surfaceSegments = new();
    }
    public void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar) 
    {

    }

    private Vector3 AssignPoint(float inputVar1Val, float inputVar2Val, float outputVarVal, GraphVariable inputVar1, GraphVariable inputVar2, GraphVariable outputVar) 
    {
        return new Vector3(0, 0, 0);
    }
}

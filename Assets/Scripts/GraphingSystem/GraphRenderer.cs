using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;

// TODO: Do the following when refactoring all scripts
// -move the utils class to its own file
// -move the graph variable enum to its own file
// -change token type to an enum
// -store token type in its own file

// graph variable enum to avoid string comparison
public enum GraphVariable {
    X,
    Y,
    Z,
    Constant
}

// helpful functions for multiple graph types
public static class GraphUtils
{
    // use binary search to find the edge point to cutoff the graph
    public static float BinarySearchEdge(
        // one value inside the range, one value outside the range
        float inputLow, float inputHigh,
        // function used to find new points
        // use a delegate to decouple evaluation logic
        Func<float, float> evalFunc, 
        // range you are trying to stay between
        float minOutput, float maxOutput, 
        // only search this many times, or up to this accuracy
        int maxIterations = 20, float epsilon = 0.0001f) 
    {
        // keep track of current low and current high
        float low = inputLow;
        float high = inputHigh;

        // use binary search
        for (int i = 0; i < maxIterations && Mathf.Abs(high - low) > epsilon; i++)
        {
            // find midpoint
            float mid = (low + high) / 2f;

            // find new value at this midpoint
            float val = evalFunc(mid);

             // still out of range, search left
            if (float.IsNaN(val) || val < minOutput || val > maxOutput)
            {
                high = mid;
            }
             // still in range, store as best candidate
            else
            {
                low = mid;
            }
        }

         // best estimate of last in-range input
        return low;
    }
}

// interface for all graph renderers
public interface IGraphRenderer
{
    void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar);

    // get min value of inputted variable axis
    protected static float GetAxisMin(GraphSettings settings, GraphVariable variable)
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
    protected static float GetAxisMax(GraphSettings settings, GraphVariable variable)
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
    protected static float EvaluateEquation(ParseTreeNode node, Dictionary<string, float> vars)
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
                    "/" => right != 0 ? left / right : (node.left.token.type == EquationParser.TYPE_NUMBER ? throw new GraphEvaluationException("Cannot divide by zero.") : float.NaN),
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
        // this.lineRenderer = renderer;
        // this.lineRenderer.useWorldSpace = false;
        // this.lineRenderer.startWidth = .02f;
        // this.lineRenderer.endWidth = .02f;

        this.graphParent = parent;
        this.segmentRenderers = new();
    }

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
        segments.Add(new List<Vector3>());

        // deal with constants later, as this would require multiple variable mappings at the same time
        // i.e. x = 5, z = 0 to get a constant line on the xz plane, as just x = 5 would have to decide which var to set to 0

        // find the mins and maxes of the graph for the input and output
        float inputMin = IGraphRenderer.GetAxisMin(settings, inputVar);
        float inputMax = IGraphRenderer.GetAxisMax(settings, inputVar);
        float outputMin = IGraphRenderer.GetAxisMin(settings, outputVar);
        float outputMax = IGraphRenderer.GetAxisMax(settings, outputVar);   

        // track when the graph is in range to only graph valid points
        bool inRange = false;
        Vector3? previousPoint = null;
        float previousInputVal = inputMin;

        // go through each point of the indepedent variable and calculate the value of the RHS, then plot
        for (float inputVal = inputMin; inputVal < inputMax; inputVal += settings.step)
        {
            // set the variable dictionary to store the current input val
            variables[inputVar.ToString().ToLower()] = inputVal;
            // find what the output evaluates to when given the input at this point
            float outputVal = IGraphRenderer.EvaluateEquation(equationTree, variables);
            // determine the correct axis of the graph to add the point to
            Vector3 currentPoint = AssignPoint(inputVal, outputVal, inputVar, outputVar);

            // point is in graphing range
            if (!float.IsNaN(outputVal) && outputVal >= outputMin && outputVal <= outputMax) {
                // point just entered graphing range
                // use binary search to find edge point
                if(!inRange && previousPoint.HasValue)  {
                    float entryInputVal = GraphUtils.BinarySearchEdge(
                        previousInputVal, inputVal,
                        v => {
                            variables[inputVar.ToString().ToLower()] = v;
                            return IGraphRenderer.EvaluateEquation(equationTree, variables);
                        },
                        outputMin, outputMax
                    );
                    variables[inputVar.ToString().ToLower()] = entryInputVal;
                    float entryOutputVal = IGraphRenderer.EvaluateEquation(equationTree, variables);
                    Vector3 entryPoint = AssignPoint(entryInputVal, entryOutputVal, inputVar, outputVar);
                    segments.Last().Add(entryPoint);
                }
                // add the current point when in range
                segments.Last().Add(currentPoint);
                inRange = true;
            }
            // point left graphing range
            // use binary search to find edge point
            else if(inRange && previousPoint.HasValue) {
                float exitInputVal = GraphUtils.BinarySearchEdge(
                    previousInputVal, inputVal,
                    v => {
                        variables[inputVar.ToString().ToLower()] = v;
                        return IGraphRenderer.EvaluateEquation(equationTree, variables);
                    },
                    outputMin, outputMax
                );

                variables[inputVar.ToString().ToLower()] = exitInputVal;
                float exitOutputVal = IGraphRenderer.EvaluateEquation(equationTree, variables);
                Vector3 exitPoint = AssignPoint(exitInputVal, exitOutputVal, inputVar, outputVar);
                segments.Last().Add(exitPoint);
                // stop the line here and start a new segment
                segments.Add(new List<Vector3>());
                inRange = false;
            }

            previousPoint = inRange ? currentPoint : null;
            previousInputVal = inputVal;
        }

        // basic object pooling, only add new line renderers when needed, and prioritize using old ones
        // this will only matter for graph editing (later)
        for (int i = 0; i < segments.Count; i++)
        {
            if (i >= segmentRenderers.Count)
            {
                GameObject segmentObj = new GameObject($"Segment {i}");
                segmentObj.transform.SetParent(graphParent, false);

                LineRenderer newRenderer = segmentObj.gameObject.AddComponent<LineRenderer>();
                newRenderer.useWorldSpace = false;
                newRenderer.startWidth = newRenderer.endWidth = 0.02f;
                segmentRenderers.Add(newRenderer);
            }

            LineRenderer r = segmentRenderers[i];
            List<Vector3> segment = segments[i];
            r.positionCount = segment.Count;
            r.SetPositions(segment.ToArray());
            r.enabled = true;
        }

        // disable any unused renderers
        for (int i = segments.Count; i < segmentRenderers.Count; i++)
        {
            segmentRenderers[i].enabled = false;
        }
    }

    // assigns the point to the correct axis
    private Vector3 AssignPoint(float inputVarVal, float outputVarVal, GraphVariable inputVar, GraphVariable outputVar)
    {
        return (inputVar, outputVar) switch
        {
            (GraphVariable.X, GraphVariable.Y) => new Vector3(inputVarVal, outputVarVal, 0),
            (GraphVariable.X, GraphVariable.Z) => new Vector3(inputVarVal, 0, outputVarVal),
            (GraphVariable.Y, GraphVariable.X) => new Vector3(outputVarVal, inputVarVal, 0),
            (GraphVariable.Y, GraphVariable.Z) => new Vector3(0, inputVarVal, outputVarVal),
            (GraphVariable.Z, GraphVariable.X) => new Vector3(outputVarVal, 0, inputVarVal),
            (GraphVariable.Z, GraphVariable.Y) => new Vector3(0, outputVarVal, inputVarVal),
            // this should already be caught in GraphManager
            _ => throw new GraphEvaluationException("Point lies on unknown plane (only XY, XZ, ZY planes supported).")
        };
    }
}

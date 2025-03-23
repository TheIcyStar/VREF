using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEngine.Animations;
using System;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

// graph variable enum to avoid string comparison
public enum GraphVariable {
    X,
    Y,
    Z,
    Constant
}

// interface for all graph renderers to be more modular for later changes
public interface IGraphRenderer
{
    void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar);

    // default implementation for getting min values
    public static float GetAxisMin(GraphSettings settings, GraphVariable variable)
    {
        return variable switch
        {
            GraphVariable.X => settings.xMin,
            GraphVariable.Y => settings.yMin,
            GraphVariable.Z => settings.zMin,
            _ => throw new GraphEvaluationException("Unknown variable attempting to be graphed.")
        };
    }

    // default implementation for getting max values
    public static float GetAxisMax(GraphSettings settings, GraphVariable variable)
    {
        return variable switch
        {
            GraphVariable.X => settings.xMax,
            GraphVariable.Y => settings.yMax,
            GraphVariable.Z => settings.zMax,
            _ => throw new GraphEvaluationException("Unknown variable attempting to be graphed.")
        };
    }
}

// this renderer will always assume the unused variable is 0
// and therefore can only graph on a 2d plane (xy, yz, xz)
public class LineGraphRenderer : IGraphRenderer
{
    private LineRenderer lineRenderer;

    public LineGraphRenderer(LineRenderer renderer)
    {
        this.lineRenderer = renderer;
        this.lineRenderer.useWorldSpace = false;
        this.lineRenderer.startWidth = .02f;
        this.lineRenderer.endWidth = .02f;
    }

    public void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar)
    {
        // since its explicit, dont need LHS of equal sign
        equationTree = equationTree.right;

        // initialize the list of points
        List<Vector3> points = new List<Vector3>();

        // extract the input var
        GraphVariable inputVar = inputVars.First();

        // deal with constants later, as this would require multiple variable mappings at the same time
        // i.e. x = 5, z = 0 to get a constant line on the xz plane, as just x = 5 would have to decide which var to set to 0

        // find the mins and maxes of the graph for the input only
        // output range will be cutoff by a shader
        float inputMin = IGraphRenderer.GetAxisMin(settings, inputVar);
        float inputMax = IGraphRenderer.GetAxisMax(settings, inputVar);

        // go through each point of the indepedent variable and calculate the value of the RHS, then plot
        for (float inputVarVal = inputMin; inputVarVal <= inputMax; inputVarVal += settings.step)
        {
            float outputVarVal = EvaluateEquation(equationTree, inputVar, inputVarVal);

            // add the point to the correct axis of the graph
            points.Add(AssignPoint(inputVarVal, outputVarVal, inputVar, outputVar));
        }

        // give the points to the line renderer
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    // evaluates the equation to solve for the RHS value at a certain independent variable point
    private float EvaluateEquation(ParseTreeNode node, GraphVariable inputVar, float inputVarVal)
    {
        switch (node.token.type)
        {
            case EquationParser.TYPE_NUMBER:
                return float.TryParse(node.token.text, out float num) ? num : throw new GraphEvaluationException($"Incorrect number format for '{node.token.text}'.");
            case EquationParser.TYPE_VARIABLE:
                return inputVarVal;
            case EquationParser.TYPE_OPERATOR:
                // treat missing left operand as 0 so that unary minus works without left operand (negation)
                // maybe there are some operators that work with only a left operand? so right is also set to 0
                // every missing operand error should be stopped in the parser before it gets here, so it shouldn't matter
                float left = node.left != null ? EvaluateEquation(node.left, inputVar, inputVarVal) : 0;
                float right = node.right != null ? EvaluateEquation(node.right, inputVar, inputVarVal) : 0;
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
                // error should already be caught in parser
                float expression = node.right != null ? EvaluateEquation(node.right, inputVar, inputVarVal) : throw new GraphEvaluationException($"Missing expression inside function '{node.token.text}'.");
                return node.token.text switch
                {
                    "sin" => Mathf.Sin(expression),
                    "log" => Mathf.Log(expression),
                    "sqrt" => Mathf.Sqrt(expression),
                    // should never happen, should be caught in parser
                    _ => throw new GraphEvaluationException($"Unsupported function type '{node.token.text}'.")
                };
        }
        return 0;
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

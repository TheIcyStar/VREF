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
            _ => 0
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
            _ => 0
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
        // initialize the list of points
        List<Vector3> points = new List<Vector3>();

        // extract the input var
        GraphVariable inputVar;
        if(inputVars.Count == 0) inputVar = GraphVariable.Constant;
        else inputVar = inputVars.First();

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
    // string switch is another potential reason to change the tree to use some unified token,
    // although strings are very modular and are easy for new additions
    private float EvaluateEquation(ParseTreeNode node, GraphVariable inputVar, float inputVarVal)
    {
        if (node == null) return 0;
        switch (node.token.type)
        {
            case EquationParser.TYPE_NUMBER:
                return float.TryParse(node.token.text, out float num) ? num : 0;
            case EquationParser.TYPE_VARIABLE:
                return node.token.text switch
                {
                    "x" => inputVar == GraphVariable.X ? inputVarVal : 0,
                    "y" => inputVar == GraphVariable.Y ? inputVarVal : 0,
                    "z" => inputVar == GraphVariable.Z ? inputVarVal : 0,
                    _ => 0
                };
            case EquationParser.TYPE_OPERATOR:
                float left = node.left != null ? EvaluateEquation(node.left, inputVar, inputVarVal) : 0;
                float right = node.right != null ? EvaluateEquation(node.right, inputVar, inputVarVal) : 0;
                return node.token.text switch
                {
                    "+" => left + right,
                    "-" => left - right,
                    "*" => left * right,
                    "/" => right != 0 ? left / right : 0,
                    "^" => Mathf.Pow(left, right),
                    _ => 0
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
            _ => new Vector3(inputVarVal, outputVarVal, 0)
        };
    }
}

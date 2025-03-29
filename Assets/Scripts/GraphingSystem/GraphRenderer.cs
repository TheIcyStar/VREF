using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;


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
                GameObject segmentObj = new GameObject($"Line Segment {i + 1}");
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
    private List<(MeshFilter filter, MeshRenderer renderer)> segmentSurfaces = new();

    public SurfaceGraphRenderer(Transform parent) 
    {
        this.graphParent = parent;
        this.segmentSurfaces = new();
    }
    public void RenderGraph(ParseTreeNode equationTree, GraphSettings settings, HashSet<GraphVariable> inputVars, GraphVariable outputVar) 
    {
        // extract the input vars
        GraphVariable inputVar1 = inputVars.ElementAt(0);
        GraphVariable inputVar2 = inputVars.ElementAt(1);

        // initialize the variable dictionary
        Dictionary<string, float> variables = new() {
            {inputVar1.ToString().ToLower(), 0f},
            {inputVar2.ToString().ToLower(), 0f}
        };

        // since its explicit, dont need LHS of equal sign
        equationTree = equationTree.right;

        // initialize list of surface segments
        List<List<List<Vector3>>> segments = new();

        // initialize the grid of points
        List<List<Vector3>> grid = new();

        // find the mins and maxes of the graph for the inputs and output
        float inputMin1 = IGraphRenderer.GetAxisMin(settings, inputVar1);
        float inputMax1 = IGraphRenderer.GetAxisMax(settings, inputVar1);
        float inputMin2 = IGraphRenderer.GetAxisMin(settings, inputVar2);
        float inputMax2 = IGraphRenderer.GetAxisMax(settings, inputVar2);
        float outputMin = IGraphRenderer.GetAxisMin(settings, outputVar);
        float outputMax = IGraphRenderer.GetAxisMax(settings, outputVar);

        // hold off on disjoint surfaces for now
        // -------------------------------------
        // track when the graph is in range to only graph valid points
        // bool previousInRange = false;

        // go through each point of the indepedent variable and calculate the value of the RHS, then plot
        for (float inputVal1 = inputMin1; inputVal1 < inputMax1; inputVal1 += settings.step)
        {
            // initialize new row of points
            List<Vector3> row = new();

            for (float inputVal2 = inputMin2; inputVal2 < inputMax2; inputVal2 += settings.step) {
                // set the variable dictionary to store the current input vals
                variables[inputVar1.ToString().ToLower()] = inputVal1;
                variables[inputVar2.ToString().ToLower()] = inputVal2;

                // find what the output evaluates to when given the input at this point
                float outputVal = IGraphRenderer.EvaluateEquation(equationTree, variables);
                
                // hold off on disjoint surfaces for now
                // -------------------------------------
                // check if current function value is in the output range
                // bool currentInRange = !float.IsNaN(outputVal) && outputVal >= outputMin && outputVal <= outputMax;
                //
                // only add points when in range
                // if (currentInRange) {
                //     // just came back in range, start a new segment
                //     if(!previousInRange) segments.Add(new List<Vector3>());

                //     // determine the correct axis of the graph to add the point to
                //     Vector3 currentPoint = AssignPoint(inputVal1, inputVal2, outputVal, inputVar1, inputVar2, outputVar);

                //     // add the point to the current segment
                //     segments.Last().Add(currentPoint);
                // }
                //
                // previousInRange = currentInRange;

                // temporary
                // for non-disjoint surfaces only
                row.Add(AssignPoint(inputVal1, inputVal2, outputVal, inputVar1, inputVar2, outputVar));
            }

            // temporary
            // for non-disjoint surfaces only
            grid.Add(row);
        }

        // temporary
        // for non-disjoint surfaces only
        segments.Add(grid);

        // basic object pooling, only add new mesh renderers and filters when needed, and prioritize using old ones
        // this will only matter for graph editing (later)
        for (int i = 0; i < segments.Count; i++)
        {
            if (i >= segmentSurfaces.Count)
            {
                GameObject segmentObj = new GameObject($"Surface Segment {i + 1}");
                segmentObj.transform.SetParent(graphParent, false);

                MeshFilter filter = segmentObj.AddComponent<MeshFilter>();
                MeshRenderer renderer = segmentObj.AddComponent<MeshRenderer>();

                // renderer.material = [material]

                segmentSurfaces.Add((filter, renderer));
            }

            MeshFilter meshFilter = segmentSurfaces[i].filter;
            Mesh mesh = new Mesh();

            List<List<Vector3>> currentGrid = segments[i];

            // mesh generation logic
            var (vertices, triangles) = GenerateMesh(currentGrid);
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            segmentSurfaces[i].renderer.enabled = true;
        }

        // disable any unused objects
        for (int i = segments.Count; i < segmentSurfaces.Count; i++)
        {
            segmentSurfaces[i].renderer.enabled = false;
        }
    }

    // x is z, z is y, and y is x
    private Vector3 AssignPoint(float inputVar1Val, float inputVar2Val, float outputVarVal, GraphVariable inputVar1, GraphVariable inputVar2, GraphVariable outputVar) 
    {
        return (inputVar1, inputVar2, outputVar) switch {
            (GraphVariable.X, GraphVariable.Y, GraphVariable.Z) => new Vector3(outputVarVal, inputVar1Val, inputVar2Val),
            (GraphVariable.Y, GraphVariable.Z, GraphVariable.X) => new Vector3(inputVar2Val, outputVarVal, inputVar1Val),
            (GraphVariable.Z, GraphVariable.X, GraphVariable.Y) => new Vector3(inputVar1Val, inputVar2Val, outputVarVal),
            // this should already be stopped
            _ => throw new GraphEvaluationException("Point lies on unknown slice (only XYZ slice supported).")
        };
    }

    // generates a mesh based on a grid of points
    private (List<Vector3>, List<int>) GenerateMesh(List<List<Vector3>> currentGrid)
    {
        // FROM WHAT I UNDERSTAND OF MESH GENERATION/COMPUTER GRAPHICS:
        // given a grid like this with letters being points (vertices):
        // A --- B --- C
        // |     |     |
        // |     |     |
        // D --- E --- F
        // |     |     |
        // |     |     |
        // G --- H --- I
        //
        // you must create triangles for each square (called quads)
        // quads consist of two triangles
        // A --- B
        // |   / |
        // | /   |
        // D --- E
        //
        // so iterates through all the points
        // and create a list of vertices (A=(1,0,1), B=(2,3,1), ...) 
        // and triangles (A, D, B, B, D, E) <-- the mesh reads triangles three values at a time
        // so this creates triangles ADB and BDE
        // and give that to the mesh

        // initialize the list of vertices and the list of triangles
        List<Vector3> vertices = new();
        List<int> triangles = new();

        // get the row and column values of the grid
        // (this is the only thing the specific grid structure is needed for)
        // (perhaps maybe store the actual list when evaluating already flattened
        // and then also store the row and column values?)
        int rows = currentGrid.Count;
        int cols = currentGrid[0].Count;

        // flatten the grid into a vertex list
        // this is to map each vertex (point) to an index
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                // add the point at that spot of the grid to the vertex list
                vertices.Add(currentGrid[x][y]);
            }
        }

        // generate triangle indices (two per quad)
        // go only up to rows - 1 and cols - 1 since
        // the last triangles will use x + 1 and y + 1
        for (int x = 0; x < rows - 1; x++)
        {
            for (int y = 0; y < cols - 1; y++)
            {
                // find the vertex index for each point of the triangle
                // these have to line up with the flattened list
                //  x,y  ---  x,y+1
                //   |      /   |
                //   |    /     |
                //   |  /       |
                // x+1,y --- x+1,y+1
                // mutliple by cols since the grid was flattened
                int topLeft = x * cols + y;                     // (x, y)
                int topRight = x * cols + y + 1;                // (x, y+1)
                int bottomLeft = (x + 1) * cols + y;            // (x+1, y)
                int bottomRight = (x + 1) * cols + y + 1;       // (x+1, y+1)

                // winding order is how you add the points to the triangle
                // winding order defines the front of the triangle
                // winding counter-clockwise (like how this code is)
                // is considered the front in unity

                // these are the two triangles that make the quad
                // topLeft -> bottomLeft -> topRight is counter-clockwise
                triangles.Add(topLeft);
                triangles.Add(bottomLeft);
                triangles.Add(topRight);

                // topRight -> bottomLeft -> bottomRight is also counter-clockwise
                triangles.Add(topRight);
                triangles.Add(bottomLeft);
                triangles.Add(bottomRight);
            }
        }

        return (vertices, triangles);
    }
}

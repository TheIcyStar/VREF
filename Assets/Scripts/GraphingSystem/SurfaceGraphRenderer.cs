using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfaceGraphRenderer : IGraphRenderer
{
    private Transform graphParent;
    // gameobject contains the MeshRenderer and the MeshFilter
    private List<(MeshFilter filter, MeshRenderer renderer)> segmentSurfaces = new();
    private Material meshColor;

    public SurfaceGraphRenderer(Transform parent, Material meshColor) 
    {
        this.graphParent = parent;
        this.segmentSurfaces = new();
        this.meshColor = meshColor;
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

                renderer.material = meshColor;

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
            (GraphVariable.X, GraphVariable.Y, GraphVariable.Z) or (GraphVariable.Y, GraphVariable.X, GraphVariable.Z) 
            => new Vector3(outputVarVal, inputVar1Val, inputVar2Val),
            (GraphVariable.Y, GraphVariable.Z, GraphVariable.X) or (GraphVariable.Z, GraphVariable.Y, GraphVariable.X)
            => new Vector3(inputVar2Val, outputVarVal, inputVar1Val),
            (GraphVariable.Z, GraphVariable.X, GraphVariable.Y) or (GraphVariable.X, GraphVariable.Z, GraphVariable.Y)
            => new Vector3(inputVar1Val, inputVar2Val, outputVarVal),
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


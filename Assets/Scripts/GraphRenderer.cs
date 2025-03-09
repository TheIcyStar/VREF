using UnityEngine;
using System.Collections.Generic;

// interface for all graph renderers to be more modular for later changes
public interface IGraphRenderer
{
    void RenderGraph(ParseTreeNode equationTree, GraphSettings settings);
}

public class LineGraphRenderer : IGraphRenderer
{
    private LineRenderer lineRenderer;

    public LineGraphRenderer(LineRenderer renderer)
    {
        this.lineRenderer = renderer;
    }

    public void RenderGraph(ParseTreeNode equationTree, GraphSettings settings)
    {
        List<Vector3> points = new List<Vector3>((int)((settings.xMax - settings.xMin) / settings.step) + 1);

        for (float x = settings.xMin; x <= settings.xMax; x += settings.step)
        {
            float y = EvaluateEquation(equationTree, x);
            points.Add(new Vector3(x, y, 0));
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private float EvaluateEquation(ParseTreeNode node, float x)
    {
        if (node == null) return 0;
        switch (node.token.type)
        {
            case EquationParser.TYPE_NUMBER:
                return float.TryParse(node.token.text, out float num) ? num : 0;
            case EquationParser.TYPE_VARIABLE:
                return x;
            case EquationParser.TYPE_OPERATOR:
                float left = node.left != null ? EvaluateEquation(node.left, x) : 0;
                float right = node.right != null ? EvaluateEquation(node.right, x) : 0;
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
}

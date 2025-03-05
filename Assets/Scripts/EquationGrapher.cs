using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

// ONLY SUPPORTS y = mx + b RIGHT NOW
// eventually, classify the equation in parser through rules (?)
public class EquationGrapher : MonoBehaviour
{
    // the root node of the equation tree
    public ParseTreeNode equationTree;
    // graphing range and resolution
    private float xMin = -10f, xMax = 10f, step = 1f;
    // 2d for now

    // private void Start() {
    //     // equationTree = CreateTestTree();
    //     if(equationTree != null && lineRenderer != null) {
    //         GraphEquation();
    //     }
    // }

    // evaulates each point in the range and adds them to the line renderer
    public void GraphEquation() {
        List<Vector3> points = new List<Vector3>();

        for(float x = xMin; x <= xMax; x += step) {
            float y = EvaluateEquation(equationTree, x);
            points.Add(new Vector3(x, y, 0));
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private float EvaluateEquation(ParseTreeNode node, float x) {
        // number node
        if(node.token.type == EquationParser.TYPE_NUMBER) {
            if(float.TryParse(node.token.text, out float number)) {
                return number;
            }
            // failed parse
            return 0;
        }

        // variable node
        //  change this to list of variable values when adding multiple variables
        if (node.token.type == EquationParser.TYPE_VARIABLE) {
            return x;
        }

        // operators
        if(node.token.type == EquationParser.TYPE_OPERATOR) {
            // empty children of operators should not be possible
            float left = (node.left != null) ? EvaluateEquation(node.left, x) : 0;
            float right = (node.right != null) ? EvaluateEquation(node.right, x) : 0;

            switch(node.token.text) {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/": return left / right;
                case "^": return Mathf.Pow(left, right);
            }
        }

        // shouldnt reach here
        return 0;
    }

    // private ParseTreeNode CreateTestTree()
    // {
    //     // mx + b
    //     EquationToken m = new EquationToken(".5", EquationParser.TYPE_NUMBER);
    //     EquationToken x = new EquationToken("x", EquationParser.TYPE_VARIABLE);
    //     EquationToken mult = new EquationToken("*", EquationParser.TYPE_OPERATOR);
    //     EquationToken b = new EquationToken("1", EquationParser.TYPE_NUMBER);
    //     EquationToken plus = new EquationToken("+", EquationParser.TYPE_OPERATOR);

    //     // parse tree:
    //     //     +
    //     //    / \
    //     //   *   b
    //     //  / \
    //     // m   x

    //     ParseTreeNode multiplyNode = new ParseTreeNode(mult) { left = new ParseTreeNode(m), right = new ParseTreeNode(x) };
    //     ParseTreeNode root = new ParseTreeNode(plus) { left = multiplyNode, right = new ParseTreeNode(b) };

    //     return root;
    // }
}

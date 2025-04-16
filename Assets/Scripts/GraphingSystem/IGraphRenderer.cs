using System.Collections.Generic;
using UnityEngine;

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
            case TokenType.Number:
                return float.TryParse(node.token.text, out float num) ? num : throw new GraphEvaluationException($"Incorrect number format for '{node.token.text}'.");
            case TokenType.Variable:
                if (!vars.TryGetValue(node.token.text, out float val))
                    throw new GraphEvaluationException($"Variable '{node.token.text}' not found.");
                return val;
            case TokenType.Operator:
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
                    "/" => right != 0 ? left / right : (node.right.token.type == TokenType.Number ? throw new GraphEvaluationException("Cannot divide by zero.") : float.NaN),
                    "^" => Mathf.Pow(left, right),
                    // should never happen, should be caught in parser
                    _ => throw new GraphEvaluationException($"Unsupported operator '{node.token.text}'.")
                };
            case TokenType.Function:
                // functions have their expression on the right
                float arg = EvaluateEquation(node.right, vars);
                return node.token.text switch
                {
                    "sin" => Mathf.Sin(arg),
                    "cos" => Mathf.Cos(arg),
                    "tan" => Mathf.Tan(arg),
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

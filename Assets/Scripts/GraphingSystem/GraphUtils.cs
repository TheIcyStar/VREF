using System.Collections.Generic;
using UnityEngine;

public static class GraphUtils
{
    public static float CalculateScaleFactor(GraphSettings settings, float baseRange = 2f)
    {
        return baseRange / (CalculateMaxRange(settings) * 2);
    }

    public static float CalculateMaxRange(GraphSettings settings)
    {
        return Mathf.Max(
            Mathf.Abs(settings.xMin), Mathf.Abs(settings.xMax),
            Mathf.Abs(settings.yMin), Mathf.Abs(settings.yMax),
            Mathf.Abs(settings.zMin), Mathf.Abs(settings.zMax)
        );
    }

    // determines the type of equation by analyzing the parse tree
    // also returns the input and output variables
    // ONLY WORKS WITH [output var] = [f(input var)] FOR NOW
    public static (EquationType, HashSet<GraphVariable>, GraphVariable) DetermineEquationType(ParseTreeNode equationTree) {
        // initialize a set to store all the input vars
        HashSet<GraphVariable> inputVars = new HashSet<GraphVariable>();
        GraphVariable outputVar;

        if(equationTree == null || equationTree.token.text != "=") {
            throw new GraphEvaluationException("Explicit equation not found.");
        }

        // find output var (left of equal sign)
        outputVar = ConvertToGraphVariable(equationTree.left.token.text);

        // intialize equation type
        EquationType equationType;

        // add all input variables to the set (right of equal sign)
        DetermineVariables(equationTree.right, inputVars);

        // check to see if there is no explicit variable
        if(inputVars.Contains(outputVar)) throw new GraphEvaluationException("Variable found on left and right hand side of equation.");

        // this logic needs to change to support constants, parametrics, planes, etc... in the future
        if (inputVars.Count == 1)                         equationType = EquationType.LINE;
        else if (inputVars.Count == 2)                    equationType = EquationType.SURFACE;
        else                                              throw new GraphEvaluationException("Unsupported variable amount in right hand side of equation.");

        return (equationType, inputVars, outputVar);
    }

    // finds all the variables in the given tree and adds them to a set
    // potentially could create the set during tree creation if this
    // full tree traversal ends up being too costly
    public static void DetermineVariables(ParseTreeNode equationTree, HashSet<GraphVariable> variables) {
        if (equationTree == null) return;

        if (equationTree.token.type == TokenType.Variable) {
            variables.Add(ConvertToGraphVariable(equationTree.token.text));
        }

        DetermineVariables(equationTree.left, variables);
        DetermineVariables(equationTree.right, variables);
    }

    // converts a string to a GraphVariable
    // if we really want to optimize everything down the line
    // we can change the tree to use an enum for every possible
    // token to avoid using strings entirely
    public static GraphVariable ConvertToGraphVariable(string stringVar)
    {
        return stringVar switch
        {
            "x" => GraphVariable.X,
            "y" => GraphVariable.Y,
            "z" => GraphVariable.Z,
            _ => throw new GraphEvaluationException("Unknown/missing variable attempting to be graphed.")
        };
    }
}

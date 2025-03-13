using Unity.Mathematics;
using UnityEngine;

public class ParseTreeNode
{
    public EquationToken token;
    public ParseTreeNode left;
    public ParseTreeNode right;

    public ParseTreeNode(EquationToken token) {
        this.token = token;
        left = null;
        right = null;
    }
}
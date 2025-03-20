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

    /// <summary>
    /// Recursively serializes the equation tree
    /// </summary>
    /// <returns>JSON string</returns>
    public string ToJSON() {
        return $"{{\"token\":{token.ToJSON()},\"left\":{(left != null ? left.ToJSON() : "null")},\"right\":{(right != null ? right.ToJSON() : "null")}}}";
    }
}
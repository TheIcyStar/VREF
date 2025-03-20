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
        return $"{{\"token\":{this.token.ToJSON()},\"left\":{(this.left != null ? this.left.ToJSON() : "null")},\"right\":{(this.right != null ? this.right.ToJSON() : "null")}}}";
    }
}
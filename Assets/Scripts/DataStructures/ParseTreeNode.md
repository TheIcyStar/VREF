# ParseTreeNode

Represents a node in a parse tree for an equation.

## Public Fields

-   `EquationToken token`: The token associated with this node.
-   `ParseTreeNode left`: The left child node.
-   `ParseTreeNode right`: The right child node.

## Public Methods

-   `ParseTreeNode(EquationToken token)`
    -   Constructor for the ParseTreeNode class. Initializes the token and sets the left and right children to null.
-   `string ToJSON()`
    -   Recursively serializes the equation tree into a JSON string.
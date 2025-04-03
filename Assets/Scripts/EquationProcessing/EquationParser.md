# EquationParser

Parses a list of EquationTokens into a ParseTreeNode representing the equation's abstract syntax tree.

## Private Fields

-   `List<EquationToken> tokens`: The list of tokens to parse.
-   `int tokenIndex`: The index of the current token being processed.

## Public Methods

-   `ParseTreeNode Parse(List<EquationToken> tokens)`
    -   Parses the given list of tokens into a ParseTreeNode.
    -   Throws `ParserException` if there are no tokens to parse or if there are unexpected tokens.

## Private Methods

-   `EquationToken CurrentToken()`
    -   Gets the current token.
    -   Returns a null token if `tokenIndex` is out of range.
-   `bool TypeMatch(TokenType type)`
    -   Checks if the current token's type matches the given type.
-   `EquationToken UseToken(TokenType type)`
    -   Consumes the current token if its type matches the given type and increments `tokenIndex`.
    -   Throws `ParserException` if the token type does not match.
-   `ParseTreeNode ParseExplicit()`
    -   Parses an explicit function definition.
-   `ParseTreeNode ParseExpression()`
    -   Parses an expression.
-   `ParseTreeNode ParseTerm()`
    -   Parses a term.
-   `ParseTreeNode ParseUnary()`
    -   Parses a unary operation.
-   `ParseTreeNode ParsePower()`
    -   Parses a power operation.
-   `ParseTreeNode ParsePrimary()`
    -   Parses a primary expression.
-   `ParseTreeNode ParseFunction()`
    -   Parses a function call.
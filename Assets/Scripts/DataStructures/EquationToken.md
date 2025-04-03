# EquationToken

Represents a token within an equation.

## Public Properties

-   `string text`: The text of the token.
-   `TokenType type`: The type of the token.

## Public Methods

-   `EquationToken(string text, TokenType type)`
    -   Constructor for the EquationToken class. Initializes the text and type of the token.
-   `string ToJSON()`
    -   Converts the EquationToken to a JSON string representation.
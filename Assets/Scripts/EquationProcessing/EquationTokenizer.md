# EquationTokenizer

Tokenizes an equation string from a TMP_InputField.

## Private Fields

-   `TMP_InputField equationInput`: The input field containing the equation string.
-   `List<EquationToken> tokens`: The list of tokens generated from the input string.
-   `List<(int start, int end, int tokenIndex)> tokenRanges`: A list of tuples representing the start and end indices of each token in the input string, along with the token index.
-   `int[] rangeMap`: A map used for O(1) lookup of token indices based on character indices in the input string.
-   `const int MAX_CHARACTERS`: The maximum number of characters allowed in the input string.

## Public Properties

-   `List<EquationToken> tokens`: Gets the list of tokens.

## Public Methods

-   `EquationTokenizer(TMP_InputField equationInput)`
    -   Constructor that initializes the tokenizer with the input field.
-   `void BuildTokenRanges()`
    -   Builds or rebuilds the token ranges and the range map.
-   `int RemoveTokenAtCursor(int cursorIndex)`
    -   Removes the token at the specified cursor index and updates the input string and token list.
    -   Returns the updated cursor index.
-   `int InsertTokenAtCursor(string text, TokenType type, int cursorIndex)`
    -   Inserts a token at the specified cursor index and updates the input string and token list.
    -   Returns the updated cursor index.
-   `List<EquationToken> CleanUpEquation()`
    -   Cleans up the token list by combining numbers and adding implicit multiplication.

## Private Methods

-   `void UpdateTokenRanges(int startIndex)`
    -   Updates the token ranges and the range map starting from the specified token index.
-   `void CombineNumbers(List<EquationToken> cleanTokens)`
    -   Combines number tokens into single tokens, handling decimal points.
-   `void AddImplicitMultiplication(List<EquationToken> cleanTokens)`
    -   Adds implicit multiplication operators to the token list.
# EquationManager

Manages the processing of equations, from tokenization to parsing and graphing.

## Public Fields

-   `GraphManager graphManager`: Reference to the GraphManager for graphing the equation.

## Private Fields

-   `EquationTokenizer tokenizer`: Instance of EquationTokenizer for tokenizing the equation.
-   `EquationParser parser`: Instance of EquationParser for parsing the tokens.

## Public Methods

-   `void InitializeEqManager(TMP_InputField equationInput)`
    -   Initializes the EquationManager with the input field and creates tokenizer and parser instances.
    -   Throws `EquationUIException` if the input field is not assigned.
-   `void ProcessEquation()`
    -   Processes the equation from input, tokenizes, parses, and sends it to the GraphManager.
-   `int InsertToken(string text, TokenType type, int cursorIndex)`
    -   Wrapper for inserting a token at the specified cursor index in the tokenizer.
-   `int RemoveToken(int cursorIndex)`
    -   Wrapper for removing a token at the specified cursor index in the tokenizer.
-   `void InitializeTokenizer()`
    -   Initializes the tokenizer by building token ranges.
# EquationKeyboardManager

This class manages the equation input via a virtual keyboard, handling token insertion, deletion, and equation confirmation.

## Public Fields

### `equationInput : TMP_InputField`

text field from the UI

### `equationErrorText : TMP_Text`

error text field from the UI

### `equationManager : EquationManager`

handles all equation logic (tokenizing, parsing)

## Public Methods

### `Start() : void`

### `ConfirmEquation() : void`

called when the graph button is pressed

### `BackspaceToken() : void`

backspace button is pressed

### `InsertNumber(string number) : void`

digits

### `InsertOperator(string op) : void`

operators (+, -, *, /)

### `InsertVariable(string variable) : void`

variables

### `InsertLeftParen() : void`

parenthesis

### `InsertRightParen() : void`

parenthesis

### `InsertEquals() : void`

equal sign

### `InsertDecimal() : void`

decimal point

### `InsertFunction(string function) : void`

function

## Private Fields

### `cursorIndex : int`

string index of the cursor

## Private Methods

### `OnCursorMoved(string inputText) : void`

string parameter is unused but Unity's structure needs it

### `TokenPressed(string text, TokenType type) : void`

inserts a token at the current cursor position and updates the token list accordingly
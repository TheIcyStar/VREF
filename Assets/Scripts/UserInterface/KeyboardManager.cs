using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class KeyboardManager : MonoBehaviour
{
    // text field from the UI
    public TMP_InputField equationInput;
    // string index of the cursor
    private int cursorIndex;
    // handles all equation logic (tokenizing, parsing)
    public EquationManager equationManager;

    public void Start() 
    {
        equationManager.InitializeEqManager(equationInput);
        cursorIndex = 0;
        equationInput.onEndEdit.AddListener(OnCursorMoved);
        equationManager.InitializeTokenizer();
    }

    // string parameter is unused but Unity's structure needs it
    private void OnCursorMoved(string inputText) {
        cursorIndex = equationInput.caretPosition;
    }

    // called when the graph button is pressed
    public void ConfirmEquation()
    {
        equationManager.ProcessEquation();
    }

    // backspace button is pressed
    public void BackspaceToken() {
        cursorIndex = equationManager.RemoveToken(cursorIndex);
    }

    // inserts a token at the current cursor position and updates the token list accordingly
    private void TokenPressed(string text, int type)
    {
        cursorIndex = equationManager.InsertToken(text, type, cursorIndex);
    }

    // wrapper functions used by the buttons since unity doesnt allow
    //  more than 1 parameter to be passed in through the inspector

    // digits
    public void InsertNumber(string number) { TokenPressed(number, EquationParser.TYPE_NUMBER); }

    // operators (+, -, *, /)
    public void InsertOperator(string op) { TokenPressed(op, EquationParser.TYPE_OPERATOR); }

    // variables
    public void InsertVariable(string variable) { TokenPressed(variable, EquationParser.TYPE_VARIABLE); }

    // parenthesis
    public void InsertLeftParen() { TokenPressed("(", EquationParser.TYPE_LEFTPAREN); }
    public void InsertRightParen() { TokenPressed(")", EquationParser.TYPE_RIGHTPAREN); }

    // equal sign
    public void InsertEquals() { TokenPressed("=", EquationParser.TYPE_RELOP); }

    // decimal point
    public void InsertDecimal() { TokenPressed(".", EquationParser.TYPE_DECIMAL); }

    // function
    public void InsertFunction(string function) { TokenPressed(function, EquationParser.TYPE_FUNCTION); }
}

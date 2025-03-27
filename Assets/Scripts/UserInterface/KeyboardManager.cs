using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class KeyboardManager : MonoBehaviour
{
    // text field from the UI
    public TMP_InputField equationInput;
    // error text field from the UI
    public TMP_Text equationErrorText;
    // string index of the cursor
    private int cursorIndex;
    // handles all equation logic (tokenizing, parsing)
    public EquationManager equationManager;

    public void Start() 
    {
        // catch any weird unlinking bugs
        try {
            equationManager.InitializeEqManager(equationInput);
        }
        catch(EquationUIException equie) {
            equationErrorText.text = equie.Message;
        }

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
        // attempt to fully render the equation
        try {
            equationManager.ProcessEquation();
            equationErrorText.text = "";
        } 
        // catch tokenizer
        catch (TokenizerException te) {
            equationErrorText.text = te.Message;
        }
        // catch parser
        catch (ParserException pe) {
            equationErrorText.text = pe.Message;
        }
        // catch graph
        catch (GraphEvaluationException ge) {
            equationErrorText.text = ge.Message;
        }
        // catch everything else
        catch (Exception e) {
            equationErrorText.text = e.Message;
        }
    }

    // backspace button is pressed
    public void BackspaceToken() {
        // attempt to remove the token
        try {
            cursorIndex = equationManager.RemoveToken(cursorIndex);
        } catch (TokenizerException te) {
            equationErrorText.text = te.Message;
        }
    }

    // inserts a token at the current cursor position and updates the token list accordingly
    private void TokenPressed(string text, int type)
    {
        // attempt to insert the token
        try {
            cursorIndex = equationManager.InsertToken(text, type, cursorIndex);
        } catch (TokenizerException te) {
            equationErrorText.text = te.Message;
        }
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

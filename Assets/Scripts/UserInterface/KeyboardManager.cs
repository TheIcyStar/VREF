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
    private void TokenPressed(string text, TokenType type)
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
    public void InsertNumber(string number) { TokenPressed(number, TokenType.Number); }

    // operators (+, -, *, /)
    public void InsertOperator(string op) { TokenPressed(op, TokenType.Operator); }

    // variables
    public void InsertVariable(string variable) { TokenPressed(variable, TokenType.Variable); }

    // parenthesis
    public void InsertLeftParen() { TokenPressed("(", TokenType.LeftParen); }
    public void InsertRightParen() { TokenPressed(")", TokenType.RightParen); }

    // equal sign
    public void InsertEquals() { TokenPressed("=", TokenType.Relop); }

    // decimal point
    public void InsertDecimal() { TokenPressed(".", TokenType.Decimal); }

    // function
    public void InsertFunction(string function) { TokenPressed(function, TokenType.Function); }
}

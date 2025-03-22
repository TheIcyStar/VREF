using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class EquationManager : MonoBehaviour
{
    public GraphManager graphManager;
    private EquationTokenizer tokenizer;
    private EquationParser parser;


    // temporarily store the parse tree for debugging
    private ParseTreeNode debugParseTree;
    // temporary function to pass along debug info to keyboardManager
    public string GetTokenizerDebugInfo()
    {
        return tokenizer != null ? tokenizer.GetDebugInfo() : "Tokenizer not initialized.";
    }
    public string GetParserDebugInfo()
    {
        return parser.DebugParseTree(debugParseTree);
    }

    public void InitializeEqManager(TMP_InputField equationInput) {
        if (equationInput == null) throw new EquationUIException("Equation input field is not assigned.");

        tokenizer = new EquationTokenizer(equationInput);
        parser = new EquationParser();
    }

    // process the equation from start to finish, then send it to be graphed
    // keeps logic separated between keyboard, tokenizer, and parser
    public void ProcessEquation() {
        // clean up tokens (make implicit * explicit, compress numbers)
        List<EquationToken> tokens = tokenizer.CleanUpEquation();

        // parse the token list into a tree
        ParseTreeNode equationTree = parser.Parse(tokens);

        // TEMPORARY
        debugParseTree = equationTree;

        // send it to the graph manager
        graphManager.CreateNewGraph(equationTree);
    }

    // wrapper methods for the keyboard
    public int InsertToken(string text, int type, int cursorIndex)
    {
        return tokenizer.InsertTokenAtCursor(text, type, cursorIndex);
    }

    public int RemoveToken(int cursorIndex)
    {
        return tokenizer.RemoveTokenAtCursor(cursorIndex);
    }

    public void InitializeTokenizer() {
        tokenizer.BuildTokenRanges();
    }
}

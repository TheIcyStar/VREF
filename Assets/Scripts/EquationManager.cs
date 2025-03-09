using UnityEngine;
using TMPro;

public class EquationManager : MonoBehaviour
{
    public GraphManager graphManager;
    private EquationTokenizer tokenizer;
    private EquationParser parser;

    public void InitializeEqManager(TMP_InputField equationInput) {
        tokenizer = new EquationTokenizer(equationInput);
        parser = new EquationParser();
    }

    // process the equation from start to finish, then send it to be graphed
    // keeps logic separated between keyboard, tokenizer, and parser
    public void ProcessEquation() {
        // clean up tokens (make implicit * explicit, compress numbers)
        tokenizer.CleanUpEquation();

        // parse the token list into a tree
        ParseTreeNode equationTree = parser.Parse(tokenizer.tokens);

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

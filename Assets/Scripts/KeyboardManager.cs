using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class KeyboardManager : MonoBehaviour
{
    // TODO: ADD A TOKEN COMPRESSOR TO MAKE NUMBERS WORK CORRECTLY
    //       RIGHT NOW ONLY WHOLE NUMBERS WORK
    //       COMPRESS THE TOKEN LIST AFTER GRAPH BUTTON IS PRESSED BUT BEFORE SENDING TO PARSER
    //       ALSO ADD NUMBERS TO THE UI

    // text field from the UI
    public TMP_InputField equationInput;
    // string index of the cursor
    private int cursorIndex;
    // list of the tokens
    private List<EquationToken> tokens = new List<EquationToken>();
    // list of the starting and ending string index positions of each token
    private List<(int start, int end, int tokenIndex)> tokenRanges = new List<(int, int, int)>();
    // map used for O(1) lookup of the above list
    // keys are the string index and values are the corresponding token index
    private int[] rangeMap;
    // fixed max character count, can be changed later whenever we make the 
    // text box stretch to equation size but right now its hard coded
    private const int MAX_CHARACTERS = 256;

    public void Start() 
    {
        cursorIndex = 0;
        equationInput.onEndEdit.AddListener(OnCursorMoved);
        BuildTokenRanges();
    }

    // builds/rebuilds the entire token list and range map
    private void BuildTokenRanges()
    {
        tokenRanges.Clear();
        rangeMap = new int[MAX_CHARACTERS];

        // initialize sentinel value marking end of map
        rangeMap[0] = -2;
        for (int i = 1; i < MAX_CHARACTERS; i++) {
            rangeMap[i] = -1;
        }

        // only used for hard rebuilds
        if (tokens.Count > 0) {
            UpdateTokenRanges(0);
        }
    }

    // updates the token map over a specified range starting from a token index
    private void UpdateTokenRanges(int startIndex)
    {
        // no tokens on update (should not be possible)
        if (tokens.Count == 0) return;

        // add to the end of ranges if startIndex is out of range
        // (might cause desync problems)
        // (but token was already added so idk)
        if (startIndex >= tokenRanges.Count) {
            startIndex = tokenRanges.Count;
        }

        // maybe with the previous two errors
        // we can rebuild the token ranges entirely

        // find the string index to start at
        // use previous range as new range might need to be added
        int currentIndex = (startIndex == 0) ? 0 : tokenRanges[startIndex - 1].end + 1;

        // only update current token and everything in front
        for (int i = startIndex; i < tokens.Count; i++) {
            int tokenLength = tokens[i].text.Length;

            if (i < tokenRanges.Count) {
                tokenRanges[i] = (currentIndex, currentIndex + tokenLength - 1, i);
            }
            else {
                tokenRanges.Add((currentIndex, currentIndex + tokenLength - 1, i));
            }

            // assign range map in one pass
            for (int j = 0; j < tokenLength; j++) {
                rangeMap[currentIndex + j] = i;
            }

            currentIndex += tokenLength;
        }

        // add -2 to the end of the map
        rangeMap[(currentIndex > MAX_CHARACTERS) ? MAX_CHARACTERS : currentIndex] = -2;

        // clean up any removed indices in the map
        int x = currentIndex + 1;
        while (x < MAX_CHARACTERS && rangeMap[x] != -1) {
            rangeMap[x] = -1;
            x++;
        }

        // note: rangeMap does not get updated when the last token gets removed
        //       but it updates the moment a token is added (it still works fine)
    }

    // string parameter is unused but Unity's structure needs it
    private void OnCursorMoved(string inputText) {
        cursorIndex = equationInput.caretPosition;
    }

    public void ConfirmEquation()
    {
        EquationParser parser = new EquationParser();
        ParseTreeNode expressionTree = parser.Parse(tokens);
    }

    // backspace button is pressed
    public void BackspaceToken() {
        // only delete if there is a token to the left
        if(cursorIndex == 0 || tokens.Count == 0) return;

        // find closest token to the left
        // subtracting 1 will allow the token to be deleted to be selected
        int tokenIndex = rangeMap[cursorIndex - 1];

        // delete the token and update the string
        int tokenLength = tokens[tokenIndex].text.Length;
        equationInput.text = equationInput.text.Remove(tokenRanges[tokenIndex].start, tokenLength);
        tokens.RemoveAt(tokenIndex);

        // remove the previous range
        tokenRanges.RemoveAt(tokenIndex);

        // update the ranges and the cursor
        UpdateTokenRanges(tokenIndex);

        // update the cursor index after deletion
        cursorIndex = (tokenIndex > 0) ? tokenRanges[tokenIndex - 1].end + 1: 0;
    }

    // inserts a token at the current cursor position and updates the token list accordingly
    private void InsertTokenAtCursor(string text, int type)
    {
        EquationToken newToken = new EquationToken(text, type);

        // no previous tokens
        if (tokens.Count == 0) {
            tokens.Add(newToken);
            equationInput.text = text;
            UpdateTokenRanges(0);
            cursorIndex = tokenRanges[0].end + 1;
            return;
        }

        // find closest token to the right of cursor
        int tokenIndex = rangeMap[cursorIndex];
        
        // add token to the end
        if (tokenIndex == -2) {
            tokens.Add(newToken);
            equationInput.text += text;
            UpdateTokenRanges(tokens.Count - 1);
            cursorIndex = tokenRanges[tokens.Count - 1].end + 1;
            return;
        }

        // maybe can combine count == 0 and index == -2
        
        // swap tokens when cursor is inside a token
        if (cursorIndex > tokenRanges[tokenIndex].start) {
            equationInput.text = equationInput.text[..tokenRanges[tokenIndex].start] +
                                 text +
                                 equationInput.text[(tokenRanges[tokenIndex].end + 1)..];
            tokens[tokenIndex] = newToken;
        }
        else {
            // otherwise add the token
            equationInput.text = equationInput.text.Insert(cursorIndex, text);
            tokens.Insert(tokenIndex, newToken);
        }

        // if the cursor is at the end of a token, it will already map to the next token instead
        // so no need to check at the end

        // update the map after insertion
        UpdateTokenRanges(tokenIndex);

        // update cursor position
        cursorIndex = tokenRanges[tokenIndex].end + 1;
    }

    // wrapper functions used by the buttons since unity doesnt allow
    //  more than 1 parameter to be passed in through the inspector

    // digits
    public void InsertNumber(string number) { InsertTokenAtCursor(number, EquationParser.TYPE_NUMBER); }

    // operators (+, -, *, /)
    public void InsertOperator(string op) { InsertTokenAtCursor(op, EquationParser.TYPE_OPERATOR); }

    // variables
    public void InsertVariable(string variable) { InsertTokenAtCursor(variable, EquationParser.TYPE_VARIABLE); }

    // parenthesis
    public void InsertLeftParen() { InsertTokenAtCursor("(", EquationParser.TYPE_LEFTPAREN); }
    public void InsertRightParen() { InsertTokenAtCursor(")", EquationParser.TYPE_RIGHTPAREN); }

    // equal sign
    public void InsertEquals() { InsertTokenAtCursor("=", EquationParser.TYPE_OPERATOR); }

    // decimal point
    public void InsertDecimal() { InsertTokenAtCursor(".", EquationParser.TYPE_DECIMAL); }

    // function
    public void InsertFunction(string function) { InsertTokenAtCursor(function, EquationParser.TYPE_FUNCTION); }
}

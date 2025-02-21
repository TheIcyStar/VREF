using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class KeyboardManager : MonoBehaviour
{
    public TMP_InputField equationInput;
    private int cursorIndex = 0;
    // use a list to store the starting and ending positions of each token
    private List<(int start, int end, int tokenIndex)> tokenRanges = new List<(int, int, int)>();
    // map used for O(1) lookup of the above list (on cursor pos change)
    // keys are the string index and values are the corresponding token index
    // CHANGE TO PURE ARRAY IMPLEMENTATION
    private Dictionary<int, int> rangeMap = new Dictionary<int, int>();
    // use another list for the tokens themselves
    private List<EquationToken> tokens = new List<EquationToken>();

    public void Start() 
    {
        equationInput.onEndEdit.AddListener(OnCursorMoved);
        BuildTokenRanges();
    }

    // builds/rebuilds the entire token list and range map
    private void BuildTokenRanges()
    {
        tokenRanges.Clear();
        rangeMap.Clear();
        if (tokens.Count > 0) {
            UpdateTokenRanges(0);
        }
    }

    // updates the token map over a specified range starting from a token index
    private void UpdateTokenRanges(int startIndex)
    {
        // INDEX OUT OF RANGE WHEN ADDING TO BEGINNING OF LIST THAT ALREADY HAS TOKENS

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
        int currentIndex = (tokenRanges.Count == 0) ? 0 : tokenRanges[startIndex - 1].end + 1;

        // only remap current range and everything in front
        // reminder: keys are string index
        // CHANGE TO PURE ARRAY IMPLEMENTATION
        foreach (var key in new List<int>(rangeMap.Keys)) {
            if(key >= currentIndex) {
                rangeMap.Remove(key);
            }
        }

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
    }

    // string parameter is unused but Unity's structure needs it
    private void OnCursorMoved(string inputText) {
        cursorIndex = equationInput.caretPosition;
    }

    public void ConfirmEquation()
    {
        EquationParser parser = new EquationParser();
        // [Type] expressionTree = parser.Parse(tokens);
    }

    // backspace button is pressed
    public void BackspaceToken() {
        // INDEX OUT OF RANGE FOR STRING.REMOVE
        // AFTER REMOVING A FUNCTION STRING ()

        // only delete if there is a token to the left
        if(cursorIndex > 0) {
            // find closest token to the right
            int tokenIndex = GetClosestTokenIndex(cursorIndex);

            // decrement to find the index of the token to delete
            tokenIndex--;

            // delete tokenIndex - 1 and update string
            int tokenLength = tokens[tokenIndex].text.Length;
            equationInput.text = equationInput.text.Remove(cursorIndex - tokenLength, tokenLength);
            tokens.RemoveAt(tokenIndex);
            UpdateTokenRanges(tokenIndex);
            cursorIndex--;
        }
    }

    // inserts a token at the current cursor position and updates the token list accordingly
    private void InsertTokenAtCursor(string text, int type)
    {
        // find closest token to the right
        // i.e. xysin|() where | is cursor position would find ( and not sin
        int tokenIndex = GetClosestTokenIndex(cursorIndex);
        EquationToken newToken = new EquationToken(text, type);

        // update the text
        equationInput.text = equationInput.text.Insert(cursorIndex, text);

        // swap tokens when cursor is inside a token
        // dont have to get end of token because of rightmost token logic described above
        if (tokenIndex < tokens.Count && cursorIndex > tokenRanges[tokenIndex].start) {
            tokens[tokenIndex] = newToken; 
        }
        // otherwise add the token
        else {
            tokens.Insert(tokenIndex, newToken);
        }

        // update the map after insertion
        UpdateTokenRanges(tokenIndex);

        // manually update cursor position
        cursorIndex += text.Length;

        DebugTokenList();
    }

    // O(1) lookup of closest token using the range map
    // CHANGE TO PURE ARRAY IMPLEMENTATION
    private int GetClosestTokenIndex(int cursorPos) 
    {
        if (rangeMap.TryGetValue(cursorPos, out int tokenIndex)) {
            return tokenIndex;
        }

        // handle edges of the text area if value is not in map
        if (tokenRanges.Count > 0 && cursorPos <= tokenRanges[0].start) {
            return 0;
        }

        for (int i = tokenRanges.Count - 1; i >= 0; i--) {
            if (tokenRanges[i].end < cursorPos)
                return tokenRanges[i].tokenIndex + 1;
        }

        return 0;
    }

    private void DebugTokenList()
{
        string tokenString = "Tokens: ";
        foreach (var token in tokens) {
            tokenString += $"[{token.text}] ";
        }

        Debug.Log(tokenString);

        if (rangeMap.Count == 0) {
            Debug.Log("Range Map is empty.");
        }
        else {
            string rangeMapString = "Range Map: ";
            foreach (var kvp in rangeMap) {
                rangeMapString += $"({kvp.Key} -> {kvp.Value}), ";
            }
            Debug.Log(rangeMapString);
        }

        if (tokenRanges.Count == 0) {
            Debug.Log("Token Ranges is empty.");
        }
        else {
            string rangeListString = "Token Ranges: ";
            foreach (var range in tokenRanges) {
                rangeListString += $"[{range.start}-{range.end} -> Token {range.tokenIndex}], ";
            }
            Debug.Log(rangeListString);
        }
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

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Android;

public class EquationTokenizer
{   
    // text field from the UI passed in through keyboardManager
    private TMP_InputField equationInput;
    // list of the tokens
    public List<EquationToken> tokens { get; private set; } = new List<EquationToken>();
    // list of the starting and ending string index positions of each token
    private List<(int start, int end, int tokenIndex)> tokenRanges = new List<(int, int, int)>();
    // map used for O(1) lookup of the above list
    // keys are the string index and values are the corresponding token index
    private int[] rangeMap = new int[MAX_CHARACTERS];
    // fixed max character count, can be changed later whenever we make the 
    // text box stretch to equation size but right now its hard coded
    private const int MAX_CHARACTERS = 256;

    public EquationTokenizer(TMP_InputField equationInput){
        this.equationInput = equationInput;
    }

    // builds/rebuilds the entire token list and range map
    public void BuildTokenRanges()
    {
        tokenRanges.Clear();
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

    // backspace button is pressed
    public int RemoveTokenAtCursor(int cursorIndex) {
        // only delete if there is a token to the left
        if(cursorIndex == 0 || tokens.Count == 0) return 0;

        // find closest token to the left
        // subtracting 1 will allow the correct token to be chosen (left of cursor)
        //  because the range map would normally find whats right of the cursor
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
        return cursorIndex;
    }

    // inserts a token at the current cursor position and updates the token list accordingly
    public int InsertTokenAtCursor(string text, int type, int cursorIndex)
    {
        EquationToken newToken = new EquationToken(text, type);

        // no previous tokens
        if (tokens.Count == 0) {
            tokens.Add(newToken);
            equationInput.text = text;
            UpdateTokenRanges(0);
            cursorIndex = tokenRanges[0].end + 1;
            return cursorIndex;
        }

        // find closest token to the right of cursor
        int tokenIndex = rangeMap[cursorIndex];
        
        // add token to the end
        if (tokenIndex == -2) {
            tokens.Add(newToken);
            equationInput.text += text;
            UpdateTokenRanges(tokens.Count - 1);
            cursorIndex = tokenRanges[tokens.Count - 1].end + 1;
            return cursorIndex;
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
        // so no need to check if the cursor is before the end

        // update the map after insertion
        UpdateTokenRanges(tokenIndex);

        // update cursor position
        cursorIndex = tokenRanges[tokenIndex].end + 1;
        return cursorIndex;
    }

    // cleans up the token list
    public void CleanUpEquation()
    {
        // TODO: ADD A TOKEN COMPRESSOR TO MAKE NUMBERS WORK CORRECTLY
        //       RIGHT NOW ONLY WHOLE NUMBERS WORK
        //       COMPRESS THE TOKEN LIST AFTER GRAPH BUTTON IS PRESSED BUT BEFORE SENDING TO PARSER
        //       THIS WOULD ALSO ADD * BETWEEN SOME TOKENS
        //       ALSO ADD NUMBERS TO THE UI
    }
}

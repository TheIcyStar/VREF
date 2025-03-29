using TMPro;
using System.Collections.Generic;

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
        // no tokens on update (happens when deleting the last token)
        if (tokens.Count == 0) return;

        // starting index cannot be greater than the total number of tokens
        if (startIndex > tokenRanges.Count) throw new TokenizerException("Starting index past end of token ranges to update.");

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
    public int RemoveTokenAtCursor(int cursorIndex) 
    {
        // only delete if there is a token to the left
        if(cursorIndex <= 0 || tokens.Count == 0) return 0;

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
        // check that cursorIndex is valid
        if (cursorIndex < 0 || cursorIndex >= rangeMap.Length) throw new TokenizerException("Cursor is out of range to insert.");

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

    // cleans up the token list by:
    // - adding implicit multiplication
    // - combining numbers into one token
    // for now this just returns a new list of tokens based on
    // the old one because i dont want to touch the
    // tokenizer code and range maps at all
    public List<EquationToken> CleanUpEquation()
    {
        // empty equation, nothing to clean
        if (tokens.Count <= 0) return tokens;

        // initialize clean token list starting empty, will obtain main token list's values in CombineNumbers
        List<EquationToken> cleanTokens = new List<EquationToken>();

        // combine all the numbers
        CombineNumbers(cleanTokens);

        // add implicit multiplication
        AddImplicitMultiplication(cleanTokens);

        return cleanTokens;
    }

    // combines numbers by:
    // prepending all numbers left of '.' up until a non-number
    // appending all number right of '.' up until a non-number
    // 123.456.789 will be one token to catch the error later (when trying to parse)
    private void CombineNumbers(List<EquationToken> cleanTokens) 
    {
        // iterate through all the tokens
        for (int i = 0; i < tokens.Count; i++) {
            // if the token is not the last element, check if num.num structure exists
            if (i < tokens.Count - 2 && tokens[i].type == EquationParser.TYPE_NUMBER && tokens[i + 1].text == "." && tokens[i + 2].type == EquationParser.TYPE_NUMBER) {
                // check if previous token is also a number
                // ^1 gets last element
                if (cleanTokens.Count > 0 && cleanTokens[^1].type == EquationParser.TYPE_NUMBER) {
                    // merge the current token group from old list with the most recent token added to new list
                    cleanTokens[^1] = new EquationToken(cleanTokens[^1].text + tokens[i].text + "." + tokens[i + 2].text, EquationParser.TYPE_NUMBER);
                }
                // if not, just add it normally
                else {
                    cleanTokens.Add(new EquationToken(tokens[i].text + "." + tokens[i + 2].text, EquationParser.TYPE_NUMBER));
                }
                
                // skip the next two tokens
                i += 2;
            }
            // lone decimal point (can only happen as first token)
            else if(tokens[i].text == ".") {
                // create a number out of it
                if(i < tokens.Count - 1 && tokens[i + 1].type == EquationParser.TYPE_NUMBER) {
                    cleanTokens.Add(new EquationToken("0." + tokens[i + 1].text, EquationParser.TYPE_NUMBER));

                    // skip the next token
                    i++;
                }
                // add just the '.' (might cause problems, probably should cause an error here)
                else {
                    throw new TokenizerException("Unexpected decimal point.");
                }
            }
            // non decimal number merge
            else if (tokens[i].type == EquationParser.TYPE_NUMBER) {
                // check if previous token is also a number
                if (cleanTokens.Count > 0 && cleanTokens[^1].type == EquationParser.TYPE_NUMBER) {
                    // merge the current token from old list with the most recent token added to new list
                    cleanTokens[^1] = new EquationToken(cleanTokens[^1].text + tokens[i].text, EquationParser.TYPE_NUMBER);
                }
                // if not, just add it normally
                else {
                    cleanTokens.Add(tokens[i]);
                }
            }
            // non-number token, add normally
            else {
                cleanTokens.Add(tokens[i]);
            }
        }
    }

    // makes implicit multiplication explicit between:
    // number * [variable | function | parenthesis]
    // ) * [variable | function | number | parenthesis]
    // variable * [function | variable | (]

    // should we allow variable * number to be implicit?
    // ex: xyz7.349
    private void AddImplicitMultiplication(List<EquationToken> cleanTokens) 
    {
        for (int i = 0; i < cleanTokens.Count - 1; i++) {
            EquationToken current = cleanTokens[i];
            EquationToken next = cleanTokens[i + 1];

            // number followed by variable, function, or parenthesis
            if (current.type == EquationParser.TYPE_NUMBER 
            && (next.type == EquationParser.TYPE_VARIABLE || next.type == EquationParser.TYPE_FUNCTION || next.type == EquationParser.TYPE_LEFTPAREN)) 
            {
                cleanTokens.Insert(i + 1, new EquationToken("*", EquationParser.TYPE_OPERATOR));
                i++;
            }
            // closing parenthesis followed by variable, function, number, or another parenthesis
            else if (current.type == EquationParser.TYPE_RIGHTPAREN 
            && (next.type == EquationParser.TYPE_VARIABLE || next.type == EquationParser.TYPE_FUNCTION || next.type == EquationParser.TYPE_NUMBER || next.type == EquationParser.TYPE_LEFTPAREN)) 
            {
                cleanTokens.Insert(i + 1, new EquationToken("*", EquationParser.TYPE_OPERATOR));
                i++;
            }
            // variable followed by function, variable, or opening parenthesis
            else if (current.type == EquationParser.TYPE_VARIABLE 
            && (next.type == EquationParser.TYPE_FUNCTION || next.type == EquationParser.TYPE_VARIABLE || next.type == EquationParser.TYPE_LEFTPAREN)) 
            {
                cleanTokens.Insert(i + 1, new EquationToken("*", EquationParser.TYPE_OPERATOR));
                i++;
            }
        }
    }
}

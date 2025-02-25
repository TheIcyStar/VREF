using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;

public class EquationParser
{
    // DOES NOT WORK CORRECTLY YET
    // the problem: null nodes are being created somehow
    //  and are spread throughout the tree
    //  general structure SEEMS to be accurate aside from the nulls

    private List<EquationToken> tokens;
    private int tokenIndex;

    public const int TYPE_NUMBER = 0;
    public const int TYPE_VARIABLE = 1;
    public const int TYPE_FUNCTION = 2;
    public const int TYPE_OPERATOR = 3;
    public const int TYPE_DECIMAL = 4;
    public const int TYPE_LEFTPAREN = 5;
    public const int TYPE_RIGHTPAREN = 6;

    // grammar:
    // add suport for relational operators in the equation rule later
    /*
    Equation     ->    Expression ('=' Expression)?             // not implemented
    Linear       ->    (Variable | Number) = Expression         // not implemented
    Expression   ->    Term (( '+' | '-' ) Term )*
    Term         ->    Unary (( '*' | '/' ) Unary )*
    Unary        ->    ( '-' Unary ) | Power
    Power        ->    Primary ( '^' Unary )*
    Primary      ->    Function | Number | Variable | '(' Expression ')'
    Function     ->    ('sin' | 'cos' | 'tan' | 'sqrt' | 'log' | ... ) '(' Expression ')'
    Variable     ->    'x' | 'y' | 'z' | ...
    Number       ->    [0-9]+('.'[0-9]+)?
    */

    // takes list of tokens from keyboard and parses them into a tree
    public ParseTreeNode Parse(List<EquationToken> tokens) {
        this.tokens = tokens;
        tokenIndex = 0;
        return ParseExpression();
    }

    // returns true if the current token is the same type as the type passed in
    // increments to the next token if true
    // returns false otherwise
    private bool TypeMatch(int type) {
        if(tokenIndex < tokens.Count && CurrentToken().type == type) {
            tokenIndex++;
            return true;
        }
        return false;
    }

    // returns a used token
    private EquationToken UseToken(int type) {
        if(TypeMatch(type)) {
            return tokens[tokenIndex - 1];
        }
        return null;
    }

    // gets the current token
    // returns null if out of range
    private EquationToken CurrentToken() {
        return (tokenIndex < tokens.Count) ? tokens[tokenIndex] : null;
    }

    // PARSE EQUATION GOES HERE
    // ---------------------------

    // parses an expression from the token list following the rule of the grammar:
    //  Expression -> Term (( '+' | '-' ) Term )*
    private ParseTreeNode ParseExpression() {
        ParseTreeNode node = ParseTerm();

        EquationToken op = CurrentToken();

        while (op != null && op.type == TYPE_OPERATOR && (op.text == "+" || op.text == "-")) {
            op = UseToken(TYPE_OPERATOR);
            ParseTreeNode right = ParseTerm();
            node = new ParseTreeNode(op) { left = node, right = right };
        }

        return node;
    }

    // parses a term from the token list following the rule of the grammar:
    //  Term -> Unary (( '*' | '/' ) Unary )*
    private ParseTreeNode ParseTerm() {
        ParseTreeNode node = ParseUnary();

        EquationToken op = CurrentToken();

        while (op != null && op.type == TYPE_OPERATOR && (op.text == "*" || op.text == "/")) {
            op = UseToken(TYPE_OPERATOR);
            ParseTreeNode right = ParseUnary();
            node = new ParseTreeNode(op) { left = node, right = right };
            op = CurrenToken();
        }

        return node;
    }

    // parses a unary operator from the token list following the rule of the grammar:
    //  Unary -> ( '-' Unary ) | Power
    private ParseTreeNode ParseUnary() {
        EquationToken op = CurrentToken();

        if(op != null && op.type == TYPE_OPERATOR && op.text == "-") {
            op = UseToken(TYPE_OPERATOR);
            ParseTreeNode node = ParseUnary();
            return new ParseTreeNode(op) { right = node };
        }

        return ParsePower();
    }

    // parses a power from the token list following the rule of the grammar:
    //  Power -> Primary ( '^' Unary )*
    private ParseTreeNode ParsePower() {
         ParseTreeNode node = ParsePrimary();

         EquationToken op = CurrentToken();

         while (op != null && op.type == TYPE_OPERATOR && op.text == "^") {
            op = UseToken(TYPE_OPERATOR);
            ParseTreeNode right = ParseUnary();
            node = new ParseTreeNode(op) { left = node, right = right };
        }

        return node;
    }

    // parses a primary attribute (function, number, variable, or expression in parenthesis)
    //  from the token list following the rule of the grammar:
    //  Primary -> Function | Number | Variable | '(' Expression ')'
    private ParseTreeNode ParsePrimary() {
        EquationToken primary = CurrentToken();

        if(primary != null && primary.type == TYPE_FUNCTION) {
            return ParseFunction();
        }
        else if(primary != null && primary.type == TYPE_NUMBER) {
            return ParseNumber();
        }
        else if(primary != null && primary.type == TYPE_VARIABLE) {
            return ParseVariable();
        }
        else if(TypeMatch(TYPE_LEFTPAREN)) {
            ParseTreeNode node = ParseExpression();
            UseToken(TYPE_RIGHTPAREN);
            return node;
        }

        // should never happen
        return null;
    }

    // parses a function from the token list following the rule of the grammar:
    //  Function -> ('sin' | 'cos' | 'tan' | 'sqrt' | 'log' | ... ) '(' Expression ')'
    private ParseTreeNode ParseFunction() {
        EquationToken function = UseToken(TYPE_FUNCTION);
        UseToken(TYPE_LEFTPAREN);
        ParseTreeNode node = ParseExpression();
        UseToken(TYPE_RIGHTPAREN);
        return new ParseTreeNode(function) { right = node };
    }

    // parses a number from the token list following the rule of the grammar:
    //  Number -> [0-9]+('.'[0-9]+)?
    //  the number should already be in this format
    private ParseTreeNode ParseNumber() {
        return new ParseTreeNode(UseToken(TYPE_NUMBER));
    }

    // parses a variable from the token list following the rule of the grammar:
    //  Variable -> 'x' | 'y' | 'z' | ...
    private ParseTreeNode ParseVariable() {
        return new ParseTreeNode(UseToken(TYPE_VARIABLE));
    }
}

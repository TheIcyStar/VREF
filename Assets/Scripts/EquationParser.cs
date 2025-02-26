using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;

public class EquationParser
{
    // WORKS!
    // i tested x/z+(sin(x*z))-y and it worked perfectly
    // there is more to test but im moving on to the grapher now

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
    // --------------------------------------------------------------
    // the lower the rule of the grammar, the more precedence it has
    // this is because it will be lower on the tree and calculated first
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
        // parse a term on the LHS
        ParseTreeNode node = ParseTerm();

        // find the current token to check if its an operator (+ -)
        EquationToken op = CurrentToken();

        // keep checking for (( '+' | '-' ) Term )* until a different token
        while (op != null && op.type == TYPE_OPERATOR && (op.text == "+" || op.text == "-")) {
            // use up the operator
            op = UseToken(TYPE_OPERATOR);

            // parse a term on the RHS
            ParseTreeNode right = ParseTerm();

            // make the operator the parent of the two terms
            node = new ParseTreeNode(op) { left = node, right = right };

            // check the next token for another operator
            op = CurrentToken();
        }

        // return the root of the expression tree (also the root of the whole tree since expressions are first,
        //  but eventually the root of the whole tree will change to an equation format (linear, planar, ...))
        return node;
    }

    // parses a term from the token list following the rule of the grammar:
    //  Term -> Unary (( '*' | '/' ) Unary )*
    private ParseTreeNode ParseTerm() {
        // parse a unary operation on the LHS
        ParseTreeNode node = ParseUnary();

        // find the current token to check if its an operator (* /)
        EquationToken op = CurrentToken();

        // keep checking for (( '*' | '/' ) Unary )* until a different token
        while (op != null && op.type == TYPE_OPERATOR && (op.text == "*" || op.text == "/")) {
            // use up the operator
            op = UseToken(TYPE_OPERATOR);

            // parse a unary operation on the RHS
            ParseTreeNode right = ParseUnary();

            // make the operator the parent of the two unary operations
            node = new ParseTreeNode(op) { left = node, right = right };

            // check the next token for another operator
            op = CurrentToken();
        }

        // return the root of the term tree
        return node;
    }

    // parses a unary operator from the token list following the rule of the grammar:
    //  Unary -> ( '-' Unary ) | Power
    private ParseTreeNode ParseUnary() {
        // find the current token to check if its an operator (-)
        EquationToken op = CurrentToken();

        // check for ( '-' Unary )
        if(op != null && op.type == TYPE_OPERATOR && op.text == "-") {
            // use up the operator
            op = UseToken(TYPE_OPERATOR);

            // parse the unary on the RHS
            ParseTreeNode node = ParseUnary();

            // return the root of the unary tree
            return new ParseTreeNode(op) { right = node };
        }

        // no unary expression found, so parse a power instead and return the root it returns
        return ParsePower();
    }

    // parses a power from the token list following the rule of the grammar:
    //  Power -> Primary ( '^' Unary )*
    private ParseTreeNode ParsePower() {
        // parse a primary on the LHS
        ParseTreeNode node = ParsePrimary();

        // find the current token to check if its a caret
        EquationToken op = CurrentToken();

        // keep checking for Primary ( '^' Unary )* until a different token
        while (op != null && op.type == TYPE_OPERATOR && op.text == "^") {
            // use up the operator
            op = UseToken(TYPE_OPERATOR);

            // parse a unary on the RHS
            ParseTreeNode right = ParseUnary();

            // make the exponentiation operator the root of the power tree
            node = new ParseTreeNode(op) { left = node, right = right };
        }

        // return the root of the power tree
        return node;
    }

    // parses a primary attribute (function, number, variable, or expression in parenthesis)
    //  from the token list following the rule of the grammar:
    //  Primary -> Function | Number | Variable | '(' Expression ')'
    private ParseTreeNode ParsePrimary() {
        // find the current token to check which kind of primary it is
        EquationToken primary = CurrentToken();

        // dont use TypeMatch when checked to not consume tokens
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
        // use up the function token, but also save it
        EquationToken function = UseToken(TYPE_FUNCTION);

        // use up a left parenthesis
        UseToken(TYPE_LEFTPAREN);

        // parse the expression inside the function
        ParseTreeNode node = ParseExpression();

        // use up a right parenthesis
        UseToken(TYPE_RIGHTPAREN);

        // return the root of the function tree with the only child being the root of the expression tree
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

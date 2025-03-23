using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using NUnit.Framework.Interfaces;
using UnityEngine.Rendering;
using System;

public class EquationParser
{
    // NOTES/TODO: recursive descent parsers (like this one) are simple and should be sufficient,
    //             but if there are performance issues, or we need a more complicated grammar in the future,
    //             we can implement a recurisve predictive parser instead

    // passed in token list
    private List<EquationToken> tokens;
    // index of the current token
    private int tokenIndex;

    // node types
    // public because the grapher uses them
    public const int TYPE_NULL = -1;
    public const int TYPE_NUMBER = 0;
    public const int TYPE_VARIABLE = 1;
    public const int TYPE_FUNCTION = 2;
    public const int TYPE_OPERATOR = 3;
    public const int TYPE_RELOP = 4;
    public const int TYPE_DECIMAL = 5;
    public const int TYPE_LEFTPAREN = 6;
    public const int TYPE_RIGHTPAREN = 7;

    // grammar:
    // add suport for relational operators in the explicit function rule later
    // --------------------------------------------------------------
    // the lower the rule of the grammar, the more precedence it has
    // this is because it will be lower on the tree and calculated first
    // should probably rewrite this comment with regular definitions to make it simpler
    // ONLY '=' SUPPORTED FOR EXPLICIT FUNCTIONS
    /*
    Explicit     ->    Variable ('=' | '>' | '<' | '>=' | '<=') Expression                      // right-associative (handled in implementation)
    Expression   ->    Term (( '+' | '-' ) Term )*                                              // left-associative (handled in implementation)
    Term         ->    Unary (( '*' | '/' ) Unary )*                                            // left-associative (handled in implementation)
    Unary        ->    ( '-' Unary ) | Power                                                    // right-associative (immediate right recursive)
    Power        ->    Primary ( '^' Unary )?                                                   // right-associative (right recursive)
    Primary      ->    Function | Number | Variable | '(' Expression ')'                        
    Function     ->    ('sin' | 'cos' | 'tan' | 'sqrt' | 'log' | ... ) '(' Expression ')'                       
    Variable     ->    'x' | 'y' | 'z' | ...
    Number       ->    [0-9]+('.'[0-9]+)?
    */

    // takes list of tokens from keyboard and parses them into a tree
    // then sends the tree to the graph manager
    public ParseTreeNode Parse(List<EquationToken> tokens) {
        // no tokens to parse
        if (tokens == null || tokens.Count == 0) throw new ParserException("No tokens to parse.");

        this.tokens = tokens;
        tokenIndex = 0;

        // parse the whole token list
        ParseTreeNode root = ParseExplicit();

        // check if all tokens were used
        if (tokenIndex < tokens.Count) throw new ParserException($"Unexpected tokens after expression: '{string.Join("", tokens.GetRange(tokenIndex, tokens.Count - tokenIndex).ConvertAll(t => t.text))}'.");

        return root;
    }

    // gets the current token
    // returns a null token if tokenIndex is out of range
    private EquationToken CurrentToken() {
        try {return tokens[tokenIndex];} catch (ArgumentOutOfRangeException) {return new EquationToken("null", -1);}
    }

    // returns true if the current token is the same type as the type passed in, returns false otherwise
    private bool TypeMatch(int type) {
        return CurrentToken().type == type;
    }

    // calls TypeMatch to verify that the token matches
    // then increments the token index and returns the used token
    private EquationToken UseToken(int type) {
        if(TypeMatch(type)) {
            tokenIndex++;
            return tokens[tokenIndex - 1];
        }

        // wrong token
        throw new ParserException($"but found: '{CurrentToken().text}'.");
    }

    // parses an explicit from the token list following the rule of the grammar:
    // Explicit -> Variable ('=' | '>' | '<' | '>=' | '<=') Expression
    private ParseTreeNode ParseExplicit() {
        // parse a variable on the LHS
        ParseTreeNode variable;
        try {variable = new ParseTreeNode(UseToken(TYPE_VARIABLE));} catch (ParserException pe) {throw new ParserException($"Expected variable, {pe.Message}");}

        // try to use up the relational operator ('=' | '>' | '<' | '>=' | '<=')
        // ONLY '=' SUPPORTED
        EquationToken relop;
        try {relop = UseToken(TYPE_RELOP);} catch (ParserException pe) {throw new ParserException($"Expected relational operator after variable (only '=' supported), {pe.Message}");}

        // parse the expression on the RHS
        ParseTreeNode expression;
        try{expression = ParseExpression();} catch(InvalidOperationException) {throw new ParserException("Missing or invalid expression after '='.");}

        // return the root of the explicit function tree (which is also the root of the whole tree)
        return new ParseTreeNode(relop) { left = variable, right = expression };
    }

    // parses an expression from the token list following the rule of the grammar:
    //  Expression -> Term (( '+' | '-' ) Term )*
    private ParseTreeNode ParseExpression() {
        // parse a term on the LHS
        ParseTreeNode firstTerm = ParseTerm();

        // loop through the rest of the expression
        // keep checking for (( '+' | '-' ) Term )* until 
        // a different token or end of token list
        while(TypeMatch(TYPE_OPERATOR) && (CurrentToken().text == "+" || CurrentToken().text == "-")) {
            // use up the current operator
            EquationToken op = UseToken(TYPE_OPERATOR);

            // parse a term on the RHS
            ParseTreeNode nextTerm ;
            try {nextTerm = ParseTerm();} catch (InvalidOperationException) {throw new ParserException($"Missing/invalid right-hand side after operator '{op.text}'.");}

            // make the operator the parent of the two terms
            firstTerm = new ParseTreeNode(op) { left = firstTerm, right = nextTerm };
        }

        // return the root of the expression tree
        return firstTerm;
    }

    // parses a term from the token list following the rule of the grammar:
    //  Term -> Unary (( '*' | '/' ) Unary )*
    private ParseTreeNode ParseTerm() {
        // parse a unary operation on the LHS
        ParseTreeNode firstUnary = ParseUnary();

        // loop through the rest of the term
        // keep checking for (( '*' | '/' ) Unary )* until 
        // a different token or end of token list
        while (TypeMatch(TYPE_OPERATOR) && (CurrentToken().text == "*" || CurrentToken().text == "/")) {
            // use up the operator
            EquationToken op = UseToken(TYPE_OPERATOR);

            // parse a unary operation on the RHS
            ParseTreeNode nextUnary;
            try {nextUnary = ParseUnary();} catch (InvalidOperationException) {throw new ParserException($"Missing/invalid right-hand side after operator '{op.text}'.");}

            // make the operator the parent of the two unary operations
            firstUnary = new ParseTreeNode(op) { left = firstUnary, right = nextUnary };
        }

        // return the root of the term tree
        return firstUnary;
    }

    // parses a unary operator from the token list following the rule of the grammar:
    //  Unary -> ( '-' Unary ) | Power
    private ParseTreeNode ParseUnary() {
        // check for ( '-' Unary )
        if(TypeMatch(TYPE_OPERATOR) && CurrentToken().text == "-") {
            // use up the operator
            EquationToken op = UseToken(TYPE_OPERATOR);

            // parse the unary on the RHS
            ParseTreeNode unary = ParseUnary();

            // return the root of the unary tree
            return new ParseTreeNode(op) { right = unary };
        }

        // no unary expression found, so parse a power instead and return the root it returns
        return ParsePower();
    }

    // parses a power from the token list following the rule of the grammar:
    //  Power -> Primary ( '^' Unary )?
    private ParseTreeNode ParsePower() {
        // parse a primary on the LHS
        ParseTreeNode primary = ParsePrimary();

        // only check for Primary ( '^' Unary ) once 
        // as '^' is right associative, so additional
        // powers will be added in the ParseUnary call
        if(TypeMatch(TYPE_OPERATOR) && CurrentToken().text == "^") {
            // use up the operator
            EquationToken op = UseToken(TYPE_OPERATOR);

            // parse a unary on the RHS
            ParseTreeNode unary;
            try {unary = ParseUnary();} catch (InvalidOperationException) {throw new ParserException($"Missing/invalid right-hand side after operator '^'.");}

            // make the exponentiation operator the root of the power tree
            primary = new ParseTreeNode(op) { left = primary, right = unary };
        }

        // return the root of the power tree
        return primary;
    }

    // parses a primary attribute (function, number, variable, or expression in parenthesis)
    //  from the token list following the rule of the grammar:
    //  Primary -> Function | Number | Variable | '(' Expression ')'
    private ParseTreeNode ParsePrimary() {
        // check for Function | Number | Variable | '(' Expression ')'
        if(TypeMatch(TYPE_FUNCTION)) return ParseFunction();
        if(TypeMatch(TYPE_NUMBER)) return new ParseTreeNode(UseToken(TYPE_NUMBER));
        if(TypeMatch(TYPE_VARIABLE)) return new ParseTreeNode(UseToken(TYPE_VARIABLE));;
        if(TypeMatch(TYPE_LEFTPAREN)) {
            // use up the left parenthesis
            UseToken(TYPE_LEFTPAREN);

            // parse the expression inside the parenthesis
            ParseTreeNode node;
            try {node = ParseExpression();} catch (InvalidOperationException) {throw new ParserException("Invalid/missing expression inside parenthesis.");}

            // try to use up the right parenthesis
            try {UseToken(TYPE_RIGHTPAREN);} catch (ParserException) {throw new ParserException($"Missing right parenthesis to match left parenthesis.");}

            // return the root of the expression tree (parenthesis ignored)
            return node;
        }

        // not a primary token
        // throw a built in exception to easily catch and handle differently
        throw new InvalidOperationException("Unexpected/missing token in primary expression.");
    }

    // parses a function from the token list following the rule of the grammar:
    //  Function -> ('sin' | 'cos' | 'tan' | 'sqrt' | 'log' | ... ) '(' Expression ')'
    private ParseTreeNode ParseFunction() {
        // use up the function token (already checked that it exists in ParsePrimary)
        // store the function token to output the function name for other errors
        EquationToken function = UseToken(TYPE_FUNCTION);

        // try to use up a left parenthesis
        try {UseToken(TYPE_LEFTPAREN);} catch(ParserException) {throw new ParserException($"Missing left parenthesis after function: '{function.text}'.");}

        // parse the expression inside the function
        ParseTreeNode node;
        try {node = ParseExpression();} catch(InvalidOperationException) {throw new ParserException($"Missing expression inside function: '{function.text}'.");}

        // try to use up a right parenthesis
        try {UseToken(TYPE_RIGHTPAREN);} catch (ParserException) {throw new ParserException($"Missing right parenthesis after function: '{function.text}'.");}

        // return the root of the function tree with the only child being the root of the expression tree
        return new ParseTreeNode(function) { right = node };
    }

    // temporary functions to debug the parse tree
    public string DebugParseTree(ParseTreeNode root)
    {
        if (root == null)
            return "Parse tree is null.";

        return DebugParseTreeHelper(root, 0);
    }

    private string DebugParseTreeHelper(ParseTreeNode node, int depth)
    {
        if (node == null) return "";

        string indent = new string(' ', depth * 2);
        string result = $"{indent}- {node.token.text} [{node.token.type}]\n";

        if (node.left != null)
            result += DebugParseTreeHelper(node.left, depth + 1);
        if (node.right != null)
            result += DebugParseTreeHelper(node.right, depth + 1);

        return result;
    }
}

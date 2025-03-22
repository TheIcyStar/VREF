using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using NUnit.Framework.Interfaces;
using UnityEngine.Rendering;

public class EquationParser
{
    // NOTES/TODO: recursive descent parsers (like this one) are apparently exponentially slow
    //             but i dont think that will matter for this project
    //             if it becomes a problem, we can implement a recurisve predictive parser instead
    //             but this one is a lot simpler

    // passed in token list
    private List<EquationToken> tokens;
    // index of the current token
    private int tokenIndex;

    // node types
    // public because the grapher uses them
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
    Power        ->    Primary ( '^' Unary )*                                                   // right-associative (right recursive)
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

        // try to parse the whole token list
        try {
            ParseTreeNode equationTree = ParseExplicit();

            // not all tokens were used
            if (tokenIndex < tokens.Count) {
                throw new ParserException($"Unexpected token: {tokens[tokenIndex].text}");
            }

            return equationTree;
        } catch (ParserException pe) {
            throw new ParserException($"Parser failed: {pe.Message}");
        }
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

    // calls TypeMatch to use the token
    // and returns the used token
    private EquationToken UseToken(int type) {
        if(TypeMatch(type)) {
            return tokens[tokenIndex - 1];
        }

        // wrong token
        throw new ParserException($"Expected token of type {type}, but found: {CurrentToken()?.text ?? "null"}");
    }

    // gets the current token
    // all uses of CurrentToken in the parser should never be out of bounds 
    // (it will throw a generic error they somehow are)
    private EquationToken CurrentToken() {
        return (tokenIndex < tokens.Count) ? tokens[tokenIndex] : throw new ParserException("Token index out of range when parsing.");
    }

    // parses an explicit from the token list following the rule of the grammar:
    // Explicit -> Variable ('=' | '>' | '<' | '>=' | '<=') Expression
    private ParseTreeNode ParseExplicit() {
        if(tokenIndex + 3 > tokens.Count) throw new ParserException("Explicit function must be in this format: [variable] = [expression]");

        // parse a variable on the LHS
        ParseTreeNode node = ParseVariable();

        // try to find the current token to check if its a relational operator (= < > <= >=)
        EquationToken relop = CurrentToken();

        // check for ('=' | '>' | '<' | '>=' | '<=') Expression
        // ONLY '=' SUPPORTED
        if (relop.type != TYPE_RELOP || relop.text != "=") {
            throw new ParserException($"Expected relational operator after variable (only '=' supported), but found: {relop?.text ?? "null"}");
        }

        // use up the relational operator
        relop = UseToken(TYPE_RELOP);

        // parse the expression on the RHS
        ParseTreeNode right = ParseExpression();

        // return the root of the explicit function tree (which is also the root of the whole tree)
        return new ParseTreeNode(relop) { left = node, right = right };
    }

    // parses an expression from the token list following the rule of the grammar:
    //  Expression -> Term (( '+' | '-' ) Term )*
    private ParseTreeNode ParseExpression() {
        // parse a term on the LHS
        ParseTreeNode node = ParseTerm();

        // loop through the rest of the expression
        // keep checking for (( '+' | '-' ) Term )* until 
        // a different token or end of token list
        while (tokenIndex < tokens.Count) {
            // find the current token to check if its an operator (+ -)
            EquationToken op = CurrentToken();

            if (op.type == TYPE_OPERATOR && (op.text == "+" || op.text == "-")) {
                // use up the operator
                op = UseToken(TYPE_OPERATOR);

                // parse a term on the RHS
                ParseTreeNode right = ParseTerm();

                // make the operator the parent of the two terms
                // left term can either be the term on the left
                // or the previous operator parent tree
                node = new ParseTreeNode(op) { left = node, right = right };
            } else {
                // no more + or -, stop parsing the expression
                break;
            }
        }

        // return the root of the expression tree
        return node;
    }

    // parses a term from the token list following the rule of the grammar:
    //  Term -> Unary (( '*' | '/' ) Unary )*
    private ParseTreeNode ParseTerm() {
        // parse a unary operation on the LHS
        ParseTreeNode node = ParseUnary();

        // loop through the rest of the term
        // keep checking for (( '*' | '/' ) Unary )* until 
        // a different token or end of token list
        while (tokenIndex < tokens.Count) {
            // find the current token to check if its an operator (* /)
            // should never be out of bounds (will throw error if it somehow is)
            EquationToken op = CurrentToken();

            if (op.type == TYPE_OPERATOR && (op.text == "*" || op.text == "/")) {
                // use up the operator
                op = UseToken(TYPE_OPERATOR);

                // parse a unary operation on the RHS
                ParseTreeNode right = ParseUnary();

                // make the operator the parent of the two unary operations
                node = new ParseTreeNode(op) { left = node, right = right };
            } else {
                // no more * or /, stop parsing the term
                break;
            }
        }

        // return the root of the term tree
        return node;
    }

    // parses a unary operator from the token list following the rule of the grammar:
    //  Unary -> ( '-' Unary ) | Power
    private ParseTreeNode ParseUnary() {
        if(tokenIndex < tokens.Count) {
            // find the current token to check if its an operator (-)
            EquationToken op = CurrentToken();

            // check for ( '-' Unary )
            if(op.type == TYPE_OPERATOR && op.text == "-") {
                // use up the operator
                op = UseToken(TYPE_OPERATOR);

                // parse the unary on the RHS
                ParseTreeNode node = ParseUnary();

                // return the root of the unary tree
                return new ParseTreeNode(op) { right = node };
            }
        }

        // no unary expression found, so parse a power instead and return the root it returns
        return ParsePower();
    }

    // parses a power from the token list following the rule of the grammar:
    //  Power -> Primary ( '^' Power )*
    private ParseTreeNode ParsePower() {
        // parse a primary on the LHS
        ParseTreeNode node = ParsePrimary();

        // loop through the rest of the power
        // keep checking for Primary ( '^' Unary )* until 
        // a different token or end of token list
        while(tokenIndex < tokens.Count) {
            // find the current token to check if its a caret
            EquationToken op = CurrentToken();
            
            if(op.type == TYPE_OPERATOR && op.text == "^") {
                // use up the operator
                op = UseToken(TYPE_OPERATOR);

                // parse a unary on the RHS
                ParseTreeNode right = ParseUnary();

                // make the exponentiation operator the root of the power tree
                node = new ParseTreeNode(op) { left = node, right = right };
            } else {
                // no more ^, stop parsing the power
                break;
            }
        }

        // return the root of the power tree
        return node;
    }

    // parses a primary attribute (function, number, variable, or expression in parenthesis)
    //  from the token list following the rule of the grammar:
    //  Primary -> Function | Number | Variable | '(' Expression ')'
    private ParseTreeNode ParsePrimary() {
        if(tokenIndex >= tokens.Count) throw new ParserException("Unexpected end of input while parsing primary expression.");

        // find the current token to check which kind of primary it is
        EquationToken primary = CurrentToken();

        // dont use TypeMatch when checked to not consume tokens
        if(primary.type == TYPE_FUNCTION) return ParseFunction();
        if(primary.type == TYPE_NUMBER) return ParseNumber();
        if(primary.type == TYPE_VARIABLE) return ParseVariable();

        if(TypeMatch(TYPE_LEFTPAREN)) {
            ParseTreeNode node = ParseExpression();
            try {
                UseToken(TYPE_RIGHTPAREN);
            } catch (ParserException) {
                throw new ParserException("Expected ')' to match '('");
            }
            return node;
        }

        // not a primary token
        throw new ParserException($"Unexpected token in primary expression: {primary?.text ?? "null"}");
    }

    // parses a function from the token list following the rule of the grammar:
    //  Function -> ('sin' | 'cos' | 'tan' | 'sqrt' | 'log' | ... ) '(' Expression ')'
    private ParseTreeNode ParseFunction() {
        if(tokenIndex + 4 > tokens.Count) throw new ParserException("Function must be in this format: [definition]([expression])");

        EquationToken current = CurrentToken();
        if(current.type != TYPE_FUNCTION) throw new ParserException("Missing function declaration while parsing function.");

        // use up the function token, but also save it
        EquationToken function = UseToken(TYPE_FUNCTION);

        current = CurrentToken();
        if(current.type != TYPE_LEFTPAREN) throw new ParserException($"Missing left parenthesis while parsing function: {function.text}");

        // use up a left parenthesis
        UseToken(TYPE_LEFTPAREN);

        // parse the expression inside the function
        ParseTreeNode node = ParseExpression();

        current = CurrentToken();
        if(current.type != TYPE_RIGHTPAREN) throw new ParserException($"Missing right parenthesis while parsing function: {function.text}");

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

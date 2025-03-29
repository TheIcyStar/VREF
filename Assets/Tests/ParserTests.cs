using System.Collections.Generic;
using NUnit.Framework;

public class ParserTests
{
    EquationParser parser;

    [SetUp]
    public void Setup()
    {
        parser = new EquationParser();
    }

    [Test]
    public void ParseAddition()
    {
        var tokens = new List<EquationToken>
        {
            new EquationToken("y", TokenType.Variable),
            new EquationToken("=", TokenType.Relop),
            new EquationToken("2", TokenType.Number),
            new EquationToken("+", TokenType.Operator),
            new EquationToken("3", TokenType.Number)
        };

        var result = parser.Parse(tokens);

        Assert.AreEqual("=", result.token.text);
        Assert.AreEqual(TokenType.Relop, result.token.type);
        Assert.AreEqual("y", result.left.token.text);
        Assert.AreEqual(TokenType.Variable, result.left.token.type);
        Assert.AreEqual("+", result.right.token.text);
        Assert.AreEqual(TokenType.Operator, result.right.token.type);
        Assert.AreEqual("2", result.right.left.token.text);
        Assert.AreEqual(TokenType.Number, result.right.left.token.type);
        Assert.AreEqual("3", result.right.right.token.text);
        Assert.AreEqual(TokenType.Number, result.right.right.token.type);
    }

    [Test]
    public void ParseParenthesis()
    {
        var tokens = new List<EquationToken>
        {
            new EquationToken("y", TokenType.Variable),
            new EquationToken("=", TokenType.Relop),
            new EquationToken("sin", TokenType.Function),
            new EquationToken("(", TokenType.LeftParen),
            new EquationToken("4", TokenType.Number),
            new EquationToken(")", TokenType.RightParen)
        };

        var result = parser.Parse(tokens);

        Assert.AreEqual("=", result.token.text);
        Assert.AreEqual(TokenType.Relop, result.token.type);
        Assert.AreEqual("y", result.left.token.text);
        Assert.AreEqual(TokenType.Variable, result.left.token.type);
        Assert.AreEqual("sin", result.right.token.text);
        Assert.AreEqual(TokenType.Function, result.right.token.type);
        Assert.AreEqual("4", result.right.right.token.text);
        Assert.AreEqual(TokenType.Number, result.right.right.token.type);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
            new EquationToken("y", EquationParser.TYPE_VARIABLE),
            new EquationToken("=", EquationParser.TYPE_RELOP),
            new EquationToken("2", EquationParser.TYPE_NUMBER),
            new EquationToken("+", EquationParser.TYPE_OPERATOR),
            new EquationToken("3", EquationParser.TYPE_NUMBER)
        };

        var result = parser.Parse(tokens);

        Assert.AreEqual("=", result.token.text);
        Assert.AreEqual(EquationParser.TYPE_RELOP, result.token.type);
        Assert.AreEqual("y", result.left.token.text);
        Assert.AreEqual(EquationParser.TYPE_VARIABLE, result.left.token.type);
        Assert.AreEqual("+", result.right.token.text);
        Assert.AreEqual(EquationParser.TYPE_OPERATOR, result.right.token.type);
        Assert.AreEqual("2", result.right.left.token.text);
        Assert.AreEqual(EquationParser.TYPE_NUMBER, result.right.left.token.type);
        Assert.AreEqual("3", result.right.right.token.text);
        Assert.AreEqual(EquationParser.TYPE_NUMBER, result.right.right.token.type);
    }

    [Test]
    public void ParseParenthesis()
    {
        var tokens = new List<EquationToken>
        {
            new EquationToken("y", EquationParser.TYPE_VARIABLE),
            new EquationToken("=", EquationParser.TYPE_RELOP),
            new EquationToken("sin", EquationParser.TYPE_FUNCTION),
            new EquationToken("(", EquationParser.TYPE_LEFTPAREN),
            new EquationToken("4", EquationParser.TYPE_NUMBER),
            new EquationToken(")", EquationParser.TYPE_RIGHTPAREN)
        };

        var result = parser.Parse(tokens);

        Assert.AreEqual("=", result.token.text);
        Assert.AreEqual(EquationParser.TYPE_RELOP, result.token.type);
        Assert.AreEqual("y", result.left.token.text);
        Assert.AreEqual(EquationParser.TYPE_VARIABLE, result.left.token.type);
        Assert.AreEqual("sin", result.right.token.text);
        Assert.AreEqual(EquationParser.TYPE_FUNCTION, result.right.token.type);
        Assert.AreEqual("4", result.right.right.token.text);
        Assert.AreEqual(EquationParser.TYPE_NUMBER, result.right.right.token.type);
    }
}

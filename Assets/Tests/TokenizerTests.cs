using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class TokenizerTests
{
    EquationTokenizer tokenizer;

    [SetUp]
    public void Setup()
    {
        var gameObject = new GameObject();
        var inputField = gameObject.AddComponent<TMP_InputField>();
        tokenizer = new EquationTokenizer(inputField);
    }

    [Test]
    public void CleanAddition()
    {
        tokenizer.tokens.Clear();
        tokenizer.InsertTokenAtCursor("2", EquationParser.TYPE_NUMBER, 0);
        tokenizer.InsertTokenAtCursor("+", EquationParser.TYPE_OPERATOR, 1);
        tokenizer.InsertTokenAtCursor("3", EquationParser.TYPE_NUMBER, 2);

        var cleanTokens = tokenizer.CleanUpEquation();

        Assert.AreEqual(3, cleanTokens.Count);
        Assert.AreEqual("2", cleanTokens[0].text);
        Assert.AreEqual(EquationParser.TYPE_NUMBER, cleanTokens[0].type);
        Assert.AreEqual("+", cleanTokens[1].text);
        Assert.AreEqual(EquationParser.TYPE_OPERATOR, cleanTokens[1].type);
        Assert.AreEqual("3", cleanTokens[2].text);
        Assert.AreEqual(EquationParser.TYPE_NUMBER, cleanTokens[2].type);
    }
}
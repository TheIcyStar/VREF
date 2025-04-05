using NUnit.Framework;
using UnityEngine;
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
        tokenizer.InsertTokenAtCursor("2", TokenType.Number, 0);
        tokenizer.InsertTokenAtCursor("+", TokenType.Operator, 1);
        tokenizer.InsertTokenAtCursor("3", TokenType.Number, 2);

        var cleanTokens = tokenizer.CleanUpEquation();

        Assert.AreEqual(3, cleanTokens.Count);
        Assert.AreEqual("2", cleanTokens[0].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[0].type);
        Assert.AreEqual("+", cleanTokens[1].text);
        Assert.AreEqual(TokenType.Operator, cleanTokens[1].type);
        Assert.AreEqual("3", cleanTokens[2].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[2].type);
    }

    [Test]
    public void CleanSubtraction()
    {
        tokenizer.tokens.Clear();
        tokenizer.InsertTokenAtCursor("4", TokenType.Number, 0);
        tokenizer.InsertTokenAtCursor("-", TokenType.Operator, 1);
        tokenizer.InsertTokenAtCursor("2", TokenType.Number, 2);

        var cleanTokens = tokenizer.CleanUpEquation();

        Assert.AreEqual(3, cleanTokens.Count);
        Assert.AreEqual("4", cleanTokens[0].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[0].type);
        Assert.AreEqual("-", cleanTokens[1].text);
        Assert.AreEqual(TokenType.Operator, cleanTokens[1].type);
        Assert.AreEqual("2", cleanTokens[2].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[2].type);
    }
        [Test]
    public void CleanAddSub()
    {
        tokenizer.tokens.Clear();
        tokenizer.InsertTokenAtCursor("4", TokenType.Number, 0);
        tokenizer.InsertTokenAtCursor("-", TokenType.Operator, 1);
        tokenizer.InsertTokenAtCursor("3", TokenType.Number, 2);
        tokenizer.InsertTokenAtCursor("+", TokenType.Operator, 3);
        tokenizer.InsertTokenAtCursor("2", TokenType.Number, 4);

        var cleanTokens = tokenizer.CleanUpEquation();

        Assert.AreEqual(5, cleanTokens.Count);
        Assert.AreEqual("4", cleanTokens[0].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[0].type);
        Assert.AreEqual("-", cleanTokens[1].text);
        Assert.AreEqual(TokenType.Operator, cleanTokens[1].type);
        Assert.AreEqual("3", cleanTokens[2].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[2].type);
        Assert.AreEqual("+", cleanTokens[3].text);
        Assert.AreEqual(TokenType.Operator, cleanTokens[3].type); 
        Assert.AreEqual("2", cleanTokens[4].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[4].type);
    }
        [Test]
     public void CleanAddAdd()
    {
        tokenizer.tokens.Clear();
        tokenizer.InsertTokenAtCursor("5", TokenType.Number, 0);
        tokenizer.InsertTokenAtCursor("+", TokenType.Operator, 1);
        tokenizer.InsertTokenAtCursor("5", TokenType.Number, 2);
        tokenizer.InsertTokenAtCursor("+", TokenType.Operator, 3);
        tokenizer.InsertTokenAtCursor("2", TokenType.Number, 4);

        var cleanTokens = tokenizer.CleanUpEquation();

        Assert.AreEqual(5, cleanTokens.Count);
        Assert.AreEqual("5", cleanTokens[0].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[0].type);
        Assert.AreEqual("+", cleanTokens[1].text);
        Assert.AreEqual(TokenType.Operator, cleanTokens[1].type);
        Assert.AreEqual("5", cleanTokens[2].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[2].type);
        Assert.AreEqual("+", cleanTokens[3].text);
        Assert.AreEqual(TokenType.Operator, cleanTokens[3].type); 
        Assert.AreEqual("2", cleanTokens[4].text);
        Assert.AreEqual(TokenType.Number, cleanTokens[4].type);
    }
}
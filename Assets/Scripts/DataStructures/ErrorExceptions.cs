using System;
using UnityEngine;

public class TokenizerException : Exception
{
    public TokenizerException(string message) : base(message) {}
}

public class ParserException : Exception
{
    public ParserException(string message) : base(message) {}
}

public class EquationUIException : Exception
{
    public EquationUIException(string message) : base(message) {}
}

using System;

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

public class GraphEvaluationException : Exception
{
    public GraphEvaluationException(string message) : base(message) {}
}

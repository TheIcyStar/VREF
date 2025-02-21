using UnityEngine;

public class EquationParser
{
    public const int TYPE_NUMBER = 0;
    public const int TYPE_VARIABLE = 1;
    public const int TYPE_FUNCTION = 2;
    public const int TYPE_OPERATOR = 3;
    public const int TYPE_DECIMAL = 4;
    public const int TYPE_LEFTPAREN = 5;
    public const int TYPE_RIGHTPAREN = 6;

    // takes list of tokens from keyboard and parses them into a tree
    // public [type] parse(List<EquationToken> tokens)
}

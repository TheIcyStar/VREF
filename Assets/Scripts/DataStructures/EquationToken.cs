public class EquationToken
{
    public string text { get; private set; }
    public TokenType type { get; private set; }

    public EquationToken(string text, TokenType type) {
        this.text = text;
        this.type = type;
    }

    public string ToJSON() {
        return $"{{\"text\":\"{text}\",\"type\":{(int)type}}}";
    }
}

using UnityEngine;

public class EquationToken
{
    public string text { get; private set; }
    public int type { get; private set; }

    public EquationToken(string text, int type) {
        this.text = text;
        this.type = type;
    }

    public string ToJSON() {
        return JsonUtility.ToJson(this);
    }
}

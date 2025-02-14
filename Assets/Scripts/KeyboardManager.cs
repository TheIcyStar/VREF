using UnityEngine;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    public TMP_InputField equationInput;

    public void KeyPress(string key)
    {
        equationInput.text += key;
    }

    public void DeleteLastCharacter()
    {
        if (equationInput.text.Length > 0)
            equationInput.text = equationInput.text.Substring(0, equationInput.text.Length - 1);
    }
}

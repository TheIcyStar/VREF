using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsKeyboardController : MonoBehaviour
{
    public TMP_InputField targetField;

    private static string[,] KEYBOARD_ROWS = {
        {"1","2","3"},
        {"4","5","6"},
        {"7","8","9"},
        {"0",".","_"}
    };

    void Start() {
        Transform layout = transform.GetChild(0);
        Transform keyButton = layout.GetChild(0);

        for(int row_i=0; row_i < 4; row_i++){
            for(int i=0; i < 3; i++){
                Transform newKey = Instantiate(keyButton, layout, false);
                string keyValue = KEYBOARD_ROWS[row_i,i];

                newKey.name = "key"+keyValue;
                newKey.GetChild(0).GetComponentInChildren<TMP_Text>().text = keyValue;

                RectTransform newKeyTransform = newKey.GetComponent<RectTransform>();
                newKeyTransform.anchoredPosition = new Vector2(5 + i * 35, row_i*-35);

                //Set up the key's events
                EventTrigger newKeyEventTrigger = newKey.GetComponentInChildren<EventTrigger>();
                EventTrigger.Entry newEntry = new EventTrigger.Entry();
                newEntry.eventID = EventTriggerType.PointerClick;
                newEntry.callback.AddListener((_) => {
                    onBoardKeyPress(keyValue);
                });

                newKeyEventTrigger.triggers.Add(newEntry);
            }
        }

        Transform clearKey = Instantiate(keyButton, layout, false);
        clearKey.name = "keyClear";
        TMP_Text clearText = clearKey.GetChild(0).GetComponentInChildren<TMP_Text>();
        clearText.text = "Clear";
        clearText.fontSize = 8f;

        RectTransform clearKeyTransform = clearKey.GetComponent<RectTransform>();
        clearKeyTransform.anchoredPosition = new Vector2(5 + 3 * 35, 0);

        EventTrigger clearEventTrigger = clearKey.GetComponentInChildren<EventTrigger>();
        EventTrigger.Entry clearEntry = new EventTrigger.Entry();
        clearEntry.eventID = EventTriggerType.PointerClick;
        clearEntry.callback.AddListener((_) => ClearInput());

        clearEventTrigger.triggers.Add(clearEntry);
    }

    public void ClearInput() {
        if (targetField != null) {
            targetField.text = "";
        }
    }

    private void onBoardKeyPress(string text){
        if(text == "<-"){
            Debug.Log($"{Math.Max(0,targetField.text.Length - 1)}");
            targetField.text = targetField.text.Substring(0,Math.Max(0,targetField.text.Length - 1));
        } else {
            targetField.text += text;
        }
    }
}

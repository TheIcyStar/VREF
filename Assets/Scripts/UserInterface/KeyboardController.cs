using TMPro;
using UnityEngine;



public class KeyboardController : MonoBehaviour {
    private static string[,] KEYBOARD_ROWS = {
        {"1","2","3","4","5","6","7","8","9","0"},
        {"q","w","e","r","t","y","u","i","o","p"},
        {"a","s","d","f","g","h","j","k","l",":"},
        {"z","x","c","v","b","n","m",".","/",""}
    };

    void Start() {
        Transform layout = transform.GetChild(0);
        Transform keyButton = layout.GetChild(0);

        for(int row_i=0; row_i < 4; row_i++){
            for(int i=0; i < 10; i++){
                Transform newKey = Instantiate(keyButton, layout, false);

                newKey.name = "key"+KEYBOARD_ROWS[row_i,i];
                newKey.GetChild(0).GetComponentInChildren<TMP_Text>().text = KEYBOARD_ROWS[row_i,i];

                RectTransform newKeyTransform = newKey.GetComponent<RectTransform>();
                newKeyTransform.anchoredPosition = new Vector2(5 + i * 35, row_i*-35);

            }
        }
    }

    void Update()
    {

    }
}

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ServerTestUiController : MonoBehaviour {
    public GameObject inputField;
    public GameObject statusTextBox;

    public void handleClick() {
        // string inputText = inputField.text;
        string inputText = inputField.GetComponent<TMP_InputField>().text;
        // Debug.Log(inputText);
        // statusTextBox.GetComponent<TMP_Text>().text = inputText;

        statusTextBox.GetComponent<TMP_Text>().text = "Querying server...";
        StartCoroutine(GetRequest(inputText));
    }

    IEnumerator GetRequest(string inputText){
        using (UnityWebRequest webRequest = UnityWebRequest.Get(inputText)){
            yield return webRequest.SendWebRequest();


            if(webRequest.result == UnityWebRequest.Result.Success){
                Debug.Log("Pinged server");
                statusTextBox.GetComponent<TMP_Text>().text = "Server found";
            } else {
                Debug.Log("Connection failed: "+webRequest.error);
                statusTextBox.GetComponent<TMP_Text>().text = webRequest.error;
            }
        }
    }
}

using UnityEngine;

public class ServerConnection : MonoBehaviour {
    public string hostname;
    public static ServerConnection instance {get; private set;}

    public void Awake() { //Ensures there's only one ServerConnection object
        if(instance != null & instance != this){
            Destroy(this);
        } else {
            instance = this;
        }
    }
}

using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject player;
    public GameObject menuWelcome;

    void start(){

    }
    public void TeleportToPlayerPosition()
    {
        menuWelcome.transform.position = player.transform.position;
    }

}

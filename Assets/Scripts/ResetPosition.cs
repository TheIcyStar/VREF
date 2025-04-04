using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    public Transform GraphPrefab; 

    public void ResetObjectPosition()
    {
        GraphPrefab.position = Vector3.zero; 
    }
}

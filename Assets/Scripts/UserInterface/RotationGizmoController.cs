using UnityEngine;

public class RotationGizmoController : MonoBehaviour
{
    [SerializeField] private GraphInstance graphInstance;

    public void SetGraphRotation(Vector3 rotation) {
        graphInstance.GizmoRotateGraph(rotation);
    }
}

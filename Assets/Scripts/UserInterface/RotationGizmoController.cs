using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class RotationGizmoController : MonoBehaviour
{
    [SerializeField] private GraphInstance graphInstance;

    public void SetGraphRotation(Vector3 rotation) {
        graphInstance.GizmoRotateGraph(rotation);
        transform.localRotation = Quaternion.Euler(rotation);
    }
}

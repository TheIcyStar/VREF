using UnityEngine;

public class RotationGizmoFace : MonoBehaviour
{
    [SerializeField] private RotationGizmoController controller;
    [SerializeField] private Vector3 targetRotation;

    public void OnSelectEntered()
    {
        controller.SetGraphRotation(targetRotation);
    }
}

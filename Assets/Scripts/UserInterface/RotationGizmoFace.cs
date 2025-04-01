using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotationGizmoFace : MonoBehaviour
{
    [SerializeField] private RotationGizmoController controller;
    [SerializeField] private Vector3 targetRotation;

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        controller.SetGraphRotation(targetRotation);
    }
}

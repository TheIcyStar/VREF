using System.ComponentModel;
using UnityEngine;

public class RotationGizmoFace : MonoBehaviour
{
    [SerializeField] private RotationGizmoController controller;
    [SerializeField] private Vector3 targetRotation;
    [SerializeField] private MeshRenderer highlightRenderer;

    public void Awake()
    {
        highlightRenderer.enabled = false;   
    }

    public void OnSelectEntered()
    {
        controller.SetGraphRotation(targetRotation);
    }

    public void OnHoverEntered() 
    {
        highlightRenderer.enabled = true;
    }

    public void OnHoverExited()
    {
        highlightRenderer.enabled = false;
    }
}

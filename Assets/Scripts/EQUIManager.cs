using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EQUIManager : MonoBehaviour
{
    public Transform equationUI;
    public Transform playerHead;
    public float distance = 2f;
    public float height = 10f;
    private XRInput inputActions;
    private bool isUIActive = false;

    private void Awake()
    {
        inputActions = new XRInput();

        inputActions.XRActions.MoveUI.performed += ctx => MoveUIInFrontOfPlayer();
        inputActions.XRActions.ToggleUI.performed += ctx => ToggleUIVisibility();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }


    private void MoveUIInFrontOfPlayer()
    {
        Vector3 forward = playerHead.forward;

        // keeps the UI parallel with the floor
        forward.y = 0;
        forward.Normalize();

        equationUI.position = playerHead.position + forward * distance + Vector3.up * height;
        equationUI.position = new Vector3(equationUI.position.x, 1, equationUI.position.z);
        equationUI.LookAt(playerHead.position);
        equationUI.Rotate(0, 180, 0);
    }

    private void ToggleUIVisibility()
    {
        isUIActive = !isUIActive;
        equationUI.gameObject.SetActive(isUIActive);
    }
}

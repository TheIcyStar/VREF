using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EQUIManager : MonoBehaviour
{
    public Transform equationUI;
    public Transform playerHead;
    [SerializeField] public float distance = 2f;
    [SerializeField] public float height = 2.8f;
    private XRInput inputActions;
    private bool isUIActive = true;

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


    public void MoveUIInFrontOfPlayer()
    {
        if(!isUIActive) { ToggleUIVisibility(); }

        Vector3 forward = playerHead.forward;

        // keeps the UI parallel with the floor
        forward.y = 0;
        forward.Normalize();

        //equationUI.LookAt(playerHead.position);
        equationUI.position = playerHead.position + forward * distance + Vector3.up * height;
        equationUI.rotation = Quaternion.LookRotation(forward, Vector3.up);
        //equationUI.Rotate(0, 180, 0);
    }

    public void ToggleUIVisibility()
    {
        isUIActive = !isUIActive;
        equationUI.gameObject.SetActive(isUIActive);
    }
}

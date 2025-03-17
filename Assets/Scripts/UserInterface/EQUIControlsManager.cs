using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EQUIManager : MonoBehaviour
{
    // NOTE: VALUES IN THE INSPECTOR OVERRIDE THE VALUES IN CODE
    // I KEEP FORGETTING THIS, CHECK THE INSPECTOR IF SOMETHINGS NOT WORKING CORRECTLY

    // the players main camera
    public Transform playerHead;
    // distance away from the player the UI relocates to
    [SerializeField] public float distance = 2f;
    // the exact height the UI relocates to
    [SerializeField] public float height = 1.5f;
    // canvas group of the equation UI used for toggling visibility
    public CanvasGroup equiCanvasGroup;
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


    private void MoveUIInFrontOfPlayer()
    {
        if(!isUIActive) ToggleUIVisibility();

        // vector that points in the direction the player is looking
        Vector3 forward = playerHead.forward;

        // keeps the UI parallel with the floor
        forward.y = 0;
        forward.Normalize();

        // use the desired height and distance to place the UI directly infront of the player
        // playerHead position gives the player's location in world space
        // forward * distance gives direction and magnitude away from this location
        // for now height is not based on view direction, but easily could be toggleable later
        this.transform.position = new Vector3(playerHead.position.x + forward.x * distance, height, playerHead.position.z + forward.z * distance);
        this.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void ToggleUIVisibility()
    {
        isUIActive = !isUIActive;

        if (equiCanvasGroup != null)
        {
            equiCanvasGroup.alpha = isUIActive ? 1f : 0f;
            equiCanvasGroup.interactable = isUIActive;
            equiCanvasGroup.blocksRaycasts = isUIActive;
        }
    }
}

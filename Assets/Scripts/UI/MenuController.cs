//using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{

    public GameObject menuPanel;
    public InputActionReference openMenuAction;
    private void Awake()
    {
        openMenuAction.action.Enable();
        openMenuAction.action.performed += ToggleMenu;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy() {

        openMenuAction.action.Disable();
        openMenuAction.action.performed -= ToggleMenu;
        InputSystem.onDeviceChange -= OnDeviceChange;

    }
    private void ToggleMenu(InputAction.CallbackContext context) { 

        menuPanel.SetActive(!menuPanel.activeSelf); // the variable menuPanel of menuController has not been assigned.
    }
    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {

        switch (change) { 

        case InputDeviceChange.Disconnected:
            openMenuAction.action.Disable();
            openMenuAction.action.performed -= ToggleMenu;
            break;
        case InputDeviceChange.Reconnected:
            openMenuAction.action.Enable();
            openMenuAction.action.performed += ToggleMenu;
            break;
        }
    }
}
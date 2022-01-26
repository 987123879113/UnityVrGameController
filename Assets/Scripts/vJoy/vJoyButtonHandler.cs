using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class vJoyButtonHandler : XRBaseInteractable
{
    private vJoyController JoystickController;
    public int ButtonId;

    public Material SensorOnMaterial;
    public Material SensorOffMaterial;
    public MeshRenderer SensorLight;

    public void Start()
    {
        JoystickController = FindObjectOfType<vJoyController>();

        if (SensorLight == null) {
            SensorLight = GetComponent<MeshRenderer>();
        }

        SensorLight.material = SensorOffMaterial;
    }

    public void OnHoverEnterEvent(XRBaseInteractor interactor)
    {
        Debug.Log("Entered zone");
        SensorLight.material = SensorOnMaterial;
        JoystickController.SetButton(ButtonId, true);
    }

    public void OnHoverExitEvent(XRBaseInteractor interactor)
    {
        JoystickController.SetButton(ButtonId, false);
        SensorLight.material = SensorOffMaterial;
    }
}

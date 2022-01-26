using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using vJoyInterfaceWrap;

public class vJoyController : MonoBehaviour
{
    public vJoy joystick;
    private uint id = 1;

    private long axisMax = 0;

    List<UnityEngine.XR.InputDevice> _inputDevices;

    // Start is called before the first frame update
    void Start()
    {
        // Create one joystick object and a position structure.
        joystick = new vJoy();

        // Get the driver attributes (Vendor ID, Product ID, Version Number)
        if (!joystick.vJoyEnabled())
        {
            //Debug.LogFormat("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
            return;
        }
        else
        {
            //Debug.LogFormat("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());
        }

        // Get the state of the requested device
        VjdStat status = joystick.GetVJDStatus(id);
        /*
        switch (status)
        {
            case VjdStat.VJD_STAT_OWN:
                Debug.LogFormat("vJoy Device {0} is already owned by this feeder\n", id);
                break;
            case VjdStat.VJD_STAT_FREE:
                Debug.LogFormat("vJoy Device {0} is free\n", id);
                break;
            case VjdStat.VJD_STAT_BUSY:
                Debug.LogFormat("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                return;
            case VjdStat.VJD_STAT_MISS:
                Debug.LogFormat("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                return;
            default:
                Debug.LogFormat("vJoy Device {0} general error\nCannot continue\n", id);
                return;
        };
        */

        // Check which axes are supported
        bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
        bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
        bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
        bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
        bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
        // Get the number of buttons and POV Hat switchessupported by this vJoy device
        int nButtons = joystick.GetVJDButtonNumber(id);
        int ContPovNumber = joystick.GetVJDContPovNumber(id);
        int DiscPovNumber = joystick.GetVJDDiscPovNumber(id);

        // Print results
        /*
        Debug.LogFormat("\nvJoy Device {0} capabilities:\n", id);
        Debug.LogFormat("Numner of buttons\t\t{0}\n", nButtons);
        Debug.LogFormat("Numner of Continuous POVs\t{0}\n", ContPovNumber);
        Debug.LogFormat("Numner of Descrete POVs\t\t{0}\n", DiscPovNumber);
        Debug.LogFormat("Axis X\t\t{0}\n", AxisX ? "Yes" : "No");
        Debug.LogFormat("Axis Y\t\t{0}\n", AxisX ? "Yes" : "No");
        Debug.LogFormat("Axis Z\t\t{0}\n", AxisX ? "Yes" : "No");
        Debug.LogFormat("Axis Rx\t\t{0}\n", AxisRX ? "Yes" : "No");
        Debug.LogFormat("Axis Rz\t\t{0}\n", AxisRZ ? "Yes" : "No");
        */

        // Test if DLL matches the driver
        uint DllVer = 0, DrvVer = 0;
        bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
        if (match)
        {
            //Debug.LogFormat("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
        }
        else
        {
            //Debug.LogFormat("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);
        }

        // Acquire the target
        if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
        {
            //Debug.LogFormat("Failed to acquire vJoy device number {0}.\n", id);
            return;
        }
        else
        {
            //Debug.LogFormat("Acquired: vJoy device number {0}.\n", id);
        }

        joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref axisMax);

        //Debug.LogFormat("Axis max: {0}", axisMax);

        joystick.ResetVJD(id);

        _inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice, _inputDevices);
    }

    private void OnDestroy()
    {
        joystick.RelinquishVJD(id);
    }

    private int getXForButton(int button)
    {
        switch (button) {
            case 1: // Left
                return 0;
            case 2: // Left up
                return 0;
            case 3: // Up
                return (int)(axisMax / 2);
            case 4: // Right up
                return (int)axisMax;
            case 5: // Right
                return (int)axisMax;
            default:
                return (int)(axisMax / 2);
        }
    }

    private int getYForButton(int button)
    {
        switch (button)
        {
            case 1: // Left
                return (int)(axisMax / 2);
            case 2: // Left up
                return 0;
            case 3: // Up
                return 0;
            case 4: // Right up
                return 0;
            case 5: // Right
                return (int)(axisMax / 2);
            default:
                return (int)(axisMax / 2);
        }
    }

    uint buttons = 0;
    private void Update()
    {
        int axisX = (int)(axisMax / 2);
        int axisY = (int)(axisMax / 2);
        int axisXRot = (int)(axisMax / 2);
        int axisYRot = (int)(axisMax / 2);

        for (int i = 1; i < 6; i++) {
            if (((buttons >> i) & 1) != 0)
            {
                int stickL = i;
                int stickR = 0;

                for (int j = 1; j < 6; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    if (((buttons >> j) & 1) != 0)
                    {
                        stickR = j;
                        break;
                    }
                }

                axisX = getXForButton(stickL);
                axisY = getYForButton(stickL);

                axisXRot = getXForButton(stickR);
                axisYRot = getYForButton(stickR);

                break;
            }
        }

        joystick.SetAxis(axisX, id, HID_USAGES.HID_USAGE_X);
        joystick.SetAxis(axisY, id, HID_USAGES.HID_USAGE_Y);
        joystick.SetAxis(axisXRot, id, HID_USAGES.HID_USAGE_RX);
        joystick.SetAxis(axisYRot, id, HID_USAGES.HID_USAGE_RY);

        if (_inputDevices != null)
        {
            foreach (InputDevice inputDevice in _inputDevices)
            {
                bool button;

                if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left))
                {
                    // Left arrow
                    UnityEngine.XR.Interaction.Toolkit.InputHelpers.IsPressed(inputDevice, UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button.PrimaryButton, out button);
                    joystick.SetBtn(button, id, 1);

                    // X button
                    UnityEngine.XR.Interaction.Toolkit.InputHelpers.IsPressed(inputDevice, UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button.SecondaryButton, out button);
                    joystick.SetBtn(button, id, 3);
                }
                else if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Right))
                {
                    // Right arrow
                    UnityEngine.XR.Interaction.Toolkit.InputHelpers.IsPressed(inputDevice, UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button.PrimaryButton, out button);
                    joystick.SetBtn(button, id, 2);

                    // O button
                    UnityEngine.XR.Interaction.Toolkit.InputHelpers.IsPressed(inputDevice, UnityEngine.XR.Interaction.Toolkit.InputHelpers.Button.SecondaryButton, out button);
                    joystick.SetBtn(button, id, 4);
                }
            }
        }

        // Individual button presses
        for (int i = 10; i < 32; i++)
        {
            joystick.SetBtn(((buttons >> i) & 1) != 0, id, (uint)(i - 10 + 5));
        }
    }

    public void SetButton(int buttonId, bool value)
    {
        if (value)
        {
            buttons |= (uint)(1 << buttonId);
        }
        else
        {
            buttons &= (uint)~(1 << buttonId);
        }
    }
}

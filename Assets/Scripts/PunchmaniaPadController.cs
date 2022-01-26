using SimpleTcp;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PunchmaniaPadController : MonoBehaviour
{
    public Transform PadLeftTop;
    public Transform PadLeftMiddle;
    public Transform PadLeftBottom;
    public Transform PadRightTop;
    public Transform PadRightMiddle;
    public Transform PadRightBottom;

    public MeshRenderer PadLeftTopLed;
    public MeshRenderer PadLeftMiddleLed;
    public MeshRenderer PadLeftBottomLed;
    public MeshRenderer PadRightTopLed;
    public MeshRenderer PadRightMiddleLed;
    public MeshRenderer PadRightBottomLed;

    public Material PadLedOn;
    public Material PadLedOff;

    List<byte> buffer;
    bool[] lightStatusCurrent = {false, false, false, false, false, false};
    bool[] lightStatusNext = {false, false, false, false, false, false};

    int[] motorStatusCurrent = {0, 0, 0, 0, 0, 0};
    int[] motorStatusNext = {150, 150, 150, 150, 150, 150};

    void Start()
    {
        buffer = new List<byte>();
        SimpleTcpServer server = new SimpleTcpServer("127.0.0.1:8000");
        server.Events.DataReceived += DataReceived;
        server.Start();
    }

    void DataReceived(object sender, DataReceivedEventArgs e)
    {
        buffer.AddRange(e.Data);

        // Parse for new packets
        while (buffer.Count > 0) {
            if ((buffer[0] & 0xc0) == 0xc0) {
                var lights = buffer[0] & ~0xc0;

                for (int i = 0; i < 6; i++) {
                    lightStatusNext[i] = (lights & (1 << i)) != 0;
                }

                buffer.RemoveRange(0, 1);
            } else if (buffer[0] == 0x8f) {
                if (buffer.Count < 7) {
                    // Don't have full packet yet
                    break;
                }

                for (int i = 0; i < 6; i++) {
                    motorStatusNext[i] = buffer[i+1];
                }

                buffer.RemoveRange(0, 7);
            } else {
                // Unknown data
                Debug.Log(System.String.Format("Unknown byte {0:x2}", buffer[0]));
                buffer.RemoveRange(0, 1);
            }
        }
    }

    void Update()
    {
        MeshRenderer[] lights = {
            PadLeftTopLed,
            PadLeftMiddleLed,
            PadLeftBottomLed,
            PadRightTopLed,
            PadRightMiddleLed,
            PadRightBottomLed,
        };

        Transform[] motors = {
            PadLeftTop,
            PadLeftMiddle,
            PadLeftBottom,
            PadRightTop,
            PadRightMiddle,
            PadRightBottom,
        };

        for (int i = 0; i < lightStatusNext.Length; i++) {
            if (lightStatusNext[i] != lightStatusCurrent[i]) {
                lights[i].material = lightStatusNext[i] ? PadLedOn : PadLedOff;
                lightStatusCurrent[i] = lightStatusNext[i];
            }
        }

        for (int i = 0; i < motorStatusNext.Length; i++) {
            if (motorStatusNext[i] != motorStatusCurrent[i]) {
                // Rotate pad
                var rot = motorStatusNext[i] - 50.0f;

                if (rot < 0)
                    rot = 0;
                else if (rot > 100)
                    rot = 100;

                rot = 90.0f * (1.0f - (rot / 100.0f));

                if (i < 3)
                    rot *= -1;

                motors[i].localRotation = Quaternion.identity;
                motors[i].Rotate(0.0f, rot, 0.0f, Space.World);
                motorStatusCurrent[i] = motorStatusNext[i];
            }
        }
    }
}

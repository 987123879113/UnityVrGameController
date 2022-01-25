using UnityEngine;
using UnityEngine.UI;

public class WebcamRender : MonoBehaviour
{
    void Start()
    {
        var tex = new WebCamTexture("OBS-Camera");
        tex.Play();

        GetComponent<Renderer>().material.mainTexture = tex;
    }
}
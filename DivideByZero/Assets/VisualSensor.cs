using UnityEngine;
using System.Collections;

public class VisualSensor : MonoBehaviour
{

    public Texture2D VisionBox;
    public Camera VisionCamera;

    void OnPostRender()
    {

        VisionBox.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        VisionBox.Apply();
        
    }
}

using UnityEngine;
using System.Collections;

public class LowBitImageFX : MonoBehaviour
{
    private Material FXMaterial;

    void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        if (!FXMaterial)
        {
            FXMaterial = new Material( Shader.Find( "Shader Forge/16BitImage" ) );
        }

        //mat is the material containing your shader
        Graphics.Blit( source, destination, FXMaterial );
    }
}

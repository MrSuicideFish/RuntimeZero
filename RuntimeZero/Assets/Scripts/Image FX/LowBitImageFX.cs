using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LowBitImageFX : MonoBehaviour
{
    private Material FXMaterial;

    public float NumTiles = 1.0f;
    public Color EdgeColor;
    public float Threshold = 1.0f;

    void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        if (!FXMaterial)
        {
            FXMaterial = new Material( Shader.Find( "Hidden/LowBitOverlay" ) );
        }

        FXMaterial.SetFloat( "_NumTiles", NumTiles);
        FXMaterial.SetColor( "_EdgeColor", EdgeColor);
        FXMaterial.SetFloat( "_Threshhold", Threshold );

        //mat is the material containing your shader
        Graphics.Blit( source, Camera.main.targetTexture, FXMaterial );
    }
}

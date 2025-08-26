#pragma warning disable CS0649
	
using UnityEngine;
	//Hi
public class GBRender : MonoBehaviour
{
    public int resolutionX = 160, resolutionY = 144;
    private RenderTexture renderTexture;
    public Material Renderer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(this.renderTexture == null)
        {
            this.renderTexture = new RenderTexture(resolutionX, resolutionY, 24);
            this.renderTexture.enableRandomWrite = true;
            this.renderTexture.filterMode = FilterMode.Point;
            this.renderTexture.wrapMode = TextureWrapMode.Clamp;
            this.renderTexture.antiAliasing = 1;
            this.renderTexture.useMipMap = false;
            this.renderTexture.Create();

            if (this.Renderer != null)
            {
                this.Renderer.mainTexture = this.renderTexture;
            }
        }
        
        Graphics.Blit(source, this.renderTexture);
        //Graphics.Blit(this.renderTexture, destination);
    }
}
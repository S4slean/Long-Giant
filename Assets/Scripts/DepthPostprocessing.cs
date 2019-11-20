using UnityEngine;

public class DepthPostprocessing : MonoBehaviour
{
    [SerializeField]
    private Material postprocessMaterial;
    [SerializeField]
    private float waveSpeed;
    [SerializeField]
    public bool waveActive;

    private float waveDistance;

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
    }

    private void Update()
    {
        if (waveActive)
        {
            waveDistance = waveDistance + waveSpeed * Time.deltaTime;
        }
        else
        {
            waveDistance = 0;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        postprocessMaterial.SetFloat("_WaveDistance", waveDistance);
        Graphics.Blit(source, destination, postprocessMaterial);
    }
}
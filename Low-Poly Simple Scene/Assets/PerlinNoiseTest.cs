using UnityEngine;
using System.Collections;

public class PerlinNoiseTest : MonoBehaviour {
    
    public int width = 256;
    public int height = 256;

    public float scale = 20f;

    public float scrollSpeed = 0.03f;

    private float xOrg = 0f;
    private float yOrg = 0f;

    private Texture2D noiseTex;
    private Renderer rend;

    void Start() {
        rend = GetComponent<Renderer>();

        noiseTex = new Texture2D(width, height);
        rend.material.mainTexture = noiseTex;
    }

    void Update() {
        for(int y=0; y<height; y++)
        {
            for(int x=0; x<height; x++)
            {
                noiseTex.SetPixel(x, y, CalculateColor(x, y));
            }
        }

        noiseTex.Apply();

        xOrg += scrollSpeed;
        yOrg += scrollSpeed;
    }

    Color CalculateColor(int x, int y) {
        float xCoord = xOrg + (float)x / width * scale;
        float yCoord = yOrg + (float)y / height * scale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);

        return new Color(sample, sample, sample);
    }
}
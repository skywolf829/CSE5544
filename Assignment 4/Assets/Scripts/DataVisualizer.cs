using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DataVisualizer : MonoBehaviour
{
    public TextAsset t;
    public Mesh arrowMesh;
    public GameObject TMProPrefab;
    public Gradient gradient;
    public int samplesPerRow, samplesPerCol;
    float[] info;
    int xSize, ySize;
    ParticleSystem.Particle[] particles;
    Vector2[] positions;
    Vector2[] directions;
    Vector2[,] positions2D;
    Vector2[,] directions2D;

    GameObject textureVis;
    bool grabbing = false;
    Vector3 lastMousePos;
    // Start is called before the first frame update
    void Start()
    {
        DataImporter.GetXY(t, out xSize, out ySize);
        info = DataImporter.LoadData(t, out positions, out directions);
        DataImporter.LoadData(t, out positions2D, out directions2D);
        print("xSize " + xSize);
        print("ySize " + ySize);
        print("Xmin " + info[0]);
        print("Xmax " + info[1]);
        print("Ymin " + info[2]);
        print("Ymax " + info[3]);
        print("MinMag " + info[4]);
        print("MaxMag " + info[5]);
        GetComponent<ParticleSystemRenderer>().mesh = arrowMesh;
        CreateParticleVisualization(samplesPerRow, samplesPerCol);
        CreateTextureVisualization();
    }
    void CreateTextureVisualization()
    {
        Texture2D texture = new Texture2D(xSize, ySize, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        Color[] colors = new Color[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = gradient.Evaluate(Mathf.InverseLerp(info[4], info[5], directions[i].magnitude));
        }
        texture.SetPixels(colors);
        texture.Apply(false);
        
        textureVis = new GameObject
        {
            name = "Sprite"
        };
        textureVis.transform.localScale = Vector3.one;
        textureVis.AddComponent<SpriteRenderer>();
        Sprite s = Sprite.Create(texture, new Rect(0, 0, xSize, ySize), Vector2.zero, xSize / (info[1] - info[0]));
        textureVis.transform.position = new Vector2(-(info[1] - info[0]) / 2f, -(info[3] - info[2]) / 2f);
        textureVis.GetComponent<SpriteRenderer>().sprite = s;
    }
    void CreateParticleVisualization(int particlesPerRow, int particlesPerColumn)
    {
        ParticleSystem.MainModule mainModule = GetComponent<ParticleSystem>().main;
        mainModule.maxParticles = particlesPerRow * particlesPerColumn;
        particles = new ParticleSystem.Particle[particlesPerColumn * particlesPerRow];
        int particleSpot = 0;
        GameObject g = new GameObject();
        for (int r = 0; r < particlesPerRow; r++)
        {
            for (int c = 0; c < particlesPerColumn; c++)
            {
                int row = (int)(((float)r / (particlesPerRow-1)) * (xSize - 1));
                int col = (int)(((float)c / (particlesPerColumn-1)) * (ySize-1));
                particles[particleSpot].position = positions2D[row, col];
                g.transform.position = positions2D[row, col];
                g.transform.LookAt(positions2D[row, col] + directions2D[row, col]);
                particles[particleSpot].rotation3D = g.transform.eulerAngles;
                particles[particleSpot].startSize = Mathf.Min((1f * xSize / particlesPerRow), (1f * ySize / particlesPerColumn))
                    * Mathf.InverseLerp(info[4], info[5], directions2D[row, col].magnitude);
                particles[particleSpot].startColor = 
                    gradient.Evaluate(Mathf.InverseLerp(info[4], info[5], directions2D[row, col].magnitude)); 
                particleSpot++;
            }
        }
        Destroy(g);        
        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
    }

    public void ToggleTexture()
    {
        if (textureVis)
        {
            textureVis.SetActive(!textureVis.activeInHierarchy);
        }
    }
    public void ToggleFlow()
    {
        if (GetComponent<ParticleSystemRenderer>())
        {
            GetComponent<ParticleSystemRenderer>().enabled = !GetComponent<ParticleSystemRenderer>().enabled;

        }
    }

    public void NumColsChanged(string s)
    {
        int x;
        if(int.TryParse(s, out x))
        {
            samplesPerRow = Mathf.Clamp(x, 1, xSize);
        }
        CreateParticleVisualization(samplesPerRow, samplesPerCol);
    }
    public void NumRowsChanged(string s)
    {
        int y;
        if (int.TryParse(s, out y))
        {
            samplesPerCol = Mathf.Clamp(y, 1, ySize);
        }
        CreateParticleVisualization(samplesPerRow, samplesPerCol);
    }

    private void Update()
    {
        float d = Input.GetAxis("Mouse ScrollWheel");
        if (d < 0)
        {
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + 0.2f, 0.1f, 1.5f);
        }
        else if (d > 0)
        {
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - 0.2f, 0.1f, 1.5f);
        }
        if (Input.GetMouseButton(2))
        {
            if (grabbing)
            {
                Camera.main.transform.position += (lastMousePos - Input.mousePosition) * 0.001f;
            }
            lastMousePos = Input.mousePosition;
            grabbing = true;
        }
        else
        {
            grabbing = false;
            lastMousePos = Input.mousePosition;
        }
    }
}

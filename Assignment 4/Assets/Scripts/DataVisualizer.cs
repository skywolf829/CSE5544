using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ParticleSystem))]
public class DataVisualizer : MonoBehaviour
{
    public TextAsset t;
    public Mesh arrowMesh;
    public GameObject TMProPrefab;
    public Gradient gradient;
    public int samplesPerRow, samplesPerCol;

    public GameObject gradientKnobPrefab;
    public GameObject gradientDisplay;
    public GameObject colorWheel;
    public GameObject RedImage, GreenImage, BlueImage, AlphaImage, ColorBoxImage;

    List<GameObject> gradientColorKnobs;
    List<GameObject> gradientAlphaKnobs;
    List<GradientColorKey> gradientColorKeys;
    List<GradientAlphaKey> gradientAlphaKeys;
    GameObject knobSelected = null;
    GameObject knobHolding = null;

    float[] info;
    int xSize, ySize;
    ParticleSystem.Particle[] particles;
    Vector2[] positions;
    Vector2[] directions;
    Vector2[,] positions2D;
    Vector2[,] directions2D;
    bool jitterEnabled = false;

    GameObject textureVis;
    bool grabbing = false;
    bool holdingRedSlider, holdingGreenSlider, holdingBlueSlider, holdingAlphaSlider;
    bool holdingColorSquare, holdingColorCircle;
    Vector3 lastMousePos;

    GraphicRaycaster graphicRaycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;

    // Start is called before the first frame update
    void Start()
    {
        graphicRaycaster = GameObject.FindWithTag("Canvas").GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.FindWithTag("Canvas").GetComponent<EventSystem>();

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
        CreateGradientTextureFromGradient();
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
    void UpdateTextureVisualizationGradient()
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


        Sprite s = Sprite.Create(texture, new Rect(0, 0, xSize, ySize), Vector2.zero, xSize / (info[1] - info[0]));
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
                int row = (int)(((float)r / (particlesPerRow - 1)) * (xSize - 1));
                int col = (int)(((float)c / (particlesPerColumn - 1)) * (ySize - 1));
                int nextRow = (int)(((float)(r + 1) / (particlesPerRow - 1)) * (xSize - 1));
                int prevRow = (int)(((float)(r - 1) / (particlesPerRow - 1)) * (xSize - 1));
                int nextCol = (int)(((float)(c + 1) / (particlesPerColumn - 1)) * (ySize - 1));
                int prevCol = (int)(((float)(c - 1) / (particlesPerColumn - 1)) * (ySize - 1));

                if (!jitterEnabled) { 
                    particles[particleSpot].position = positions2D[row, col];
                    g.transform.position = positions2D[row, col];
                    g.transform.LookAt(positions2D[row, col] + directions2D[row, col]);
                    particles[particleSpot].rotation3D = g.transform.eulerAngles;
                    particles[particleSpot].startSize = Mathf.Min((1f * xSize / particlesPerRow), (1f * ySize / particlesPerColumn))
                        * Mathf.InverseLerp(info[4], info[5], directions2D[row, col].magnitude);
                    particles[particleSpot].startColor =
                        //gradient.Evaluate(Mathf.InverseLerp(info[4], info[5], directions2D[row, col].magnitude));
                        Color.black;

                }
                else
                {
                    Vector2 randomJitter = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                    Vector2 newPos = positions2D[row, col];
                    Vector2 newDir = directions2D[row, col];

                    if(randomJitter.x > 0 && randomJitter.y > 0)
                    {
                        
                        int numAveraged = 1;
                        if(c < particlesPerColumn - 1)
                        {
                            newPos = new Vector2(newPos.x, Mathf.Lerp(positions2D[row, col].y, positions2D[row, nextCol].y, randomJitter.y));
                            newDir += directions2D[row, nextCol];
                            numAveraged++;
                        }
                        if(r < particlesPerRow - 1)
                        {

                            newPos = new Vector2(Mathf.Lerp(positions2D[row, col].x, positions2D[nextRow, col].x, randomJitter.x), newPos.y);
                            newDir += directions2D[nextRow, col];
                            numAveraged++;
                        }
                        if(c < particlesPerColumn - 1 && r < particlesPerRow - 1)
                        {
                            newDir += directions2D[nextRow, nextCol];
                            numAveraged++;
                        }
                        newDir /= numAveraged;
                    }
                    else if(randomJitter.x > 0 && randomJitter.y < 0)
                    {
                        int numAveraged = 1;
                        if (c < particlesPerColumn - 1)
                        {
                            newPos = new Vector2(newPos.x, Mathf.Lerp(positions2D[row, col].y, positions2D[row, nextCol].y, randomJitter.y));
                            newDir += directions2D[row, nextCol];
                            numAveraged++;
                        }
                        if (r > 0)
                        {

                            newPos = new Vector2(Mathf.Lerp(positions2D[row, col].x, positions2D[prevRow, col].x, -randomJitter.x), newPos.y);
                            newDir += directions2D[prevRow, col];
                            numAveraged++;
                        }
                        if (c < particlesPerColumn - 1 && r > 0)
                        {
                            newDir += directions2D[prevRow, nextCol];
                            numAveraged++;
                        }
                        newDir /= numAveraged;
                    }
                    else if(randomJitter.x < 0 && randomJitter.y < 0)
                    {
                        int numAveraged = 1;
                        if (c > 0)
                        {
                            newPos = new Vector2(newPos.x, Mathf.Lerp(positions2D[row, col].y, positions2D[row, prevCol].y, -randomJitter.y));
                            newDir += directions2D[row, prevCol];
                            numAveraged++;
                        }
                        if (r > 0)
                        {

                            newPos = new Vector2(Mathf.Lerp(positions2D[row, col].x, positions2D[prevRow, col].x, -randomJitter.x), newPos.y);
                            newDir += directions2D[prevRow, col];
                            numAveraged++;
                        }
                        if (c > 0 && r > 0)
                        {
                            newDir += directions2D[prevRow, prevCol];
                            numAveraged++;
                        }
                        newDir /= numAveraged;
                    }
                    else if(randomJitter.x < 0 && randomJitter.y > 0)
                    {
                        int numAveraged = 1;
                        if (c > 0)
                        {
                            newPos = new Vector2(newPos.x, Mathf.Lerp(positions2D[row, col].y, positions2D[row, prevCol].y, -randomJitter.y));
                            newDir += directions2D[row, prevCol];
                            numAveraged++;
                        }
                        if (r < particlesPerRow - 1)
                        {

                            newPos = new Vector2(Mathf.Lerp(positions2D[row, col].x, positions2D[nextRow, col].x, randomJitter.x), newPos.y);
                            newDir += directions2D[nextRow, col];
                            numAveraged++;
                        }
                        if (c > 0 && r < particlesPerRow - 1)
                        {
                            newDir += directions2D[nextRow, prevCol];
                            numAveraged++;
                        }
                        newDir /= numAveraged;
                    }
                    
                    particles[particleSpot].position = newPos;
                    g.transform.position = newPos;
                    g.transform.LookAt(newPos + newDir);
                    particles[particleSpot].rotation3D = g.transform.eulerAngles;
                    particles[particleSpot].startSize = Mathf.Min((1f * xSize / particlesPerRow), (1f * ySize / particlesPerColumn))
                        * Mathf.InverseLerp(info[4], info[5], newDir.magnitude);
                    particles[particleSpot].startColor =
                        //gradient.Evaluate(Mathf.InverseLerp(info[4], info[5], newDir.magnitude));
                        Color.black;
                }
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
            samplesPerRow = Mathf.Clamp(x, 2, xSize);
        }
        CreateParticleVisualization(samplesPerRow, samplesPerCol);
    }
    public void NumRowsChanged(string s)
    {
        int y;
        if (int.TryParse(s, out y))
        {
            samplesPerCol = Mathf.Clamp(y, 2, ySize);
        }
        CreateParticleVisualization(samplesPerRow, samplesPerCol);
    }
    public void ToggleJitter()
    {
        jitterEnabled = !jitterEnabled;
        CreateParticleVisualization(samplesPerRow, samplesPerCol);
    }
    void UpdateGradient()
    {
        gradient.SetKeys(gradientColorKeys.ToArray(), gradientAlphaKeys.ToArray());
        UpdateGradientTexture();
    }
    void UpdateGradientTexture()
    {
        Texture2D gradientTexture = new Texture2D((int)gradientDisplay.GetComponent<RectTransform>().rect.width,
            (int)gradientDisplay.GetComponent<RectTransform>().rect.height, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };

        Color[] c = new Color[gradientTexture.height * gradientTexture.width];
        for (int x = 0; x < gradientTexture.width; x++)
        {            
            for(int y = 0; y < gradientTexture.height; y++)
            {
                c[x + y * gradientTexture.width] = gradient.Evaluate((float)x / gradientTexture.width); 
            }            
        }
        gradientTexture.SetPixels(c);
        gradientTexture.Apply(false);
        gradientDisplay.GetComponent<RawImage>().texture = gradientTexture;
    }
    void CreateGradientTextureFromGradient()
    {
        gradientColorKeys = new List<GradientColorKey>();
        gradientColorKnobs = new List<GameObject>();
        gradientAlphaKeys = new List<GradientAlphaKey>();
        gradientAlphaKnobs = new List<GameObject>();

        for(int i = 0; i < gradient.colorKeys.Length; i++)
        {
            gradientColorKeys.Add(gradient.colorKeys[i]);
            GameObject knob = GameObject.Instantiate(gradientKnobPrefab);
            knob.tag = "GradientKnob";
            knob.transform.SetParent(gradientDisplay.transform);
            knob.GetComponent<RectTransform>().localPosition = new Vector2(
                Mathf.Lerp(-gradientDisplay.GetComponent<RectTransform>().rect.width / 2f, gradientDisplay.GetComponent<RectTransform>().rect.width / 2f, gradient.colorKeys[i].time),
                - gradientDisplay.GetComponent<RectTransform>().rect.height / 2f - 
                knob.GetComponent<RectTransform>().rect.height / 2f);
            gradientColorKnobs.Add(knob);
        }
        for (int i = 0; i < gradient.alphaKeys.Length; i++)
        {
            gradientAlphaKeys.Add(gradient.alphaKeys[i]);
            GameObject knob = GameObject.Instantiate(gradientKnobPrefab);
            knob.tag = "GradientKnob";
            knob.transform.SetParent(gradientDisplay.transform);
            knob.GetComponent<RectTransform>().localPosition = new Vector2(
                Mathf.Lerp(-gradientDisplay.GetComponent<RectTransform>().rect.width / 2f, gradientDisplay.GetComponent<RectTransform>().rect.width / 2f, gradient.alphaKeys[i].time),
                gradientDisplay.GetComponent<RectTransform>().rect.height / 2f +
                knob.GetComponent<RectTransform>().rect.height / 2f);
            gradientAlphaKnobs.Add(knob);
        }

        UpdateGradient();
    }
    void UpdateRGBAGradients(GradientAlphaKey key)
    {
        Gradient redGradient = new Gradient();
        Gradient greenGradient = new Gradient();
        Gradient blueGradient = new Gradient();
        Gradient alphaGradient = new Gradient();

        redGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        redGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0),
            new GradientColorKey(Color.white, 1)
        };
        greenGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        greenGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0),
            new GradientColorKey(Color.white, 1)
        };

        blueGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        blueGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0),
            new GradientColorKey(Color.white, 1)
        };

        alphaGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(0, 0),
            new GradientAlphaKey(1, 1)
        };
        alphaGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.white, 0),
            new GradientColorKey(Color.white, 1)
        };

        RedImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)RedImage.GetComponent<RectTransform>().rect.width,
            (int)RedImage.GetComponent<RectTransform>().rect.height, redGradient);
        GreenImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)GreenImage.GetComponent<RectTransform>().rect.width,
            (int)GreenImage.GetComponent<RectTransform>().rect.height, greenGradient);
        BlueImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)BlueImage.GetComponent<RectTransform>().rect.width,
            (int)BlueImage.GetComponent<RectTransform>().rect.height, blueGradient);
        AlphaImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)AlphaImage.GetComponent<RectTransform>().rect.width,
            (int)AlphaImage.GetComponent<RectTransform>().rect.height, alphaGradient);
        ColorBoxImage.GetComponent<RawImage>().texture = CreateSquareMapTexture((int)ColorBoxImage.GetComponent<RectTransform>().rect.width,
            (int)ColorBoxImage.GetComponent<RectTransform>().rect.height, Color.white);

        RedImage.transform.GetChild(0).localPosition = new Vector2(RedImage.transform.GetComponent<RectTransform>().rect.width / 2f, 0);
        GreenImage.transform.GetChild(0).localPosition = new Vector2(GreenImage.transform.GetComponent<RectTransform>().rect.width / 2f, 0);
        BlueImage.transform.GetChild(0).localPosition = new Vector2(BlueImage.transform.GetComponent<RectTransform>().rect.width / 2f, 0);
        AlphaImage.transform.GetChild(0).localPosition = new Vector2(Mathf.Lerp(-AlphaImage.transform.GetComponent<RectTransform>().rect.width / 2f,
            AlphaImage.transform.GetComponent<RectTransform>().rect.width / 2f, key.alpha), 0);
    }
    void UpdateRGBAGradients(GradientColorKey key)
    {
        Gradient redGradient = new Gradient();
        Gradient greenGradient = new Gradient();
        Gradient blueGradient = new Gradient();
        Gradient alphaGradient = new Gradient();

        redGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        redGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0, key.color.g, key.color.b), 0),
            new GradientColorKey(new Color(1, key.color.g, key.color.b), 1)
        };
        greenGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        greenGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(key.color.r, 0, key.color.b), 0),
            new GradientColorKey(new Color(key.color.r, 1, key.color.b), 1)
        };

        blueGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1, 0),
            new GradientAlphaKey(1, 1)
        };
        blueGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(key.color.r, key.color.g, 0), 0),
            new GradientColorKey(new Color(key.color.r, key.color.g, 1), 1)
        };

        alphaGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(0, 0),
            new GradientAlphaKey(1, 1)
        };
        alphaGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(key.color, 0),
            new GradientColorKey(key.color, 1)
        };

        RedImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)RedImage.GetComponent<RectTransform>().rect.width,
            (int)RedImage.GetComponent<RectTransform>().rect.height, redGradient);
        GreenImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)GreenImage.GetComponent<RectTransform>().rect.width,
            (int)GreenImage.GetComponent<RectTransform>().rect.height, greenGradient);
        BlueImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)BlueImage.GetComponent<RectTransform>().rect.width,
            (int)BlueImage.GetComponent<RectTransform>().rect.height, blueGradient);
        AlphaImage.GetComponent<RawImage>().texture = CreateTextureFromGradient((int)AlphaImage.GetComponent<RectTransform>().rect.width,
            (int)AlphaImage.GetComponent<RectTransform>().rect.height, alphaGradient);
        ColorBoxImage.GetComponent<RawImage>().texture = CreateSquareMapTexture((int)ColorBoxImage.GetComponent<RectTransform>().rect.width,
            (int)ColorBoxImage.GetComponent<RectTransform>().rect.height, key.color);

        RedImage.transform.GetChild(0).localPosition = new Vector2(Mathf.Lerp(-RedImage.transform.GetComponent<RectTransform>().rect.width / 2f,
            RedImage.transform.GetComponent<RectTransform>().rect.width / 2f, key.color.r), 0);
        GreenImage.transform.GetChild(0).localPosition = new Vector2(Mathf.Lerp(-GreenImage.transform.GetComponent<RectTransform>().rect.width / 2f,
            GreenImage.transform.GetComponent<RectTransform>().rect.width / 2f, key.color.g), 0);
        BlueImage.transform.GetChild(0).localPosition = new Vector2(Mathf.Lerp(-BlueImage.transform.GetComponent<RectTransform>().rect.width / 2f,
            BlueImage.transform.GetComponent<RectTransform>().rect.width / 2f, key.color.b), 0);
        AlphaImage.transform.GetChild(0).localPosition = new Vector2(Mathf.Lerp(-AlphaImage.transform.GetComponent<RectTransform>().rect.width / 2f,
            AlphaImage.transform.GetComponent<RectTransform>().rect.width / 2f, key.color.a), 0);

    }    
    void UpdateColorWheelPlacement()
    {
        if (!gradientColorKnobs.Contains(knobSelected)) return;
        int index = gradientColorKnobs.IndexOf(knobSelected);
        float h, s, v;
        Color.RGBToHSV(gradientColorKeys[index].color, out h, out s, out v);
        Vector2 direction = new Vector2(Mathf.Cos(2 * h * Mathf.PI), Mathf.Sin(2 * h * Mathf.PI)).normalized;
        colorWheel.transform.GetChild(0).localPosition = direction *
            colorWheel.transform.GetChild(1).GetComponent<RectTransform>().rect.width / 2f;
        colorWheel.transform.GetChild(0).localEulerAngles =
            new Vector3(0, 0, h * 360);
    }
    void UpdateColorSquarePlacement()
    {
        if (!gradientColorKnobs.Contains(knobSelected)) return;
        int index = gradientColorKnobs.IndexOf(knobSelected);
        float h, s, v;
        Color.RGBToHSV(gradientColorKeys[index].color, out h, out s, out v);
        ColorBoxImage.transform.GetChild(0).localPosition = new Vector2(
             Mathf.Lerp(-ColorBoxImage.GetComponent<RectTransform>().rect.width / 2f, 
             ColorBoxImage.GetComponent<RectTransform>().rect.width / 2f,
             s),
             Mathf.Lerp(-ColorBoxImage.GetComponent<RectTransform>().rect.height / 2f,
             ColorBoxImage.GetComponent<RectTransform>().rect.height / 2f,
             v)
            );
    }
    Texture2D CreateTextureFromGradient(int width, int height, Gradient g)
    {
        Texture2D gradientTexture = new Texture2D(width, height, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };

        Color[] c = new Color[gradientTexture.height * gradientTexture.width];
        for (int x = 0; x < gradientTexture.width; x++)
        {
            for (int y = 0; y < gradientTexture.height; y++)
            {
                c[x + y * gradientTexture.width] = g.Evaluate((float)x / gradientTexture.width);
            }
        }
        gradientTexture.SetPixels(c);
        gradientTexture.Apply(false);
        return gradientTexture;
    }
    Texture2D CreateSquareMapTexture(int width, int height, Color c)
    {
        Texture2D gradientTexture = new Texture2D(width, height, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        float h, s, v;
        Color.RGBToHSV(c, out h, out s, out v);
        Color fullHue = Color.HSVToRGB(h, 1, 1);
        Color[] colors = new Color[gradientTexture.height * gradientTexture.width];
        for (int x = 0; x < gradientTexture.width; x++)
        {
            for (int y = 0; y < gradientTexture.height; y++)
            {
                colors[x + y * gradientTexture.width] =
                    Color.Lerp(Color.Lerp(Color.white, fullHue, (float)x / gradientTexture.width),
                    Color.black, 1f - (float)y / gradientTexture.height);
            }
        }
        gradientTexture.SetPixels(colors);
        gradientTexture.Apply(false);
        return gradientTexture;
    }

    private void UpdateDisplay()
    {
        UpdateGradient();
        if (knobSelected)
        {
            UpdateColorSquarePlacement();
            UpdateColorWheelPlacement();
        }
        if(knobSelected && gradientAlphaKnobs.Contains(knobSelected))
        {
            int index = gradientAlphaKnobs.IndexOf(knobSelected);
            UpdateRGBAGradients(gradientAlphaKeys[index]);
        }
        if (knobSelected && gradientColorKnobs.Contains(knobSelected))
        {
            int index = gradientColorKnobs.IndexOf(knobSelected);
            UpdateRGBAGradients(gradientColorKeys[index]);
        }
        UpdateTextureVisualizationGradient();
    }
    private void UpdateCamera()
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
            grabbing = true;
        }
        else
        {
            grabbing = false;
        }
    }
    private void UpdateUIControls()
    {
        bool needToUpdateDisplay = false;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);
            bool createNewAlphaNode = false, createNewColorNode = false;
            bool moveRed = false, moveGreen = false, moveBlue = false, moveAlpha = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.tag == "GradientKnob")
                {
                    knobHolding = result.gameObject;
                    knobSelected = result.gameObject;
                }
                if (result.gameObject.name == "AlphaGradientBackground") createNewAlphaNode = true;
                if (result.gameObject.name == "ColorGradientBackground") createNewColorNode = true;
                if (result.gameObject == RedImage) moveRed = true;
                if (result.gameObject == GreenImage) moveGreen = true;
                if (result.gameObject == BlueImage) moveBlue = true;
                if (result.gameObject == AlphaImage) moveAlpha = true;
                if (result.gameObject == ColorBoxImage) holdingColorSquare = true;
                if (result.gameObject == colorWheel) holdingColorCircle = true;
            }
            if (!knobHolding)
            {
                if (createNewColorNode && gradientColorKeys.Count < 8)
                {
                    GameObject knob = GameObject.Instantiate(gradientKnobPrefab);
                    knob.tag = "GradientKnob";
                    knob.transform.SetParent(gradientDisplay.transform);
                    knob.transform.localPosition = new Vector2(-gradientDisplay.transform.position.x + Input.mousePosition.x,
                        -gradientDisplay.GetComponent<RectTransform>().rect.height / 2f
                        - knob.GetComponent<RectTransform>().rect.height / 2f);
                    gradientColorKnobs.Add(knob);
                    float t = Mathf.InverseLerp(
                        -gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                        gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                        knob.transform.localPosition.x);
                    GradientColorKey gck = new GradientColorKey(gradient.Evaluate(t), t);
                    gradientColorKeys.Add(gck);
                    knobSelected = knob;
                    needToUpdateDisplay = true;
                }
                else if (createNewAlphaNode && gradientAlphaKeys.Count < 8)
                {
                    GameObject knob = GameObject.Instantiate(gradientKnobPrefab);
                    knob.tag = "GradientKnob";
                    knob.transform.SetParent(gradientDisplay.transform);
                    knob.transform.localPosition = new Vector2(-gradientDisplay.transform.position.x + Input.mousePosition.x,
                        +gradientDisplay.GetComponent<RectTransform>().rect.height / 2f
                        + knob.GetComponent<RectTransform>().rect.height / 2f);
                    gradientAlphaKnobs.Add(knob);
                    float t = Mathf.InverseLerp(
                        -gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                        gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                        knob.transform.localPosition.x);
                    GradientAlphaKey gck = new GradientAlphaKey(gradient.Evaluate(t).a, t);
                    gradientAlphaKeys.Add(gck);
                    needToUpdateDisplay = true;
                }
                else if (moveRed && gradientColorKnobs.Contains(knobSelected))
                {
                    holdingRedSlider = true;
                    int index = gradientColorKnobs.IndexOf(knobSelected);
                    float newR = Mathf.InverseLerp(
                        -RedImage.GetComponent<RectTransform>().rect.width / 2f,
                        RedImage.GetComponent<RectTransform>().rect.width / 2f,
                        -RedImage.transform.position.x + Input.mousePosition.x);
                    RedImage.transform.GetChild(0).localPosition = new Vector2(
                        Mathf.Lerp(-RedImage.GetComponent<RectTransform>().rect.width / 2f,
                        RedImage.GetComponent<RectTransform>().rect.width / 2f, newR), 0);
                    gradientColorKeys[index] = new GradientColorKey(
                        new Color(newR, gradientColorKeys[index].color.g, gradientColorKeys[index].color.b),
                        gradientColorKeys[index].time);
                    needToUpdateDisplay = true;
                }
                else if (moveGreen && gradientColorKnobs.Contains(knobSelected))
                {
                    holdingGreenSlider = true;
                    int index = gradientColorKnobs.IndexOf(knobSelected);
                    float newG = Mathf.InverseLerp(
                        -GreenImage.GetComponent<RectTransform>().rect.width / 2f,
                        GreenImage.GetComponent<RectTransform>().rect.width / 2f,
                        -GreenImage.transform.position.x + Input.mousePosition.x);
                    GreenImage.transform.GetChild(0).localPosition = new Vector2(
                        Mathf.Lerp(-GreenImage.GetComponent<RectTransform>().rect.width / 2f,
                        GreenImage.GetComponent<RectTransform>().rect.width / 2f, newG), 0);
                    gradientColorKeys[index] = new GradientColorKey(
                        new Color(gradientColorKeys[index].color.r, newG, gradientColorKeys[index].color.b),
                        gradientColorKeys[index].time);
                    needToUpdateDisplay = true;
                }
                else if (moveBlue && gradientColorKnobs.Contains(knobSelected))
                {
                    holdingBlueSlider = true;
                    int index = gradientColorKnobs.IndexOf(knobSelected);
                    float newB = Mathf.InverseLerp(
                        -BlueImage.GetComponent<RectTransform>().rect.width / 2f,
                        BlueImage.GetComponent<RectTransform>().rect.width / 2f,
                        -BlueImage.transform.position.x + Input.mousePosition.x);
                    BlueImage.transform.GetChild(0).localPosition = new Vector2(
                        Mathf.Lerp(-BlueImage.GetComponent<RectTransform>().rect.width / 2f,
                        BlueImage.GetComponent<RectTransform>().rect.width / 2f, newB), 0);
                    gradientColorKeys[index] = new GradientColorKey(
                        new Color(gradientColorKeys[index].color.r, gradientColorKeys[index].color.g, newB),
                        gradientAlphaKeys[index].time);
                    needToUpdateDisplay = true;
                }
                else if (moveAlpha && gradientAlphaKnobs.Contains(knobSelected))
                {
                    holdingAlphaSlider = true;
                    int index = gradientAlphaKnobs.IndexOf(knobSelected);
                    float newA = Mathf.InverseLerp(
                        -AlphaImage.GetComponent<RectTransform>().rect.width / 2f,
                        AlphaImage.GetComponent<RectTransform>().rect.width / 2f,
                        -AlphaImage.transform.position.x + Input.mousePosition.x);
                    AlphaImage.transform.GetChild(0).localPosition = new Vector2(
                        Mathf.Lerp(-AlphaImage.GetComponent<RectTransform>().rect.width / 2f,
                        AlphaImage.GetComponent<RectTransform>().rect.width / 2f, newA), 0);
                    gradientAlphaKeys[index] = new GradientAlphaKey(newA,
                        gradientAlphaKeys[index].time);
                    needToUpdateDisplay = true;
                }
                if (holdingColorCircle && holdingColorSquare) holdingColorCircle = false;
            }

            else
            {
                if (gradientColorKnobs.Contains(knobHolding))
                {
                    needToUpdateDisplay = true;
                }
                if (gradientAlphaKnobs.Contains(knobHolding))
                {
                    needToUpdateDisplay = true;
                }
            }
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (knobHolding)
            {
                knobHolding.GetComponent<RectTransform>().localPosition = new Vector2(knobHolding.GetComponent<RectTransform>().localPosition.x
                    + Input.mousePosition.x - lastMousePos.x,
                    knobHolding.GetComponent<RectTransform>().localPosition.y);

                knobHolding.GetComponent<RectTransform>().localPosition =
                    new Vector2(Mathf.Clamp(knobHolding.GetComponent<RectTransform>().localPosition.x,
                    -gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                     gradientDisplay.GetComponent<RectTransform>().rect.width / 2f),
                    knobHolding.GetComponent<RectTransform>().localPosition.y);

                if (gradientColorKnobs.Contains(knobHolding))
                {
                    int index = gradientColorKnobs.IndexOf(knobHolding);
                    float newT = Mathf.InverseLerp(-gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                     gradientDisplay.GetComponent<RectTransform>().rect.width / 2f, knobHolding.transform.localPosition.x);
                    gradientColorKeys[index] = new GradientColorKey(gradientColorKeys[index].color, newT);
                    needToUpdateDisplay = true;
                }
                else if (gradientAlphaKnobs.Contains(knobHolding))
                {
                    int index = gradientAlphaKnobs.IndexOf(knobHolding);
                    float newT = Mathf.InverseLerp(-gradientDisplay.GetComponent<RectTransform>().rect.width / 2f,
                     gradientDisplay.GetComponent<RectTransform>().rect.width / 2f, knobHolding.transform.localPosition.x);
                    gradientAlphaKeys[index] = new GradientAlphaKey(gradientAlphaKeys[index].alpha, newT);
                    needToUpdateDisplay = true;
                }
            }
            if (holdingRedSlider)
            {
                holdingRedSlider = true;
                int index = gradientColorKnobs.IndexOf(knobSelected);
                float newR = Mathf.InverseLerp(
                    -RedImage.GetComponent<RectTransform>().rect.width / 2f,
                    RedImage.GetComponent<RectTransform>().rect.width / 2f,
                    -RedImage.transform.position.x + Input.mousePosition.x);
                RedImage.transform.GetChild(0).localPosition = new Vector2(
                    Mathf.Lerp(-RedImage.GetComponent<RectTransform>().rect.width / 2f,
                    RedImage.GetComponent<RectTransform>().rect.width / 2f, newR), 0);
                gradientColorKeys[index] = new GradientColorKey(
                        new Color(newR, gradientColorKeys[index].color.g, gradientColorKeys[index].color.b),
                        gradientColorKeys[index].time);
                needToUpdateDisplay = true;
            }
            if (holdingGreenSlider)
            {
                holdingGreenSlider = true;
                int index = gradientColorKnobs.IndexOf(knobSelected);
                float newG = Mathf.InverseLerp(
                    -GreenImage.GetComponent<RectTransform>().rect.width / 2f,
                    GreenImage.GetComponent<RectTransform>().rect.width / 2f,
                    -GreenImage.transform.position.x + Input.mousePosition.x);
                GreenImage.transform.GetChild(0).localPosition = new Vector2(
                    Mathf.Lerp(-GreenImage.GetComponent<RectTransform>().rect.width / 2f,
                    GreenImage.GetComponent<RectTransform>().rect.width / 2f, newG), 0);
                gradientColorKeys[index] = new GradientColorKey(
                    new Color(gradientColorKeys[index].color.r, newG, gradientColorKeys[index].color.b),
                    gradientColorKeys[index].time);
                needToUpdateDisplay = true;
            }
            if (holdingBlueSlider)
            {
                holdingBlueSlider = true;
                int index = gradientColorKnobs.IndexOf(knobSelected);
                float newB = Mathf.InverseLerp(
                    -BlueImage.GetComponent<RectTransform>().rect.width / 2f,
                    BlueImage.GetComponent<RectTransform>().rect.width / 2f,
                    -BlueImage.transform.position.x + Input.mousePosition.x);
                BlueImage.transform.GetChild(0).localPosition = new Vector2(
                    Mathf.Lerp(-BlueImage.GetComponent<RectTransform>().rect.width / 2f,
                    BlueImage.GetComponent<RectTransform>().rect.width / 2f, newB), 0);
                gradientColorKeys[index] = new GradientColorKey(
                    new Color(gradientColorKeys[index].color.r, gradientColorKeys[index].color.g, newB),
                    gradientColorKeys[index].time);
                needToUpdateDisplay = true;
            }
            if (holdingAlphaSlider)
            {
                holdingAlphaSlider = true;
                int index = gradientAlphaKnobs.IndexOf(knobSelected);
                float newA = Mathf.InverseLerp(
                    -AlphaImage.GetComponent<RectTransform>().rect.width / 2f,
                    AlphaImage.GetComponent<RectTransform>().rect.width / 2f,
                    -AlphaImage.transform.position.x + Input.mousePosition.x);
                AlphaImage.transform.GetChild(0).localPosition = new Vector2(
                    Mathf.Lerp(-AlphaImage.GetComponent<RectTransform>().rect.width / 2f,
                    AlphaImage.GetComponent<RectTransform>().rect.width / 2f, newA), 0);
                gradientAlphaKeys[index] = new GradientAlphaKey(newA,
                    gradientAlphaKeys[index].time);
                needToUpdateDisplay = true;
            }
            if (holdingColorCircle && knobSelected && gradientColorKnobs.Contains(knobSelected))
            {
                int index = gradientColorKnobs.IndexOf(knobSelected);
                Vector2 relativeMousePos = Input.mousePosition - colorWheel.transform.position;
                colorWheel.transform.GetChild(0).localPosition = relativeMousePos.normalized *
                    colorWheel.transform.GetChild(1).GetComponent<RectTransform>().rect.width / 2f;
                colorWheel.transform.GetChild(0).localEulerAngles =
                    new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, relativeMousePos.normalized));
                float currentH, currentS, currentV;
                float newH;
                Color.RGBToHSV(gradientColorKeys[index].color, out currentH, out currentS, out currentV);
                newH = (colorWheel.transform.GetChild(0).localEulerAngles.z) / 360f;
                gradientColorKeys[index] = new GradientColorKey(Color.HSVToRGB(newH, currentS, currentV), gradientColorKeys[index].time);
                needToUpdateDisplay = true;
            }
            if (holdingColorSquare && knobSelected && gradientColorKnobs.Contains(knobSelected))
            {
                int index = gradientColorKnobs.IndexOf(knobSelected);
                Vector2 relativeMousePos = Input.mousePosition - ColorBoxImage.transform.position;
                ColorBoxImage.transform.GetChild(0).transform.localPosition = new Vector2(
                    Mathf.Clamp(relativeMousePos.x,
                        -ColorBoxImage.GetComponent<RectTransform>().rect.width / 2f,
                        ColorBoxImage.GetComponent<RectTransform>().rect.width / 2f
                    ),
                    Mathf.Clamp(relativeMousePos.y,
                        -ColorBoxImage.GetComponent<RectTransform>().rect.height / 2f,
                        ColorBoxImage.GetComponent<RectTransform>().rect.height / 2f
                    ));

                float s, v;
                s = Mathf.InverseLerp(-ColorBoxImage.GetComponent<RectTransform>().rect.width / 2f,
                        ColorBoxImage.GetComponent<RectTransform>().rect.width / 2f,
                        ColorBoxImage.transform.GetChild(0).transform.localPosition.x);
                v = Mathf.InverseLerp(-ColorBoxImage.GetComponent<RectTransform>().rect.height / 2f,
                        ColorBoxImage.GetComponent<RectTransform>().rect.height / 2f,
                        ColorBoxImage.transform.GetChild(0).transform.localPosition.y);
                float currentH, currentS, currentV;
                Color.RGBToHSV(gradientColorKeys[index].color, out currentH, out currentS, out currentV);

                gradientColorKeys[index] = new GradientColorKey(Color.HSVToRGB(currentH, s, v), gradientColorKeys[index].time);
                needToUpdateDisplay = true;
            }


        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            knobHolding = null;
            holdingAlphaSlider = holdingBlueSlider = holdingGreenSlider = holdingRedSlider = false;
            holdingColorSquare = holdingColorCircle = false;
        }

        if (Input.GetKeyDown(KeyCode.Delete) && knobSelected)
        {
            if (gradientAlphaKnobs.Contains(knobSelected) && gradientAlphaKnobs.Count > 2)
            {
                int i = gradientAlphaKnobs.IndexOf(knobSelected);
                gradientAlphaKnobs.RemoveAt(i);
                Destroy(knobSelected);
                gradientAlphaKeys.RemoveAt(i);
                needToUpdateDisplay = true;
            }
            if (gradientColorKnobs.Contains(knobSelected) && gradientColorKnobs.Count > 2)
            {
                int i = gradientColorKnobs.IndexOf(knobSelected);
                gradientColorKnobs.RemoveAt(i);
                Destroy(knobSelected);
                gradientColorKeys.RemoveAt(i);
                needToUpdateDisplay = true;
            }
        }

        if (needToUpdateDisplay) UpdateDisplay();
    }
    private void Update()
    {
        UpdateCamera();
        UpdateUIControls();
       

        for(int i = 0; i < gradientColorKnobs.Count; i++)
        {
            if (knobSelected && gradientColorKnobs[i] == knobSelected)
            {
                gradientColorKnobs[i].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                gradientColorKnobs[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < gradientAlphaKnobs.Count; i++)
        {
            if (knobSelected && gradientAlphaKnobs[i] == knobSelected)
            {
                gradientAlphaKnobs[i].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                gradientAlphaKnobs[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        lastMousePos = Input.mousePosition;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
public class DataVisualizer : MonoBehaviour
{
    public TextAsset dataset;
    public string fileLocation = "Assets/Data/dataset1.txt";
    public float xSpacing, zSpacing;
    public float maxHeight;
    public float xTicks, yTicks, zTicks;
    public GameObject vizGameObject;
    public GameObject Legend;
    public Mesh arrow, sphere;
    public GameObject TMProPrefab;
    public Material red, blue, green;
    List<List<float>> data;

    ParticleSystem.Particle[] particles;
    float cutoffPlaneHeight;
    int currentViz = 0;

    void Start()
    {

        data = Dataset1Reader.ReadFile(dataset);
        ParticleSystem.MainModule mainModule = vizGameObject.GetComponent<ParticleSystem>().main;
        mainModule.maxParticles = data.Count * data[0].Count;
        InstantiateParticles();        
    }
    private void Update()
    {
        if(currentViz == 2 || currentViz == 3)
        {
            Legend.SetActive(true);
        }
        else
        {
            Legend.SetActive(false);
        }
    }
    float GetDataForIndex(int i)
    {
        return data[(int)(i / data[0].Count)][i % data[0].Count];
    }

    float GetDataAbove(int i)
    {
        return data[(int)((i-data[0].Count) / data[0].Count)][(i-data[0].Count) % data[0].Count];
    }
    float GetDataBelow(int i)
    {
        return data[(int)((i + data[0].Count) / data[0].Count)][(i + data[0].Count) % data[0].Count];

    }
    float GetDataRight(int i)
    {
        return data[(int)((i +1) / data[0].Count)][(i +1) % data[0].Count];

    }
    float GetDataLeft(int i)
    {
        return data[(int)((i - 1) / data[0].Count)][(i - 1) % data[0].Count];

    }
    float GetDataTopLeft(int i)
    {
        return data[(int)((i - 1 - data[0].Count) / data[0].Count)][(i - 1 - data[0].Count) % data[0].Count];
    }
    float GetDataTopRight(int i)
    {
        return data[(int)((i + 1 - data[0].Count) / data[0].Count)][(i + 1 - data[0].Count) % data[0].Count];
    }
    float GetDataBottomLeft(int i)
    {
        return data[(int)((i - 1 + data[0].Count) / data[0].Count)][(i - 1 + data[0].Count) % data[0].Count];
    }
    float GetDataBottomRight(int i)
    {
        return data[(int)((i + 1 + data[0].Count) / data[0].Count)][(i + 1 + data[0].Count) % data[0].Count];
    }

    int Spot1DFromSpot2D(int x, int y)
    {
        return y * data[0].Count + x;
    }

    int[] Spot2DFromSpot1D(int s)
    {
        return new int[]
        {
            s%data[0].Count,
            (int)(s / data[0].Count)
        };
    }

    List<List<Color>> CreateColoring()
    {
        List<List<bool>> colored = new List<List<bool>>();
        List<List<Color>> colors = new List<List<Color>>();
        for (int i = 0; i < data.Count; i++)
        {
            List<bool> l = new List<bool>();
            List<Color> c = new List<Color>();
            for (int j = 0; j < data[i].Count; j++)
            {
                l.Add(false);
                c.Add(Color.black);
            }
            colored.Add(l);
            colors.Add(c);
        }

        List<int> spotsToDo = new List<int>();
        for (int y = 0; y < data.Count; y++)
        {
            for (int x = 0; x < data[y].Count; x++)
            {
                if (!colored[y][x] && data[y][x] != 0)
                {
                    colors[y][x] = Random.ColorHSV();
                    colored[y][x] = true;
                    spotsToDo.Add(Spot1DFromSpot2D(x, y));
                    while(spotsToDo.Count > 0)
                    {
                        int spotX = Spot2DFromSpot1D(spotsToDo[0])[0];
                        int spotY = Spot2DFromSpot1D(spotsToDo[0])[1];

                        // Paint below
                        if (spotY < data.Count - 1)
                        {
                            if (!colored[spotY+1][spotX] && data[spotY + 1][spotX] != 0)
                            {
                                colors[spotY + 1][spotX] = colors[spotY][spotX];
                                colored[spotY + 1][spotX] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX, spotY + 1));
                            }
                        }
                        // Paint above
                        if (spotY > 0)
                        {
                            if (!colored[spotY - 1][spotX] && data[spotY - 1][spotX] != 0)
                            {
                                colors[spotY - 1][spotX] = colors[spotY][spotX];
                                colored[spotY - 1][spotX] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX, spotY - 1));
                            }
                        }
                        // Paint right
                        if (spotX < data[spotY].Count - 1)
                        {
                            if (!colored[spotY][spotX+1] && data[spotY][spotX + 1] != 0)
                            {
                                colors[spotY][spotX + 1] = colors[spotY][spotX];
                                colored[spotY][spotX + 1] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX + 1, spotY));
                            }
                        }
                        // Paint left
                        if (spotX > 0)
                        {
                            if (!colored[spotY][spotX -1] && data[spotY][spotX - 1] != 0)
                            {
                                colors[spotY][spotX - 1] = colors[spotY][spotX];
                                colored[spotY][spotX - 1] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX - 1, spotY));
                            }
                        }
                        // Paint top left
                        if (spotY > 0 && spotX > 0)
                        {
                            if (!colored[spotY - 1][spotX -1] && data[spotY - 1][spotX - 1] != 0)
                            {
                                colors[spotY - 1][spotX - 1] = colors[spotY][spotX];
                                colored[spotY - 1][spotX - 1] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX - 1, spotY - 1));
                            }

                        }
                        // Paint top right
                        if (spotY > 0 && spotX < data[spotY].Count - 1)
                        {
                            if (!colored[spotY - 1][spotX+1] && data[spotY - 1][spotX + 1] != 0)
                            {
                                colors[spotY - 1][spotX + 1] = colors[spotY][spotX];
                                colored[spotY - 1][spotX + 1] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX + 1, spotY - 1));
                            }

                        }
                        // Paint bot left
                        if (spotY < data.Count - 1 && spotX > 0)
                        {
                            if (!colored[spotY + 1][spotX-1] && data[spotY + 1][spotX - 1] != 0)
                            {
                                colors[spotY + 1][spotX - 1] = colors[spotY][spotX];
                                colored[spotY + 1][spotX - 1] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX - 1, spotY + 1));
                            }

                        }
                        // Paint bot right
                        if (spotY < data.Count - 1 && spotX < data[spotY].Count - 1)
                        {
                            if (!colored[spotY + 1][spotX+1] && data[spotY + 1][spotX + 1] != 0)
                            {
                                colors[spotY + 1][spotX + 1] = colors[spotY][spotX];
                                colored[spotY + 1][spotX + 1] = true;
                                spotsToDo.Add(Spot1DFromSpot2D(spotX + 1, spotY + 1));
                            }
                        }
                        spotsToDo.RemoveAt(0);
                    }
                }
                else if(data[y][x] == 0)
                {
                    colored[y][x] = true;
                }
            }
        }
           
        return colors;
    }


    public void SetCutoffPlaneHeight(Slider slider)
    {

        cutoffPlaneHeight = slider.value;
        if (currentViz == 1 || currentViz == 4)
        {
            StartCoroutine(SliderChanged());
        }
    }

    public void InstantiateParticles()
    {
        StopAllCoroutines();
        currentViz = 0;
        particles = new ParticleSystem.Particle[data.Count * data[0].Count];
        for(int i = 0; i < data.Count; i++)
        {
            print(data[i].Count);
        }
        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            particles[i].position = new Vector3(xSpacing * (i % data[0].Count), 0, zSpacing * ((int)(i / data[0].Count)));
            particles[i].startSize = 1;
            particles[i].startColor = Color.black;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
        GameObject xGrid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject zGrid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject yGrid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zGrid.GetComponent<Renderer>().material = blue;
        xGrid.GetComponent<Renderer>().material = red;
        yGrid.GetComponent<Renderer>().material = green;


        xGrid.transform.localScale = new Vector3(data[0].Count * xSpacing, 1, 1);
        zGrid.transform.localScale = new Vector3(1, 1, data.Count * zSpacing);
        yGrid.transform.localScale = new Vector3(1, maxHeight, 1);


        xGrid.transform.position = new Vector3(data[0].Count * xSpacing / 2f, 0, -1);
        zGrid.transform.position = new Vector3(-1, 0, data.Count * zSpacing / 2f);
        yGrid.transform.position = new Vector3(-1, maxHeight / 2f, -1);

        GameObject xLabel = Instantiate(TMProPrefab);
        xLabel.transform.position = new Vector3(data[0].Count * xSpacing / 2f, 0, -20);
        xLabel.GetComponent<TextMeshPro>().text = "X sample # (row)";
        xLabel.name = "xLabel";
        xLabel.transform.LookAt(xLabel.transform.position - Vector3.up);

        GameObject yLabel = Instantiate(TMProPrefab);
        yLabel.transform.position = new Vector3(0, maxHeight + 10, 0);
        yLabel.GetComponent<TextMeshPro>().text = "float value";
        yLabel.name = "yLabel";
        yLabel.transform.LookAt(yLabel.transform.position - Vector3.right);


        GameObject zLabel = Instantiate(TMProPrefab);
        zLabel.transform.position = new Vector3(-20, 0, data.Count * zSpacing / 2f);
        zLabel.GetComponent<TextMeshPro>().text = "Z sample # (column)";
        zLabel.name = "zLabel";
        zLabel.transform.LookAt(zLabel.transform.position - Vector3.up);
        zLabel.transform.Rotate(Vector3.forward, 90);

        GameObject zero = Instantiate(TMProPrefab);
        zero.transform.position = new Vector3(-5, 0, -5);
        zero.GetComponent<TextMeshPro>().text = "0";
        zero.GetComponent<TextMeshPro>().fontSize = 48;
        zero.name = "zero";
        zero.transform.LookAt(zero.transform.position - Vector3.up);

        for (int i = 1; i <= xTicks; i++)
        {
            GameObject xTick = Instantiate(TMProPrefab);
            xTick.transform.position = new Vector3((i / (float)xTicks) * data[0].Count * xSpacing, 0, -5);
            xTick.GetComponent<TextMeshPro>().text = "" + (int)((i / (float)xTicks) * data[0].Count);
            xTick.GetComponent<TextMeshPro>().fontSize = 48;
            xTick.name = "xTick" + i;
            xTick.transform.LookAt(xTick.transform.position - Vector3.up);
        }
        for (int i = 1; i <= yTicks; i++)
        {
            GameObject yTick = Instantiate(TMProPrefab);
            yTick.transform.position = new Vector3(-5, (i / (float)yTicks) * maxHeight, -5);
            yTick.GetComponent<TextMeshPro>().text = "" + ((i / (float)yTicks));
            yTick.GetComponent<TextMeshPro>().fontSize = 48;
            yTick.name = "yTick" + i;
            yTick.transform.LookAt(yTick.transform.position - Vector3.right);
        }
        for (int i = 1; i <= zTicks; i++)
        {
            GameObject zTick = Instantiate(TMProPrefab);
            zTick.transform.position = new Vector3(-5, 0, (i / (float)zTicks) * data.Count * zSpacing);
            zTick.GetComponent<TextMeshPro>().text = "" + (int)((i / (float)zTicks) * data.Count);
            zTick.GetComponent<TextMeshPro>().fontSize = 48;
            zTick.name = "zTick" + i;
            zTick.transform.LookAt(zTick.transform.position - Vector3.up);
        }

    }

    public void StartVisualization1()
    { 
        currentViz = 1;
        this.StopAllCoroutines();
        //vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
        StartCoroutine(Visualization1Pretty(2));
    }

    IEnumerator Visualization1Pretty(float seconds)
    {
        float startTime = Time.time;
        while(Time.time - startTime < seconds)
        {
            for (int i = 0; i < data.Count * data[0].Count; i++)
            {
                float dataVal = GetDataForIndex(i);

                particles[i].position = Vector3.Lerp(particles[i].position, 
                    new Vector3(particles[i].position.x, dataVal * maxHeight, particles[i].position.z),
                    0.25f);
                particles[i].startColor = Color.Lerp(particles[i].startColor, 
                    Color.Lerp(Color.black, Color.white, dataVal), 
                    0.25f);
                particles[i].startSize = Mathf.Lerp(particles[i].startSize,
                    dataVal > cutoffPlaneHeight ? 1 : 0,
                    0.25f);
            }

            vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);

            yield return null;
        }
        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            float dataVal = data[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].position = new Vector3(particles[i].position.x, dataVal * maxHeight, particles[i].position.z);
            particles[i].startColor = Color.Lerp(Color.black, Color.white, dataVal);
            particles[i].startSize = dataVal > cutoffPlaneHeight ? 1 : 0;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
    }

    public void StartVisualization2()
    {
        currentViz = 2;
        this.StopAllCoroutines();
        //vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
        StartCoroutine(Visualization2Pretty(2));
    }

    IEnumerator Visualization2Pretty(float seconds)
    {
        float startTime = Time.time;
        while (Time.time - startTime < seconds)
        {
            for (int i = 0; i < data.Count * data[0].Count; i++)
            {
                float dataVal = GetDataForIndex(i);

                particles[i].position = Vector3.Lerp(particles[i].position,
                    new Vector3(particles[i].position.x, 0, particles[i].position.z),
                    0.25f);
                particles[i].startColor = Color.Lerp(particles[i].startColor,
                    Color.white,
                    0.25f);
                particles[i].startSize = Mathf.Lerp(particles[i].startSize,
                    dataVal,
                    0.25f);
            }

            vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);

            yield return null;
        }
        
        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            float dataVal = data[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].position = new Vector3(particles[i].position.x, 0, particles[i].position.z);
            particles[i].startColor = Color.white;
            particles[i].startSize = dataVal;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
    }

    public void StartVisualization3()
    {
        currentViz = 3;
        this.StopAllCoroutines();
        //vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
        StartCoroutine(Visualization3Pretty(2));
    }

    
    IEnumerator Visualization3Pretty(float seconds)
    {
        float startTime = Time.time;
        List<List<Color>> colors = CreateColoring();
        startTime = Time.time;
        
        while (Time.time - startTime < seconds)
        {
            for (int i = 0; i < data.Count * data[0].Count; i++)
            {
                float dataVal = GetDataForIndex(i);

                particles[i].position = Vector3.Lerp(particles[i].position,
                    new Vector3(particles[i].position.x, 0, particles[i].position.z),
                    0.25f);
                particles[i].startColor = Color.Lerp(particles[i].startColor,
                    colors[(int)(i / data[0].Count)][i%data[0].Count],
                    0.25f);
                particles[i].startSize = Mathf.Lerp(particles[i].startSize,
                    dataVal,
                    0.25f);
            }

            vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);

            yield return null;
        }
        
        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            float dataVal = data[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].position = new Vector3(particles[i].position.x, 0, particles[i].position.z);
            particles[i].startColor = colors[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].startSize = dataVal;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
    }

    public void StartVisualization4()
    {
        currentViz = 4;
        this.StopAllCoroutines();
        //vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
        StartCoroutine(Visualization4Pretty(2));
    }

    IEnumerator Visualization4Pretty(float seconds)
    {
        float startTime = Time.time;
        List<List<Color>> colors = CreateColoring();

        while (Time.time - startTime < seconds)
        {
            for (int i = 0; i < data.Count * data[0].Count; i++)
            {
                float dataVal = GetDataForIndex(i);

                particles[i].position = Vector3.Lerp(particles[i].position,
                    new Vector3(particles[i].position.x, dataVal * maxHeight, particles[i].position.z),
                    0.25f);
                particles[i].startColor = Color.Lerp(particles[i].startColor,
                     colors[(int)(i / data[0].Count)][i % data[0].Count],
                    0.25f);
                particles[i].startSize = Mathf.Lerp(particles[i].startSize,
                    dataVal > cutoffPlaneHeight ? 1 : 0,
                    0.25f);
            }

            vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);

            yield return null;
        }
        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            float dataVal = data[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].position = new Vector3(particles[i].position.x, dataVal * maxHeight, particles[i].position.z);
            particles[i].startColor = colors[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].startSize = dataVal > cutoffPlaneHeight ? 1 : 0;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
    }
    IEnumerator SliderChanged()
    {

        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            float dataVal = GetDataForIndex(i);
            if (dataVal < cutoffPlaneHeight)
            {
                particles[i].startSize = 0;
            }
            else
            {
                particles[i].startSize = 1;
            }
        }
        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);

        yield return null;
    }
}


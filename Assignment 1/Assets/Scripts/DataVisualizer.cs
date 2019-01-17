using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataVisualizer : MonoBehaviour
{
    public string fileLocation = "Assets/Data/dataset1.txt";
    public float xSpacing, zSpacing;
    public float maxHeight;
    public GameObject vizGameObject;
    public Mesh arrow, sphere;
    List<List<float>> data;

    ParticleSystem.Particle[] particles;

    void Start()
    {
        data = Dataset1Reader.ReadFile(fileLocation);
        ParticleSystem.MainModule mainModule = vizGameObject.GetComponent<ParticleSystem>().main;
        mainModule.maxParticles = data.Count * data[0].Count;
        InstantiateParticles();        
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




    public void InstantiateParticles()
    {
        StopAllCoroutines();
        particles = new ParticleSystem.Particle[data.Count * data[0].Count];
        for(int i = 0; i < data.Count * data[0].Count; i++)
        {
            particles[i].position = new Vector3(xSpacing * (i % data[0].Count), 0, zSpacing * ((int)(i / data[0].Count)));
            particles[i].startSize = 1;
            particles[i].startColor = Color.black;
        }
        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);       
    }

    public void StartVisualization1()
    {
        this.StopAllCoroutines();
        vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
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
                    1,
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
            particles[i].startSize = 1;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
    }

    public void StartVisualization2()
    {
        this.StopAllCoroutines();
        vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
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
        this.StopAllCoroutines();
        vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
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
        this.StopAllCoroutines();
        vizGameObject.GetComponent<ParticleSystemRenderer>().mesh = sphere;
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
                    1,
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
            particles[i].startSize = 1;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
    }
}


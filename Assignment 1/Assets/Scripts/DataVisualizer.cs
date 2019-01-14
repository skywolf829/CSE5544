using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataVisualizer : MonoBehaviour
{
    public string fileLocation = "Assets/Data/dataset1.txt";
    public float xSpacing, zSpacing;
    public float maxHeight;
    public GameObject vizGameObject;

    List<List<float>> data;

    ParticleSystem.Particle[] particles;

    void Start()
    {
        data = Dataset1Reader.ReadFile(fileLocation);
        ParticleSystem.MainModule mainModule = vizGameObject.GetComponent<ParticleSystem>().main;
        mainModule.maxParticles = data.Count * data[0].Count;
        SetParticlePoints();
        Visualization1();
    }
    

    void SetParticlePoints()
    {
        particles = new ParticleSystem.Particle[data.Count * data[0].Count];

        for(int i = 0; i < data.Count * data[0].Count; i++)
        {
            particles[i].position = new Vector3(xSpacing * (i % data[0].Count), 0, zSpacing * ((int)(i / data[0].Count)));
            particles[i].startSize = 1;
            particles[i].startColor = Color.black;
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);
        
    }

    void Visualization1()
    {
        for (int i = 0; i < data.Count * data[0].Count; i++)
        {
            float dataVal = data[(int)(i / data[0].Count)][i % data[0].Count];
            particles[i].position = new Vector3(particles[i].position.x, dataVal * maxHeight, particles[i].position.z);
            particles[i].startColor = Color.Lerp(Color.black, Color.white, dataVal);
        }

        vizGameObject.GetComponent<ParticleSystem>().SetParticles(particles, data[0].Count * data.Count);

    }
}

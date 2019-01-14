using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dataset1Reader 
{
   public static List<List<float>> ReadFile(string file)
    {
        List<List<float>> values = new List<List<float>>();

        string[] lines = System.IO.File.ReadAllLines(file);

        foreach (string line in lines)
        {
            char[] lineArray = line.ToCharArray();
            string currentValue = "";
            int currentPosition = 0;
            List<float> currentValues = new List<float>();

            while(currentPosition < line.Length)
            {
                if (lineArray[currentPosition] == ' ')
                {
                    currentValues.Add(float.Parse(currentValue));
                    currentValue = "";
                }
                else
                {
                    currentValue = currentValue + lineArray[currentPosition];
                }
                currentPosition++;
            }

            if(currentValue != "")
            {
                currentValues.Add(float.Parse(currentValue));
                currentValue = "";
            }

            values.Add(currentValues);
        }

        return values;
    }
}

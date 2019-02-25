using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
public static class DataParser
{
    public static string[] GetHeaders(TextAsset t)
    {
        string text = t.text;
        if(text == "")
            text = System.Text.Encoding.Default.GetString(t.bytes);
        
        string[] lines = text.Split(new char[] { '\n' });
        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        return CSVParser.Split(lines[0]);
    }

    public static string[] GetHeaders(string filename)
    {
        string[] data;
        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            line = reader.ReadLine();
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] X = CSVParser.Split(line);
            data = X;            
        }
        return data;
    }
    public static List<string[]> ReadEHRDataSet(TextAsset t)
    {
        string text = t.text;
        if (text == "")
            text = System.Text.Encoding.Default.GetString(t.bytes);
        List<string[]> data = new List<string[]>();
        string[] lines = text.Split(new char[] { '\n' });

        for(int i = 1; i < lines.Length; i++)
        {
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            //Separating columns to array
            string[] X = CSVParser.Split(lines[i]);

            data.Add(X);
        }
        
        return data;
    }
    public static List<string[]> ReadEHRDataSet(string filename)
    {
        List<string[]> data = new List<string[]>();
        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            line = reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                
                //Define pattern
                Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                //Separating columns to array
                string[] X = CSVParser.Split(line);

                data.Add(X);
            }
        }
        return data;
    }

    public static Dictionary<string, List<string[]>> dataToPatient(List<string[]> data)
    {
        Dictionary<string, List<string[]>> patientData = new Dictionary<string, List<string[]>>();

        for(int i = 0; i < data.Count; i++)
        {
            if (!patientData.ContainsKey(data[i][0]))
            {
                patientData.Add(data[i][0], new List<string[]>());
            }
            patientData[data[i][0]].Add(data[i]);            
        }
        
        return patientData;
    }
    
    public static void orderByDate(Dictionary<string, List<string[]>> data)
    {

        foreach (string k in data.Keys)
        {
            data[k].Sort((x, y) => string.Compare(x[9], y[9]));
        }

    }

    public static string getEarliestDate(List<string[]> data)
    {
        data.Sort((x, y) => string.Compare(x[9], y[9]));
        return (data[0][9]);
    }
    public static string getLatestDate(List<string[]> data)
    {
        data.Sort((x, y) => string.Compare(x[9], y[9]));
        return (data[data.Count -1][9]);
    }

   
}

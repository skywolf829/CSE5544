using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataImporter 
{
    public static void GetXY(TextAsset t, out int x, out int y)
    {
        string[] lines = t.text.Split(new char[] { '\n' });
        string[] firstLine = lines[0].Split(new char[] { ' ' });

        int numX, numY;
        int.TryParse(firstLine[0], out numX);
        int.TryParse(firstLine[1], out numY);
        x = numX;
        y = numY;
    }
    public static float[] LoadData(TextAsset t, out Vector2[] positions, out Vector2[] directions)
    {
        string[] lines = t.text.Split(new char[] { '\n' });
        string[] firstLine = lines[0].Split(new char[] { ' ' });

        float xMin = 0, xMax = 0, yMin = 0, yMax = 0;
        float smallestMagnitude = float.PositiveInfinity, largestMagnitude = 0;

        int numX, numY;
        int.TryParse(firstLine[0], out numX);
        int.TryParse(firstLine[1], out numY);

        positions = new Vector2[numX * numY];
        directions = new Vector2[numX * numY];

        for(int i = 1; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(new string[] { "  " }, System.StringSplitOptions.RemoveEmptyEntries);
            float x, y, xdir, ydir;
            if (!float.TryParse(line[0], out x)) Debug.Log("Error reading x");
            if (!float.TryParse(line[1], out y)) Debug.Log("Error reading y");
            if (!float.TryParse(line[2], out xdir)) Debug.Log("Error reading xdir");
            if (!float.TryParse(line[3], out ydir)) Debug.Log("Error reading ydir");
            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;           
            positions[i - 1] = new Vector2(x, y);
            directions[i - 1] = new Vector2(xdir, ydir);
            if (directions[i - 1].magnitude < smallestMagnitude) smallestMagnitude = directions[i - 1].magnitude;
            if (directions[i - 1].magnitude > largestMagnitude) largestMagnitude = directions[i - 1].magnitude;
        }
        return new float[6] { xMin, xMax, yMin, yMax, smallestMagnitude, largestMagnitude };
    }

    public static float[] LoadData(TextAsset t, out Vector2[,] positions, out Vector2[,] directions)
    {
        string[] lines = t.text.Split(new char[] { '\n' });
        string[] firstLine = lines[0].Split(new char[] { ' ' });

        float xMin = 0, xMax = 0, yMin = 0, yMax = 0;
        float smallestMagnitude = float.PositiveInfinity, largestMagnitude = 0;

        int numX, numY;
        int.TryParse(firstLine[0], out numX);
        int.TryParse(firstLine[1], out numY);

        positions = new Vector2[numX , numY];
        directions = new Vector2[numX , numY];

        for (int i = 1; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(new string[] { "  " }, System.StringSplitOptions.RemoveEmptyEntries);
            float x, y, xdir, ydir;
            if (!float.TryParse(line[0], out x)) Debug.Log("Error reading x");
            if (!float.TryParse(line[1], out y)) Debug.Log("Error reading y");
            if (!float.TryParse(line[2], out xdir)) Debug.Log("Error reading xdir");
            if (!float.TryParse(line[3], out ydir)) Debug.Log("Error reading ydir");
            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;
            positions[(i-1) % numY, (i - 1) / numY] = new Vector2(x, y);
            directions[(i - 1) % numY, (i - 1) / numY] = new Vector2(xdir, ydir);
            if (directions[(i - 1) % numY, (i - 1) / numY].magnitude < smallestMagnitude)
                smallestMagnitude = directions[(i - 1) % numY, (i - 1) / numY].magnitude;
            if (directions[(i - 1) % numY, (i - 1) / numY].magnitude > largestMagnitude)
                largestMagnitude = directions[(i - 1) % numY, (i - 1) / numY].magnitude;
        }
        return new float[6] { xMin, xMax, yMin, yMax, smallestMagnitude, largestMagnitude };
    }
}

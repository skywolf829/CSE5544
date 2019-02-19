using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class DataVisualization : MonoBehaviour
{

    public string EHRDataset;
    public GameObject TMProPrefab;
    public Material barMat;
    public float size;
    public float axisSize;

    float yMax, yMin, xMax, xMin;
    
    float patientBarHeight;
    float patientBarPadding;

    Vector3 lastMousePos;
    bool grabbing;

    List<string[]> data;
    string[] headers;
    Dictionary<string, List<string[]>> patientData;
    Dictionary<GameObject, string> hoverData;
    string earliestDate, latestDate;

    GameObject details, detailsText;
    // Start is called before the first frame update
    void Start()
    {
        hoverData = new Dictionary<GameObject, string>();
        headers = DataParser.GetHeaders(Path.Combine(new string[] { Application.dataPath, "Resources", EHRDataset }));
        data = DataParser.ReadEHRDataSet(Path.Combine(new string[] { Application.dataPath, "Resources", EHRDataset }));
        patientData = DataParser.dataToPatient(data);
        DataParser.orderByDate(patientData);

        earliestDate = DataParser.getEarliestDate(data);
        latestDate = DataParser.getLatestDate(data);

        Camera.main.orthographic = true;
        Camera.main.transform.position = new Vector3(size * Screen.width / (float)Screen.height, size, -10);
        Camera.main.transform.LookAt(Camera.main.transform.position + Vector3.forward);
        Camera.main.orthographicSize = size;

        yMax = size * 2;
        yMin = 0;
        xMin = 0;
        xMax = size * 2 * Screen.width / (float)Screen.height;

        float sizePerPatient = ((yMax - yMin) - axisSize) / patientData.Count;
        patientBarHeight = sizePerPatient * 0.65f;
        patientBarPadding = (sizePerPatient - patientBarHeight);

        details = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(details.GetComponent<BoxCollider>());
        detailsText = GameObject.Instantiate(TMProPrefab);
        detailsText.GetComponent<TextMeshPro>().color = Color.black;
        Camera.main.orthographicSize = Camera.main.orthographicSize * 1.25f;
        StartCoroutine(CreateVisualization());
    }

    private void Update()
    {
        
        RaycastHit rch = new RaycastHit();
        float d = Input.GetAxis("Mouse ScrollWheel");
        if (d < 0)
        {
            Camera.main.orthographicSize = Camera.main.orthographicSize + 3;
        }
        else if(d > 0)
        {
            Camera.main.orthographicSize = Camera.main.orthographicSize - 3;

        }
        if (Input.GetMouseButton(2))
        {
            if (grabbing)
            {
                Camera.main.transform.position += (lastMousePos - Input.mousePosition) * size / 1000f;
            }
            lastMousePos = Input.mousePosition;
            grabbing = true;
        }
        else
        {
            grabbing = false;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rch)){
            if(rch.collider != null && hoverData.ContainsKey(rch.collider.gameObject))
            {
                details.SetActive(true);
                detailsText.SetActive(true);
                detailsText.GetComponent<TextMeshPro>().text = hoverData[rch.collider.gameObject];
                detailsText.GetComponent<RectTransform>().sizeDelta = CreateRectFromText(hoverData[rch.collider.gameObject]);
                details.transform.localScale = new Vector3(detailsText.GetComponent<RectTransform>().sizeDelta.x,
                    detailsText.GetComponent<RectTransform>().sizeDelta.y, 1);

                details.transform.position = rch.collider.gameObject.transform.position + new Vector3(0, 0, -2) + 
                    new Vector3(detailsText.GetComponent<RectTransform>().sizeDelta.x / 2f, -detailsText.GetComponent<RectTransform>().sizeDelta.y / 2f, 0);
                detailsText.transform.position = rch.collider.gameObject.transform.position + new Vector3(0, 0, -3) +
                    new Vector3(detailsText.GetComponent<RectTransform>().sizeDelta.x / 2f, -detailsText.GetComponent<RectTransform>().sizeDelta.y / 2f, 0); ;
            }
            else
            {
                details.SetActive(false);
                detailsText.SetActive(false);
            }
        }
        else
        {
            details.SetActive(false);
            detailsText.SetActive(false);
        }
    }
    Vector2 CreateRectFromText(string text)
    {
        string[] lines = text.Split(new char[] { '\n' });
        float h = lines.Length * 0.05f * (yMax - yMin);
        int longestLength = 0;
        for(int i = 0; i < lines.Length; i++)
        {
            longestLength =  lines[i].Length > longestLength ? lines[i].Length : longestLength;
        }
        float w = longestLength * 0.01f * (xMax - xMin);
        return new Vector2(w, h);
    }
    IEnumerator CreateVisualization()
    {
        // Create the axis and the legend
        GameObject xAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        xAxis.transform.name = "x-axis";
        xAxis.GetComponent<Renderer>().material = barMat;
        xAxis.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        xAxis.GetComponent<Renderer>().material.color = Color.white;
        xAxis.transform.localScale = new Vector3((xMax - xMin), axisSize, 1);
        xAxis.transform.position = new Vector3((xMax - xMin) / 2f, yMin + axisSize / 2f, 0);

        GameObject yAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        yAxis.transform.name = "y-axis";
        yAxis.GetComponent<Renderer>().material = barMat;
        yAxis.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        yAxis.GetComponent<Renderer>().material.color = Color.white;
        yAxis.transform.localScale = new Vector3(axisSize, yMax - yMin, 1);
        yAxis.transform.position = new Vector3((xMax - xMin) / 2f, (yMax - yMin) / 2f, 0);

        /*
        System.DateTime currentTime = System.DateTime.Parse(earliestDate);
        while(System.DateTime.Compare(currentTime, System.DateTime.Parse(latestDate)) < 0)
        {
            GameObject timeAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            timeAxis.transform.name = "timeAxis";
            timeAxis.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            timeAxis.GetComponent<Renderer>().material.color = Color.white;
            timeAxis.transform.localScale = new Vector3(axisSize, yMax - yMin, 1);
            float yearDif = differenceInYears(earliestDate, latestDate);
            float yearFromStart = differenceInYears(earliestDate, currentTime.ToShortDateString());
            timeAxis.transform.position = new Vector3((xMax - xMin) * (yearFromStart / yearDif), (yMax - yMin) / 2f, 0);
            GameObject text = GameObject.Instantiate(TMProPrefab);
            text.transform.position = new Vector3((xMax - xMin) * (yearFromStart / yearDif), (yMax - yMin) / 2f, -2);
            text.GetComponent<TextMeshPro>().text = currentTime.ToShortDateString();
            currentTime = currentTime.AddYears(1);
        }
        */
        GameObject xAxisText = Instantiate(TMProPrefab);
        xAxisText.GetComponent<TextMeshPro>().text = "Days since TBI";
        xAxisText.GetComponent<RectTransform>().sizeDelta = new Vector3((xMax - xMin) / 10f, (yMax - yMin) / 20f, 1);
        xAxisText.transform.position = new Vector3((xMax - xMin) / 2f, -xAxisText.GetComponent<RectTransform>().sizeDelta.y * 1.25f, 0);

        GameObject yAxisText = Instantiate(TMProPrefab);
        yAxisText.GetComponent<TextMeshPro>().text = "Patient ID";
        yAxisText.GetComponent<RectTransform>().sizeDelta = new Vector3((xMax - xMin) / 10f, (yMax - yMin) / 20f, 1);
        yAxisText.transform.position = new Vector3((xMax - xMin) / 2f, yMax + yAxisText.GetComponent<RectTransform>().sizeDelta.y / 2f, 0);

        for (int a = 0; a < 10; a++)
        {
            float x = ((xMax - xMin) / 2f) * (a / 10f) + (xMax - xMin) / 2f;
            float y = (yMax - yMin) / 2f;

            GameObject timeAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            timeAxis.transform.name = "timeAxis";
            timeAxis.GetComponent<Renderer>().material = barMat;
            timeAxis.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            timeAxis.GetComponent<Renderer>().material.color = Color.white;
            timeAxis.transform.localScale = new Vector3(axisSize / 10f, yMax - yMin, 1);
            timeAxis.transform.position = new Vector3(x, y, 1);

            GameObject text = Instantiate(TMProPrefab);
            text.GetComponent<TextMeshPro>().text = "" + 365 * (a / 10f) * differenceInYears(earliestDate, latestDate) / 2f;
            text.transform.GetComponent<RectTransform>().sizeDelta = new Vector2((xMax - xMin) / 18f, (yMax - yMin) / 30f);
            text.transform.position = new Vector3(x, -text.GetComponent<RectTransform>().sizeDelta.y / 2f, 0);
        }
        for (int a = -1; a > -10; a--)
        {
            float x = ((xMax - xMin) / 2f) * (a / 10f) + (xMax - xMin) / 2f;
            float y = (yMax - yMin) / 2f;

            GameObject timeAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            timeAxis.transform.name = "timeAxis";
            timeAxis.GetComponent<Renderer>().material = barMat;
            timeAxis.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            timeAxis.GetComponent<Renderer>().material.color = Color.white;
            timeAxis.transform.localScale = new Vector3(axisSize / 10f, yMax - yMin, 1);
            timeAxis.transform.position = new Vector3(x, y, 1);

            GameObject text = Instantiate(TMProPrefab);
            text.GetComponent<TextMeshPro>().text = "" + 365 * (a / 10f) * differenceInYears(earliestDate, latestDate) / 2f;
            text.transform.GetComponent<RectTransform>().sizeDelta = new Vector2((xMax - xMin) / 18f, (yMax - yMin) / 30f);
            text.transform.position = new Vector3(x, -text.GetComponent<RectTransform>().sizeDelta.y / 2f, 0);

        }

        // Populate the graph one person at a time
        GameObject g = new GameObject();
        g.name = "PatientBarParent";
        int i = 0;
        foreach (string x in patientData.Keys)
        {
            GameObject patientBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patientBar.transform.name = "patient " + x;
            patientBar.GetComponent<Renderer>().material = barMat;
            patientBar.GetComponent<Renderer>().material.color = Color.black;
            patientBar.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            patientBar.transform.localScale = patientToBarScale(patientData[x]);
            patientBar.transform.position = patientToBarPosTimecentric(patientData[x], i);
            patientBar.transform.parent = g.transform;

            GameObject patientTBI = GameObject.CreatePrimitive(PrimitiveType.Cube);

            patientTBI.transform.localScale = new Vector3(patientBarHeight, patientBarHeight, patientBarHeight);
            patientTBI.transform.parent = patientBar.transform;
            patientTBI.transform.name = "patient " + x + " TBI";
            patientTBI.GetComponent<Renderer>().material = barMat;
            patientTBI.GetComponent<Renderer>().material.color = Color.blue;
            patientTBI.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            patientTBI.transform.position = patientToBarPosTimecentric(patientData[x], i);
            patientTBI.transform.position = new Vector3((xMax - xMin) / 2f + (xMax - xMin) * yearsToCenterTime(patientData[x][0][6]) / differenceInYears(earliestDate, latestDate),
                patientTBI.transform.position.y,
                -2);
            patientTBI.SetActive(false);

            

            for(int j = 0; j < patientData[x].Count - 1; j++)
            {
                GameObject patientEncounterBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patientEncounterBar.transform.name = "patientEncounter " + patientData[x][j][8];
                patientEncounterBar.GetComponent<Renderer>().material = barMat;
                patientEncounterBar.GetComponent<Renderer>().material.color = Random.ColorHSV();
                patientEncounterBar.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                patientEncounterBar.transform.localScale = patientToBarScale(patientData[x]);
                patientEncounterBar.transform.position = patientToBarPosTimecentric(patientData[x], i);
                patientEncounterBar.transform.position = new Vector3(xPosFromDates(patientData[x][j][9], patientData[x][j + 1][9]),
                    patientEncounterBar.transform.position.y,
                    -5);
                patientEncounterBar.transform.localScale = new Vector3(xScaleFromDates(patientData[x][j][9], patientData[x][j + 1][9]),
                   patientEncounterBar.transform.localScale.y,
                   patientEncounterBar.transform.localScale.z);
                patientEncounterBar.transform.parent = patientBar.transform;

                string dataString = "";
                dataString += "PatientID: " + x + "\n";
                dataString += "EncounterID: " + patientData[x][j][8] + "\n";
                dataString += "Encounter Date: " + patientData[x][j][9] + "\n";
                dataString += "Patient TBI Date: " + patientData[x][j][6] + "\n";
                dataString += "Patient sex: " + patientData[x][j][1] + "\n";
                dataString += "Patient age: " + patientData[x][j][2] + "\n";
                dataString += "Provider specialty: " + patientData[x][j][11] + "\n";
                if(patientData[x][j][14] == "1")
                {
                    dataString += headers[14] + "\n";
                }
                for(int k = 18; k < patientData[x][j].Length; k++)
                {
                    if(patientData[x][j][k] == "1")
                    {
                        dataString += headers[k] + "\n";
                    }
                }
                hoverData.Add(patientEncounterBar, dataString);
                yield return null;
            }


            patientBar.transform.position = new Vector3(patientBar.transform.position.x - (patientTBI.transform.position.x - (xMax - xMin) / 2f),
                patientBar.transform.position.y, patientBar.transform.position.z);
            i++;
            yield return null;
        }

    }
    private Vector3 patientToBarScale(List<string[]> patient)
    {
        Vector3 barScale = new Vector3();

        string earliestDataEntry = DataParser.getEarliestDate(patient);
        string latestDataEntry = DataParser.getLatestDate(patient);
        
        float xStart = differenceInYears(earliestDate, earliestDataEntry) / differenceInYears(earliestDate, latestDate);
        float xEnd = differenceInYears(earliestDate, latestDataEntry) / differenceInYears(earliestDate, latestDate);

        float width = (xEnd - xStart) * (xMax - xMin);
        float height = patientBarHeight;
        barScale = new Vector3(width, height, 0.5f);

        return barScale;
    }
    private float xPosFromDate(string date)
    {
        float xStart = differenceInYears(earliestDate, date) / differenceInYears(earliestDate, latestDate);

        float x = xStart * (xMax - xMin);
        return x;
    }
    private float xPosFromDates(string startDate, string endDate)
    {
        float x = 0;

        float xStart = differenceInYears(earliestDate, startDate) / differenceInYears(earliestDate, latestDate);
        float xEnd = differenceInYears(earliestDate, endDate) / differenceInYears(earliestDate, latestDate);

        x = (((xEnd - xStart) / 2f) + xStart) * (xMax - xMin);

        return x;
    }

    private float xScaleFromDates(string startDate, string endDate)
    {
        float x = 0;

        float xStart = differenceInYears(earliestDate, startDate) / differenceInYears(earliestDate, latestDate);
        float xEnd = differenceInYears(earliestDate, endDate) / differenceInYears(earliestDate, latestDate);

        x = (xEnd - xStart) * (xMax - xMin);

        return x;

    }
    private Vector3 patientToBarPosTimecentric(List<string[]> patient, int i)
    {
        Vector3 barPos = new Vector3();
        string earliestDataEntry = DataParser.getEarliestDate(patient);
        string latestDataEntry = DataParser.getLatestDate(patient);

        float xStart = differenceInYears(earliestDate, earliestDataEntry) / differenceInYears(earliestDate, latestDate);
        float xEnd = differenceInYears(earliestDate, latestDataEntry) / differenceInYears(earliestDate, latestDate);

        float xPos = (((xEnd - xStart) / 2f) + xStart) * (xMax - xMin);
        float yPos = patientBarHeight / 2f + patientBarPadding * (i + 1) + patientBarHeight * i + axisSize;
        barPos = new Vector3(xPos, yPos, 0);

        return barPos;
    }

    private Vector3 patientToBarPosFirstTBI(List<string[]> patient, int i)
    {
        Vector3 barPos = new Vector3();
        string earliestDataEntry = DataParser.getEarliestDate(patient);
        string latestDataEntry = DataParser.getLatestDate(patient);

        float xStart = differenceInYears(earliestDate, earliestDataEntry) / differenceInYears(earliestDate, latestDate);
        float xEnd = differenceInYears(earliestDate, latestDataEntry) / differenceInYears(earliestDate, latestDate);
        float offset = yearsToCenterTime(patient[0][6]) / differenceInYears(earliestDate, latestDate);

        float xPos = (((xEnd - xStart) / 2f) + xStart + offset)* (xMax - xMin);
        float yPos = patientBarHeight / 2f + patientBarPadding * (i + 1) + patientBarHeight * i + axisSize;
        barPos = new Vector3(xPos, yPos, 0);

        return barPos;
    }
    private float differenceInYears(string time1, string time2)
    {
        return (System.DateTime.Parse(time2).Ticks - System.DateTime.Parse(time1).Ticks) /
            (Mathf.Pow(10, 7) * 60 * 60 * 24 * 365);
    }
    private float yearsToCenterTime(string t)
    {
        return (System.DateTime.Parse(t).Ticks - ((System.DateTime.Parse(latestDate).Ticks - System.DateTime.Parse(earliestDate).Ticks) / 2f
            + System.DateTime.Parse(earliestDate).Ticks)) /
            (Mathf.Pow(10, 7) * 60 * 60 * 24 * 365);
    }
}

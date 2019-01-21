using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SliderTextUpdate : MonoBehaviour
{
    public void SetValue(Slider slider)
    {
        GetComponent<TextMeshProUGUI>().text = ""+slider.value;
    }
}

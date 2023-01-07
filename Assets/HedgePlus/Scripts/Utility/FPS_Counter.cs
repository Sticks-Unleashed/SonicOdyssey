using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS_Counter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Display;
    [SerializeField] int GoodFPS, BadFPS;
    [SerializeField] float RefreshRate; //How often the counter refreshes
    int curFPS;
    int minFPS = 255;
    int maxFPS;
    static string[] fpsLabel = new string[255];

    private void Start()
    {
        for (int i = 0; i < fpsLabel.Length; i++)
        {
            fpsLabel[i] = string.Format("{00:00}", i);
        }
        StartCoroutine("Refresh");
    }

    IEnumerator Refresh()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(RefreshRate);
            curFPS = (int)(1f / Time.unscaledDeltaTime);
            curFPS = Mathf.Clamp(curFPS, 0, 255);
            if (maxFPS < curFPS)
            {
                maxFPS = curFPS;
            }
            if (minFPS > curFPS)
            {
                minFPS = curFPS;
            }
        }
    }


    string FPSValue (int value)
    {
        string color = "white";
        if (value > GoodFPS)
        {
            color = "green";
        }
        if (value < BadFPS)
        {
            color = "red";
        }

        return "<color=" + color + ">" + fpsLabel[value] + "</color>";
    }

    private void Update()
    {
        Display.text = FPSValue(curFPS) + "[" + FPSValue(minFPS) + "/" + FPSValue(maxFPS) + "]";
    }
}

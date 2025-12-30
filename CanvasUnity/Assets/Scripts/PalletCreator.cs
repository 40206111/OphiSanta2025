using System.Collections.Generic;
using UnityEngine;

public class PalletCreator
{
    public List<Color> Colours = new List<Color>();

    public const int MaxColours = 5;

    private float range = 0.25f;

    public void CreatePallet()
    {
        Colours.Clear();
        var center = Random.Range(0, 1.0f);
        var hueMin = Mathf.Max(center - range, 0);
        var hueMax = Mathf.Min(center + range, 1);

        for (int i = 0; i < MaxColours; i++)
        {
            Colours.Add(Random.ColorHSV(hueMin, hueMax, 1.0f, 1.0f, 0.4f, 1.0f));
        }
    }

}

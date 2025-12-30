using System.Collections.Generic;
using UnityEngine;

public class PalletCreator
{
    public List<Color> Colours = new List<Color>();

    public const int MaxColours = 5;

    private float range = 0.3f;

    public void CreatePallet()
    {
        Colours.Clear();
        var center = Random.Range(0, 1.0f);
        var hueMin = center - range;
        var hueMax = center + range;

        for (int i = 0; i < MaxColours; i++)
        {
            Colours.Add(Random.ColorHSV(hueMin, hueMax, 1.0f, 1.0f, 0.4f, 1.0f));
        }
    }

}

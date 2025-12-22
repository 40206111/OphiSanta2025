using System.Collections.Generic;
using UnityEngine;

public class PalletCreator
{
    public List<Color> Colours = new List<Color>();

    public const int MaxColours = 5;

    public void CreatePallet()
    {
        Colours.Clear();
        for (int i = 0; i < MaxColours; i++)
        {
            var randR = Random.Range(0, 1.0f);
            var randG = Random.Range(0, 1.0f);
            var randB = Random.Range(0, 1.0f);
            Colours.Add(new Color(randR, randG, randB));
        }
    }

}

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
        var hueMin = center - range;
        var hueMax = center + range;
        float overflow = hueMin < 0.0f ? hueMin : 0.0f;
        overflow = hueMax > 1.0f ? 1 - hueMax : overflow;
        var absOverflow = Mathf.Abs(overflow);

        hueMin = Mathf.Max(hueMin, 0);
        hueMax = Mathf.Min(hueMax, 1);

        for (int i = 0; i < MaxColours; i++)
        {
            var randUseOffset = overflow != 0 && Random.Range(0.0f, 1.0f) <= absOverflow;
            var min = randUseOffset && overflow > 0 ? 0 : hueMin;
            var max = randUseOffset && overflow > 0 ? overflow : hueMax;
            min = randUseOffset && overflow < 0 ? 1 + overflow : min;
            max = randUseOffset && overflow < 0 ? 1 : max;
            var colour = Random.ColorHSV(min, max, 0.6f, 1.0f, 0.4f, 1.0f);
            colour.a = 1.0f;
            Colours.Add(colour);
        }
    }

}

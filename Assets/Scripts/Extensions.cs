using System;
using System.Collections.Generic;

[Serializable]
public class StringList
{
    public List<string> List;

    public bool Contains(string str)
    {
        return List.Contains(str);
    }
}

public static class Extensions
{
    public static void Shuffle<T>(this IList<T> ts) {
        int count = ts.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i)
        {
            int r = UnityEngine.Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }

    public static UnityEngine.Color ColorFromHSV(float hue, float saturation, float value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        float f = hue / 60f - MathF.Floor(hue / 60f);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return new UnityEngine.Color(255, v, t, p);
        else if (hi == 1)
            return new UnityEngine.Color(255, q, v, p);
        else if (hi == 2)
            return new UnityEngine.Color(255, p, v, t);
        else if (hi == 3)
            return new UnityEngine.Color(255, p, q, v);
        else if (hi == 4)
            return new UnityEngine.Color(255, t, p, v);
        else
            return new UnityEngine.Color(255, v, p, q);
    }
}

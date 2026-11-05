using UnityEngine;

// Procedurally generated sound effects (no audio files needed).
// Usage: DesertSound.Jump(); DesertSound.Coin();
public static class DesertSound
{
    const int Rate = 44100;
    static AudioClip jumpClip, coinClip;
    static AudioSource src;

    public static void Jump() { Play(ref jumpClip, MakeJump); }
    public static void Coin() { Play(ref coinClip, MakeCoin); }

    static void Play(ref AudioClip clip, System.Func<AudioClip> make)
    {
        if (clip == null) clip = make();
        if (src == null)
        {
            var g = new GameObject("DesertSound");
            Object.DontDestroyOnLoad(g);
            src = g.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
        }
        src.PlayOneShot(clip, 0.6f);
    }

    // Rising "boing" sweep
    static AudioClip MakeJump()
    {
        float len = 0.22f; int n = (int)(Rate * len);
        var d = new float[n]; float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = Mathf.Lerp(280f, 720f, t);
            phase += 2f * Mathf.PI * f / Rate;
            float env = Mathf.Pow(1f - t, 1.5f) * Mathf.Min(1f, i / 200f);
            d[i] = Mathf.Sign(Mathf.Sin(phase)) * 0.25f * env + Mathf.Sin(phase) * 0.4f * env;
        }
        return Build("jump", d);
    }

    // Two-note "ding" (B5 -> E6)
    static AudioClip MakeCoin()
    {
        float len = 0.3f; int n = (int)(Rate * len);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / Rate;
            float f = t < 0.08f ? 987.77f : 1318.5f;
            float local = t < 0.08f ? t : t - 0.08f;
            float env = Mathf.Exp(-local * (t < 0.08f ? 8f : 11f)) * Mathf.Min(1f, i / 100f);
            d[i] = (Mathf.Sin(2f * Mathf.PI * f * t) * 0.5f + Mathf.Sin(4f * Mathf.PI * f * t) * 0.15f) * env;
        }
        return Build("coin", d);
    }

    static AudioClip Build(string name, float[] data)
    {
        var c = AudioClip.Create(name, data.Length, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }
}

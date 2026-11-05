using UnityEngine;

public class DesertItem : MonoBehaviour
{
    public enum Kind { Coin, Hazard, Goal }
    public Kind kind;
    Vector3 baseScale; float phase;

    void Start() { baseScale = transform.localScale; phase = Random.value * 6f; }

    void Update()
    {
        if (kind != Kind.Coin) return;
        float s = 0.25f + 0.75f * Mathf.Abs(Mathf.Cos(Time.time * 4f + phase));
        transform.localScale = new Vector3(baseScale.x * s, baseScale.y, baseScale.z);
    }

    void OnTriggerEnter2D(Collider2D o) { Touch(o); }
    void OnTriggerStay2D(Collider2D o) { if (kind == Kind.Hazard) Touch(o); }

    void Touch(Collider2D o)
    {
        if (DesertGame.I == null || o.GetComponent<DesertPlayer>() == null) return;
        if (kind == Kind.Coin) { DesertGame.I.AddCoin(); DesertSound.Coin(); Destroy(gameObject); }
        else if (kind == Kind.Hazard) DesertGame.I.Hurt();
        else DesertGame.I.Win();
    }
}

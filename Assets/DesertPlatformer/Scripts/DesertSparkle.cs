using UnityEngine;

public class DesertSparkle : MonoBehaviour
{
    float t; Vector3 v; SpriteRenderer r;

    void Start()
    {
        r = GetComponent<SpriteRenderer>();
        v = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(0.6f, 1.6f), 0f);
        transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += v * Time.deltaTime;
        var c = r.color; c.a = 1f - t / 0.5f; r.color = c;
        if (t > 0.5f) Destroy(gameObject);
    }
}

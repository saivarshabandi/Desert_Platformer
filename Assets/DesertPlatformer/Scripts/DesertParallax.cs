using UnityEngine;

// speed 1 = stays fixed to the camera (sky). Smaller = scrolls faster (closer layers).
[DefaultExecutionOrder(100)]
public class DesertParallax : MonoBehaviour
{
    [Range(0f, 1f)] public float speed = 0.5f;
    Transform cam; float width;

    void Start()
    {
        cam = Camera.main.transform;
        var sr = GetComponent<SpriteRenderer>();
        float w = sr.sprite.bounds.size.x;
        width = w * transform.lossyScale.x;
        for (int i = -1; i <= 1; i += 2)
        {
            var c = new GameObject(name + "_clone");
            c.transform.SetParent(transform, false);
            c.transform.localPosition = new Vector3(w * i, 0f, 0f);
            var r = c.AddComponent<SpriteRenderer>();
            r.sprite = sr.sprite; r.sortingOrder = sr.sortingOrder;
        }
    }

    void LateUpdate()
    {
        float cx = cam.position.x;
        float shift = Mathf.Round(cx * (1f - speed) / width) * width;
        Vector3 p = transform.position;
        p.x = cx * speed + shift;
        transform.position = p;
    }
}

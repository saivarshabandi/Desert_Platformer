using UnityEngine;

public class DesertCamera : MonoBehaviour
{
    public Transform target;
    public float minX = 0f, maxX = 50f, lead = 2f;

    void LateUpdate()
    {
        if (target == null) return;
        float x = Mathf.Clamp(target.position.x + lead, minX, maxX);
        Vector3 p = transform.position;
        p.x = Mathf.Lerp(p.x, x, 1f - Mathf.Exp(-8f * Time.deltaTime));
        transform.position = p;
    }
}

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class DesertPlayer : MonoBehaviour
{
    public float speed = 6.5f, jumpForce = 14f;
    public Sprite idle, jump, sparkle;
    public Sprite[] run;
    Rigidbody2D rb; SpriteRenderer sr;
    float move, lastGround = -10f, nextSpark;
    bool wasGrounded;
    bool jumpDown, jumpHeld;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    void ReadInput()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) { move = 0; jumpDown = jumpHeld = false; return; }
        move = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
        jumpDown = k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame;
        jumpHeld = k.spaceKey.isPressed || k.wKey.isPressed || k.upArrowKey.isPressed;
#else
        move = Input.GetAxisRaw("Horizontal");
        jumpDown = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
#endif
    }

    void Update()
    {
        ReadInput();
        bool grounded = Time.time - lastGround < 0.1f;
        if (jumpDown && grounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            lastGround = -10f; grounded = false;
            DesertSound.Jump();
        }
        if (!jumpHeld && rb.linearVelocity.y > 0.1f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.9f);

        if (grounded && Mathf.Abs(move) > 0.1f && Time.time > nextSpark) { nextSpark = Time.time + 0.07f; Spark(1); }
        if (grounded && !wasGrounded) Spark(6);
        wasGrounded = grounded;

        if (move != 0) sr.flipX = move < 0;
        if (!grounded) sr.sprite = jump;
        else if (Mathf.Abs(move) > 0.1f && run.Length > 0) sr.sprite = run[(int)(Time.time * 10f) % run.Length];
        else sr.sprite = idle;

        bool hurt = DesertGame.I != null && DesertGame.I.Invulnerable;
        sr.enabled = !hurt || ((int)(Time.time * 20f) % 2 == 0);
    }

    void FixedUpdate() { rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y); }

    void OnCollisionEnter2D(Collision2D c) { Check(c); }
    void OnCollisionStay2D(Collision2D c) { Check(c); }
    void Check(Collision2D c)
    {
        for (int i = 0; i < c.contactCount; i++)
            if (c.GetContact(i).normal.y > 0.5f) { lastGround = Time.time; return; }
    }

    void Spark(int n)
    {
        if (sparkle == null) return;
        for (int i = 0; i < n; i++)
        {
            var g = new GameObject("spark");
            g.transform.position = transform.position + new Vector3(Random.Range(-0.25f, 0.25f), -0.38f, 0f);
            var r = g.AddComponent<SpriteRenderer>();
            r.sprite = sparkle; r.sortingOrder = 11;
            g.AddComponent<DesertSparkle>();
        }
    }

    public void Bounce() { rb.linearVelocity = new Vector2(0f, 11f); }
    public void StopMoving() { rb.linearVelocity = Vector2.zero; }
}

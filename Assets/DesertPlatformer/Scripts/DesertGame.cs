using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DesertGame : MonoBehaviour
{
    public static DesertGame I;
    public Image[] hearts;
    public Sprite heartFull, heartEmpty;
    public Text coinText, message;
    public DesertPlayer player;
    public Vector3 spawn;
    public float invulnUntil;
    int life = 3, coins;
    bool over;

    public bool Invulnerable { get { return Time.time < invulnUntil; } }

    void Awake() { I = this; Time.timeScale = 1f; }
    void Start() { Refresh(); }

    void Update()
    {
        if (over) { if (RestartPressed()) SceneManager.LoadScene(SceneManager.GetActiveScene().name); return; }
        if (player.transform.position.y < -9f) Hurt(true);
    }

    bool RestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    public void AddCoin() { coins++; Refresh(); }

    public void Hurt(bool respawn = false)
    {
        if (over || (Invulnerable && !respawn)) return;
        life--;
        invulnUntil = Time.time + 1.2f;
        Refresh();
        if (life <= 0) { Show("Game over\nPress R to restart"); over = true; player.gameObject.SetActive(false); return; }
        if (respawn) { player.transform.position = spawn; player.StopMoving(); }
        else player.Bounce();
    }

    public void Win()
    {
        if (over) return;
        over = true;
        Show("You win!  Coins: " + coins + "\nPress R to play again");
    }

    void Show(string s) { message.text = s; message.gameObject.SetActive(true); }

    void Refresh()
    {
        for (int i = 0; i < hearts.Length; i++) hearts[i].sprite = i < life ? heartFull : heartEmpty;
        coinText.text = coins.ToString();
    }
}

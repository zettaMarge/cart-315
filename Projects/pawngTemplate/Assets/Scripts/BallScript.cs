
using System.Collections;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public AudioSource bonkSfx;
    private Rigidbody2D rb;
    public float ballSpeed = 1;
    private int[] dirs = { -1, 1 };
    public int scoreL;
    public int scoreR;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(Launch());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "rW")
        {
            ++scoreL;
            ResetGame();
            Debug.Log(scoreL);
        }
        else if (collision.gameObject.name == "lW")
        {
            ++scoreR;
            ResetGame();
            Debug.Log(scoreR);
        }

        if (collision.gameObject.name == "tW" || collision.gameObject.name == "bW")
        {
            bonkSfx.pitch = 0.75f;
            bonkSfx.Play();
        }
        else if (collision.gameObject.tag == "Paddle")
        {
            bonkSfx.pitch = 1f;
            bonkSfx.Play();
        }
    }

    public void ResetGame()
    {
        transform.position = new(0, 0, 0);
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        int xDir = dirs[Random.Range(0, dirs.Length)];
        int yDir = dirs[Random.Range(0, dirs.Length)];

        yield return new WaitForSeconds(1);

        rb.AddForce(transform.right * xDir * ballSpeed);
        rb.AddForce(transform.up * yDir * ballSpeed);
    }
}

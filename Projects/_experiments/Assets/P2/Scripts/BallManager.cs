using System.Collections;
using TMPro;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    [SerializeField]
    private float ballSpeed = 15;

    [SerializeField]
    private GameObject[] _players;

    [SerializeField]
    private TextMeshProUGUI _leftScoreTxtBox;

    [SerializeField]
    private TextMeshProUGUI _rightScoreTxtBox;

    private Rigidbody2D _rb;
    private readonly int[] _dirs = { -1, 1 };
    private int _scoreL = 0;
    private int _scoreR = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        StartCoroutine(ResetGame());
    }

    // Update is called once per frame
    void Update()
    {
        _leftScoreTxtBox.text = _scoreL.ToString();
        _rightScoreTxtBox.text = _scoreR.ToString();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            if (collision.gameObject.name == "Left")
            {
                ++_scoreR;
            }
            else
            {
                ++_scoreL;
            }

            StartCoroutine(ResetGame());
            Debug.Log($"Left Shrimp: {_scoreL} | Right Shrimp: {_scoreR}");
        }
    }

    public IEnumerator ResetGame()
    {
        foreach (GameObject player in _players)
        {
            player.GetComponent<ShrimpControl>().ResetPosition();
        }

        transform.position = new(0, 0, 0);
        _rb.linearVelocity = Vector2.zero;
        _rb.rotation = 0;
        int xDir = _dirs[Random.Range(0, _dirs.Length)];
        int yDir = _dirs[Random.Range(0, _dirs.Length)];

        yield return new WaitForSeconds(1);

        _rb.AddForce(transform.right * xDir * ballSpeed);
        _rb.AddForce(transform.up * yDir * ballSpeed);
    }
}

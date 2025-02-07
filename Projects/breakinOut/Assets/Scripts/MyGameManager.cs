using UnityEngine;
using UnityEngine.SceneManagement;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Instance;
    private int lives = 3;
    private int score = 0;

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DecreaseLife()
    {
        --lives;

        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene("GamaOvar");
    }

    public void StartGame()
    {
        lives = 3;
        score = 0;
        SceneManager.LoadScene("Breakout");
    }

    public void IncreaseScore()
    {
        ++score;
    }
}

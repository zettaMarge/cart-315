using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreUITxt;

    private int _score = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _scoreUITxt.text = _score.ToString();
    }

    public void AddScore(int incr)
    {
        _score += incr;
    }

    public void ResetScore()
    {
        _score = 0;
    }
}

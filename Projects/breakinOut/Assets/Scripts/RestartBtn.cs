using UnityEngine;
using UnityEngine.UI;

public class RestartBtn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => MyGameManager.Instance.StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

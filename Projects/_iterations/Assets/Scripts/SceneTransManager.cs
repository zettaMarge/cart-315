using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransManager : MonoBehaviour
{
    public static SceneTransManager Instance;

    private bool _isInit = false;
    private Vector3 _playerOverworldPosition = new(0, 0, 0);

    private void Awake()
    {
        if (Instance is not null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isInit = true;

        Image screen = GameObject.Find("FadeToBlack").GetComponent<Image>();
        Color updatedColor = screen.color;
        updatedColor.a = 0;
        screen.color = updatedColor;

        //StartBattle(_playerOverworldPosition);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_isInit)
        {
            if (scene.name.ToLower().Contains("battle"))
            {
                GameObject.Find("EnemyManager").GetComponent<EnemyManager>().SpawnEnemies();
                StartCoroutine(FadeScreen(false));
            }
            else if (scene.name.ToLower().Contains("overworld"))
            {
                GameObject player = GameObject.Find("Player");
                player.transform.position = _playerOverworldPosition;
                StartCoroutine(FadeScreen(false));
                //TODO enable movement
            }
        }
    }

    public void StartBattle(Vector3 playerOverworldPosition)
    {
        _playerOverworldPosition = playerOverworldPosition;
        StartCoroutine(FadeScreen(true, () => SceneManager.LoadScene("LegacyBattle")));
    }

    public void EndBattle()
    {
        StartCoroutine(FadeScreen(true, () => SceneManager.LoadScene("OverworldSeparate")));
    }

    private IEnumerator FadeScreen(bool toBlack, Action endFadeCallback = null)
    {
        Image screen = GameObject.Find("FadeToBlack").GetComponent<Image>();

        if (toBlack)
        {
            while (screen.color.a < 1)
            {
                Color updatedColor = screen.color;
                updatedColor.a += Time.deltaTime;
                screen.color = updatedColor;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (screen.color.a > 0)
            {
                Debug.Log(Time.deltaTime);
                Color updatedColor = screen.color;
                updatedColor.a -= Time.deltaTime;
                screen.color = updatedColor;
                yield return new WaitForEndOfFrame();
            }
        }

        if (endFadeCallback is not null)
        {
            endFadeCallback();
        }
    }
}

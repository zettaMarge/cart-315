using System.Collections;
using UnityEngine;

public class AlienSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private int _maxOnScreen = 7;

    private int _currentOnScreen = 0;

    private float halfHorzExtent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        halfHorzExtent = Camera.main.orthographicSize * Screen.width / Screen.height;
        StartCoroutine(SpawnAlien());
    }

    // Update is called once per frame
    void Update()
    {
        _currentOnScreen = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    private IEnumerator SpawnAlien()
    {
        if (_currentOnScreen < _maxOnScreen)
        {
            //Start at either one of the camera's horizontal edges
            int x = Random.Range(0, 2) == 0 ? Mathf.FloorToInt(-halfHorzExtent) : Mathf.CeilToInt(halfHorzExtent) + 1;
            float y = Random.Range(-1.5f, 2f);

            Vector3 position = new(x, y, 0);
            Instantiate(_prefab, position, Quaternion.identity);
        }

        float time = Random.Range(0.25f, 1);
        yield return new WaitForSeconds(time);

        StartCoroutine(SpawnAlien());
    }
}

using System.Collections;
using UnityEngine;

public class AlienSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private int _maxNbAliens = 7;

    private int _nbAliens = 0;
    private float _halfCameraHorzDist;
    private bool _isEnable = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _halfCameraHorzDist = Camera.main.orthographicSize * Screen.width / Screen.height;
        StartCoroutine(SpawnAlien());
    }

    // Update is called once per frame
    void Update()
    {
        _nbAliens = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    public void StopAliens()
    {
        _isEnable = false;
    }

    public void StartAliens()
    {
        _isEnable = true;
        StartCoroutine(SpawnAlien());
    }

    private IEnumerator SpawnAlien()
    {
        if (_nbAliens < _maxNbAliens)
        {
            //Spawn new alien at either one of the camera's horizontal edges
            int x = Random.Range(0, 2) == 0 ?
                Mathf.FloorToInt(-_halfCameraHorzDist) :
                Mathf.CeilToInt(_halfCameraHorzDist) + 1;
            float y = Random.Range(-2f, 2.5f);

            Vector3 position = new(x, y, 0);
            Instantiate(_prefab, position, Quaternion.identity);
        }

        float time = Random.Range(0.25f, 1);
        yield return new WaitForSeconds(time);

        if (_isEnable)
        {
            StartCoroutine(SpawnAlien());
        }
    }
}

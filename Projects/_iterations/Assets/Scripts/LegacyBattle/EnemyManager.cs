using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    private Vector3[] _spawnPos = { new Vector3(-0.18f, 0, 0.5f), new Vector3(3, 0, 0.5f), new Vector3(5.82f, 0, 0.5f) };
    private bool _isSpawning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _spawnPos.Length; ++i)
        {
            Instantiate(_prefab, _spawnPos[i], Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && !_isSpawning)
        {
            Debug.Log("End battle");
        }
    }
}

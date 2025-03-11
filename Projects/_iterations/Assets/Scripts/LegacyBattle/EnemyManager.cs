using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    private Vector3[] _spawnPos = { new Vector3(-0.18f, 0, 0.5f), new Vector3(3, 0, 0.5f)};

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < _spawnPos.Length; ++i)
        {
            Instantiate(_prefab, _spawnPos[i], Quaternion.identity);
        }
    }
}

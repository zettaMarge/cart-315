using System.Collections;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject spawnPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(spawnNewCircle());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator spawnNewCircle()
    {
        float xPos = Random.Range(-6, 6);
        Instantiate(spawnPrefab, new Vector3(xPos, transform.position.y, 0), transform.rotation);

        float waitTime = Random.Range(0.25f, 1.5f);
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(spawnNewCircle());
    }
}

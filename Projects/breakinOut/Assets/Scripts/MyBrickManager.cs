using UnityEngine;
using System.Linq;

public class MyBrickManager : MonoBehaviour
{
    public GameObject brickPrefab;
    public float spacingH = 1;
    public float spacingV = 1;
    public int rows = 3;
    public int cols = 5;
    public int nbBricks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nbBricks = rows * cols;
        InitBrickWall();
    }

    // Update is called once per frame
    void Update()
    {
        nbBricks = GetComponentsInChildren<Transform>().Where(child => child.gameObject.tag == "Brick").Count();

        if (nbBricks == 0)
        {
            MyGameManager.Instance.GameOver();
        }
    }

    public void InitBrickWall()
    {
        for (int i = 0; i < cols; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                float xPos = -cols + i * spacingH + transform.position.x;
                float yPos = rows - j * spacingV + transform.position.y;

                Instantiate(brickPrefab, new Vector3(xPos, yPos, 0), transform.rotation, transform);
            }
        }
    }
}

using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float distance = 15;

    [SerializeField]
    private int score = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new(transform.position.x, transform.position.y, transform.position.z);

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            movement.y -= distance * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            movement.y += distance * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movement.x += distance * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movement.x -= distance * Time.deltaTime;
        }

        transform.position = movement;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Circle")
        {
            Destroy(collision.gameObject);
            ++score;
            Debug.Log($"Score: {score}");
        }
    }
}

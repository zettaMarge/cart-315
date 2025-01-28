using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float _speed = 7;

    [SerializeField]
    private float _thrust = 3;

    private float _halfHorzExtent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _halfHorzExtent = Camera.main.orthographicSize * Screen.width / Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        WrapScreen();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log($"u dead");
        }
    }

    private void MovePlayer()
    {
        Vector3 movement = new(transform.position.x, transform.position.y, transform.position.z);

        //Jetpack thrust or gravity (vertical movement)
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            GetComponent<Rigidbody2D>().AddForce(transform.up * _thrust);
        }
        else
        {
            GetComponent<Rigidbody2D>().AddForce(-transform.up * _thrust*1.5f);
        }

        //Horizontal movement
        //Flip player sprite based on direction
        Vector3 scale = gameObject.transform.localScale;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movement.x += _speed * Time.deltaTime;
            scale.x = Mathf.Abs(gameObject.transform.localScale.x);
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movement.x -= _speed * Time.deltaTime;
            scale.x = -Mathf.Abs(gameObject.transform.localScale.x);
        }

        transform.position = movement;
        transform.localScale = scale;
    }

    private void WrapScreen()
    {
        Vector3 movementWrap = new(transform.position.x, transform.position.y, transform.position.z);

        if (transform.localScale.x > 0 && movementWrap.x - transform.localScale.x / 2 > _halfHorzExtent)
        {
            //facing right, wrap to left of screen
            movementWrap.x = -_halfHorzExtent;
            transform.position = movementWrap;
        }
        else if (transform.localScale.x < 0 && movementWrap.x - transform.localScale.x / 2 < -_halfHorzExtent)
        {
            //facing left, wrap to right of screen
            movementWrap.x = _halfHorzExtent;
            transform.position = movementWrap;
        }
    }
}

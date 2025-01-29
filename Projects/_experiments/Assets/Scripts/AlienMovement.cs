using System;
using UnityEngine;

public class AlienMovement : MonoBehaviour
{
    enum FlightPattern
    {
        Wave,
        Straight,
        Down
    }

    [SerializeField]
    private FlightPattern _pattern = FlightPattern.Down;

    [SerializeField]
    private float _speed = 5;

    [SerializeField]
    private float _angleModifier = 0.1f;

    [SerializeField]
    private float _heightModifier = 3;

    private float _angle = 0;
    private float _halfCameraHorzDist;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _halfCameraHorzDist = Camera.main.orthographicSize * Screen.width / Screen.height;

        //Randomize which type of movement is used
        _pattern = (FlightPattern)UnityEngine.Random.Range(0,3);

        if (_pattern == FlightPattern.Wave)
        {
            _speed = 3;
        }

        //Adjust rotation & speed so that, if coming from the right, the alien will move to the left & vice-versa
        _speed = transform.position.x > 0 ? -_speed : _speed;
        Vector3 angles = transform.rotation.eulerAngles;
        angles.z = transform.position.x > 0 ? 0 : 180;
        transform.rotation = Quaternion.Euler(angles);

        //Randomize the color
        float h = UnityEngine.Random.Range(0f, 1f);
        foreach (SpriteRenderer spr in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            spr.color = Color.HSVToRGB(h, 0.75f, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new(transform.position.x, transform.position.y, transform.position.z);

        //Horizontal movement
        movement.x += _speed * Time.deltaTime;

        //Vertical movement
        switch(_pattern)
        {
            case FlightPattern.Wave:
            {
                movement.y += (float)Math.Cos(_angle) * _heightModifier * Time.deltaTime;
                _angle -= _angleModifier;
                break;
            }
            case FlightPattern.Down:
            {
                movement.y -= Math.Abs(_speed)/2 * Time.deltaTime;
                break;
            }
            default: break; //Just fly straight ahead
        }

        transform.position = movement;

        //Handle offscreen despawn
        if (
            (_speed > 0 && movement.x - transform.localScale.x/2 > _halfCameraHorzDist) ||
            (_speed < 0 && movement.x + transform.localScale.x/2 < -_halfCameraHorzDist)
        )
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Handle environemnt collision despawn
        if (collision.gameObject.tag == "Environment")
        {
            Destroy(gameObject);
        }
    }
}

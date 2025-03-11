using UnityEngine;

public class OverworldMovement : MonoBehaviour
{
    [SerializeField]
    private KeyCode _upKeybind = KeyCode.UpArrow;
    [SerializeField]
    private KeyCode _downKeybind = KeyCode.DownArrow;
    [SerializeField]
    private KeyCode _leftKeybind = KeyCode.LeftArrow;
    [SerializeField]
    private KeyCode _rightKeybind = KeyCode.RightArrow;
    [SerializeField]
    private KeyCode _jumpKeybind = KeyCode.Space;
    [SerializeField]
    private float _speed = 7.5f;
    [SerializeField]
    private float _jumpForce = 50;

    private bool _inBattle = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_inBattle)
        {
            if (Input.GetKey(_upKeybind))
            {
                transform.position += new Vector3(0,0,1) * _speed * Time.deltaTime;
            }

            if (Input.GetKey(_downKeybind))
            {
                transform.position += new Vector3(0, 0, -1) * _speed * Time.deltaTime;
            }

            if (Input.GetKey(_leftKeybind))
            {
                transform.position += new Vector3(-1, 0, 0) * _speed * Time.deltaTime;
            }

            if (Input.GetKey(_rightKeybind))
            {
                transform.position += new Vector3(1, 0, 0) * _speed * Time.deltaTime;
            }

            if (Input.GetKeyDown(_jumpKeybind))
            {
                GetComponent<Rigidbody>().AddForce(transform.up * _jumpForce, ForceMode.Impulse);
            }
        }
    }
}

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
    private float _moveBuildupSpeed = 30;
    [SerializeField]
    private float _maxSpeed = 5;
    [SerializeField]
    private float _jumpForce = 4;
    [SerializeField]
    private bool _combinedBattle = false;

    private bool _inBattle = false;
    private bool _isGrounded = true;
    private Rigidbody _rb;
    private (KeyCode? hor, KeyCode? ver) _currentDirs = (null, null);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_inBattle)
        { 
            HandleDirectionInputs();

            Vector3 movement = new Vector3(
                    _currentDirs.hor == _leftKeybind? -1 : _currentDirs.hor == _rightKeybind ? 1 : 0,
                    0,
                    _currentDirs.ver == _downKeybind ? -1 : _currentDirs.ver == _upKeybind ? 1 : 0
                );

            float currentSpeed = _rb.linearVelocity.magnitude;

            if (currentSpeed > _maxSpeed)
            {
                //TODO fix that blj shit
                float overspeedAmount = currentSpeed - _maxSpeed;
                float slowDownForce = overspeedAmount * _moveBuildupSpeed;
                _rb.AddForce(movement * -1 * slowDownForce * Time.deltaTime, ForceMode.Impulse);
            }

            _rb.AddForce(movement * _moveBuildupSpeed * Time.deltaTime, ForceMode.Impulse);

            if (Input.GetKeyDown(_jumpKeybind) && _isGrounded)
            {
                _isGrounded = false;
                _rb.AddForce(new Vector3(0, 1, 0) * _jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BattleTrigger"))
        {
            _inBattle = true;

            if (_combinedBattle)
            {
                bool fromLeft = other.transform.position.x > transform.position.x;

                Vector3 adjustedPosition = gameObject.transform.position;
                adjustedPosition.z = other.gameObject.transform.position.z - 0.5f;
                adjustedPosition.x = other.gameObject.transform.position.x + (fromLeft? -1 * 1.5f : 1.5f);

                gameObject.transform.position = adjustedPosition;
                AttackManagerC.Instance.StartBattle(other.gameObject, 1, fromLeft);
            }
            else
            {
                SceneTransManager.Instance.StartBattle(transform.position, other.name);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }

    private void HandleDirectionInputs()
    {
        if (Input.GetKeyDown(_upKeybind))
        {
            _currentDirs.ver = _upKeybind;
        }
        else if (Input.GetKeyDown(_downKeybind))
        {
            _currentDirs.ver = _downKeybind;
        }
        
        if (Input.GetKeyDown(_leftKeybind))
        {
            _currentDirs.hor = _leftKeybind;
        }
        else if (Input.GetKeyDown(_rightKeybind))
        {
            _currentDirs.hor = _rightKeybind;
        }

        if (Input.GetKeyUp(_upKeybind))
        {
            _currentDirs.ver = Input.GetKey(_downKeybind) ? _downKeybind : null;
        }
        else if (Input.GetKeyUp(_downKeybind))
        {
            _currentDirs.ver = Input.GetKey(_upKeybind) ? _upKeybind : null;
        }
        
        if (Input.GetKeyUp(_leftKeybind))
        {
            _currentDirs.hor = Input.GetKey(_rightKeybind) ? _rightKeybind : null;
        }
        else if (Input.GetKeyUp(_rightKeybind))
        {
            _currentDirs.hor = Input.GetKey(_leftKeybind) ? _leftKeybind : null;
        }
    }

    public void SetBattleState(bool state)
    {
        _inBattle = state;
    }
}

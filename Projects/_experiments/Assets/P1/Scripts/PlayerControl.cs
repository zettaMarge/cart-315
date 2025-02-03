using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float _speed = 7;

    [SerializeField]
    private float _thrust = 3;

    [SerializeField]
    private int _maxNbLasers = 5;

    [SerializeField]
    private GameObject _laserPrefab;

    [SerializeField]
    private GameObject _alienSpawnerObj;

    private Vector3 _startPos;
    private float _halfCameraHorzDist;
    private int _nbLasers = 0;
    private bool _isShooting = false;
    private bool _isEnable = true;
    private bool _isGrounded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _halfCameraHorzDist = Camera.main.orthographicSize * Screen.width / Screen.height;
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        _nbLasers = FindObjectsByType(typeof(LaserManager), FindObjectsSortMode.None).Length;

        if (_isEnable)
        {
            MovePlayer();
            WrapScreen();

            if (Input.GetKey(KeyCode.V) && !_isShooting && _nbLasers <= _maxNbLasers)
            {
                StartCoroutine(Shoot());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //Stop the aliens from spawning & restart the game
            _isEnable = false;
            _isShooting = false;
            SetVisibility(false);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            GetComponent<Rigidbody2D>().linearVelocityY = 0;
            transform.position = _startPos;
            _isGrounded = false;
            _alienSpawnerObj.GetComponent<AlienSpawner>().StopAliens();
            StartCoroutine(TryRestart());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Bottom of Player touches top of platform
        if (
            collision.gameObject.tag == "Environment" &&
            collision.gameObject.transform.position.y + collision.gameObject.transform.localScale.y/2 <= transform.position.y - transform.localScale.y/2
        )
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //Bottom of Player was touching top of platform
        if (
            collision.gameObject.tag == "Environment" &&
            collision.gameObject.transform.position.y + collision.gameObject.transform.localScale.y / 2 <= transform.position.y - transform.localScale.y/2
        )
        {
            _isGrounded = false;
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
        else if (!_isGrounded)
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

        if (transform.localScale.x > 0 && movementWrap.x - transform.localScale.x / 2 > _halfCameraHorzDist)
        {
            //facing right, wrap to left of screen
            movementWrap.x = -_halfCameraHorzDist;
            transform.position = movementWrap;
        }
        else if (transform.localScale.x < 0 && movementWrap.x - transform.localScale.x / 2 < -_halfCameraHorzDist)
        {
            //facing left, wrap to right of screen
            movementWrap.x = _halfCameraHorzDist;
            transform.position = movementWrap;
        }
    }

    private void SetVisibility(bool state)
    {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = state;
        }
    }

    private IEnumerator Shoot()
    {
        _isShooting = true;
        Instantiate(_laserPrefab, new Vector3(0,0,0), Quaternion.identity);
        yield return new WaitForSeconds(0.15f);
        _isShooting = false;
    }

    private IEnumerator TryRestart()
    {
        yield return new WaitForSeconds(0.5f);

        int nbAliens = GameObject.FindGameObjectsWithTag("Enemy").Length;

        //Only restart once everything else has despawned, otherwise wait some more
        if (_nbLasers == 0 && nbAliens == 0)
        {
            SetVisibility(true);
            Object scoreManager = FindFirstObjectByType(typeof(ScoreManager));
            scoreManager.GetComponent<ScoreManager>().ResetScore();
            _alienSpawnerObj.GetComponent<AlienSpawner>().StartAliens();
            yield return new WaitForSeconds(0.5f);
            _isEnable = true;
        }
        else
        {
            StartCoroutine(TryRestart());
        }
    }
}

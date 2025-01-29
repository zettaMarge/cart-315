using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
    [SerializeField]
    private float _laserLength = 3f;

    private float _timer = 0.5f;
    private float _xrOffset = 0.35f;
    private float _xlOffset = -0.34f;
    private float _yOffset = 0.11f;
    private float _castWidth = 0.5f;

    private LineRenderer _laserLineRenderer;
    private GameObject _player;

    private Vector3 _direction;
    private Vector3 _p1;
    private Vector3 _p2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.Find("Player");

        if (_player == null)
        {
            Debug.LogError("No player found, cannot spawn laser");
            Destroy(gameObject);
        }

        _laserLineRenderer = GetComponent<LineRenderer>();
        _laserLineRenderer.enabled = true;

        _direction = _player.transform.localScale.x > 0 ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);

        _p1 = new(
            _player.transform.position.x + (_direction.x > 0 ? _xrOffset : _xlOffset),
            _player.transform.position.y + _yOffset,
            _player.transform.position.z
        );

        _p2 = new(
            _player.transform.position.x + (_direction.x > 0 ? _xrOffset : _xlOffset) + (_laserLength * _direction.x),
            _player.transform.position.y + _yOffset,
            _player.transform.position.z
        );

    }

    // Update is called once per frame
    void Update()
    {
        CheckLaserCollision();
    }

    private void CheckLaserCollision()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(_p1, _castWidth, _direction);
        //Check for laser collision
        foreach (RaycastHit2D hit in hits)
        {
            if (hit && hit.collider.gameObject.CompareTag("Enemy") && hit.distance <= _laserLength)
            {
                Destroy(hit.collider.gameObject);

                Object scoreManager = FindFirstObjectByType(typeof(ScoreManager));
                scoreManager.GetComponent<ScoreManager>().AddScore(10);

                Destroy(gameObject);
                break;
            }
        }

        //Actually draw the laser
        _laserLineRenderer.SetPosition(0, _p1);
        _laserLineRenderer.SetPosition(1, _p2);

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}

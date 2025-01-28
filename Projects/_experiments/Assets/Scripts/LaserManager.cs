using System.Collections;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
    [SerializeField]
    private float _laserWidth = 2f;

    [SerializeField]
    private float _laserLength = 5f;

    private LineRenderer _laserLineRenderer;
    private bool _isShooting = false;
    private int _score = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _laserLineRenderer = GetComponent<LineRenderer>();
        _laserLineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();
    }

    private void Shoot()
    {
        //_isShooting = true;
        Vector3 direction = transform.localScale.x > 0 ? Vector3.forward : -Vector3.forward;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _laserLength);

        if (Input.GetKey(KeyCode.V))
        {
            Debug.DrawLine(transform.position, transform.position + (_laserLength * direction));

            //Check for laser collision
            if (hit)
            {
                if (hit.collider.gameObject.CompareTag("Enemy"))
                {
                    Destroy(hit.collider.gameObject);
                    ++_score;
                    Debug.Log(_score);
                }
            }

            //Actually draw the laser
            _laserLineRenderer.SetPosition(0, transform.position);
            _laserLineRenderer.SetPosition(1, transform.position + (_laserLength * direction));
        }

        //yield return new WaitForSeconds(0.25f);

        //_isShooting = false;
    }
}

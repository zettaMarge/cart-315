using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrimpControl : MonoBehaviour
{
    [SerializeField]
    private KeyCode _forwardInput;

    [SerializeField]
    private KeyCode _turnRightInput;

    [SerializeField]
    private KeyCode _turnLeftInput;

    [SerializeField]
    private int _directionMod = 1;

    [SerializeField]
    private float _moveSpeed = 5;

    [SerializeField]
    private float _rotSpeed = 90;

    [SerializeField]
    private float _rotDelay = 0.25f;

    [SerializeField]
    private float _rotDelayIncr = 0.2f;

    [SerializeField]
    private GameObject _leadBodyPart;

    [Serializable]
    public class TrailPair
    {
        public GameObject bodyPart;
        public GameObject frontPairedAnchor;
        public Vector3 bodyInitPosition;
    }

    [SerializeField]
    private TrailPair[] _trailPairs;

    private Vector3 _leadInitPosition;
    private bool _isMoving = false;
    private bool _lastRotationLeft = true;
    private float _halfCameraHorzDist;
    private float _halfCameraVertDist;

    private void Awake()
    {
        _halfCameraVertDist = Camera.main.orthographicSize;
        _halfCameraHorzDist = _halfCameraVertDist * Screen.width / Screen.height;
        _leadInitPosition = _leadBodyPart.transform.position;

        foreach (TrailPair pair in _trailPairs)
        {
            pair.bodyInitPosition = pair.bodyPart.transform.position;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckMovementRotation();
        CheckScreenwrap();
    }

    private void CheckMovementRotation()
    {
        if (Input.GetKey(_forwardInput) || Input.GetKey(_turnRightInput) || Input.GetKey(_turnLeftInput))
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        if (Input.GetKey(_forwardInput))
        {
            _leadBodyPart.transform.position += _leadBodyPart.transform.right * _directionMod * _moveSpeed * Time.deltaTime;
        }

        float deltaRot = 0;

        if (Input.GetKey(_turnLeftInput))
        {
            deltaRot = _rotSpeed * Time.deltaTime;
            _lastRotationLeft = true;
        }
        else if (Input.GetKey(_turnRightInput))
        {
            deltaRot = -_rotSpeed * Time.deltaTime;
            _lastRotationLeft = false;
        }
        else if (_isMoving && Quaternion.Angle(_trailPairs[_trailPairs.Length - 1].bodyPart.transform.localRotation, _leadBodyPart.transform.localRotation) > 30)
        {
            _rotDelay = 0.25f;

            foreach (TrailPair pair in _trailPairs)
            {
                if (Quaternion.Angle(pair.bodyPart.transform.localRotation, _leadBodyPart.transform.localRotation) > 30)
                {
                    float bodyDeltaRot;

                    if (_lastRotationLeft)
                    {
                        bodyDeltaRot = _rotSpeed * Time.deltaTime;
                    }
                    else
                    {
                        bodyDeltaRot = -_rotSpeed * Time.deltaTime;
                    }

                    StartCoroutine(RotateBodyPart(pair.bodyPart, bodyDeltaRot));
                    _rotDelay += _rotDelayIncr;
                }
            }
        }

        _leadBodyPart.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _leadBodyPart.transform.localRotation.eulerAngles.z + deltaRot * _directionMod));

        _rotDelay = 0.25f;
        foreach (TrailPair pair in _trailPairs)
        {
            pair.bodyPart.transform.position = Vector2.MoveTowards(pair.bodyPart.transform.position, pair.frontPairedAnchor.transform.position, _moveSpeed * 2 * Time.deltaTime);
            StartCoroutine(RotateBodyPart(pair.bodyPart, deltaRot));
            _rotDelay += _rotDelayIncr;
        }
    }

    private void CheckScreenwrap()
    {
        float extraDist = 1;
        float deltaX = 0;
        float deltaY = 0;
        //Wrap if both head and tail are over threshold

        //Left-Right
        if (_leadBodyPart.transform.position.x > _halfCameraHorzDist + extraDist && _trailPairs[_trailPairs.Length - 1].bodyPart.transform.position.x > _halfCameraHorzDist + extraDist)
        {
            //going right, wrap to left of screen
            deltaX = -2 * (_halfCameraHorzDist + extraDist);
        }
        else if (_leadBodyPart.transform.position.x < -(_halfCameraHorzDist + extraDist) && _trailPairs[_trailPairs.Length - 1].bodyPart.transform.position.x < -(_halfCameraHorzDist + extraDist))
        {
            //going left, wrap to right of screen
            deltaX = 2 * (_halfCameraHorzDist + extraDist);
        }

        //Up-Down
        if (_leadBodyPart.transform.position.y > _halfCameraVertDist + extraDist && _trailPairs[_trailPairs.Length - 1].bodyPart.transform.position.y > _halfCameraVertDist + extraDist)
        {
            //going up, wrap to bottom of screen
            deltaY = -2 * (_halfCameraVertDist + extraDist);
        }
        else if (_leadBodyPart.transform.position.y < -(_halfCameraVertDist + extraDist) && _trailPairs[_trailPairs.Length - 1].bodyPart.transform.position.y < -(_halfCameraVertDist + extraDist))
        {
            //going down, wrap to top of screen
            deltaY = 2 * (_halfCameraVertDist + extraDist);
        }

        _leadBodyPart.transform.position = new(_leadBodyPart.transform.position.x + deltaX, _leadBodyPart.transform.position.y + deltaY, _leadBodyPart.transform.position.z);

        foreach (TrailPair pair in _trailPairs)
        {
            pair.bodyPart.transform.position = new(pair.bodyPart.transform.position.x + deltaX, pair.bodyPart.transform.position.y + deltaY, pair.bodyPart.transform.position.z);
        }
    }

    private IEnumerator RotateBodyPart(GameObject bodyPart, float deltaRot)
    {
        yield return new WaitForSeconds(_rotDelay);
        if (_isMoving)
        {
            bodyPart.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, bodyPart.transform.localRotation.eulerAngles.z + deltaRot * _directionMod));
        }
    }

    public void ResetPosition()
    {
        _leadBodyPart.transform.position = _leadInitPosition;
        _leadBodyPart.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

        foreach (TrailPair pair in _trailPairs)
        {
            pair.bodyPart.transform.position = pair.bodyInitPosition;
            pair.bodyPart.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }
}

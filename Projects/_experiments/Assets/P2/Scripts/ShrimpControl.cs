using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrimpControl : MonoBehaviour
{
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

    [SerializeField]
    private GameObject[] _otherBodyParts;

    [Serializable]
    public class TrailPair
    {
        public GameObject bodyPart;
        public GameObject frontPairedAnchor;
    }

    [SerializeField]
    private TrailPair[] _trailPairs;

    private bool _isMoving = false;
    private bool _lastRotationLeft = true;
    private float _halfCameraHorzDist;
    private float _halfCameraVertDist;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _halfCameraVertDist = Camera.main.orthographicSize;
        _halfCameraHorzDist = _halfCameraVertDist * Screen.width / Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        CheckMovementRotation();
        CheckScreenwrap();
    }

    private void CheckMovementRotation()
    {
        if (
    Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ||
    Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) ||
    Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)
)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            _leadBodyPart.transform.position += _leadBodyPart.transform.right * _moveSpeed * Time.deltaTime;
        }

        float deltaRot = 0;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            deltaRot = _rotSpeed * Time.deltaTime;
            _lastRotationLeft = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            deltaRot = -_rotSpeed * Time.deltaTime;
            _lastRotationLeft = false;
        }
        else if (_isMoving && Quaternion.Angle(_otherBodyParts[_otherBodyParts.Length - 1].transform.localRotation, _leadBodyPart.transform.localRotation) > 30)
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

        _leadBodyPart.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _leadBodyPart.transform.localRotation.eulerAngles.z + deltaRot));

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
        if (_leadBodyPart.transform.position.x > _halfCameraHorzDist + extraDist && _otherBodyParts[_otherBodyParts.Length - 1].transform.position.x > _halfCameraHorzDist + extraDist)
        {
            //going right, wrap to left of screen
            deltaX = -2 * (_halfCameraHorzDist + extraDist);
        }
        else if (_leadBodyPart.transform.position.x < -(_halfCameraHorzDist + extraDist) && _otherBodyParts[_otherBodyParts.Length - 1].transform.position.x < -(_halfCameraHorzDist + extraDist))
        {
            //going left, wrap to right of screen
            deltaX = 2 * (_halfCameraHorzDist + extraDist);
        }

        //Up-Down
        if (_leadBodyPart.transform.position.y > _halfCameraVertDist + extraDist && _otherBodyParts[_otherBodyParts.Length - 1].transform.position.y > _halfCameraVertDist + extraDist)
        {
            //going up, wrap to bottom of screen
            deltaY = -2 * (_halfCameraVertDist + extraDist);
        }
        else if (_leadBodyPart.transform.position.y < -(_halfCameraVertDist + extraDist) && _otherBodyParts[_otherBodyParts.Length - 1].transform.position.y < -(_halfCameraVertDist + extraDist))
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
            bodyPart.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, bodyPart.transform.localRotation.eulerAngles.z + deltaRot));
        }
    }
}

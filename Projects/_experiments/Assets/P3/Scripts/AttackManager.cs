using System;
using System.Collections;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    private enum State
    {
        Select,
        Transition,
        Attack
    }

    [SerializeField]
    private GameObject _selectHighlight;

    [SerializeField]
    private GameObject _hammer;

    [SerializeField]
    private GameObject _timerBase;

    [SerializeField]
    private GameObject[] _visualTimerSteps;

    [SerializeField]
    private float _inputTimerStep = 0.625f;

    [SerializeField]
    private float _inputlessLimit = 2.5f;

    [SerializeField]
    private int _damageValue = 2;

    private float _yInitHammerOffset;
    private Vector3 _basePosition;
    private State _currentState = State.Select;
    private GameObject[] _enemies;
    private int _idSelectedEnemy = 0;

    private string _hexOffColor = "#C54343";
    private string _hexOnColor = "#85E867";
    private float _inputTimer = 0;
    private bool _inputWasHeld = false;
    private int _nbVisualsOn = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _selectHighlight.transform.position = _enemies[0].transform.position;
        _yInitHammerOffset = _hammer.transform.position.y;
        _basePosition = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case State.Select: ManageSelectState(); break;
            case State.Attack: break;
            default: break;
        }
    }

    private void ManageSelectState()
    {
        _selectHighlight.transform.position = _enemies[_idSelectedEnemy].transform.position;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(AttackTransition(true));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _idSelectedEnemy = (_idSelectedEnemy + 1) % _enemies.Length;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _idSelectedEnemy = (_idSelectedEnemy - 1) % _enemies.Length;
        }
    }

    private void ManageAttackState()
    {
        if (!Input.GetKey(KeyCode.LeftArrow) && !_inputWasHeld)
        {
            _inputTimer += Time.deltaTime;

            if (_inputTimer >= _inputlessLimit) // If the action command input is ignored long enough
            {
                StartCoroutine(AttackFail(0.5f));
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !_inputWasHeld) // Reset the timer for checking the input instead
        {
            _inputWasHeld = true;
            _inputTimer = 0;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && _inputWasHeld)
        {
            _inputTimer += Time.deltaTime;

            if (_inputTimer >= _inputTimerStep * (_nbVisualsOn + 1) && _nbVisualsOn < 4) // Light up the visual cues in sequence after each timer step
            {
                ColorUtility.TryParseHtmlString(_hexOnColor, out Color onColor);
                _visualTimerSteps[_nbVisualsOn].GetComponent<SpriteRenderer>().color = onColor;
                ++_nbVisualsOn;
            }
            else if (_inputTimer >= _inputTimerStep * (_nbVisualsOn + 1)) // Input was held too long
            {
                StartCoroutine(AttackFail(0));
            }
        }
        else if (!Input.GetKey(KeyCode.LeftArrow) && _inputWasHeld)
        {
            if (_nbVisualsOn == 4)
            {
                StartCoroutine(AttackSuccess());
            }
            else // Early input release
            {
                StartCoroutine(AttackFail(0.5f));
            }
        }
    }

    private IEnumerator AttackFail(float damageModifier)
    {
        _currentState = State.Transition;
        yield return new WaitForSeconds(1);
        //TODO
        StartCoroutine(AttackTransition(false));
    }

    private IEnumerator AttackSuccess() 
    {
        _currentState = State.Transition;
        yield return new WaitForSeconds(1);
        //TODO
        StartCoroutine(AttackTransition(false));
    }

    private IEnumerator AttackTransition(bool towardsEnemy)
    {
        _currentState = State.Transition;
        float timeElapsed = 0;
        float transitionDuration = 2;
        Vector3 init = gameObject.transform.position;

        if (towardsEnemy)
        {
            _selectHighlight.GetComponent<SpriteRenderer>().enabled = false;
            Vector3 end = _enemies[_idSelectedEnemy].transform.position;
            end.x -= 2.5f;

            while (timeElapsed < transitionDuration)
            {
                float smoothTime = timeElapsed / transitionDuration;
                smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
                gameObject.transform.position = Vector3.Lerp(init, end, smoothTime);
                timeElapsed += Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }

            // Make hammer & timer appear after movement is completed
            //_hammer.transform.position = new(gameObject.transform.position.x, _yInitHammerOffset, 0);
            //_hammer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            SetNestledVisualsEnabled(_hammer, true);
            SetNestledVisualsEnabled(_timerBase, true);
        }
        else
        {
            // Make hammer & timer disappear & reset before movement is started
            SetNestledVisualsEnabled(_hammer, false);
            _hammer.transform.position = new(gameObject.transform.position.x, _yInitHammerOffset, 0);
            _hammer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            SetNestledVisualsEnabled(_timerBase, false);
            ColorUtility.TryParseHtmlString(_hexOffColor, out Color offColor);
            Array.ForEach(_visualTimerSteps, step => step.GetComponent<SpriteRenderer>().color = offColor);

            _nbVisualsOn = 0;
            _inputTimer = 0;

            Vector3 end = _basePosition;

            while (timeElapsed < transitionDuration)
            {
                float smoothTime = timeElapsed / transitionDuration;
                smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
                gameObject.transform.position = Vector3.Lerp(init, end, smoothTime);
                timeElapsed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            // Reinitialize enemy list & selection
            _enemies = GameObject.FindGameObjectsWithTag("Enemy");
            _idSelectedEnemy = 0;
            _selectHighlight.GetComponent<SpriteRenderer>().enabled = true;
        }

        yield return new WaitForSeconds(1);
        _currentState = towardsEnemy ? State.Attack : State.Select;
    }

    private void SetNestledVisualsEnabled(GameObject parent, bool isEnabled)
    {
        foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = isEnabled;
        }
    }
}

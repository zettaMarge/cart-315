using System;
using System.Collections;
using System.Linq;
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
    private float _inputlessLimit = 5;

    [SerializeField]
    private int _damageValue = 2;

    [SerializeField]
    private float _bonkSpeed = 0.15f;

    private Vector3 _basePosition;
    private State _currentState = State.Select;
    private GameObject[] _enemies;
    private int _idSelectedEnemy = 0;
    private bool _isGettingEnemies = false;

    private string _hexOffColor = "#C54343";
    private string _hexOnColor = "#85E867";
    private float _inputTimer = 0;
    private bool _inputWasHeld = false;
    private int _nbVisualsOn = 0;

    private AudioSource _sfxBonk;
    private AudioSource _sfxCountdown;
    private AudioSource _sfxEndTimer;
    private AudioSource _sfxSelect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _basePosition = gameObject.transform.position;

        _sfxBonk = GameObject.Find("BonkSfx").GetComponent<AudioSource>();
        _sfxCountdown = GameObject.Find("CountdownSfx").GetComponent<AudioSource>();
        _sfxEndTimer = GameObject.Find("EndTimerSfx").GetComponent<AudioSource>();
        _sfxSelect = GameObject.Find("SelectSfx").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case State.Select: ManageSelectState(); break;
            case State.Attack: ManageAttackState(); break;
            default: break;
        }
    }

    private void ManageSelectState()
    {
        if (_enemies.Length > 0)
        {
            _selectHighlight.transform.position = _enemies[_idSelectedEnemy].transform.position;
            _selectHighlight.transform.Rotate(new Vector3(0, 0, 50 * Time.deltaTime));

            if (!_selectHighlight.GetComponentInChildren<SpriteRenderer>().enabled)
            {
                SetNestledVisualsEnabled(_selectHighlight, true);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartCoroutine(PlaySfx(_sfxSelect, 1.25f));
                StartCoroutine(AttackTransition(true));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && _enemies.Length > 1)
            {
                StartCoroutine(PlaySfx(_sfxSelect, 1));
                _idSelectedEnemy = (_idSelectedEnemy + 1) % _enemies.Length;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && _enemies.Length > 1)
            {
                StartCoroutine(PlaySfx(_sfxSelect, 1));
                _idSelectedEnemy = ((_idSelectedEnemy == 0 ? _enemies.Length : _idSelectedEnemy) - 1) % _enemies.Length;
            }
        }
        else
        {
            if (!_isGettingEnemies)
            {
                StartCoroutine(GetNewEnemies());
            }
        }
    }

    private void ManageAttackState()
    {
        if (!Input.GetKey(KeyCode.LeftArrow) && !_inputWasHeld)
        {
            _inputTimer += Time.deltaTime;

            if (_inputTimer >= _inputlessLimit) // If the action command input is ignored long enough
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, -80), 3, ProcessAttack, 0.5f));
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !_inputWasHeld) // Reset the timer for checking the input instead
        {
            StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0,0,30), 1));
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

                if (_nbVisualsOn < 4)
                {
                    StartCoroutine(PlaySfx(_sfxCountdown, 1));
                }
                else
                {
                    StartCoroutine(PlaySfx(_sfxEndTimer, 1));
                }
            }
            else if (_inputTimer >= _inputTimerStep * (_nbVisualsOn + 1)) // Input was held too long
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, 100), 1, ProcessAttack, 0));
            }
        }
        else if (!Input.GetKey(KeyCode.LeftArrow) && _inputWasHeld)
        {
            if (_nbVisualsOn == 4)
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, -80), 1, ProcessAttack));
            }
            else // Early input release
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, -80), 3, ProcessAttack, 0.5f));
            }
        }
    }


    private IEnumerator RotateHammer(Quaternion start, Quaternion end, float timeMod, Func<float, IEnumerator> onFinished = null, float param = 1)
    {
        if (onFinished is not null)
            _currentState = State.Transition;

        float timeElapsed = 0;

        while (timeElapsed < _bonkSpeed * timeMod)
        {
            float smoothTime = timeElapsed / (_bonkSpeed * timeMod);
            smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
            _hammer.transform.localRotation = Quaternion.Slerp(start, end, smoothTime);
            timeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        if (onFinished is not null)
            StartCoroutine(onFinished(param));
    }

    private IEnumerator ProcessAttack(float damageModifier)
    {
        StartCoroutine(PlaySfx(_sfxBonk, 1));
        _enemies[_idSelectedEnemy].GetComponent<Enemy>().DecreaseHPs((int)Math.Floor(_damageValue * damageModifier));
        yield return new WaitForSeconds(1);
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
            SetNestledVisualsEnabled(_selectHighlight, false);
            _selectHighlight.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Vector3 end = _enemies[_idSelectedEnemy].transform.position;
            end.x -= 2.5f;

            while (timeElapsed < transitionDuration)
            {
                float smoothTime = timeElapsed / transitionDuration;
                smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
                gameObject.transform.position = Vector3.Lerp(init, end, smoothTime);
                timeElapsed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            // Make hammer & timer appear after movement is completed
            SetNestledVisualsEnabled(_hammer, true);
            _timerBase.GetComponent<SpriteRenderer>().enabled = true;
            SetNestledVisualsEnabled(_timerBase, true);
        }
        else
        {
            // Make hammer & timer disappear & reset before movement is started
            SetNestledVisualsEnabled(_hammer, false);
            _hammer.transform.localRotation = Quaternion.Euler(0,0,0);

            _timerBase.GetComponent<SpriteRenderer>().enabled = false;
            SetNestledVisualsEnabled(_timerBase, false);
            ColorUtility.TryParseHtmlString(_hexOffColor, out Color offColor);
            Array.ForEach(_visualTimerSteps, step => step.GetComponent<SpriteRenderer>().color = offColor);

            _nbVisualsOn = 0;
            _inputTimer = 0;
            _inputWasHeld = false;

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

            if (_enemies.Length > 0)
            {
                _selectHighlight.transform.position = _enemies[0].transform.position;
                SetNestledVisualsEnabled(_selectHighlight, true);
            }
        }

        _currentState = towardsEnemy ? State.Attack : State.Select;
    }

    private void SetNestledVisualsEnabled(GameObject parent, bool isEnabled)
    {
        foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = isEnabled;
        }
    }

    private IEnumerator PlaySfx(AudioSource src, float pitch)
    {
        src.pitch = pitch;
        src.Play();

        while (src.isPlaying)
        {
            yield return null;
        }

        src.pitch = 1;
    }

    private IEnumerator GetNewEnemies()
    {
        _isGettingEnemies = true;
        yield return new WaitForSeconds(1);
        _enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _isGettingEnemies = false;
    }
}

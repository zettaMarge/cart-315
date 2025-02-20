using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    private enum State
    {
        Select,
        Target,
        Transition,
        Attack
    }

    private enum Attack
    {
        Jump,
        Hammer
    }

    [SerializeField]
    private GameObject _selectHighlight;

    [SerializeField]
    private GameObject _hammer;

    [SerializeField]
    private GameObject _timerBase;

    [SerializeField]
    private GameObject _jumpVisualInput;

    [SerializeField]
    private GameObject[] _hammerVisualTimerSteps;

    [SerializeField]
    private float _inputTimerStep = 0.625f;

    [SerializeField]
    private float _inputlessLimit = 5;

    [SerializeField]
    private int _damageValue = 2;

    [SerializeField]
    private float _bonkSpeed = 0.15f;

    [SerializeField]
    private TextMeshProUGUI _selectedAttackTxtBox;

    private Vector3 _basePosition;
    private State _currentState = State.Select;
    private GameObject[] _enemies;
    private int _idSelectedEnemy = 0;
    private bool _isGettingEnemies = false;

    private int _selectedAttackId = 0;
    private bool _canAcceptJumpInput = false;
    private bool _successfulJump = false;

    private bool _inputWasHeld = false;

    private string _hexOffColor = "#C54343";
    private string _hexOnColor = "#85E867";
    private float _inputTimer = 0;
    private int _nbHammerVisualsOn = 0;

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
        _selectedAttackTxtBox.text = ((Attack)_selectedAttackId).ToString();

        switch (_currentState)
        {
            case State.Select: ManageSelectState(); break;
            case State.Target: ManageTargetState(); break;
            case State.Attack:
                {
                    switch((Attack)_selectedAttackId)
                    {
                        case Attack.Jump: ManageJumpAttack(); break;
                        case Attack.Hammer: ManageHammerAttackState(); break;
                    }
                    
                    break;
                }
            default: break;
        }
    }

    private void ManageSelectState()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(PlaySfx(_sfxSelect, 1.25f));
            _currentState = State.Target;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(PlaySfx(_sfxSelect, 1));
            _selectedAttackId = (_selectedAttackId + 1) % Enum.GetValues(typeof(Attack)).Length;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(PlaySfx(_sfxSelect, 1));
            _selectedAttackId = ((_selectedAttackId == 0 ? Enum.GetValues(typeof(Attack)).Length : _selectedAttackId) - 1) % Enum.GetValues(typeof(Attack)).Length;
        }
    }

    private void ManageTargetState()
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

    private void ManageJumpAttack()
    {
        if (Input.GetKeyDown(KeyCode.Return) && _canAcceptJumpInput && !_inputWasHeld)
        {
            StartCoroutine(PlaySfx(_sfxBonk, 1));
            _inputWasHeld = true;
            _successfulJump = true;
        }
        else if (Input.GetKeyDown(KeyCode.Return) && !_canAcceptJumpInput && !_inputWasHeld)
        {
            _inputWasHeld = true;
            _successfulJump = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && (Attack)_selectedAttackId == Attack.Jump)
        {
            _canAcceptJumpInput = true;
            ColorUtility.TryParseHtmlString(_hexOnColor, out Color onColor);
            _jumpVisualInput.GetComponent<SpriteRenderer>().color = onColor;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && (Attack)_selectedAttackId == Attack.Jump)
        {
            _canAcceptJumpInput = false;

            if (!_successfulJump)
            {
                ColorUtility.TryParseHtmlString(_hexOffColor, out Color offColor);
                _jumpVisualInput.GetComponent<SpriteRenderer>().color = offColor;
            }
        }
    }

    private void ManageHammerAttackState()
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

            if (_inputTimer >= _inputTimerStep * (_nbHammerVisualsOn + 1) && _nbHammerVisualsOn < 4) // Light up the visual cues in sequence after each timer step
            {
                ColorUtility.TryParseHtmlString(_hexOnColor, out Color onColor);
                _hammerVisualTimerSteps[_nbHammerVisualsOn].GetComponent<SpriteRenderer>().color = onColor;
                ++_nbHammerVisualsOn;

                if (_nbHammerVisualsOn < 4)
                {
                    StartCoroutine(PlaySfx(_sfxCountdown, 1));
                }
                else
                {
                    StartCoroutine(PlaySfx(_sfxEndTimer, 1));
                }
            }
            else if (_inputTimer >= _inputTimerStep * (_nbHammerVisualsOn + 1)) // Input was held too long
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, 100), 1, ProcessAttack, 0));
            }
        }
        else if (!Input.GetKey(KeyCode.LeftArrow) && _inputWasHeld)
        {
            if (_nbHammerVisualsOn == 4)
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, -80), 1, ProcessAttack));
            }
            else // Early input release
            {
                StartCoroutine(RotateHammer(_hammer.transform.localRotation, Quaternion.Euler(0, 0, -80), 3, ProcessAttack, 0.5f));
            }
        }
    }

    private IEnumerator BetterArcTransition()
    {
        float timeElapsed = 0;
        float duration = 1.25f;
        Vector3 init = gameObject.transform.position;
        Vector3 end = _enemies[_idSelectedEnemy].transform.position;
        Vector3 anchor = new((init.x + end.x)/2, 2.75f, 0);

        while (timeElapsed < duration)
        {
            if (_successfulJump)
            {
                break;
            }

            float smoothTime = timeElapsed / duration;
            smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
            float oneMinusT = 1 - smoothTime;

            gameObject.transform.position =
                (oneMinusT * oneMinusT * init) +
                (2 * smoothTime * oneMinusT * anchor) +
                (smoothTime * smoothTime * end); 

            if (_canAcceptJumpInput && !_inputWasHeld)
            {
                //slow down a bit for better timing
                timeElapsed += Time.deltaTime / 1.75f;
            }
            else
            {
                timeElapsed += Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }

        timeElapsed = 0;

        if (_successfulJump)
        {
            yield return new WaitForSeconds(0.1f);

            //arc back
            end = init;
            init = gameObject.transform.position;

            while (timeElapsed < duration / 1.5f)
            {
                float smoothTime = timeElapsed / (duration / 1.5f);
                smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
                float oneMinusT = 1 - smoothTime;

                gameObject.transform.position =
                    (oneMinusT * oneMinusT * init) +
                    (2 * smoothTime * oneMinusT * anchor) +
                    (smoothTime * smoothTime * end);

                timeElapsed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
        }

        StartCoroutine(ProcessAttack(_successfulJump ? 1 : 0.5f));
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
        if ((Attack)_selectedAttackId == Attack.Hammer || (Attack)_selectedAttackId == Attack.Jump && !_successfulJump)
        {
            StartCoroutine(PlaySfx(_sfxBonk, 1));
        }
        
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

            AdjustAttackVisuals(true);
        }
        else
        {
            AdjustAttackVisuals(false);

            _inputWasHeld = false;
            _successfulJump = false;
            _canAcceptJumpInput = false;

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
        }

        if ((Attack)_selectedAttackId == Attack.Jump && towardsEnemy)
        {
            StartCoroutine(BetterArcTransition());
        }

        _currentState = towardsEnemy ? State.Attack : State.Select;
    }

    private void AdjustAttackVisuals(bool isEnabled)
    {
        switch ((Attack)_selectedAttackId)
        {
            case Attack.Jump: 
                {
                    SpriteRenderer sr = _jumpVisualInput.GetComponent<SpriteRenderer>();
                    if (isEnabled)
                    {
                        sr.enabled = true;
                    }
                    else
                    {
                        sr.enabled = false;
                        ColorUtility.TryParseHtmlString(_hexOffColor, out Color offColor);
                        sr.color = offColor;
                    }

                    break; 
                }
            case Attack.Hammer:
                {
                    if (isEnabled)
                    {
                        SetNestledVisualsEnabled(_hammer, true);
                        _timerBase.GetComponent<SpriteRenderer>().enabled = true;
                        SetNestledVisualsEnabled(_timerBase, true);
                    }
                    else
                    {
                        SetNestledVisualsEnabled(_hammer, false);
                        _hammer.transform.localRotation = Quaternion.Euler(0, 0, 0);

                        _timerBase.GetComponent<SpriteRenderer>().enabled = false;
                        SetNestledVisualsEnabled(_timerBase, false);
                        ColorUtility.TryParseHtmlString(_hexOffColor, out Color offColor);
                        Array.ForEach(_hammerVisualTimerSteps, step => step.GetComponent<SpriteRenderer>().color = offColor);

                        _nbHammerVisualsOn = 0;
                        _inputTimer = 0;
                    }

                    break;
                }
        }
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

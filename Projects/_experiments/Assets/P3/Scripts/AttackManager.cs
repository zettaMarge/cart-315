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

    private float _yInitHammerOffset;
    private Vector3 _basePosition;
    private State _currentState = State.Select;
    private GameObject[] _enemies;
    private int _idSelectedEnemy = 0;

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
            StartCoroutine(AttackTransition(true, State.Attack));
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
        //check if input.getkey, increase timer
        //if timer beyond range & still holding, fail
        //if timer before range & release, fail
        //if timer in range & release, success
        //transition back
    }

    private IEnumerator AttackTransition(bool towardsEnemy, State nextState)
    {
        _currentState = State.Transition;
        float timeElapsed = 0;
        float transitionDuration = 2;
        Vector3 init = gameObject.transform.position;

        if (towardsEnemy)
        {
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

            // Make hammer appear after movement is completed
            _hammer.transform.position = new(gameObject.transform.position.x, _yInitHammerOffset, 0);
            _hammer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            foreach (SpriteRenderer sr in _hammer.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.enabled = towardsEnemy;
            }
        }
        else
        {
            // Make hammer disappear before movement is started
            Vector3 end = _basePosition;
            _hammer.transform.position = new(gameObject.transform.position.x, _yInitHammerOffset, 0);
            _hammer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            foreach (SpriteRenderer sr in _hammer.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.enabled = towardsEnemy;
            }

            while (timeElapsed < transitionDuration)
            {
                float smoothTime = timeElapsed / transitionDuration;
                smoothTime = smoothTime * smoothTime * (3f - 2f * smoothTime);
                gameObject.transform.position = Vector3.Lerp(init, end, smoothTime);
                timeElapsed += Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }
        }

        yield return new WaitForSeconds(1);
        _currentState = nextState;
    }
}

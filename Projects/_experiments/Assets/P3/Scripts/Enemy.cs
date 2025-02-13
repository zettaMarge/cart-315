using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int currentHps = 0;

    private SpriteRenderer _sr;
    private AudioSource _sfx;
    private ParticleSystem _particles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float h = Random.Range(0f, 1f);
        _sr = gameObject.GetComponent<SpriteRenderer>();
        _sr.color = Color.HSVToRGB(h, 0.75f, 1f);

        currentHps = Random.Range(1, 6);

        _sfx = gameObject.GetComponent<AudioSource>();
        _particles = gameObject.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Kill()
    {
        _particles.Play();
        _sfx.Play();

        while (_sr.color.a > 0)
        {
            Color currentColor = _sr.color;
            currentColor.a -= Time.deltaTime;
            _sr.color = currentColor;

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    public void DecreaseHPs(int lost)
    {
        currentHps -= lost;

        if (currentHps <= 0)
        {
            StartCoroutine(Kill());
        }
    }
}

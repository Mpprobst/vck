using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    
    [Header("Input")]
    [SerializeField] string horizInput = "Horizontal";
    [SerializeField] string actionInput = "Fire1";

    [Header("Animation")]
    [SerializeField] string walkAnimName = "moving";
    [SerializeField] string kickAnimName = "kick";
    [SerializeField] string dirAnimName = "direction";
    [SerializeField] string caughtAnimName = "caught";

    [SerializeField] GameObject model;
    [SerializeField] Sprite topHalfSprite;

    [SerializeField] private AudioClip[] hitClips, kickClips, attackClips;
    [SerializeField] private AudioClip deathClip;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private Trigger kickTrigger;
    private Health health;

    private bool canMove;
    private bool moving;
    private bool kicking;
    private bool arresting;

    private float timeSinceLastKick;
    private float kickDelay = 0.5f;
    private Vector3 startPos;

    private void Start()
    {
        //Initialize();
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        kickTrigger = GetComponentInChildren<Trigger>();
        kickTrigger.objectEnteredEvent.AddListener(KickHit);

        health = GetComponent<Health>();
        health.hitEvent = new UnityEvent();
        health.hitEvent.AddListener(Hit);
        health.deathEvent = new UnityEvent();
        health.deathEvent.AddListener(Die);
        UIManager.Instance.SetHealth((int)health.maxHealth);

        timeSinceLastKick = Time.time;
        startPos = transform.position;

        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        moving = false;
        float x = Input.GetAxisRaw(horizInput);

        if (Mathf.Abs(x) > 0)
        {
            moving = true;
        }
        if (x < 0 && model.transform.localScale.x > 0)
        {
            animator.SetInteger(dirAnimName, -1);
            model.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (x > 0 && model.transform.localScale.x < 0)
        {
            animator.SetInteger(dirAnimName, 1);
            model.transform.localScale = new Vector3(1, 1, 1);
        }

        Vector2 movement = new Vector2(x * speed * Time.deltaTime, 0);
        animator.SetBool(walkAnimName, moving);

        if (Input.GetButtonDown(actionInput))
        {
            Kick();
        }

        if (Time.time - timeSinceLastKick < kickDelay)
        {
            movement = new Vector2();
        }
        //rb.velocity = movement;
        transform.Translate(movement);

    }

    private void Kick()
    {
        audioSource.clip = kickClips[Random.Range(0, kickClips.Length)];
        audioSource.Play();
        timeSinceLastKick = Time.time;
        animator.SetTrigger(kickAnimName);
    }

    private void KickHit(GameObject hit)
    {
        ChildController child = hit.GetComponentInParent<ChildController>();
        if (child)
        {
            audioSource.clip = attackClips[Random.Range(0, attackClips.Length)];
            audioSource.Play();
            child.GetComponent<Health>().Damage(100);
            if (!child.isBad)
            {
                //health.Damage(1);
                GameManager.Instance.AddScore(-1000);
            }
            else
            {
                GameManager.Instance.AddScore(500);
            }
        }
    }

    public int GetDistanceTraveled()
    {
        int dist = Mathf.FloorToInt(transform.position.x - startPos.x);
        if (dist < 0) dist = 0;
        return dist;
    }

    private void Hit()
    {
        audioSource.clip = hitClips[Random.Range(0, hitClips.Length)];
        audioSource.Play();
        UIManager.Instance.RemoveHP();
    }

    private void Die()
    {
        audioSource.clip = deathClip;
        audioSource.Play();
        health.deathEvent = new UnityEvent();
        // TODO: do death animation
        canMove = false;
        GameManager.Instance.GameOver();
        animator.SetTrigger(caughtAnimName);
        StartCoroutine(ArrestingAnimation());
    }

    private IEnumerator ArrestingAnimation()
    {
        arresting = true;
        while (arresting)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            model.transform.localScale = new Vector3(model.transform.localScale.x * -1f, 1, 1);
        }
        Destroy(animator);
        model.transform.localScale = new Vector3(1,1,1);
        model.GetComponent<SpriteRenderer>().sprite = topHalfSprite;
    }

    public void Caught()
    {
        arresting = false;
    }

    public bool IsAlive()
    {
        return health.IsAlive();
    }
}

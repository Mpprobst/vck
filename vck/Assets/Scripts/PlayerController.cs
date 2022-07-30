using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    public float dialogueDelay = 5f;
    
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
    [SerializeField] private AudioSource primarySource, secondarySource;

    [SerializeField] private AudioClip[] startClips, hitClips, kickClips, successClips, failClips, deathClips, miscClips;

    private Rigidbody2D rb;
    private Animator animator;
    private Trigger kickTrigger;
    private Health health;

    private bool canMove;
    private bool moving;
    private bool kicking;
    private bool arresting;

    private float kickTime, dialogueTime;
    private float kickDelay = 0.5f;
    private Vector3 startPos;

    public int ChildrenKicked { get { return childrenKicked; } }
    public int DemonsVanquished { get { return demonsVanquished; } }
    private int childrenKicked, demonsVanquished;

    private void Start()
    {
        //Initialize();
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        kickTrigger = GetComponentInChildren<Trigger>();
        kickTrigger.objectEnteredEvent.AddListener(KickHit);

        health = GetComponent<Health>();
        health.hitEvent = new UnityEvent();
        health.hitEvent.AddListener(Hit);
        health.deathEvent = new UnityEvent();
        health.deathEvent.AddListener(Die);
        UIManager.Instance.SetHealth((int)health.maxHealth);

        kickTime = Time.time;
        dialogueTime = Time.time;
        startPos = transform.position;

        canMove = true;
        secondarySource.clip = GetRandomClip(startClips);
        secondarySource.PlayDelayed(1f);
        childrenKicked = 0;
        demonsVanquished = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                health.Damage(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GameManager.Instance.AddScore(500);
            }
        }
        moving = false;
        bool doKick = Input.GetButtonDown(actionInput);
        float x = Input.GetAxisRaw(horizInput);

        if (Input.touchCount > 0)
        {
            doKick = false;
            Touch touch = Input.GetTouch(0);
            float width = (float)Screen.width / 2f;
            float height = (float)Screen.height / 2f;
            Vector2 pos = touch.position;
            pos.x = (pos.x - width) / width;
            //Debug.Log($"Touch pos: {pos} and screen width is {width}");
            if (pos.x < -0.5f)
            {
                x = -1;
            }
            else if (pos.x > 0.5f)
            {
                x = 1;
            }
            else
            {
                doKick = true;
            }
        }


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

        if (doKick)
        {
            Kick();
        }

        if (Time.time - kickTime < kickDelay)
        {
            movement = new Vector2();
        }
        transform.Translate(movement);

        if (Time.time - dialogueTime > dialogueDelay)
        {
            SayHey();
        }
    }

    private void Kick()
    {
        if (Time.time - kickTime < kickDelay) return;
        primarySource.clip = GetRandomClip(kickClips);
        primarySource.Play();
        kickTime = Time.time;
        animator.SetTrigger(kickAnimName);
    }

    private void KickHit(GameObject hit)
    {
        ChildController child = hit.GetComponentInParent<ChildController>();
        if (child)
        {
            child.GetComponent<Health>().Damage(100);
            if (!child.isBad)
            {
                secondarySource.clip = GetRandomClip(failClips);
                health.Damage(1);
                GameManager.Instance.AddScore(-1000);
                childrenKicked++;
            }
            else
            {
                secondarySource.clip = GetRandomClip(successClips);
                GameManager.Instance.AddScore(500);
                demonsVanquished++;
            }
            secondarySource.PlayDelayed(1f);
        }
    }

    private void SayHey()
    {
        ChildController[] children = GameObject.FindObjectsOfType<ChildController>();
        float minDist = 5f;
        for (int i = 0; i < children.Length; i++)
        {
            float dist = Vector3.Distance(children[i].transform.position, transform.position);
            if (dist < minDist) minDist = dist;
            if (dist < 5f && !secondarySource.isPlaying && children[i].IsTargetingPlayer())
            {
                secondarySource.clip = GetRandomClip(miscClips);
                secondarySource.Play();
                break;
            }
        }
        dialogueDelay = Mathf.Clamp(minDist / 2f, 1.5f, 6f);
        dialogueTime = Time.time;
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        return clips[Random.Range(0, clips.Length)];
    }

    public int GetDistanceTraveled()
    {
        int dist = Mathf.FloorToInt(transform.position.x - startPos.x);
        if (dist < 0) dist = 0;
        return dist;
    }

    private void Hit()
    {
        primarySource.clip = GetRandomClip(hitClips);
        primarySource.Play();
        UIManager.Instance.RemoveHP();
    }

    private void Die()
    {
        secondarySource.clip = GetRandomClip(deathClips);
        secondarySource.PlayDelayed(0.25f);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChildController : MonoBehaviour
{
    public Transform model;
    public GameObject secretFace;   // demon face for bad children when revealed
    public bool isBad;
    public UnityEvent<bool> defeatEvent;
    public float speedRange = 0.25f;
    public float attackDelay = 0.5f;
    public float desapwnTime = 10f;
    public float minDist = 10f;

    private Health health;
    private MoveTowards mover;
    private Trigger attackTrigger;
    private PlayerController player;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D collider;

    private bool revealed;
    private float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();    
        health.deathEvent = new UnityEvent();
        health.deathEvent.AddListener(Defeated);

        player = GameObject.FindObjectOfType<PlayerController>();
        mover = GetComponent<MoveTowards>();
        mover.destinationReached = new UnityEvent();
        mover.destinationReached.AddListener(Attack);
        float baseSpeed = 0.5f + DifficultyManager.Instance.Difficulty/2f;
        mover.speed = Random.Range(baseSpeed - speedRange, baseSpeed + speedRange);
        mover.SetTarget(player.transform);

        attackDelay = Random.Range(attackDelay, attackDelay + 0.1f) / DifficultyManager.Instance.Difficulty;
        mover.moveDelay = 2f * attackDelay;
        attackTrigger = GetComponentInChildren<Trigger>();
        attackTrigger.objectEnteredEvent = new UnityEvent<GameObject>();
        attackTrigger.objectEnteredEvent.AddListener(TriggerHit);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        collider = GetComponentInChildren<Collider2D>();

        secretFace.SetActive(false);
        spawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("moving", mover.IsMoving());
        if (Vector3.Distance(player.transform.position, transform.position) > minDist && Time.time - spawnTime > desapwnTime)
        {
            Defeated();
        }
    }

    private void Defeated()
    {
        animator.SetTrigger("die");
        defeatEvent.Invoke(isBad);
        mover.ClearTarget();
        if (collider) Destroy(collider);
        if (rb) Destroy(rb);
        StartCoroutine(DeathAnim());
    }

    private IEnumerator DeathAnim()
    {
        float t = 0f;
        float length = 0.5f;
        float total_rot = 720f;

        while (t < length)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            float val = Mathf.Lerp(0, Mathf.PI, t / length);
            float angle = Mathf.Lerp(0, total_rot, t / length);

            float y = Mathf.Sin(val) / 2f;
            float x = (Mathf.Cos(val) - 1f) / 4f;
            model.localPosition = new Vector3(x, y, 0);
            model.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        t = 0;
        float fall_time = length / 2f;

        while (t < fall_time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            model.localPosition -= new Vector3(0, Time.deltaTime);
            float angle = Mathf.Lerp(total_rot, total_rot+total_rot/2f, t / fall_time);
            model.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        Destroy(gameObject);
    }

    private void Attack()
    {
        if (!isBad)
        {
            // if child is not bad, have it keep walking past vck
            collider.isTrigger = true;
            GameObject carrot = new GameObject();
            carrot.transform.parent = transform;
            carrot.transform.localPosition = new Vector2(model.transform.localScale.x*5f, 0);
            mover.SetTarget(carrot.transform);
        }
        else if (health.IsAlive())
        {
            // TODO: have a delay before child begins attack to give player time to react.
            // try making the attack animation lower in height so player can kick the head
            // as well as making the attack from a shorter distance so we don't have to kick
            // while child is flying at us
            if (!revealed)
            {
                secretFace.SetActive(true);
                mover.speed += 0.25f;
            }
            StartCoroutine(StartAttack());
        }
    }

    private IEnumerator StartAttack()
    {
        animator.speed = DifficultyManager.Instance.Difficulty / 2f;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(attackDelay);
        animator.SetTrigger("attack");
        animator.speed = 1f;
    }

    private void AttackEnd()
    {
        mover.Resume();
    }

    private void TriggerHit(GameObject hitObject)
    {
        PlayerController player = hitObject.GetComponentInParent<PlayerController>();
        if (player)
        {
            player.GetComponent<Health>().Damage(1f);
        }
    }
}

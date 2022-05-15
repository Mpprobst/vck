using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChildController : MonoBehaviour
{
    public Transform model;
    public bool isBad;
    public UnityEvent<bool> defeatEvent;


    private Health health;
    private MoveTowards mover;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D collider;


    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();    
        health.deathEvent = new UnityEvent();
        health.deathEvent.AddListener(Defeated);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        collider = GetComponent<Collider2D>();

        PlayerController player = GameObject.FindObjectOfType<PlayerController>();
        mover = GetComponent<MoveTowards>();
        mover.destinationReached = new UnityEvent();
        mover.destinationReached.AddListener(Attack);
        mover.SetTarget(player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("moving", mover.IsMoving());   
    }

    private void Defeated()
    {
        Debug.Log("child kicked");
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
        if (!isBad) return;
    }

    private void AttackEnd()
    {
        mover.Resume();
    }
}

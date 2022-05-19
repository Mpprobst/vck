using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] GameObject model;
    private Rigidbody2D rb;
    private Animator animator;
    private Trigger kickTrigger;
    private Health health;

    private bool moving;
    private bool kicking;

    private float timeSinceLastKick;
    private float kickDelay = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        kickTrigger = GetComponentInChildren<Trigger>();
        kickTrigger.objectEnteredEvent.AddListener(KickHit);
        health = GetComponent<Health>();
        timeSinceLastKick = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
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

        Vector2 movement = new Vector2(x * speed * Time.deltaTime * 100f, 0);
        animator.SetBool(walkAnimName, moving);

        if (Input.GetButtonDown(actionInput))
        {
            Kick();
        }

        if (Time.time - timeSinceLastKick < kickDelay)
        {
            movement = new Vector2();
        }
        rb.velocity = movement;

    }

    private void Kick()
    {
        timeSinceLastKick = Time.time;
        animator.SetTrigger(kickAnimName);
    }

    private void KickHit(GameObject hit)
    {
        Debug.Log("kickhit");
        ChildController child = hit.GetComponentInParent<ChildController>();
        if (child)
        {
            child.GetComponent<Health>().Damage(100);
            if (!child.isBad)
            {
                health.Damage(1);
            }
        }
    }
}

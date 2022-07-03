using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveTowards : MonoBehaviour
{
    public float speed = 1f;
    public float arrivalDistance = 0.5f;
    public float moveDelay = 0.25f;

    public UnityEvent destinationReached;

    private bool pause;
    private float moveTime;

    private Transform target;
    private Rigidbody2D rb;
    private Vector3 originalScale;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveTime = Time.time;
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (Vector3.Distance(target.position, transform.position) < arrivalDistance && !pause)
            {
                Pause();
                destinationReached.Invoke();
            }
            else if (pause && Time.time - moveTime > moveDelay)
            {
                moveTime = Time.time;
                Invoke("Resume", moveDelay-0.1f);
                //Resume();
            }

            if (!pause)
            {
                Vector3 diff = (target.position - transform.position).normalized * speed;
                Vector2 dir = new Vector3(diff.x, 0);

                if (dir.x < 0 && transform.localScale.x > 0)
                {
                    transform.localScale = new Vector3(originalScale.x * -1f, originalScale.y, originalScale.z);
                }
                else if (dir.x > 0 && transform.localScale.x < 0)
                {
                    transform.localScale = originalScale;
                }
                //rb.AddForce(dir);
                //transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * speed);
                transform.Translate(dir * Time.deltaTime);
            }
        }
    }

    public bool IsMoving()
    {
        return !pause && target != null;
    }

    public void SetTarget(Transform tar)
    {
        target = tar;
        Resume();
    }

    public void ClearTarget()
    {
        target = null;
        Pause();
    }

    public void Pause()
    {
        pause = true;
    }

    public void Resume()
    {
        pause = false;
        moveTime = Time.time;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public UnityEvent<GameObject> objectEnteredEvent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        objectEnteredEvent.Invoke(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        objectEnteredEvent.Invoke(collision.gameObject);
    }
}

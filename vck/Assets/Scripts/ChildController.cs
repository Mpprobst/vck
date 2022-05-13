using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChildController : MonoBehaviour
{

    public bool isBad;
    public UnityEvent<bool> defeatEvent;
    private Health health;


    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();    
        health.deathEvent = new UnityEvent();
        health.deathEvent.AddListener(Defeated);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Defeated()
    {
        defeatEvent.Invoke(isBad);
        Destroy(gameObject);
    }
}

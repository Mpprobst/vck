using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent deathEvent;
    public float maxHealth;

    private float currHealth;

    // Start is called before the first frame update
    void Start()
    {
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsAlive()
    {
        return currHealth > 0;
    }

    public void Damage(float damage)
    {
        currHealth -= damage;
        if (currHealth <= 0)
        {
            currHealth = 0;
            deathEvent.Invoke();
            Debug.Log("health is 0");
        }
        StartCoroutine(HitAnimation());
    }

    private IEnumerator HitAnimation()
    {
        float flashTime = 0.5f;
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.color = Color.red;
        yield return new WaitForSecondsRealtime(flashTime);
        renderer.color = Color.white;
    }
}

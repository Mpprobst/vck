using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent hitEvent, deathEvent;
    public float maxHealth;
    private float invincibilityTime = 0.5f;

    private float currHealth;
    private float hitTime;

    // Start is called before the first frame update
    void Start()
    {
        currHealth = maxHealth;
        hitTime = Time.time;
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
        if (Time.time - hitTime < invincibilityTime) return;
        hitTime = Time.time;

        currHealth -= damage;
        if (currHealth <= 0)
        {
            currHealth = 0;
            deathEvent.Invoke();
        }
        hitEvent.Invoke();
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

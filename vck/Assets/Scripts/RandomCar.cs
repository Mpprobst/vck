using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCar : MonoBehaviour
{
    [SerializeField] Color[] colors;
    public Transform lightsObj;
    public bool dontDestroy;

    // Start is called before the first frame update
    void Start()
    {
        if (!dontDestroy)
        {
            if (Random.Range(0f, 1f) < 0.75f) Destroy(gameObject);
            RandomCar[] otherCars = GameObject.FindObjectsOfType<RandomCar>();
            foreach (var car in otherCars)
                if (Vector3.Distance(transform.position, car.transform.position) < 5f && car != this)
                    Destroy(gameObject);
            PoliceCar[] policeCars = GameObject.FindObjectsOfType<PoliceCar>();
            foreach (var car in policeCars)
                if (Vector3.Distance(transform.position, car.transform.position) < 5f)
                    Destroy(gameObject);
        }

        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        rend.color = colors[Random.Range(0, colors.Length)];
        float dir = Random.Range(-1f, 1f);

        if (dir < 0.2f) transform.localScale.Scale(new Vector3(-1f, 1f, 1f));
        if (Random.Range(0f, 1f) < 0.7f)
        {
            for (int i = 0; i < lightsObj.childCount; i++)
                lightsObj.GetChild(i).gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

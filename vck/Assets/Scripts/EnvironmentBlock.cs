using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBlock : MonoBehaviour
{
    public Transform endpoint;
    public GameObject carPrefab;

    public void Start()
    {
        if (carPrefab)
        {
            if (GetComponentInChildren<PoliceCar>()) return;
            RandomCar car = Instantiate(carPrefab, transform).GetComponent<RandomCar>();
            float x = Random.Range(0, endpoint.transform.localPosition.x);
            car.transform.localPosition = new Vector3(x, 0.2f, 0);
        }
    }
}

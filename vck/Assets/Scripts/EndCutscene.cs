using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EndCutscene : MonoBehaviour
{
    public UnityEvent complete;
    public GameObject policePrefab;
    public Transform foreground;

    private PlayerController player;
    private MoveTowards carMover;

    public void Play()
    {
        player = GameObject.FindObjectOfType<PlayerController>();

        Vector3 spawnPos = new Vector3(player.transform.position.x-7f, 0, 0);
        GameObject spawnedCar = Instantiate(policePrefab, spawnPos, new Quaternion(), foreground);
        carMover = spawnedCar.AddComponent<MoveTowards>();
        carMover.arrivalDistance = 0.5f;
        carMover.speed = 3f;
        carMover.destinationReached = new UnityEvent();
        carMover.destinationReached.AddListener(MakeArrest);
        carMover.SetTarget(player.transform);

    }

    private void MakeArrest()
    {
        StartCoroutine(Pickup());
    }

    private IEnumerator Pickup()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        player.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(1f);

        GameObject carrot = new GameObject();
        carrot.transform.parent = carMover.transform;
        carrot.transform.localPosition = new Vector2(carMover.transform.localScale.x * 5f, 0);
        carMover.SetTarget(carrot.transform);

        yield return new WaitForSecondsRealtime(1f);

        Complete();
    }

    private void Complete()
    {
        complete.Invoke();
    }
}

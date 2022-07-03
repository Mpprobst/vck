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
    private PoliceCar policeCar;

    public void Play()
    {
        player = GameObject.FindObjectOfType<PlayerController>();

        Vector3 spawnPos = new Vector3(player.transform.position.x-7f, 0, 0);
        policeCar = Instantiate(policePrefab, spawnPos, new Quaternion(), foreground).GetComponent<PoliceCar>();
        policeCar.mover = policeCar.gameObject.AddComponent<MoveTowards>();
        policeCar.mover.arrivalDistance = 0.5f;
        policeCar.mover.speed = 3f;
        policeCar.mover.destinationReached = new UnityEvent();
        policeCar.mover.destinationReached.AddListener(MakeArrest);
        policeCar.mover.SetTarget(player.transform);
    }

    private void MakeArrest()
    {
        StartCoroutine(Pickup());
    }

    private IEnumerator Pickup()
    {
        policeCar.mover.destinationReached = new UnityEvent();
        policeCar.mover.ClearTarget();
        Destroy(GameObject.FindObjectOfType<CameraFollow>());
        player.Caught();

        yield return new WaitForSecondsRealtime(0.5f);

        Vector3 startPos = player.transform.position;
        Vector3 goalPos = policeCar.backseat.position;
        float moveTime = 1f;
        float t = 0f;
        while (t < moveTime)
        {
            player.transform.localPosition = Vector3.Lerp(startPos, goalPos, t / moveTime);
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        player.transform.parent = policeCar.backseat;
        Destroy(player);

        // TODO: play car start sfx
        yield return new WaitForSecondsRealtime(1f);

        GameObject carrot = new GameObject();
        carrot.transform.position = new Vector2(policeCar.transform.position.x + 10f, 0);
        policeCar.mover.SetTarget(carrot.transform);

        yield return new WaitForSecondsRealtime(1f);

        Complete();
    }

    private void Complete()
    {
        complete.Invoke();
    }
}

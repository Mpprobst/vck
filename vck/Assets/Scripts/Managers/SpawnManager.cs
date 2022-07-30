using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnManager : MonoBehaviour
{
    public Vector3 spawnOffset;     // just out of view of player camera 
    public int maxEntities = 4;
    public GameObject entityPrefab;
    public float badnessRatio = 0.5f;
    public float spawnDelay = 5f;

    private bool canSpawn;
    private Transform player;
    private List<ChildController> spawnedEntities;
    private float lastSpawnTime;
    private int totalSpawned, totalDestroyed;

    public void BeginSpawning()
    {
        canSpawn = true;
        player = GameObject.FindObjectOfType<PlayerController>().transform;
        spawnedEntities = new List<ChildController>();
        totalSpawned = 0;
        totalDestroyed = 0;
        SpawnEntity();
    }

    public void StopSpawning()
    {
        canSpawn = false;
        foreach (var entity in spawnedEntities)
            entity.isBad = false;   
    }

    // Update is called once per frame
    void Update()
    {
        if (!canSpawn) return;

        if (Time.time - lastSpawnTime > spawnDelay)
        {
            EntityDestroyed(false);
            SpawnEntity();
        }
    }

    private void SpawnEntity()
    {
        maxEntities = Mathf.RoundToInt(DifficultyManager.Instance.Difficulty * 2);
        lastSpawnTime = Time.time;
        if (spawnedEntities.Count >= maxEntities) return;

        float dir = Random.Range(-1f, 1.5f);
        if (dir < 0) dir = -1f;
        else dir = 1f;
        Vector3 spawnPos = new Vector3(spawnOffset.x * dir, spawnOffset.y, spawnOffset.z) + player.transform.position;
        ChildController spawned = Instantiate(entityPrefab, spawnPos, new Quaternion(), transform).GetComponent<ChildController>();
        totalSpawned++;
        // TODO: increase number of children, their speed, and attack speed as time goes on
        badnessRatio = 0.25f + (DifficultyManager.Instance.Difficulty-1f) / 5f;
        spawned.isBad = Random.Range(0f, 1f) < badnessRatio;
        spawned.defeatEvent = new UnityEvent<bool>();
        spawned.defeatEvent.AddListener(EntityDestroyed);
        spawnedEntities.Add(spawned);
    }

    private void EntityDestroyed(bool isEnemy)
    {
        for (int i = 0; i < spawnedEntities.Count; i++)
        {
            if (spawnedEntities[i] == null)
            {
                spawnedEntities.RemoveAt(i);
                totalDestroyed++;
                break;
            }
        }
    }
}

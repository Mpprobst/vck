using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    // how close player can be to next edge before spawning new env
    // probably dont need splits
    public float foregroundSpawnDist = 5f;
    public float middlegroundSpawnDist = 10f;
    public float backgroundSpawnDist = 25f;

    public Transform foreground, middleground, background;
    public EnvironmentBlockData[] foregroundEnvs, middlegroundEnvs, backgroundEnvs;

    private EnvironmentBlock recentForeground, recentMiddleground, recentBackground;
    private Transform player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<PlayerController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        SpawnBlock(foregroundEnvs, ref recentForeground, foregroundSpawnDist, foreground);
        SpawnBlock(middlegroundEnvs, ref recentMiddleground, middlegroundSpawnDist, middleground);
        SpawnBlock(backgroundEnvs, ref recentBackground, backgroundSpawnDist, background);
    }

    private void SpawnBlock(EnvironmentBlockData[] blockData, ref EnvironmentBlock recent, float spawnDist, Transform parent)
    {
        if (parent == null || blockData.Length == 0) return;
        float distance = 0;
        Vector3 pos = parent.position;
        if (recent)
        {
            distance = Vector3.Distance(player.position, recent.endpoint.position);
            pos = recent.endpoint.transform.position;
        }

        if (distance < spawnDist)
        {
            int randint = Random.Range(0, blockData.Length);
            EnvironmentBlock newBlock = Instantiate(blockData[randint].blockPrefab, pos, new Quaternion(), parent).GetComponent<EnvironmentBlock>();
            recent = newBlock;
        }
    }
}

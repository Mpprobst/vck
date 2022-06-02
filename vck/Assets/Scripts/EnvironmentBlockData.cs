using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "ScriptableObjects/BlockData", order = 1)]
public class EnvironmentBlockData : ScriptableObject
{
    public GameObject blockPrefab;
    public float probability, difficulty;
    // TODO: create blocks for driveways, intersections, houses, parked cars, electric poles with vck posters, no scrumping signs, and trees
}

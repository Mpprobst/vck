using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceCar : MonoBehaviour
{
    public Transform backseat;
    [System.NonSerialized] public MoveTowards mover;
    [SerializeField] AudioSource sirenSFX;

    public void PlaySiren()
    {
        sirenSFX.Play();
    }

}

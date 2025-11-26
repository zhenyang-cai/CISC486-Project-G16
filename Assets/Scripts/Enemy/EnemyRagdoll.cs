using System.Collections;
using UnityEngine;

public class EnemyRagdoll : MonoBehaviour
{
    public float ragdollTime = 10f;

    void Start()
    {
        StartCoroutine(DespawnAfterTime(ragdollTime));
    }

    IEnumerator DespawnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
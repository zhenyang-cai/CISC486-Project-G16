using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class SyncEnemyVars : NetworkBehaviour
{
    public readonly SyncVar<float> currentHealth = new SyncVar<float>();
    public readonly SyncVar<StateMachine> enemyStateMachine = new SyncVar<StateMachine>();

    void Awake()
    {
        currentHealth.OnChange += OnHealthChanged;
    }

    void OnHealthChanged(float previous, float next, bool asServer)
    {
        Debug.Log($"[SyncEnemyVars] OnHealthChanged prev={previous} next={next} asServer={asServer}");

        // EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
           enemyHealth.OnHealthUpdatedFromNetwork(next, asServer);
    }
}
using System.Collections;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEditor;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Properties")]
    public float startingHealth;
    public float currentHealth;
    public float ragdollTimer = 5f;
    public AudioClip deathAudio;
    [Range(0, 1)] public float deathAudioVolume = 0.5f;
    public float ragdollForce = 1500f; // How much force to apply to the death ragdoll
    
    [Header("References")]
    public Animator animator;
    public CharacterController characterController;
    public Enemy enemyScript;
    public GameObject ragdoll;

    bool _isDead = false;

    public override void OnStartServer()
    {
        currentHealth = startingHealth;
        SyncEnemyVars healthObj = gameObject.GetComponent<SyncEnemyVars>();
        if (healthObj is not null)
        {
            healthObj.currentHealth.Value = currentHealth;
        }

        if (animator is null) animator = gameObject.GetComponent<Animator>();
        if (characterController is null) characterController = gameObject.GetComponent<CharacterController>();
        if (enemyScript is null) enemyScript = gameObject.GetComponent<Enemy>();
    }

    // DISCLOSURE: Some of the code for death ragdolling was generated using AI and verified afterwards
    public void Death()
    {
        // Notify observing clients so they can play ragdoll visuals / disable animator locally.
        Debug.Log("[EnemyHealth] Server starting Death(), notifying observers.");
        RpcNotifyDeath();
        Despawn(gameObject);
    }

    // Inform all observing clients that this enemy has died so they can show local death visuals.
    [ObserversRpc]
    void RpcNotifyDeath()
    {
        Debug.Log($"[EnemyHealth] RpcNotifyDeath received on client for {gameObject.name}");

        AudioSource.PlayClipAtPoint(deathAudio, transform.position, deathAudioVolume);

        GameObject newRagdoll = Instantiate(ragdoll);
        newRagdoll.transform.position = transform.position;
        newRagdoll.transform.rotation = transform.rotation;

        // adding a little force
        Rigidbody[] bones = newRagdoll.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody bone in bones) {
            bone.AddForce(-transform.forward * 1500);
        }
    }

    // Called by SyncEnemyVars when the sync'd value changes.
    public void OnHealthUpdatedFromNetwork(float newHealth, bool asServer)
    {
        Debug.Log($"[EnemyHealth] OnHealthUpdatedFromNetwork newHealth={newHealth} asServer={asServer}");
        currentHealth = newHealth;
        if (asServer && currentHealth <= 0f && !_isDead)
        {
            _isDead = true;
            Death();
        }
    }
}

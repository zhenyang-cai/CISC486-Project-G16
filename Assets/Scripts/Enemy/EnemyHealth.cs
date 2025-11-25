using System.Collections;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Properties")]
    public float startingHealth;
    public float currentHealth;
    public float ragdollTimer = 5f;
    
    [Header("References")]
    public Animator animator;
    public CharacterController characterController;
    public Enemy enemyScript;

    Rigidbody[] _ragdollRigidbodies;
    Collider[] _ragdollColliders;
    NetworkTransform[] _childNetworkTransforms;
    bool _ragdollInitialized = false;
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
    public IEnumerator Death()
    {
        // Notify observing clients so they can play ragdoll visuals / disable animator locally.
        Debug.Log("[EnemyHealth] Server starting Death(), notifying observers.");
        RpcNotifyDeath();

        // Server-side: disable animation and control so server physics/logic stops.
        if (animator != null) animator.enabled = false;
        if (characterController != null) characterController.enabled = false;
        if (enemyScript != null) enemyScript.enabled = false;

        yield return new WaitForSeconds(ragdollTimer);
        Despawn(gameObject);
    }

    // Inform all observing clients that this enemy has died so they can show local death visuals.
    [ObserversRpc]
    void RpcNotifyDeath()
    {
        Debug.Log($"[EnemyHealth] RpcNotifyDeath received on client for {gameObject.name}");

        // Prepare ragdoll components (collect and set to safe initial state)
        InitRagdollIfNeeded();

        // Disable animator/character controller so transforms reflect the final animated pose
        if (animator != null) animator.enabled = false;
        if (characterController != null) characterController.enabled = false;

        // Disable any NetworkTransform components on child bones to avoid transform smoothing
        if (_childNetworkTransforms != null)
        {
            foreach (var nt in _childNetworkTransforms)
            {
                if (nt != null) nt.enabled = false;
            }
        }

        // Zero velocities and enable physics on ragdoll rigidbodies
        if (_ragdollRigidbodies != null)
        {
            foreach (var rb in _ragdollRigidbodies)
            {
                if (rb == null) continue;
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Enable colliders after a FixedUpdate to avoid large impulse from initial overlap
        if (_ragdollColliders != null && _ragdollColliders.Length > 0)
        {
            StartCoroutine(EnableCollidersNextFixedUpdate());
        }
    }

    void InitRagdollIfNeeded()
    {
        if (_ragdollInitialized) return;

        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        _ragdollColliders = GetComponentsInChildren<Collider>(true);
        _childNetworkTransforms = GetComponentsInChildren<NetworkTransform>(true);

        // Put rigidbodies into safe kinematic state and clear velocities
        if (_ragdollRigidbodies != null)
        {
            foreach (var rb in _ragdollRigidbodies)
            {
                if (rb == null) continue;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        // Disable colliders until ragdoll activation to avoid overlap impulses
        if (_ragdollColliders != null)
        {
            foreach (var col in _ragdollColliders)
            {
                if (col == null) continue;
                col.enabled = false;
            }
        }

        _ragdollInitialized = true;
    }

    IEnumerator EnableCollidersNextFixedUpdate()
    {
        yield return new WaitForFixedUpdate();
        if (_ragdollColliders != null)
        {
            foreach (var col in _ragdollColliders)
            {
                if (col == null) continue;
                col.enabled = true;
            }
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
            StartCoroutine(Death());
        }
    }
}

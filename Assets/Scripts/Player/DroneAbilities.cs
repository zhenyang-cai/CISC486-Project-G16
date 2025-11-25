// using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneAbilities : NetworkBehaviour
{
    [Header("Basic stun")]
    public float stunDuration = 5f;
    public float stunCooldown = 10f;
    public float stunRange = 30f;

    public float _stunCooldownTimer = 0f;

    [Header("Distraction")]
    public GameObject distraction;
    public float abilityRadius = 50f;
    public float abilityDuration = 10f;

    [Header("References")]
    public PlayerInput playerInput;
    public Camera playerCamera;
    public Canvas abilityUI;

    InputAction attackAction;
    InputAction interactAction;
    LayerMask layerMask;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            GetInputRefs();
            attackAction.performed += ctx => PerformAttack();

            abilityUI.enabled = true;

            layerMask = LayerMask.GetMask("Enemy", "Ground");
        }
    }

    void FixedUpdate()
    {
        if (_stunCooldownTimer > 0)
        {
            _stunCooldownTimer -= Time.fixedDeltaTime;
        }
    }


    void PerformAttack()
    {
        if (_stunCooldownTimer > 0f) return;

        print("attack");
        Vector3 origin = playerCamera.transform.position;
        Vector3 forward = playerCamera.transform.TransformDirection(Vector3.forward);
        Debug.DrawLine(origin, origin + forward * stunRange, Color.cyan, 1f);

        Debug.Log("[DroneAbilities] Client requesting stun via ServerStunRay");
        ServerStunRay(origin, forward, layerMask.value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerStunRay(Vector3 rayOrigin, Vector3 rayDirection, int mask)
    {
        Debug.Log($"[DroneAbilities] ServerStunRay invoked on server (mask={mask})");
        if (!IsServerInitialized) return;

        RaycastHit hit;
        if (!Physics.Raycast(rayOrigin, rayDirection, out hit, stunRange, mask))
        {
            Debug.Log("[DroneAbilities] ServerStunRay: no hit registered");
            return;
        }

        Debug.DrawLine(rayOrigin, hit.point, Color.red, 1f);
        if (!hit.collider.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            Debug.LogWarning($"[DroneAbilities] ServerStunRay hit {hit.collider.gameObject.name} but no Enemy component.");
            return;
        }

        enemy.ApplyStun();
        if (Owner != null)
            TargetStartStunCooldown(Owner);
    }

    [TargetRpc]
    void TargetStartStunCooldown(NetworkConnection conn)
    {
        _stunCooldownTimer = stunCooldown;
    }

    // Input action refs to poll for player input
    protected void GetInputRefs()
    {
        attackAction = playerInput.actions.FindAction("Attack");
        interactAction = playerInput.actions.FindAction("Interact");
    }
}
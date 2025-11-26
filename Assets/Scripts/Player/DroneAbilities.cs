// using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class DroneAbilities : NetworkBehaviour
{
    [Header("Basic stun")]
    public float stunDuration = 5f;
    public float stunCooldown = 10f;
    public float stunRange = 30f;
    public AudioClip stunAudio;
    [Range(0, 1)] public float stunAudioVolume = 0.5f;

    public float _stunCooldownTimer = 0f;

    [Header("Distraction")]
    public GameObject distraction;
    public float abilityRadius = 50f;
    public float abilityDuration = 10f;

    [Header("References")]
    // public PlayerInput playerInput;
    public PlayerInputHandler input;
    // public Camera playerCamera;
    public Canvas abilityUI;

    LayerMask layerMask;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            input.attackAction.performed += PerformAttack;

            abilityUI.enabled = true;

            layerMask = LayerMask.GetMask("Enemy", "Ground");
        }
    }

    void OnDestroy()
    {
        if (input is not null)
            if (input.attackAction is not null)
                input.attackAction.performed -= PerformAttack;
    }

    void FixedUpdate()
    {
        if (_stunCooldownTimer > 0)
        {
            _stunCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    // DISCLOSURE: Some of the code for performing Drone stuns over the network was AI generated
    void PerformAttack(InputAction.CallbackContext ctx)
    {
        if (_stunCooldownTimer > 0f) return;

        print("attack");
        
        Vector3 origin = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
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
        NotifyPlayStunSound();
        if (Owner != null)
            TargetStartStunCooldown(Owner);
    }

    [ObserversRpc]
    private void NotifyPlayStunSound() 
    {
        AudioSource.PlayClipAtPoint(stunAudio, transform.position, stunAudioVolume); // play audio for everyone
    }

    [TargetRpc]
    void TargetStartStunCooldown(NetworkConnection conn)
    {
        _stunCooldownTimer = stunCooldown;
    }
}
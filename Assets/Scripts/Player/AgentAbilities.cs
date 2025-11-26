// using System.Collections;
using System.Collections;
using System.ComponentModel;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class AgentAbilities : NetworkBehaviour
{
    [Header("Basic attack")]
    public float damage = 10f;
    public int currentAmmoMax = 10;
    public int reserveAmmoMax = 30;
    public float reloadTime = 2f;
    public int _currentAmmoCount { get; private set; }
    public int _reserveAmmoCount { get; private set; }

    [Header("Audio")]
    public AudioClip attackAudio;
    [Range(0, 1)] public float attackAudioVolume = 0.5f;
    public AudioClip reloadAudioPt1;
    [Range(0, 1)] public float reloadAudioPt1Volume = 0.5f;
    public AudioClip reloadAudioPt2;
    [Range(0, 1)] public float reloadAudioPt2Volume = 0.5f;


    [Header("References")]
    // public PlayerInput playerInput;
    public PlayerInputHandler input;
    // public Camera playerCamera;
    public Canvas abilityUI;

    // InputAction attackAction;
    // InputAction reloadAction;
    LayerMask layerMask;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            input.attackAction.performed += PerformAttack;
            input.reloadAction.performed += TryReload;
            _currentAmmoCount = currentAmmoMax;
            _reserveAmmoCount = reserveAmmoMax;

            abilityUI.enabled = true;

            layerMask = LayerMask.GetMask("Enemy", "Ground");
        }
    }

    void OnDestroy()
    {
        if (input is not null) 
        {
            if (input.attackAction is not null)
                input.attackAction.performed -= PerformAttack;
            
            if (input.reloadAction is not null)
                input.reloadAction.performed -= TryReload;
        }
    }

    // DISCLOSURE: Some of the code for performing Agent attacks over the network was AI generated
    // Perform the raycast on the client, then send the target NetworkObject to the server
    void PerformAttack(InputAction.CallbackContext ctx)
    {
        if (_currentAmmoCount == 0) return;
        _currentAmmoCount--;

        // Play audio
        NotifyPlayAttackSound();
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            // print("hit");
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 1f);
            if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out var netObj))
            {
                Debug.Log($"[AgentAbilities] Client calling ServerAttack for target ObjectId={netObj.ObjectId} name={netObj.gameObject.name}");
                ServerAttack(netObj);
            }
        }
    }

    void TryReload(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[AgentAbilities] Trying to reload. Amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount}");
        if (_currentAmmoCount == currentAmmoMax) return; // Ignore if full
        if (_reserveAmmoCount == 0) return; // Ignore if nothing in reserve
        Debug.Log($"[AgentAbilities] Invoking PerformReload(). Amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount}");
        StartCoroutine(PerformReload());
    }

    IEnumerator PerformReload()
    {
        NotifyPlayReloadPt1Sound();
        int diff = currentAmmoMax - _currentAmmoCount;
        int reloadAmount = Mathf.Clamp(diff, 1, _reserveAmmoCount);

        Debug.Log($"[AgentAbilities] Reloading. Prior amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount} reloadAmount={reloadAmount}");
        int currentAmmoStored = _currentAmmoCount;
        _currentAmmoCount = 0;
        yield return new WaitForSeconds(reloadTime);
        
        NotifyPlayReloadPt2Sound();
        _reserveAmmoCount -= reloadAmount;
        _currentAmmoCount += currentAmmoStored + reloadAmount;
        Debug.Log($"[AgentAbilities] Completed reload. New amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ServerAttack(NetworkObject target)
    {        
        Debug.Log($"[AgentAbilities] ServerAttack invoked on server. Target={target}");
        if (target == null)
        {
            Debug.LogWarning("[AgentAbilities] ServerAttack: target is null");
            return;
        }

        SyncEnemyVars healthObj = target.GetComponent<SyncEnemyVars>();
        if (healthObj is null)
        {
            Debug.LogWarning($"[AgentAbilities] ServerAttack: target {target.gameObject.name} has no SyncEnemyVars");
            return;
        }

        float before = healthObj.currentHealth.Value;
        float after = Mathf.Max(0f, before - damage); // clamp to zero
        healthObj.currentHealth.Value = after;
        Debug.Log($"[AgentAbilities] Applied damage {damage}. Health {before} -> {after}");
    }

    [ObserversRpc]
    public void NotifyPlayAttackSound() 
    {
        AudioSource.PlayClipAtPoint(attackAudio, transform.position, attackAudioVolume);
    }

    [ObserversRpc]
    public void NotifyPlayReloadPt1Sound() 
    {
        AudioSource.PlayClipAtPoint(reloadAudioPt1, transform.position, reloadAudioPt1Volume);
    }

    [ObserversRpc]
    public void NotifyPlayReloadPt2Sound() 
    {
        AudioSource.PlayClipAtPoint(reloadAudioPt2, transform.position, reloadAudioPt2Volume);
    }
}
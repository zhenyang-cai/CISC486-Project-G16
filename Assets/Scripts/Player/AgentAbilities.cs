// using System.Collections;
using System.Collections;
using System.ComponentModel;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgentAbilities : NetworkBehaviour
{
    [Header("Basic attack")]
    public float damage = 10f;
    public int currentAmmoMax = 10;
    public int reserveAmmoMax = 30;
    public float reloadTime = 2f;

    public int _currentAmmoCount;
    public int _reserveAmmoCount;

    [Header("References")]
    public PlayerInput playerInput;
    public Camera playerCamera;
    public Canvas abilityUI;

    InputAction attackAction;
    InputAction reloadAction;
    LayerMask layerMask;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            GetInputRefs();
            attackAction.performed += ctx => PerformAttack();
            reloadAction.performed += ctx => TryReload();
            _currentAmmoCount = currentAmmoMax;
            _reserveAmmoCount = reserveAmmoMax;

            abilityUI.enabled = true;

            layerMask = LayerMask.GetMask("Enemy", "Ground");
        }
    }

    // DISCLOSURE: Some of the code for performing Agent attacks over the network was AI generated
    // Perform the raycast on the client, then send the target NetworkObject to the server
    void PerformAttack()
    {
        if (_currentAmmoCount == 0) return;
        _currentAmmoCount--;
        // print("attack");
        
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            // print("hit");
            Debug.DrawLine(playerCamera.transform.position, hit.point, Color.red, 1f);
            if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out var netObj))
            {
                Debug.Log($"[AgentAbilities] Client calling ServerAttack for target ObjectId={netObj.ObjectId} name={netObj.gameObject.name}");
                ServerAttack(netObj);
            }
        }
    }

    void TryReload()
    {
        Debug.Log($"[AgentAbilities] Trying to reload. Amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount}");
        if (_currentAmmoCount == currentAmmoMax) return; // Ignore if full
        if (_reserveAmmoCount == 0) return; // Ignore if nothing in reserve
        Debug.Log($"[AgentAbilities] Invoking PerformReload(). Amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount}");
        StartCoroutine(PerformReload());
    }

    IEnumerator PerformReload()
    {
        int diff = currentAmmoMax - _currentAmmoCount;
        int reloadAmount = Mathf.Clamp(diff, 1, _reserveAmmoCount);

        Debug.Log($"[AgentAbilities] Reloading. Prior amounts: _currentAmmoCount={_currentAmmoCount} _reserveAmmoCount={_reserveAmmoCount} reloadAmount={reloadAmount}");
        int currentAmmoStored = _currentAmmoCount;
        _currentAmmoCount = 0;
        yield return new WaitForSeconds(reloadTime);
        
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

    // Input action refs to poll for player input
    protected void GetInputRefs()
    {
        attackAction = playerInput.actions.FindAction("Attack");
        reloadAction = playerInput.actions.FindAction("Reload");
    }
}
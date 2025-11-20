using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;

public class EnemySpawn : NetworkBehaviour
{
    public NetworkObject enemyPrefab;
    public List<Transform> PatrolPoints;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Spawn();
    }

    private void Spawn()
    {
        NetworkObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        ServerManager.Spawn(enemy);
        Enemy e = enemy.GetComponent<Enemy>();
        e.PatrolPoints = PatrolPoints;
    }
}

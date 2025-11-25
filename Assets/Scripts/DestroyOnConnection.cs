using UnityEngine;

public class DestroyOnConnection : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        AgentMovementNetworked agent = FindAnyObjectByType<AgentMovementNetworked>();
        if (agent != null) Destroy(gameObject);
    }
}

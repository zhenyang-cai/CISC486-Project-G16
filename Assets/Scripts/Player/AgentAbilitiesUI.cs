using TMPro;
using UnityEngine;

public class AgentAbilitiesUI : MonoBehaviour
{
    public AgentAbilities agentAbilities;
    public TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        text.text = $"{agentAbilities._currentAmmoCount} / {agentAbilities._reserveAmmoCount}";
    }
}

using UnityEngine;
using UnityEngine.UI;

public class DroneAbilitiesUI : MonoBehaviour
{
    public Image fillImage;
    public DroneAbilities droneAbilities;

    // Update is called once per frame
    void Update()
    {
        fillImage.fillAmount = (droneAbilities.stunCooldown - droneAbilities._stunCooldownTimer) / droneAbilities.stunCooldown;
    }
}

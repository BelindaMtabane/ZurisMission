using UnityEngine;

public class PlayerHUDBase : MonoBehaviour
{
    public HUDControls hudControls;
    public PlayerMovement playerMovement;
    private void OnTriggerEnter(Collider other)
    {
        if (hudControls == null) return;
        hudControls = FindFirstObjectByType<HUDControls>(); // Updated to use the recommended method

        if (other.CompareTag("Water..."))
        {
            hudControls.WaterControls();
        }
        if (other.CompareTag("SpeedBoast"))
        {
           // hudControls.MoveControls(10f);
        }
        if (other.CompareTag("SlowDown"))
        {
           // hudControls.MoveControls(2f);
        }
        if (other.CompareTag("Health") || other.CompareTag("HeatIncrease"))
        {
           // hudControls.HealthControls();
        }
    }
}

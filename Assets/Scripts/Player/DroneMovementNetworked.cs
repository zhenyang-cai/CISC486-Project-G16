using UnityEngine;

public class DroneMovementNetworked : NetworkedMovement
{
    [Header("Movement variables")]
    public float airAccel = 10f;
    public float airDecel = 20f;
    public float airMaxSpeed = 5f;
    public float airVerticalAccel = 5f;
    public float airVerticalDecel = 10f;
    public float airVerticalMaxSpeed = 5f;
    
    // Given a value, attempt to change it by a given amount towards zero.
    private float approachZero(float value, float change)
    {
        return value > 0 ? Mathf.Max(0, value - change) : Mathf.Min(0, value + change);
    }
    
    protected override void OnStartExtra()
    {
        return;
    }

    protected new void HandleRotation()
    {
        base.HandleRotation();
        playerMesh.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }

    protected override void HandleMovement()
    {
        Vector3 desiredMovement = GetMovementDirection();

        // Vertical movement
        if (desiredMovement.y == 0)
        {
            // slow down if not actively moving
            _currentVelocity.y = approachZero(_currentVelocity.y, airVerticalDecel * Time.deltaTime);
        }
        else
        {
            _currentVelocity.y += desiredMovement.y * airVerticalAccel * Time.deltaTime;
            // clamp vertical speed
            _currentVelocity.y = Mathf.Clamp(_currentVelocity.y, -airVerticalMaxSpeed, airVerticalMaxSpeed);
        }

        // Horizontal movement
        if (desiredMovement.x == 0 && desiredMovement.z == 0)
        {
            // slow down if not actively moving
            Vector3 currentHorizontal = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
            float newSpeed = approachZero(currentHorizontal.magnitude, airDecel * Time.deltaTime);

            Vector3 newHorizontal = currentHorizontal.normalized * newSpeed;
            _currentVelocity.x = newHorizontal.x;
            _currentVelocity.z = newHorizontal.z;
        }
        else
        {
            Vector3 desiredHorizonal = new Vector3(desiredMovement.x, 0, desiredMovement.z);
            Vector3 currentHorizontal = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);

            currentHorizontal += desiredHorizonal * airAccel * Time.deltaTime;
            currentHorizontal = currentHorizontal.normalized * Mathf.Clamp(currentHorizontal.magnitude, -airMaxSpeed, airMaxSpeed);

            _currentVelocity.x = currentHorizontal.x;
            _currentVelocity.z = currentHorizontal.z;
        }

        characterController.Move(_currentVelocity * Time.deltaTime);
    }
}
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject CinemachineCameraTarget;
    public bool LockCameraPosition = false;
    private const float threshold = 0.01f;
    public float CameraAngleOverride = 0.0f;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    #region Input System
    public CustomPlayerInput Input { get; private set; }
    #endregion

    void Awake()
    {
        Input = GetComponent<CustomPlayerInput>();
    }

    void Start()
    {
        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (Input.look.sqrMagnitude >= threshold && !LockCameraPosition)
        {
            cinemachineTargetYaw += Input.look.x * 1.0f;
            cinemachineTargetPitch += Input.look.y * 1.0f;
        }

        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride,
            cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class VisionSensor : MonoBehaviour
{
    public enum SuspicionState { 
        None, 
        Investigate, 
        Confirmed 
    }

    public SuspicionState State = SuspicionState.None;
    
    [Header("View Settings")]
    [Range(0f, 180f)] public float viewAngle = 60f;
    public float viewRadius = 8f;
    public float eyeHeight = 1.68f;

    [Header("Alert Range")]
    [Range(0f, 15f)] public float alertness;
    public float alertIncreaseRate = 4f;
    public float alertDecayRate = 0.8f;
    public float closeAlertRadius = 1f;
    public float closeAlertIncreaseRate = 8f;

    [Header("Masks")]
    public LayerMask targetMasks;
    public LayerMask obstacleMasks;

    private readonly Collider[] detectedTargets = new Collider[8];
    
    private float lastSeenUpdateInterval = 0.5f;
    private float nextSeenUpdateTime;
    public Vector3 LastSeenPos { get; private set; }
    public Transform ConfirmedTarget { get; private set; }

    public void TriggerAlert(Transform target)
    {
        if (target == null) return;

        ConfirmedTarget = target;
        LastSeenPos = target.position;
        alertness = 15f;
        State = SuspicionState.Confirmed;
    }

    void Update()
    {
        bool close = CheckCloseRange();
        if (close)
            alertness = Mathf.MoveTowards(alertness, 15f, closeAlertIncreaseRate * Time.deltaTime);
        else
        {
            bool view = CheckViewRange();
            if (view)
            {
                float dist = Vector3.Distance(transform.position, detectedTargets[0].transform.position);
                float factor = 1f - Mathf.Clamp01(dist / viewRadius);
                float dynamicRate = alertIncreaseRate * (1f + factor);
                alertness = Mathf.MoveTowards(alertness, 15f, dynamicRate * Time.deltaTime);
            }
            else
                alertness = Mathf.MoveTowards(alertness, 0f, alertDecayRate * Time.deltaTime);
        }

        if (alertness > 12)
        {
            State = SuspicionState.Confirmed;
            ConfirmedTarget = detectedTargets[0] ? detectedTargets[0].transform : null;
        }
        else if (alertness > 6)
        {
            State = SuspicionState.Investigate;
            ConfirmedTarget = null;
            if (Time.time >= nextSeenUpdateTime && detectedTargets[0])
            {
                LastSeenPos = detectedTargets[0].transform.position;
                nextSeenUpdateTime = Time.time + lastSeenUpdateInterval;
            }
        }
        else
        {
            State = SuspicionState.None;
            ConfirmedTarget = null;
        }
    }

    bool CheckViewRange()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, viewRadius, detectedTargets, targetMasks);
        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        for (int i = 0; i < count; i++)
        {
            Transform t = detectedTargets[i].transform;
            Vector3 dir = t.position + Vector3.up * eyeHeight - eye;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle <= viewAngle * 0.5f)
            {
                if (!Physics.Raycast(eye, dir.normalized, dir.magnitude, obstacleMasks))
                    return true;
            }
        }
        return false;
    }

    bool CheckCloseRange()
    {
        Vector3 basePos = transform.position;
        Vector3 topPos = basePos + Vector3.up * eyeHeight;

        int count = Physics.OverlapCapsuleNonAlloc(basePos, topPos, closeAlertRadius, detectedTargets, targetMasks);
        for (int i = 0; i < count; i++)
        {
            if (!detectedTargets[i].isTrigger)
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        bool close = Application.isPlaying && CheckCloseRange();
        bool view = Application.isPlaying && CheckViewRange();
        Gizmos.color = close ? Color.red :
                    view ? Color.yellow :
                    Color.green;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        Vector3 basePos = transform.position;
        Vector3 topPos = basePos + Vector3.up * eyeHeight;
        Gizmos.DrawWireSphere(basePos, closeAlertRadius);
        Gizmos.DrawWireSphere(topPos, closeAlertRadius);

    #if UNITY_EDITOR
        Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.15f);
        Handles.DrawSolidArc(
            origin,
            Vector3.up,
            Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward,
            viewAngle,
            viewRadius
        );
    #endif

        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewRadius);
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0,  viewAngle / 2f, 0) * transform.forward * viewRadius);
    }

}

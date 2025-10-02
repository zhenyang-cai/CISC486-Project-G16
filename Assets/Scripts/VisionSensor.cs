using UnityEngine;

public class VisionSensor : MonoBehaviour
{
    [Header("FOV")]
    public float viewRadius = 10f;
    public float viewAngle = 30f;
    public float peripheralAngle = 60f;
    public float height = 2f;

    [Header("Rates")]
    public float coreTimeToSpot = 0.2f;
    public float peripheralSlow = 2f;
    public float decayPerSecond = 0.6f;

    [Header("Thresholds")]
    [Range(0f,1f)] public float investigateThreshold = 0.5f;
    [Range(0f,1f)] public float confirmThreshold = 1.0f;

    [Header("Masks")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    // 输出
    public enum SuspicionState { None, Investigate, Confirmed }
    public SuspicionState State { get; private set; } = SuspicionState.None;
    [Range(0f,1f)] public float Suspicion { get; private set; }
    public Vector3 LastSeenPos { get; private set; }
    public Transform ConfirmedTarget { get; private set; }

    void Update()
    {
        bool inCore, inPeri;
        Transform target = AcquireBestTarget(out inCore, out inPeri);

        if (target)
        {
            float rate = (inCore ? 1f : 1f / Mathf.Max(1e-4f, peripheralSlow))
                         / Mathf.Max(1e-4f, coreTimeToSpot);
            Suspicion = Mathf.MoveTowards(Suspicion, 1f, rate * Time.deltaTime);
            LastSeenPos = target.position;
        }
        else
        {
            Suspicion = Mathf.MoveTowards(Suspicion, 0f, decayPerSecond * Time.deltaTime);
        }

        // 状态机
        if (Suspicion >= confirmThreshold)
        {
            State = SuspicionState.Confirmed;
            ConfirmedTarget = target;
        }
        else if (Suspicion >= investigateThreshold)
        {
            State = SuspicionState.Investigate;
            ConfirmedTarget = null;
        }
        else
        {
            State = SuspicionState.None;
            ConfirmedTarget = null;
        }
    }

    Transform AcquireBestTarget(out bool inCoreFov, out bool inPeripheral)
    {
        inCoreFov = false; inPeripheral = false;
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        Transform best = null;
        float bestDist = float.PositiveInfinity;
        bool bestCore = false, bestPeri = false;

        foreach (var h in hits)
        {
            if (ClassifyVisibility(h.transform, out bool core, out bool peri, out float dist))
            {
                if (dist < bestDist)
                {
                    best = h.transform;
                    bestDist = dist;
                    bestCore = core;
                    bestPeri = peri;
                }
            }
        }
        inCoreFov = bestCore; inPeripheral = bestPeri;
        return best;
    }

    bool ClassifyVisibility(Transform target, out bool inCoreFov, out bool inPeripheral, out float dist)
    {
        Vector3 eye = transform.position + Vector3.up * height;
        Vector3 to = (target.position + Vector3.up * height) - eye;
        dist = to.magnitude;
        if (dist > viewRadius) { inCoreFov = inPeripheral = false; return false; }

        float ang = Vector3.Angle(transform.forward, to);
        inCoreFov = ang <= viewAngle * 0.5f;
        inPeripheral = !inCoreFov && ang <= peripheralAngle * 0.5f;
        if (!(inCoreFov || inPeripheral)) return false;

        if (Physics.Raycast(eye, to.normalized, dist, obstacleMask)) return false;
        return true;
    }

    public void ResetSuspicion(float value = 0f)
    {
        Suspicion = Mathf.Clamp01(value);
        State = SuspicionState.None;
        ConfirmedTarget = null;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position + Vector3.up * height;
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, viewRadius);
        DrawArc(p, viewAngle, Color.yellow);
        DrawArc(p, peripheralAngle, new Color(1,1,0,0.2f));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(p, p + Vector3.up * (1f + Suspicion));
    }

    void DrawArc(Vector3 p, float angle, Color c)
    {
        Gizmos.color = c;
        Vector3 r = Quaternion.Euler(0, -angle/2f, 0) * transform.forward;
        Vector3 l = Quaternion.Euler(0,  angle/2f, 0) * transform.forward;
        Gizmos.DrawLine(p, p + r * viewRadius);
        Gizmos.DrawLine(p, p + l * viewRadius);
    }
}

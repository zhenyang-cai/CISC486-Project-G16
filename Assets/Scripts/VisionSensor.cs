using UnityEngine;

public class VisionSensor : MonoBehaviour
{
    public float viewRadius = 10f, viewAngle = 30f, height = 2f;
    public float peripheralAngle = 60f, timeToSpot = 0.2f;
    public LayerMask targetMask, obstacleMask;
    public Transform currentTarget;
    private Transform seenTarget;
    public Vector3 lastSeenPos;
    public float seenTimer;

    void Update()
    {
        var target = Scan();
        if (target)
        {
            if (target == seenTarget)
                seenTimer += Time.deltaTime;
            else
            {
                seenTarget = target;
                seenTimer = 0f;
            }

            if (seenTimer >= timeToSpot)
            {
                currentTarget = seenTarget;
                lastSeenPos = target.position;
            }
        }
        else
        {
            seenTarget = null;
            seenTimer = 0f;
        }
    }

    public Transform Scan()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        Transform target = null;

        foreach (var h in hits)
        {
            if (!CanSee(h.transform)) continue;

            target = h.transform;

        }
        return target;
    }

    public bool CanSee(Transform target)
    {
        Vector3 eye = transform.position + Vector3.up * height;
        Vector3 to = target.position + Vector3.up * height - eye;
        float dist = to.magnitude;
        if (dist > viewRadius) return false;

        float ang = Vector3.Angle(transform.forward, to);

        bool inCoreFov = ang <= viewAngle * 0.5f;
        bool inPeripheral = ang <= peripheralAngle * 0.5f;

        if (!(inCoreFov || inPeripheral)) return false;

        if (Physics.Raycast(eye, to.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position + Vector3.up * height;
        Gizmos.color = Color.cyan;  Gizmos.DrawWireSphere(transform.position, viewRadius);
        DrawArc(p, viewAngle, Color.yellow);
        DrawArc(p, peripheralAngle, new Color(1,1,0,0.2f));
    }
    void DrawArc(Vector3 p, float angle, Color c)
    {
        Gizmos.color = c;
        Vector3 r = Quaternion.Euler(0, -angle/2, 0) * transform.forward;
        Vector3 l = Quaternion.Euler(0,  angle/2, 0) * transform.forward;
        Gizmos.DrawLine(p, p + r * viewRadius);
        Gizmos.DrawLine(p, p + l * viewRadius);
    }
}

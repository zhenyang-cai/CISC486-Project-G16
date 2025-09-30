using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }
    public CharacterController controller { get; private set; }
    #endregion
    
    public StateMachine stateMachine { get; private set; }

    [SerializeField] protected LayerMask GroundLayers;
    [SerializeField] protected bool Grounded;
    [SerializeField] private float GroundedOffset = -0.14f;
    [SerializeField] private float GroundedRadius = 0.28f;
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        stateMachine = new StateMachine();
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        stateMachine?.UpdateActiveState();
        GroundedCheck();
    }

    protected virtual void LateUpdate()
    {
        stateMachine?.LateUpdateActiveState();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine?.FixedUpdateActiveState();
    }

    public void GroundedCheck()
    {
        Vector3 spherePosition = transform.position + Vector3.down * GroundedOffset;

        Grounded = Physics.CheckSphere(
            spherePosition,
            GroundedRadius,
            GroundLayers,
            QueryTriggerInteraction.Ignore
        );

        anim.SetBool("Grounded", Grounded);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center), FootstepAudioVolume);
            }
        }
    }
}

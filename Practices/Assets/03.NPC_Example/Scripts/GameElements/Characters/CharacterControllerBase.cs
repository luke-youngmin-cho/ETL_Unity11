using Practices.NPC_Example.AISystems.BT;
using Practices.NPC_Example.FSM;
using Practices.NPC_Example.GameElements.EffectSystems.DamageEffects;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Practices.NPC_Example.GameElements.Characters
{
    public abstract class CharacterControllerBase : MonoBehaviour, ICharacterController
    {
        public State currentState { get; private set; }

        public bool isTransitioning => _animator.GetBool(IS_DIRTY_HASH);

        public IInputCommand inputCommand { get; set; }

        [Header("Stats")]
        [field: SerializeField] public float moveSpeed { get; protected set; } = 3.5f;
        [field: SerializeField] public float jumpSpeed { get; protected set; } = 5f;
        bool _isInAir;

        DamageEffectFactory _damageEffectFactory;

        // Animator
        Animator _animator;
        readonly int STATE_HASH = Animator.StringToHash("State");
        readonly int IS_DIRTY_HASH = Animator.StringToHash("IsDirty");
        readonly int SPEED_HASH = Animator.StringToHash("Speed");
        readonly int IS_GROUNDED_HASH = Animator.StringToHash("IsGrounded");

        // Navigation
        protected NavMeshAgent agent;
        protected readonly float NAV_MESH_POSITION_TOLERANCE = 0.1f;

        // Collision
        CapsuleCollider _capsuleCollider;
        [SerializeField] LayerMask _collisionMask;


        protected virtual void Awake()
        {
            inputCommand = new NonPlayerInputCommand();
            _damageEffectFactory = new DamageEffectFactory();
            _animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
            _capsuleCollider = GetComponent<CapsuleCollider>();
            StateMachineBehaviourBase[] behaviours = _animator.GetBehaviours<StateMachineBehaviourBase>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].onStateEntered += state => currentState = state;
            }
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            if (inputCommand.doJumpTrigger)
            {
                Jump();
            }

            float speedNormalized = agent.velocity.magnitude / agent.speed;
            _animator.SetFloat(SPEED_HASH, speedNormalized);
        }

        protected virtual void FixedUpdate()
        {
        }

        public virtual void Damage(GameObject damager, float damageAmount)
        {
            // todo -> decrease hp            
            _damageEffectFactory.Create("DamageFont2", transform.position + Vector3.up * 2, damageAmount);
        }

        public void ChangeState(State newState)
        {
            _animator.SetInteger(STATE_HASH, (int)newState);
            _animator.SetBool(IS_DIRTY_HASH, true);
        }

        public virtual void PauseOperation(bool pause)
        {

        }

        void Jump()
        {
            // 위치공식 
            // 변위 = 초기위치 + 속도 * 시간 

            // S_t = S_0 + V_0 * t + 0.5 * a * t^2
            // V_0 = agent 속도 + 점프속도
            // a = 중력가속도
            // position = 점프시작위치 + (Agent 속도 + 점프속도) * 0.5 * 중력가속도 * t^2

            if (_isInAir)
            {
                Debug.Log("공중에 있을때는 점프 못해요");
                return;
            }

            _isInAir = true;
            StartCoroutine(C_InAir(transform.position, agent.velocity + Vector3.up * jumpSpeed));
        }

        IEnumerator C_InAir(Vector3 startPosition, Vector3 startVelocity)
        {
            PauseOperation(true);
            _animator.SetBool(IS_GROUNDED_HASH, false);
            float startTimeMark = Time.time;
            Vector3 previousPosition = transform.position;
            bool jumpStarted = false;

            while (true)
            {
                bool isGrounded = false;
                float elapsedTime = Time.time - startTimeMark;
                Vector3 expectedPosition = startPosition
                                           + startVelocity * elapsedTime
                                           + 0.5f * Physics.gravity * elapsedTime * elapsedTime;
                Vector3 expectedDirection = (expectedPosition - previousPosition).normalized;
                float expectedDistance = (expectedPosition - previousPosition).magnitude;

                Vector3 p0 = previousPosition + Vector3.up * _capsuleCollider.radius;
                Vector3 p1 = previousPosition + Vector3.up * (_capsuleCollider.height - _capsuleCollider.radius);


                if (Physics.CapsuleCast(p0, p1, _capsuleCollider.radius, expectedDirection, out RaycastHit raycastHit, expectedDistance, _collisionMask))
                {
                    expectedPosition = previousPosition + expectedDirection * raycastHit.distance;
                }

                // 발 한번 땅에서 뗀적있는지
                if (jumpStarted)
                {
                    if (NavMesh.SamplePosition(expectedPosition, out NavMeshHit navMeshHit, NAV_MESH_POSITION_TOLERANCE, NavMesh.AllAreas))
                    {
                        expectedPosition = navMeshHit.position;
                        isGrounded = true;
                    }
                }
                else
                {
                    if (NavMesh.SamplePosition(expectedPosition, out NavMeshHit navMeshHit, NAV_MESH_POSITION_TOLERANCE, NavMesh.AllAreas) == false)
                    {
                        jumpStarted = true;
                    }
                }

                transform.position = expectedPosition;

                if (isGrounded)
                    break;

                yield return null;
            }

            _animator.SetBool(IS_GROUNDED_HASH, true);
            _isInAir = false;
            PauseOperation(false);
        }
    }
}
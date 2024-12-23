using Practices.NPC_Example.AISystems.BT;
using Practices.NPC_Example.FSM;
using Practices.NPC_Example.GameElements.EffectSystems.DamageEffects;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
        public bool isInAir { get; protected set; }

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
        protected float slopeAngleLimit;
        protected float stepHeightLimit;

        // Collision
        protected CapsuleCollider capsuleCollider;
        [SerializeField] LayerMask _collisionMask;
        protected readonly float GROUND_CHECK_DISTANCE = 0.01f;
        [SerializeField] LayerMask _groundMask;
        public bool isGrounded { get; protected set; }
        protected float speedByGravity;

        protected virtual void Awake()
        {
            inputCommand = new NonPlayerInputCommand();
            _damageEffectFactory = new DamageEffectFactory();
            _animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
            capsuleCollider = GetComponent<CapsuleCollider>();
            StateMachineBehaviourBase[] behaviours = _animator.GetBehaviours<StateMachineBehaviourBase>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].onStateEntered += state => currentState = state;
            }
        }

        protected virtual void Start()
        {
            NavMeshBuildSettings navMeshBuildSettings = NavMesh.GetSettingsByID(0);
            slopeAngleLimit = navMeshBuildSettings.agentSlope;
            stepHeightLimit = navMeshBuildSettings.agentClimb;
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
            if (CheckGround(transform.position, out Vector3 groundPoint, out Vector3 groundNormal))
            {
                speedByGravity = 0f;
                isGrounded = true;
            }
            else
            {
                speedByGravity += Physics.gravity.magnitude * Time.fixedDeltaTime;
                isGrounded = false;
            }
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

            if (isInAir)
            {
                Debug.Log("공중에 있을때는 점프 못해요");
                return;
            }

            isInAir = true;
            StartCoroutine(C_InAir(transform.position, agent.velocity + Vector3.up * jumpSpeed));
        }

        protected IEnumerator C_InAir(Vector3 startPosition, Vector3 startVelocity)
        {
            PauseOperation(true);
            _animator.SetBool(IS_GROUNDED_HASH, false);
            float startTimeMark = Time.time;
            Vector3 previousPosition = transform.position;
            bool jumpStarted = startVelocity.y <= 0; // 위쪽으로 향할때만 점프 확인해야함

            while (true)
            {
                bool isGrounded = false;
                float elapsedTime = Time.time - startTimeMark;
                Vector3 expectedPosition = startPosition
                                           + startVelocity * elapsedTime
                                           + 0.5f * Physics.gravity * elapsedTime * elapsedTime;
                Vector3 expectedDirection = (expectedPosition - previousPosition).normalized;
                float expectedDistance = (expectedPosition - previousPosition).magnitude;

                Vector3 p0 = previousPosition + Vector3.up * capsuleCollider.radius;
                Vector3 p1 = previousPosition + Vector3.up * (capsuleCollider.height - capsuleCollider.radius);


                if (Physics.CapsuleCast(p0, p1, capsuleCollider.radius, expectedDirection, out RaycastHit raycastHit, expectedDistance, _collisionMask))
                {
                    expectedPosition = previousPosition + expectedDirection * raycastHit.distance;
                }

                // 발 한번 땅에서 뗀적있는지
                if (jumpStarted)
                {
                    if (CheckGround(expectedPosition, out Vector3 groundPoint, out Vector3 groundNormal))
                    {
                        expectedPosition = groundPoint;
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
            isInAir = false;
            PauseOperation(false);
        }

        protected bool CheckGround(Vector3 position, out Vector3 groundPoint, out Vector3 groundNormal)
        {
            if (agent.enabled)
            {
                if (NavMesh.SamplePosition(position, out NavMeshHit navMeshHit, NAV_MESH_POSITION_TOLERANCE, NavMesh.AllAreas))
                {
                    groundPoint = navMeshHit.position;
                    groundNormal = navMeshHit.normal;
                    return true;
                }
            }
            else
            {
                Vector3 rayOriginOffset = Vector3.up * capsuleCollider.height / 2.0f;
                Ray ray = new Ray(position + rayOriginOffset, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hit, rayOriginOffset.magnitude + GROUND_CHECK_DISTANCE, _groundMask))
                {
                    groundPoint = hit.point;
                    groundNormal = hit.normal;
                    return true;
                }
            }

            groundPoint = default;
            groundNormal = default;
            return false;
        }

        protected bool CheckStep(Vector3 expectedPosition, out Vector3 stepPoint)
        {
            Ray ray = new Ray(expectedPosition + Vector3.up * (stepHeightLimit + GROUND_CHECK_DISTANCE), Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 2 * (stepHeightLimit + GROUND_CHECK_DISTANCE), _groundMask))
            {
                float groundAngle = Vector3.Angle(hit.normal, Vector3.up);
                stepPoint = default;

                if (groundAngle > 0.001f)
                    return false;

                if (Mathf.Abs(expectedPosition.y - hit.point.y) > stepHeightLimit)
                    return false;

                stepPoint = hit.point;
                return true;
            }

            stepPoint = default;
            return false;
        }

        protected bool CheckSlope(Vector3 expectedPosition, Vector3 groundPoint, Vector3 groundNormal, out Vector3 slopePoint)
        {
            float groundAngle = Vector3.Angle(groundNormal, Vector3.up);

            Debug.Log($"GroundAnlge : {groundAngle}");
            if (groundAngle <= slopeAngleLimit)
            {
                Vector3 slopeDirection = (groundPoint - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, expectedPosition);
                slopePoint = transform.position + slopeDirection * distance;
                return true;
            }

            slopePoint = default;
            return false;
        }


        protected bool CheckGroundBetween(Vector3 startPosition, Vector3 endPosition, out Vector3 groundPoint)
        {
            if (agent.enabled)
            {
                if (NavMesh.SamplePosition(endPosition, out NavMeshHit navMeshHit, NAV_MESH_POSITION_TOLERANCE, NavMesh.AllAreas))
                {
                    groundPoint = navMeshHit.position;
                    return true;
                }
            }
            else
            {
                Ray ray = new Ray(startPosition, endPosition - startPosition);

                if (Physics.Raycast(ray, out RaycastHit hit, Vector3.Distance(startPosition, endPosition), _groundMask))
                {
                    float groundAngle = Vector3.Angle(hit.normal, Vector3.up);

                    if (groundAngle <= slopeAngleLimit)
                    {
                        groundPoint = hit.point;
                        return true;
                    }
                }
            }

            groundPoint = default;
            return false;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (isGrounded)
            {
                FootIK();
            }
        }

        const float FOOT_IK_DETECT_DISTANCE = 0.3f;
        const float FOOT_HEIGHT = 0.04f;

        void FootIK()
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            Vector3 ikGoalPosition = default;
            Ray targetingRay = default;
            RaycastHit hit = default;

            ikGoalPosition = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            targetingRay = new Ray(ikGoalPosition + Vector3.up * FOOT_IK_DETECT_DISTANCE, Vector3.down);

            if (Physics.Raycast(targetingRay, out hit, 2 * FOOT_IK_DETECT_DISTANCE, _groundMask))
            {
                _animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + Vector3.up * FOOT_HEIGHT);
            }

            ikGoalPosition = _animator.GetIKPosition(AvatarIKGoal.RightFoot);
            targetingRay = new Ray(ikGoalPosition + Vector3.up * FOOT_IK_DETECT_DISTANCE, Vector3.down);

            if (Physics.Raycast(targetingRay, out hit, 2 * FOOT_IK_DETECT_DISTANCE, _groundMask))
            {
                _animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + Vector3.up * FOOT_HEIGHT);
            }
        }
    }
}
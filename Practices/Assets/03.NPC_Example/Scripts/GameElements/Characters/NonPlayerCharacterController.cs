using Practices.NPC_Example.AISystems.BT;
using Practices.NPC_Example.FSM;
using Practices.NPC_Example.GameElements.EffectSystems.DamageEffects;
using UnityEngine;
using UnityEngine.AI;

namespace Practices.NPC_Example.GameElements.Characters
{
    public enum State
    {
        Move,
        InAir,
        Attack = 10
    }

    [RequireComponent(typeof(BehaviourTree))]
    public class NonPlayerCharacterController : MonoBehaviour, ICharacterController
    {
        public State currentState { get; private set; }

        public bool isTransitioning => _animator.GetBool(IS_DIRTY_HASH);


        BehaviourTree _behaviourTree;

        [Header("AI Seek")]
        [SerializeField] float _seekAngle = 90f;
        [SerializeField] float _seekRadius = 5f;
        [SerializeField] float _seekHeight = 2f;
        [SerializeField] float _seekMaxDistance = 8f;
        [SerializeField] LayerMask _seekTargetMask;
        [SerializeField] LayerMask _seekObstacleMask;

        [Header("AI Patrol")]
        [SerializeField] float _patrolRange = 8f;
        [SerializeField] LayerMask _patrolAreaMask;

        [Header("AI Monitor")]
        [SerializeField] float _monitoringTimeMin = 1f;
        [SerializeField] float _monitoringTimeMax = 3f;

        DamageEffectFactory _damageEffectFactory;

        // Animator
        Animator _animator;
        readonly int STATE_HASH = Animator.StringToHash("State");
        readonly int IS_DIRTY_HASH = Animator.StringToHash("IsDirty");
        readonly int SPEED_HASH = Animator.StringToHash("Speed");

        // Navigation
        NavMeshAgent _agent;

        private void Awake()
        {
            _damageEffectFactory = new DamageEffectFactory();
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            StateMachineBehaviourBase[] behaviours = _animator.GetBehaviours<StateMachineBehaviourBase>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].onStateEntered += state => currentState = state;
            }
        }

        private void Start()
        {
            // Before (하드 코딩)
            //_behaviourTree = GetComponent<BehaviourTree>();
            //_behaviourTree.root = new Root(_behaviourTree);
            //Sequence sequence1 = new Sequence(_behaviourTree);
            //_behaviourTree.root.Attach(sequence1);
            //Seek seek1 = new Seek(_behaviourTree, _seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask);
            //sequence1.Attach(seek1);

            // After (빌더패턴 - 함수 체이닝사용)
            _behaviourTree = GetComponent<BehaviourTree>();
            _behaviourTree.StartBuild()
                .Selector()
                    .Sequence()
                        .Seek(_seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask)
                        .Attack()
                    .CompleteCurrentComposite()
                    .RandomSelector()
                        .Patrol(_seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask, _patrolRange, _patrolAreaMask)
                        .Monitor(_seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask, _monitoringTimeMin, _monitoringTimeMax);
        }

        private void Update()
        {
            float speedNormalized = _agent.velocity.magnitude / _agent.speed;
            _animator.SetFloat(SPEED_HASH, speedNormalized);
        }

        public void Damage(GameObject damager, float damageAmount)
        {
            // todo -> decrease hp
            _behaviourTree.blackboard.target = damager.transform;
            _damageEffectFactory.Create("DamageFont2", transform.position + Vector3.up * 2, damageAmount);
        }

        public void ChangeState(State newState)
        {
            _animator.SetInteger(STATE_HASH, (int)newState);
            _animator.SetBool(IS_DIRTY_HASH, true);
        }
    }
}
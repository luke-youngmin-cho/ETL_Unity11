using Practices.NPC_Example.AISystems.BT;
using UnityEngine;

namespace Practices.NPC_Example.GameElements.Characters
{
    [RequireComponent(typeof(BehaviourTree))]
    public class NonPlayerCharacterController : CharacterControllerBase, ICharacterController
    {
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

        

        protected override void Awake()
        {
            base.Awake();

            inputCommand = new NonPlayerInputCommand();
        }

        protected override void Start()
        {
            base.Start();

            inputCommand.Enable();
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
                        .Monitor(_seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask, _monitoringTimeMin, _monitoringTimeMax)
                        .Jump();
        }

        public override void Damage(GameObject damager, float damageAmount)
        {
            base.Damage(damager, damageAmount);

            _behaviourTree.blackboard.target = damager.transform;
        }

        public override void PauseOperation(bool pause)
        {
            base.PauseOperation(pause);

            agent.enabled = pause == false;
        }
    }
}
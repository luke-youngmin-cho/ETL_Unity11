using Practices.NPC_Example.AISystems.BT;
using UnityEngine;

namespace Practices.NPC_Example.GameElements.Characters
{
    [RequireComponent(typeof(BehaviourTree))]
    public class NonPlayerCharacterController : MonoBehaviour
    {
        BehaviourTree _behaviourTree;

        [Header("AI Seek")]
        [SerializeField] float _seekAngle = 90f;
        [SerializeField] float _seekRadius = 5f;
        [SerializeField] float _seekHeight = 2f;
        [SerializeField] float _seekMaxDistance = 8f;
        [SerializeField] LayerMask _seekTargetMask;
        [SerializeField] LayerMask _seekObstacleMask;


        private void Start()
        {
            // Before (하드 코딩)
            //_behaviourTree = GetComponent<BehaviourTree>();
            //_behaviourTree.root = new Root(_behaviourTree);
            //Sequence sequence1 = new Sequence(_behaviourTree);
            //_behaviourTree.root.Attach(sequence1);
            //Seek seek1 = new Seek(_behaviourTree, _seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask);
            //sequence1.Attach(seek1);

            // After (함수 체이닝사용)
            _behaviourTree = GetComponent<BehaviourTree>();
            _behaviourTree.StartBuild()
                .Selector()
                    .Sequence()
                        .Seek(_seekAngle, _seekRadius, _seekHeight, _seekMaxDistance, _seekTargetMask, _seekObstacleMask)
                    .CompleteCurrentComposite()
                    .RandomSelector();
        }
    }
}
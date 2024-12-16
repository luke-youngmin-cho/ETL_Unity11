using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Test_BTSubject : MonoBehaviour
    {
        private BehaviourTree _behaviourTree;


        private void Awake()
        {
            _behaviourTree = GetComponent<BehaviourTree>();
            _behaviourTree.root = new Root(_behaviourTree);
            Sequence sequence1 = new Sequence(_behaviourTree);
            sequence1.children.Add(new Execution(_behaviourTree, () => Result.Success));
            sequence1.children.Add(new Execution(_behaviourTree, () => Result.Success));
            sequence1.children.Add(new Execution(_behaviourTree, () => Result.Success));
            _behaviourTree.root.child = sequence1;
        }
    }
}
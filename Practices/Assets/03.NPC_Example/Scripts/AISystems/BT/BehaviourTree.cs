using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    public class BehaviourTree : MonoBehaviour
    {
        public Blackboard blackboard { get; private set; }
        public Root root { get; set; }
    }
}
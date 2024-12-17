using UnityEngine;
using UnityEngine.AI;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Blackboard
    {
        public Blackboard(GameObject owner) 
        {
            transform = owner.transform;
            agent = owner.GetComponent<NavMeshAgent>();
        }


        // owner
        public Transform transform { get; set; }
        public NavMeshAgent agent { get; set; }

        // target
        public Transform target { get; set; }
    }
}
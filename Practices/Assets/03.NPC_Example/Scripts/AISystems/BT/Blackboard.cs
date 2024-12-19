using Practices.NPC_Example.GameElements.Characters;
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
            characterController = owner.GetComponent<ICharacterController>();
        }


        // owner
        public Transform transform { get; set; }
        public NavMeshAgent agent { get; set; }
        public ICharacterController characterController { get; set; }

        // target
        public Transform target { get; set; }
    }
}
using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Blackboard
    {
        public Blackboard(GameObject owner) 
        {
            transform = owner.transform;
        }


        // owner
        public Transform transform { get; set; }

        // target
        public Transform target { get; set; }
    }
}
using UnityEngine;

namespace Practices.NPC_Example.GameElements.Characters
{
    public interface IInputCommand
    {
        public bool enabled { get; }
        public bool doJumpTrigger { get; set; }
        public bool doAttackTrigger { get; set; }
        public Vector3 moveDirection { get; set; }

        public void Enable();
        public void Disable();
    }
}
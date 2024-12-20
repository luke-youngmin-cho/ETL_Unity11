using UnityEngine;

namespace Practices.NPC_Example.GameElements.Characters
{
    public class NonPlayerInputCommand : IInputCommand
    {
        public bool enabled { get; private set; }

        public bool doJumpTrigger
        {
            get
            {
                if (_doJumpTrigger)
                {
                    _doJumpTrigger = false;
                    return true;
                }

                return false;
            }
            set
            {
                _doJumpTrigger = value;
            }
        }

        public bool doAttackTrigger
        {
            get
            {
                if (_doAttackTrigger)
                {
                    _doAttackTrigger = false;
                    return true;
                }

                return false;
            }
            set
            {
                _doAttackTrigger = value;
            }
        }

        public Vector3 moveDirection { get; set; }


        bool _doJumpTrigger;
        bool _doAttackTrigger;


        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }
    }
}
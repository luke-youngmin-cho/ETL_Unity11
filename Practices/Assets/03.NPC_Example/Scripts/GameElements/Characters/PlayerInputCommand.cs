using UnityEngine;
using UnityEngine.InputSystem;

namespace Practices.NPC_Example.GameElements.Characters
{
    public class PlayerInputCommand : IInputCommand
    {
        public PlayerInputCommand()
        {
            _doJumpTrigger = false;
            _doAttackTrigger = false;
            _inputActions = new InputActions();
            _inputActions.Player.Jump.started += context => doJumpTrigger = true;

            _inputActions.Player.Move.performed += context =>
            {
                Vector2 value = context.ReadValue<Vector2>();
                moveDirection = new Vector3(value.x, 0f, value.y);
            };

            _inputActions.Player.Move.canceled += context =>
            {
                moveDirection = Vector3.zero;
            };
        }


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
        InputActions _inputActions;


        public void Enable()
        {
            enabled = true;
            _inputActions.Enable();
        }

        public void Disable()
        {
            enabled = false;
            _inputActions.Disable();
        }
    }
}
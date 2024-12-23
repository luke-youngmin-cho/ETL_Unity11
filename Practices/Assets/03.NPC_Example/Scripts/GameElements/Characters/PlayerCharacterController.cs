using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Practices.NPC_Example.GameElements.Characters
{
    public class PlayerCharacterController : CharacterControllerBase, ICharacterController
    {
        public enum Mode
        {
            None,
            Manual, // 축입력으로 이동
            SemiAuto, // 목표위치 입력으로 길찾기 이동
            Auto, // 시나리오를 따라 자동 이동
        }

        public Mode mode
        {
            get => _mode;
            set
            {
                if (value == mode)
                    return;

                switch (value)
                {
                    case Mode.None:
                        throw new System.Exception("Invalid value");
                    case Mode.Manual:
                        {
                            agent.enabled = false;
                        }
                        break;
                    case Mode.SemiAuto:
                        {
                            agent.enabled = true;
                        }
                        break;
                    case Mode.Auto:
                        {
                            agent.enabled = true;
                        }
                        break;
                    default:
                        break;
                }

                _mode = value;
            }
        }

        private Mode _mode;
        private bool _isGrounded;
        


        protected override void Awake()
        {
            base.Awake();
            
            inputCommand = new PlayerInputCommand();
        }

        protected override void Start()
        {
            base.Start();
                        
            mode = Mode.Manual;
            inputCommand.Enable();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            Move();
        }

        public override void PauseOperation(bool pause)
        {
            base.PauseOperation(pause);

            if (_mode == Mode.SemiAuto || _mode == Mode.Auto)
                agent.enabled = pause == false;
        }


        void Move()
        {
            switch (_mode)
            {
                case Mode.None:
                    break;
                case Mode.Manual:
                    ManualMove();
                    break;
                case Mode.SemiAuto:
                    break;
                case Mode.Auto:
                    break;
                default:
                    break;
            }
        }

        void ManualMove()
        {
            Vector3 velocity = inputCommand.moveDirection * moveSpeed;
            Debug.Log(velocity);
            //transform.position += velocity * Time.fixedDeltaTime;

            Vector3 expectedPosition = transform.position + velocity * Time.fixedDeltaTime;

            if (CheckGround(expectedPosition, out Vector3 groundPoint, out Vector3 groundNormal))
            {
                if (CheckSlope(expectedPosition, groundPoint, groundNormal, out Vector3 slopePoint))
                {
                    expectedPosition = slopePoint;
                }
                else if (CheckStep(expectedPosition, out Vector3 stepPoint))
                {
                    expectedPosition = stepPoint;
                }
                else
                {
                    return;
                }
            }
            else
            {
                expectedPosition += Vector3.down * speedByGravity;

                if (CheckGroundBetween(transform.position, expectedPosition, out groundPoint))
                {
                    expectedPosition = groundPoint;
                }
            }

            transform.position = expectedPosition;
        }
    }
}
using Practices.NPC_Example.GameElements.Characters;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Attack : Node
    {
        public Attack(BehaviourTree tree) : base(tree)
        {
        }


        enum Step
        {
            Idle,
            WaitUntilStateChanged,
            WaitUntilStateEnd,
        }

        Step _step;

        public override Result Invoke()
        {
            Result result = Result.Running;

            switch (_step)
            {
                case Step.Idle:
                    {
                        blackboard.characterController.ChangeState(State.Attack);
                        _step++;
                    }
                    break;
                case Step.WaitUntilStateChanged:
                    {
                        // 일단 어떤 상태로 진입을 했는가
                        if (blackboard.characterController.isTransitioning == false)
                        {
                            // 공격상태로 왔는가
                            if (blackboard.characterController.currentState == State.Attack)
                            {
                                _step++;
                            }
                            // 공격하려고했는데 외부의 다른 요인으로인해 다른 상태로 진입했다면
                            else
                            {
                                _step = 0;
                                result = Result.Failure;
                            }
                        }
                    }
                    break;
                case Step.WaitUntilStateEnd:
                    {
                        if (blackboard.characterController.currentState != State.Attack)
                        {
                            _step = 0;
                            result = Result.Success;
                        }
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
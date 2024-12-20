using Practices.NPC_Example.GameElements.Characters;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Jump : Node
    {
        public Jump(BehaviourTree tree) : base(tree)
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
                        if (blackboard.characterController.currentState == State.InAir)
                        {
                            result = Result.Failure;
                        }
                        else
                        {
                            blackboard.characterController.inputCommand.doJumpTrigger = true;
                            _step++;
                        }
                    }
                    break;
                case Step.WaitUntilStateChanged:
                    {
                        if (blackboard.characterController.currentState == State.InAir)
                        {
                            _step++;
                        }
                        else
                        {
                            // todo -> handle exceptions
                        }
                    }
                    break;
                case Step.WaitUntilStateEnd:
                    {
                        if (blackboard.characterController.currentState != State.InAir)
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
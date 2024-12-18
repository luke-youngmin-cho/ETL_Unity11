using Practices.NPC_Example.Utilities;
using System;
namespace Practices.NPC_Example.AISystems.BT
{
    public class RandomSelector : Composite
    {
        public RandomSelector(BehaviourTree tree) : base(tree)
        {
            _random = new Random();
        }


        Random _random;


        public override Result Invoke()
        {
            Result result = Result.Failure;

            for (int i = currentChildIndex; i < children.Count; i++)
            {
                result = children[i].Invoke();

                switch (result)
                {
                    case Result.Success:
                        {
                            currentChildIndex = 0;
                            _random.Shuffle<Node>(children);
                            return result;
                        }
                    case Result.Failure:
                        {
                            currentChildIndex++;
                        }
                        break;
                    case Result.Running:
                        {
                            return result;
                        }
                    default:
                        throw new System.Exception("Invalid result code " + result);
                }
            }

            currentChildIndex = 0;
            _random.Shuffle<Node>(children);
            return result;
        }
    }
}
namespace Practices.NPC_Example.AISystems.BT
{
    public class Sequence : Composite
    {
        public Sequence(BehaviourTree tree) : base(tree)
        {
        }

        public override Result Invoke()
        {
            Result result = Result.Success;

            for (int i = currentChildIndex; i < children.Count; i++)
            {
                result = children[i].Invoke();

                switch (result)
                {
                    case Result.Success:
                        {
                            currentChildIndex++;
                        }
                        break;
                    case Result.Failure:
                        {
                            currentChildIndex = 0;
                            return result;
                        }
                    case Result.Running:
                        {
                            return result;
                        }
                    default:
                        throw new System.Exception("Invalid result code " + result);
                }
            }

            currentChildIndex = 0;
            return result;
        }
    }
}
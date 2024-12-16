namespace Practices.NPC_Example.AISystems.BT
{
    public class Parallel : Composite
    {
        public Parallel(BehaviourTree tree, int successCountRequired) : base(tree)
        {
            _successCountRequired = successCountRequired;
        }


        private int _successCountRequired; // 성공 정책 (이 갯수 이상 자식들이 success 반환시 success . 나머지 failure)
        private int _successCount;


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
                            _successCount++;
                        }
                        break;
                    case Result.Failure:
                        {
                        }
                        break;
                    case Result.Running:
                        {
                            return result;
                        }
                    default:
                        throw new System.Exception("Invalid result code " + result);
                }

                currentChildIndex++;
            }

            currentChildIndex = 0;
            result = _successCount >= _successCountRequired ? Result.Success : Result.Failure;
            _successCount = 0; // <- 추가해주삼

            return result;
        }
    }
}
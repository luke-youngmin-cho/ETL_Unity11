using System;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Decorator : Node, IParentOfChild
    {
        public Decorator(BehaviourTree tree, Func<bool> condition) : base(tree)
        {
            _condition = condition;
        }


        public Node child { get; set; }


        private Func<bool> _condition;


        public override Result Invoke()
        {
            if (_condition.Invoke())
            {
                return child.Invoke();
            }

            return Result.Failure;
        }
    }
}
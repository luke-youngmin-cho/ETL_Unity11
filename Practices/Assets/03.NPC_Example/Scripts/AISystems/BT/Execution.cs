using System;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Execution : Node
    {
        public Execution(BehaviourTree tree, Func<Result> execute) : base(tree)
        {
            _execute = execute;
        }


        private Func<Result> _execute;


        public override Result Invoke()
        {
            return _execute.Invoke();
        }
    }
}
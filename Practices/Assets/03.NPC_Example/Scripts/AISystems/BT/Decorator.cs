using System;
using System.Reflection;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Decorator : Node, IParentOfChild
    {
        public Decorator(BehaviourTree tree, string propertyName) : base(tree)
        {
            PropertyInfo propertyInfo = blackboard.GetType().GetProperty(propertyName);
            _condition = () =>
            {
                return (bool)propertyInfo.GetValue(tree.blackboard);
            };
        }


        public Node child { get; set; }


        private Func<bool> _condition;


        public override Result Invoke()
        {
            if (_condition.Invoke())
            {
                tree.stack.Push(child);
                return Result.Success;
            }

            return Result.Failure;
        }

        public void Attach(Node child)
        {
            if (this.child != null)
                throw new InvalidOperationException("Child already exists.");

            this.child = child;
        }
    }
}
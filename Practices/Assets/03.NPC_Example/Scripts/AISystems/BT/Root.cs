using System;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Root : Node, IParentOfChild
    {
        public Root(BehaviourTree tree) : base(tree)
        {
        }


        public Node child { get; set; }

        
        public override Result Invoke()
        {
            tree.stack.Push(child);
            return Result.Success;
        }

        public void Attach(Node child)
        {
            if (this.child != null)
                throw new InvalidOperationException("Child already exists.");

            this.child = child;
        }
    }
}
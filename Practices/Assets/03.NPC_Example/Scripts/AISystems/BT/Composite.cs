using System.Collections.Generic;

namespace Practices.NPC_Example.AISystems.BT
{
    public abstract class Composite : Node, IParentOfChildren
    {
        protected Composite(BehaviourTree tree) : base(tree)
        {
            children = new List<Node>();
        }


        public List<Node> children { get; set; }


        protected int currentChildIndex;
    }
}
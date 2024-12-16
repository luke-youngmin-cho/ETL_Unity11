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
            return child.Invoke();
        }
    }
}
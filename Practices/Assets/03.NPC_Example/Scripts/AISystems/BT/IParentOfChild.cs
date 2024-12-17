namespace Practices.NPC_Example.AISystems.BT
{
    public interface IParentOfChild : IParent
    {
        Node child { get; set; }
    }
}
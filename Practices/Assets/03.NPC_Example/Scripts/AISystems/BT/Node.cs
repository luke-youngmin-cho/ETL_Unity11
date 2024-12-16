namespace Practices.NPC_Example.AISystems.BT
{
    /// <summary>
    /// 행동트리 노드 기반 클래스
    /// </summary>
    public abstract class Node
    {
        public Node(BehaviourTree tree)
        {
            this.tree = tree;
            this.blackboard = tree.blackboard;
        }


        protected BehaviourTree tree;
        protected Blackboard blackboard;


        /// <summary>
        /// 각 노드의 고유 로직
        /// </summary>
        /// <returns> 노드 실행 결과 </returns>
        public abstract Result Invoke();
    }
}
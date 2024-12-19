namespace Practices.NPC_Example.GameElements.Characters
{
    public interface ICharacterController
    {
        /// <summary>
        /// 현재 애니메이터가 실행하고있는 상태
        /// </summary>
        State currentState { get; }
        bool isTransitioning { get; }

        void ChangeState(State newState);
    }
}
using Practices.NPC_Example.GameElements.Characters;
using System;
using UnityEngine;

namespace Practices.NPC_Example.FSM
{
    public class StateMachineBehaviourBase : StateMachineBehaviour
    {
        [SerializeField] State _state;
        readonly int IS_DIRTY_HASH = Animator.StringToHash("IsDirty");
        public event Action<State> onStateEntered;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            animator.SetBool(IS_DIRTY_HASH, false);
            onStateEntered?.Invoke(_state);
        }
    }
}
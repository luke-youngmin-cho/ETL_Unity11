using UnityEngine.AI;

namespace Practices.NPC_Example.Utilities
{
    public static class NavMeshAgentExtensions
    {
        public static bool IsReachedToTarget(this NavMeshAgent agent)
        {
            return agent.hasPath == false ||
                   agent.remainingDistance <= agent.stoppingDistance;
        }
    }
}
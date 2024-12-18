using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Seek : TargetDetection
    {
        public Seek(BehaviourTree tree, float angle, float radius, float height, float maxDistance, LayerMask targetMask, LayerMask obstacleMask) 
            : base(tree, angle, radius, height, maxDistance, targetMask, obstacleMask)
        {
        }

        public override Result Invoke()
        {
            // 1. 기존에 감지된 타겟이 있는지 확인
            //  있으면
            //      타겟에 도착했는지 확인
            //      타겟을 추적중인지 확인
            //      타겟이 한계범위를 벗어났는지 확인
            //  없으면

            if (blackboard.target)
            {
                float distance = Vector3.Distance(blackboard.transform.position, blackboard.target.position);

                // 목표 추적 완료 (목표에 도착함)
                if (distance <= blackboard.agent.stoppingDistance)
                {
                    return Result.Success;
                }
                // 목표 추적중
                else if (distance < maxDistance)
                {
                    blackboard.agent.SetDestination(blackboard.target.position);
                    return Result.Running;
                }
                // 목표 추적 범위 벗어남
                else
                {
                    blackboard.target = null;
                    blackboard.agent.ResetPath();
                    return Result.Failure;
                }
            }
            else
            {
                if (TryDetectTarget(out Transform target))
                {
                    return Result.Running;
                }
            }

            return Result.Failure;
        }
    }
}
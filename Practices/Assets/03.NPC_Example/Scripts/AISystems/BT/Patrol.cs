using Practices.NPC_Example.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Patrol : TargetDetection
    {
        public Patrol(BehaviourTree tree, 
                    float angle, 
                    float radius, 
                    float height, 
                    float maxDistance, 
                    LayerMask targetMask,
                    LayerMask obstacleMask,
                    float rangeRadius,
                    LayerMask groundMask)
            : base(tree, angle, radius, height, maxDistance, targetMask, obstacleMask)
        {
            _rangeRadius = rangeRadius;
            _groundMask = groundMask;
        }


        float _rangeRadius;
        Vector3 _positionSpanwed;
        LayerMask _groundMask;
        bool _isMoving;


        public override Result Invoke()
        {
            if (blackboard.target)
            {
                _isMoving = false;
                return Result.Success;
            }

            if (blackboard.agent.IsReachedToTarget() == false)
            {
                if (TryDetectTarget(out Transform target))
                {
                    _isMoving = false;
                    blackboard.target = target;
                    return Result.Success;
                }
                else
                {
                    return Result.Running;
                }
            }
            else
            {
                if (_isMoving)
                {
                    _isMoving = false;
                    return Result.Failure;
                }
                else
                {
                    if (TryMoveToRandomDestination())
                    {
                        _isMoving = true;
                        return Result.Running;
                    }
                }
            }

            _isMoving = false;
            return Result.Failure;
        }

        bool TryMoveToRandomDestination()
        {
            int tryCount = 3;

            while (tryCount-- > 0)
            {
                Vector3 currentPosition = blackboard.transform.position;
                Vector2 xz = Random.insideUnitCircle * _rangeRadius;
                Vector3 destination = new Vector3(xz.x, currentPosition.y, xz.y);
                Ray ray = new Ray(destination + Vector3.up * 100f, Vector3.down);

                // NavMesh 에서 특정 위치를 캐스팅하는 방법 
                // 1. NavMesh.Raycast .. Obstacle 등을 모두 내부에서 처리
                // 2. SourcePosition 으로 NavMesh.SamplePosition 을 통해서 가장 근접한 위치 찾기 .. 단순히 인접한 Position 만 찾기때문에 추가 예외처리해야할수도있따.

                if (Physics.Raycast(ray, out RaycastHit raycastHit, float.PositiveInfinity, _groundMask | obstacleMask))
                {
                    int layer = raycastHit.collider.gameObject.layer;
                    int layerFlag = 1 << layer;

                    // hit ground
                    if ((layerFlag & _groundMask) > 0)
                    {
                        if (NavMesh.SamplePosition(raycastHit.point, out NavMeshHit navMeshHit, 0.5f, NavMesh.AllAreas))
                        {
                            blackboard.agent.SetDestination(navMeshHit.position);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    public class Seek : Node
    {
        public Seek(BehaviourTree tree, 
                    float angle, 
                    float radius, 
                    float height, 
                    float maxDistance, 
                    LayerMask targetMask,
                    LayerMask obstacleMask) 
            : base(tree)
        {
            _angle = angle;
            _radius = radius;
            _height = height;
            _maxDistance = maxDistance;
            _targetMask = targetMask;
            _obstacleMask = obstacleMask;
        }


        float _angle;
        float _radius;
        float _height;
        float _maxDistance;
        LayerMask _targetMask;
        LayerMask _obstacleMask;

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
                else if (distance < _maxDistance)
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

        bool TryDetectTarget(out Transform target)
        {
            bool isDetected = false;
            Transform closest = null;
            Collider[] cols =
                Physics.OverlapCapsule(blackboard.transform.position,
                                       blackboard.transform.position + Vector3.up * _height,
                                       _radius,
                                       _targetMask);

            if (cols.Length > 0)
            {
                float minDistance = 0;

                // 시야범위내에 있는 타겟을 모두 찾고, 가장 가까운 타겟으로 설정
                for (int i = 0; i < cols.Length; i++)
                {
                    if (IsInSight(cols[i].transform))
                    {
                        float distance = Vector3.Distance(blackboard.transform.position, cols[i].transform.position);

                        if (closest)
                        {
                            if (distance < minDistance)
                            {
                                closest = cols[i].transform;
                                minDistance = distance;
                                isDetected = true;
                            }
                        }
                        else
                        {
                            closest = cols[i].transform;
                            minDistance = distance;
                            isDetected = true;
                        }
                    }
                }                
            }

            target = closest;
            blackboard.target = target;
            return isDetected;
        }



        bool IsInSight(Transform target)
        {
            // radian : 0 ~ 2 𝝿 
            // degree : 0 ° ~ 360 °
            Vector3 origin = blackboard.transform.position; // 내 위치
            Vector3 forward = blackboard.transform.forward; // 내 앞쪽 방향벡터
            Vector3 lookDir = (target.position - origin).normalized; // 타겟을 바라보는 방향벡터 (크기 1로 정규화)
            float theta = Mathf.Acos(Vector3.Dot(forward, lookDir)) * Mathf.Rad2Deg; // 앞쪽방향벡터와 타겟방향벡터 사이각

            // cos 은 실수 전체 대역이지만, acos 은 유효한 값 대역을 -1 ~ 1 을 정의역으로 갖는다
            // acos 이 정의역(입력축) -1 ~ 1 에서는 치역(출력축) 0 ~ 𝝿 -> 출력각도가 항상 양수이므로 절댓값 안써도됨.
            if (theta <= _angle / 2.0f)
            {
                if (Physics.Raycast(origin + Vector3.up * _height / 2.0f,
                                    lookDir,
                                    out RaycastHit hit,
                                    Vector3.Distance(target.position, origin),
                                    _obstacleMask))
                {
                    return false; // 장애물에 막힘...
                }
                else
                {
                    return true; // OK..!
                }
            }

            return false; // 시야각을 벗어남
        }
    }
}
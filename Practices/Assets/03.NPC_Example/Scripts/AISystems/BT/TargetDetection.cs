using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    public abstract class TargetDetection : Node
    {
        public TargetDetection(BehaviourTree tree,
                               float angle,
                               float radius,
                               float height,
                               float maxDistance,
                               LayerMask targetMask,
                               LayerMask obstacleMask)
            : base(tree)
        {
            this.angle = angle;
            this.radius = radius;
            this.height = height;
            this.maxDistance = maxDistance;
            this.targetMask = targetMask;
            this.obstacleMask = obstacleMask;
        }


        protected float angle;
        protected float radius;
        protected float height;
        protected float maxDistance;
        protected LayerMask targetMask;
        protected LayerMask obstacleMask;


        protected bool TryDetectTarget(out Transform target)
        {
            bool isDetected = false;
            Transform closest = null;
            Collider[] cols =
                Physics.OverlapCapsule(blackboard.transform.position,
                                       blackboard.transform.position + Vector3.up * height,
                                       radius,
                                       targetMask);

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
            if (theta <= angle / 2.0f)
            {
                if (Physics.Raycast(origin + Vector3.up * height / 2.0f,
                                    lookDir,
                                    out RaycastHit hit,
                                    Vector3.Distance(target.position, origin),
                                    obstacleMask))
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

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            Vector3 origin = blackboard.transform.position;
            Vector3 center = origin + Vector3.up * height / 2.0f; // 중심점 위치 (y 축 보정 포함)
            Vector3 forward = blackboard.transform.forward;

            Gizmos.color = blackboard.target ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.5f);

            // Draw the arc (sector range)
            int segments = 50; // 부채꼴 세그먼트 개수
            float step = angle / segments; // 각도 간격
            Vector3 lastPoint = center + Quaternion.Euler(0, -angle / 2, 0) * forward * radius; // 초기 점

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -angle / 2 + step * i;
                Vector3 nextPoint = center + Quaternion.Euler(0, currentAngle, 0) * forward * radius;

                Gizmos.DrawLine(lastPoint, nextPoint);
                lastPoint = nextPoint;
            }

            // Draw lines to the edges of the arc
            Vector3 leftEdge = center + Quaternion.Euler(0, -angle / 2, 0) * forward * radius;
            Vector3 rightEdge = center + Quaternion.Euler(0, angle / 2, 0) * forward * radius;
            Gizmos.DrawLine(center, leftEdge);
            Gizmos.DrawLine(center, rightEdge);
        }
    }
}
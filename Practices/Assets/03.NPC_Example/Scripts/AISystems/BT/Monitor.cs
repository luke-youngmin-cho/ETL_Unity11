using UnityEngine;

namespace Practices.NPC_Example.AISystems.BT
{
    /// <summary>
    /// 랜덤 시간동안 대기하다가 (Running)
    /// 대기 끝나면 반환 (Failure)
    /// 대기중에 타겟이 감지되면 (Success)
    /// </summary>
    public class Monitor : TargetDetection
    {
        public Monitor(BehaviourTree tree,
                       float angle,
                       float radius,
                       float height,
                       float maxDistance,
                       LayerMask targetMask,
                       LayerMask obstacleMask,
                       float minTime,
                       float maxTime)
            : base(tree, angle, radius, height, maxDistance, targetMask, obstacleMask)
        {
            _monitoringTimeMin = minTime;
            _monitoringTimeMax = maxTime;
        }


        float _monitoringTimeMin;
        float _monitoringTimeMax;
        float _monitoringTime;
        float _monitoringTimeMark;
        bool _isMonitoring;


        public override Result Invoke()
        {
            if (blackboard.target)
            {
                _isMonitoring = false;
                return Result.Success;
            }

            if (_isMonitoring)
            {
                float elapsedTime = Time.time - _monitoringTimeMark; // 감시 경과 시간

                // 시간 초과할때까지 대상을 찾지 못함
                if (elapsedTime > _monitoringTime)
                {
                    _isMonitoring = false;
                    return Result.Failure;
                }
                else
                {
                    if (TryDetectTarget(out Transform target))
                    {
                        _isMonitoring = false;
                        blackboard.target = target;
                        return Result.Success; // 감시 대상 찾음 !
                    }
                    else
                    {
                        return Result.Running; // 계속 감시중...
                    }
                }
            }
            else
            {
                if (TryDetectTarget(out Transform target))
                {
                    _isMonitoring = false;
                    blackboard.target = target;
                    return Result.Success; // 감시 대상 찾음 !
                }
                // 감시 시작
                else
                {
                    _monitoringTime = Random.Range(_monitoringTimeMin, _monitoringTimeMax);
                    _monitoringTimeMark = Time.time;
                    _isMonitoring = true;
                    return Result.Running; // 계속 감시중...
                }
            }
        }
    }
}
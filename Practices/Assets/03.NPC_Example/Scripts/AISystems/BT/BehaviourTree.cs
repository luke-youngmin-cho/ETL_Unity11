using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Practices.NPC_Example.AISystems.BT
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BehaviourTree : MonoBehaviour
    {
        public Blackboard blackboard { get; private set; }
        public Root root { get; set; }
        private bool _isRunning;
        public Stack<Node> stack = new Stack<Node>(); // Stack Reserve 는 트리 빌드시 최대 높이 * 자식수들 기반으로~


        private void Update()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            StartCoroutine(C_Tick());
        }

        IEnumerator C_Tick()
        {
            stack.Push(root);

            while (stack.Count > 0)
            {
                Node node = stack.Pop();
                Result result = node.Invoke();

                if (result == Result.Running)
                {
                    stack.Push(node);
                    yield return null;
                }
            }

            _isRunning = false;
        }

        private void OnDrawGizmos()
        {
            if (stack.Count > 0)
                stack.Peek().OnDrawGizmos();
        }


        #region Builder

        Node _current;
        Stack<Composite> _compositeStack;

        public BehaviourTree StartBuild()
        {
            _compositeStack = new Stack<Composite>();
            blackboard = new Blackboard(gameObject);
            root = new Root(this);
            _current = root;
            return this;
        }

        public BehaviourTree Selector()
        {
            Composite composite = new Selector(this);
            Attach(_current, composite);
            return this;
        }

        public BehaviourTree RandomSelector()
        {
            Composite composite = new RandomSelector(this);
            Attach(_current, composite);
            return this;
        }

        public BehaviourTree Sequence()
        {
            Composite composite = new Sequence(this);
            Attach(_current, composite);
            return this;
        }

        public BehaviourTree Parallel(int successCountRequired)
        {
            Composite composite = new Parallel(this, successCountRequired);
            Attach(_current, composite);
            return this;
        }

        public BehaviourTree Decorator(string propertyName)
        {
            Node node = new Decorator(this, propertyName);
            Attach(_current, node);
            return this;
        }

        public BehaviourTree Seek(float angle, float radius, float height, float maxDistance, LayerMask targetMask, LayerMask obstacleMask)
        {
            Node node = new Seek(this, angle, radius, height, maxDistance, targetMask, obstacleMask);
            Attach(_current, node);
            return this;
        }

        public BehaviourTree Patrol(float angle, float radius, float height, float maxDistance, LayerMask targetMask, LayerMask obstacleMask, float rangeRadius, LayerMask groundMask)
        {
            Node node = new Patrol(this, angle, radius, height, maxDistance, targetMask, obstacleMask, rangeRadius, groundMask);
            Attach(_current, node);
            return this;
        }

        public BehaviourTree Monitor(float angle, float radius, float height, float maxDistance, LayerMask targetMask, LayerMask obstacleMask, float monitoringTimeMin, float monitoringTimeMax)
        {
            Node node = new Monitor(this, angle, radius, height, maxDistance, targetMask, obstacleMask, monitoringTimeMin, monitoringTimeMax);
            Attach(_current, node);
            return this;
        }

        public BehaviourTree Attack()
        {
            Node node = new Attack(this);
            Attach(_current, node);
            return this;
        }

        public BehaviourTree CompleteCurrentComposite()
        {
            if (_compositeStack.Count > 0)
                _compositeStack.Pop();
            else
                throw new System.Exception("완성할 컴포지트가 없어요...");

            if (_compositeStack.Count > 0)
                _current = _compositeStack.Peek();

            return this;
        }

        void Attach(Node parent, Node child)
        {
            if (parent is IParent)
                ((IParent)parent).Attach(child);
            else
                throw new System.Exception($"{parent} 는 자식을 가질 수 없습니다.");

            if (child is IParent)
            {
                if (child is Composite)
                    _compositeStack.Push((Composite)child);

                _current = child;
            }
            else
            {
                if (_compositeStack.Count > 0)
                    _current = _compositeStack.Peek();
                else
                    _current = null;
            }
        }
        #endregion
    }
}
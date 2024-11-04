using System;
using UnityEngine;
using UnityEngine.UI;


namespace ARP.Test
{
    // 인터페이스는 다른 객체가 상호작용 할수있는 기능들을 제공하는 추상 개념이므로 
    // 멤버들의 접근제한은 public 임
    public interface IAvailable
    {
        bool this[int index] { get; }
        bool IsAvailable { get; }
        void Use();
        event Action<bool> OnIsAvailableChanged; // event 가 안붙으면 그냥 변수이므로 멤버가될수없다
    }


    // Serialization : 인스턴스를 string/byte[] 형태의 범용적인 데이터셋으로 변환
    // Deserialization : string/byte[] 형태의 범용적인 데이터셋을 프로그램의 특정 타입 인스턴스로 변환
    public class UI_DelegateTest : MonoBehaviour
    {
        // SerializeField Attribute :
        // 멤버변수에 대한 데이터를 직렬화하여 UnityEditor 의 인스펙터창에 노출시키는 특성
        // 사용하는 이유 : 캡슐화된 (private/protected) 멤버변수는 Editor 에서 기본적으로 직렬화하지 않기때문
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _toggleTarget;
        public Predicate<Vector3> match;
        public Action<int, float> action;
        public Func<long, long, int> func;

        private void Awake()
        {
            // _button.onClick += ?? // 일반적인 C# event delegate 에 구독하는 형태
            // _button.onClick.AddListener(??); // Unity 에서 제공하는 UnityAction 객체에 구독하는 형태
            _button.onClick.AddListener(() => _toggleTarget.SetActive(_toggleTarget.activeSelf == false));

            match += IsOrigin;
            match += (position) => position == Vector3.zero;
            // Predicate<Vector3> 에 구독을 한다면 : 파라미터 Vector3 한개, 반환타입 bool 이 기정사실

            if (match.Invoke(Vector3.zero))
            {

            }

            action += LogSum;
            action += (a, b) => Debug.Log(a + b);
            action.Invoke(3, 4.2f);

            func += Sum;
            func += (a, b) =>
            {
                int tmp = (int)(a + b);
                return tmp;
            };
            int sum = func.Invoke(3, 5);
        }

        private bool IsOrigin(Vector3 position)
        {
            return position == Vector3.zero;
        }

        // 람다식은 C# 에서 인라인함수를 구현할 때 사용.
        // 인라인 함수 : 현재 코드라인에 바로 삽입하는 함수
        // 인라인함수 왜씀 ? : 함수오버헤드 없음, 큰 의미없는 함수들이 많아지는것을 방지하여 코드 가독성도 좋아짐 (간단한 함수일때는)

        // (position) => position == Vector3.zero;

        private void LogSum(int a, float b)
        {
            Debug.Log(a + b);
        }

        // (a, b) => Debug.Log(a + b);

        private int Sum(long a, long b)
        {
            int tmp = (int)(a + b);
            return tmp;
        }

        //(a, b) =>
        //{
        //    int tmp = (int)(a + b);
        //    return tmp;
        //}
    }
}
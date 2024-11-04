using UnityEngine;
using UnityEngine.EventSystems;
using ARP.Test.As;
using ARP.Test.Bs;
using A = ARP.Test.As.A;
using static Global;
using System.Collections.Generic;

namespace ARP.Test
{
    public class MouseTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        struct Coord
        {
            public float x, y, z;

            public static Coord operator +(Coord op1, Coord op2)
            {
                return new Coord
                {
                    x = op1.x + op2.x,
                    y = op1.y + op2.y,
                    z = op1.z + op2.z
                };
            }

            public static bool operator ==(Coord op1, Coord op2)
                => op1.x.Equals(op2.x) && op1.y.Equals(op2.y) && op1.z.Equals(op2.z);

            public static bool operator !=(Coord op1, Coord op2)
                => !(op1 == op2);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            Vector3 sum = Vector3.forward + Vector3.back;
            Coord sum2 = new Coord { x = 0, y = 0, z = 0 } + new Coord { x = 1, y = 1, z = 1 };

            if (sum2 == new Coord { x = 0, y = 0, z = 0 })
            {
            }

            throw new System.NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            A a1 = new A(142);
            A a2 = new A(351);

            a1.SayMyAge();
            a2.SayMyAge();
            
            // is : 아닐 가능성이 있을때 예외처리용
            if (a1 is A)
            {
                ((A)a1).SayMyAge();
            }

            // as : 무조건 되야할때 사용
            A aaa = a1 as A;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            List<int> list = new List<int>();

            using (IEnumerator<int> e = list.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    Debug.Log(e.Current);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }
    }
}
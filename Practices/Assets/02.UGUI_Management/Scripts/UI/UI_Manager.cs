using Practices.UGUI_Management.Singletons;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Practices.UGUI_Management.UI
{
    public class UI_Manager : Singleton<UI_Manager>
    {
        public UI_Manager()
        {
            _uis = new Dictionary<Type, UI_Base>(EXPECTED_MAX_UI_COUNT_IN_SCENE); // Reserve 수 : 한 장면에서 최대 사용할 수 있는 UI 갯수
            _popupStack = new List<UI_Popup>(EXPECTED_MAX_POPUP_COUNT_IN_SCENE); // Reserve 수 : 한 장면에서 최대 동시에 띄울 수 있는 Popup 갯수
        }


        public IEnumerable<UI_Popup> popups => _popupStack;

        const int EXPECTED_MAX_UI_COUNT_IN_SCENE = 30;
        const int EXPECTED_MAX_POPUP_COUNT_IN_SCENE = 10;
        Dictionary<Type, UI_Base> _uis; // 현재 장면에서 사용할 수 있는 UI 목록
        UI_Screen _screen; // 현재 장면의 스크린을 차지하는 UI
        List<UI_Popup> _popupStack; // 현재 활성화되어있는 팝업들을 순차적으로 관리하는 스택


        public void Register(UI_Base ui)
        {
            if (_uis.TryAdd(ui.GetType(), ui))
            {
                Debug.Log($"Registered UI {ui.GetType()}");

                if (ui is UI_Popup)
                {
                    ui.onShow += () => Push((UI_Popup)ui);
                    ui.onHide += () => Pop((UI_Popup)ui);
                }
            }
            else
            {
                throw new Exception($"Failed to register ui {ui.GetType()}. already exist.");
            }
        }

        public void Unregister(UI_Base ui)
        {
            if (_uis.Remove(ui.GetType()))
            {
                if (ui is UI_Popup)
                {
                    Pop((UI_Popup)ui);
                }
            }
        }

        public T Resolve<T>()
            where T : UI_Base
        {
            if (_uis.TryGetValue(typeof(T), out UI_Base result))
            {
                return (T)result;
            }
            else
            {
                string path = $"UI/Canvas - {typeof(T).Name.Substring(3)}";
                UI_Base prefab = Resources.Load<UI_Base>(path);

                if (prefab == null)
                    throw new Exception($"Failed to resolve ui {typeof(T)}. Not exist.");

                return (T)GameObject.Instantiate(prefab);
            }
        }

        public void SetScreen(UI_Screen screen)
        {
            // 이미 활성화된 스크린 UI 가 있으면 끔
            if (_screen != null)
            {
                _screen.inputActionsEnabled = false;
                _screen.Hide();
            }

            _screen = screen;
            _screen.sortingOrder = 0;
            _screen.inputActionsEnabled = true;
        }

        public void Push(UI_Popup popup)
        {
            int popupIndex = _popupStack.FindLastIndex(ui => ui == popup);

            // 이미 이 팝업이 활성화 되어있다면, 제거하고 가장뒤로 보내야함
            if (popupIndex >= 0)
            {
                _popupStack.RemoveAt(popupIndex);
            }

            int sortingOrder = 1;

            if (_popupStack.Count > 0)
            {
                UI_Popup prevPopup = _popupStack[^1];
                prevPopup.inputActionsEnabled = false;
                sortingOrder = prevPopup.sortingOrder + 1;
            }

            popup.sortingOrder = sortingOrder;
            popup.inputActionsEnabled = true;
            _popupStack.Add(popup);
            Debug.Log($"Pushed {popup.name}");
        }

        public void Pop(UI_Popup popup)
        {
            int popupIndex = _popupStack.FindLastIndex(ui => ui == popup);

            if (popupIndex < 0)
                return;

            // 빼려는게 마지막이었으면 이전꺼를 활성화
            if (popupIndex == _popupStack.Count - 1)
            {
                _popupStack[popupIndex].inputActionsEnabled = false;

                // 이전 팝업이 존재한다면
                if (popupIndex > 0)
                    _popupStack[popupIndex - 1].inputActionsEnabled = true;
            }

            _popupStack.RemoveAt(popupIndex);

            Debug.Log($"Popped {popup.name}");
        }
    }
}
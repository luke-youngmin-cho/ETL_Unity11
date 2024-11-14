using Practices.MVC_Example.Database;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Practices.MVC_Example.UI
{
    public class UI_Inventory : MonoBehaviour
    {
        [SerializeField] InventorySlot _inventorySlotPrefab;
        [SerializeField] Transform _content;
        [SerializeField] ItemSpecRepository _itemSpecRepository;
        List<InventorySlot> _inventorySlots;
        string _inventoryDataPath;
        int _inventorySlotNum = 40;
        [SerializeField] InputActionReference _leftClick;
        [SerializeField] InputActionReference _rightClick;
        GraphicRaycaster _graphicRaycaster;
        InventorySlot _selectedSlot;
        List<RaycastResult> _raycastResults;
        PointerEventData _pointerEventData;
        [SerializeField] Image _previewSelectedSlot;
        event Action<int, InventorySlotData> onSlotDataChanged;

        private void Awake()
        {
            _raycastResults = new List<RaycastResult>();
            _pointerEventData = new PointerEventData(EventSystem.current);
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _inventoryDataPath = Application.persistentDataPath + "/InventoryData.json";
            _inventorySlots = new List<InventorySlot>(_inventorySlotNum);
            
            for (int i = 0; i < _inventorySlotNum; i++)
            {
                InventorySlot slot = Instantiate(_inventorySlotPrefab, _content);
                slot.index = i;
                slot.itemId = -1;
                slot.itemIcon = null;
                slot.itemNum = 0;
                _inventorySlots.Add(slot);
            }

            onSlotDataChanged += (slotIndex, changedData) =>
            {
                // 아이템이 있는 슬롯
                if (changedData.itemId >= 0)
                {
                    ItemSpec itemSpec = _itemSpecRepository.Get(changedData.itemId); // 현재 슬롯 데이터에 해당하는 아이템 사양서 가져오기
                    _inventorySlots[slotIndex].itemIcon = itemSpec.icon;
                    _inventorySlots[slotIndex].itemNum = changedData.itemNum;
                }
                // 빈슬롯
                else
                {
                    _inventorySlots[slotIndex].itemIcon = null;
                    _inventorySlots[slotIndex].itemNum = 0;
                }
            };

            _leftClick.action.Enable();
            _leftClick.action.performed += OnLeftClick;
        }

        private void Start()
        {
            InventoryData inventoryData = LoadInventoryData();
            RefreshAllInventorySlots(inventoryData);
        }

        private void Update()
        {
            if (_selectedSlot)
            {
                _previewSelectedSlot.transform.position = Mouse.current.position.ReadValue();
            }
        }

        private void OnLeftClick(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
                return;

            _pointerEventData.position = Mouse.current.position.ReadValue();
            _raycastResults.Clear();
            _graphicRaycaster.Raycast(_pointerEventData, _raycastResults);

            // 선택된 슬롯 있음
            if (_selectedSlot)
            {
                // UI 가 클릭됨
                if (_raycastResults.Count > 0)
                {
                    // 슬롯이 캐스팅됐는지?
                    if (_raycastResults[0].gameObject.TryGetComponent(out InventorySlot slot))
                    {
                        // 이미 선택된 슬롯 선택시 선택 취소
                        if (slot == _selectedSlot)
                        {
                            _selectedSlot = null;
                            _previewSelectedSlot.enabled = false;
                        }
                        // 다른 슬롯 선택시 데이터 스왑
                        else
                        {
                            SwapCommand(_selectedSlot.index, slot.index);
                            _selectedSlot = null;
                            _previewSelectedSlot.enabled = false;
                        }
                    }
                }
                // Battle field 가 클릭됨
                {
                    // nothing to do
                }
            }
            // 선택된 슬롯 없음
            else
            {
                // UI 가 클릭됨
                if (_raycastResults.Count > 0)
                {
                    // 슬롯이 캐스팅됐는지?
                    if (_raycastResults[0].gameObject.TryGetComponent(out InventorySlot slot))
                    {
                        if (slot.itemNum != 0)
                        {
                            _selectedSlot = slot;
                            _previewSelectedSlot.sprite = _selectedSlot.itemIcon;
                            _previewSelectedSlot.enabled = true;
                        }
                    }
                }
                // Battle field 가 클릭됨
                {
                    // nothing to do
                }
            }
        }

        /// <summary>
        /// 인벤토리 데이터를 전체 순회하면서 슬롯View 를 모두 갱신하는 함수
        /// </summary>
        /// <param name="inventoryData"></param>
        public void RefreshAllInventorySlots(InventoryData inventoryData)
        {
            for (int i = 0; i < inventoryData.slotDataList.Count; i++)
            {
                InventorySlotData slotData = inventoryData.slotDataList[i];

                // 아이템이 있는 슬롯
                if (slotData.itemId >= 0)
                {
                    ItemSpec itemSpec = _itemSpecRepository.Get(slotData.itemId); // 현재 슬롯 데이터에 해당하는 아이템 사양서 가져오기
                    _inventorySlots[i].itemIcon = itemSpec.icon;
                    _inventorySlots[i].itemNum = slotData.itemNum;
                }
                // 빈슬롯
                else
                {
                    _inventorySlots[i].itemIcon = null;
                    _inventorySlots[i].itemNum = 0;
                }
            }
        }

        /// <summary>
        /// 인벤토리 View 를 갱신하기위해서 저장되어있던 인벤토리 데이터를 불러옴
        /// </summary>
        private InventoryData LoadInventoryData()
        {
            if (File.Exists(_inventoryDataPath) == false) 
            {
                CreateDefaultInventoryData();
            }

            string jsonData = File.ReadAllText(_inventoryDataPath); // 경로에서 텍스트 읽어옴
            InventoryData inventoryData = JsonUtility.FromJson<InventoryData>(jsonData); // Deserialize (읽은 텍스트를 InventoryData로 변환)
            return inventoryData;
        }
        
        /// <summary>
        /// 인벤토리 데이터가 한번도 만들어진적이 없을때
        /// 최초로한번 기본값으로 생성해줌
        /// </summary>
        private void CreateDefaultInventoryData()
        {
            InventoryData inventoryData = new InventoryData(_inventorySlotNum); // 아이템 추가할 용량 2 설정
            inventoryData.slotDataList.Add(new InventorySlotData { itemId = 10001, itemNum = 52 });
            inventoryData.slotDataList.Add(new InventorySlotData { itemId = 10002, itemNum = 2 });

            // 나머지는 빈 슬롯 추가
            for (int i = inventoryData.slotDataList.Count; i < _inventorySlotNum; i++)
            {
                inventoryData.slotDataList.Add(new InventorySlotData { itemId = -1, itemNum = 0 });
            }
            string jsonData = JsonUtility.ToJson(inventoryData); // Serialize (InventoryData 를 json 포맷의 문자열로)
            File.WriteAllText(_inventoryDataPath, jsonData); // 바뀐 json 문자열 파일로 저장
        }

        private void SwapCommand(int slot1Index, int slot2Index)
        {
            InventoryData inventoryData = LoadInventoryData();
            InventorySlotData slot1Data = inventoryData.slotDataList[slot1Index];
            inventoryData.slotDataList[slot1Index] = inventoryData.slotDataList[slot2Index];
            inventoryData.slotDataList[slot2Index] = slot1Data;
            SaveInventoryData(inventoryData);
            onSlotDataChanged.Invoke(slot1Index, inventoryData.slotDataList[slot1Index]);
            onSlotDataChanged.Invoke(slot2Index, inventoryData.slotDataList[slot2Index]);
        }

        private void SaveInventoryData(InventoryData inventoryData)
        {
            string jsonData = JsonUtility.ToJson(inventoryData);
            File.WriteAllText(_inventoryDataPath, jsonData);
        }
    }
}
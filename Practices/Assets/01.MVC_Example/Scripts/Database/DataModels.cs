/*
 * 사용자 정의 자료형은 기본적으로 Serialize 가 안된다.
 * C# 에서는 System.Serializable Attribute 를 부여해서 Serialize 가 가능하도록 명시해야한다.
 */
using System;
using System.Collections.Generic;

namespace Practices.MVC_Example.Database
{
    [Serializable]
    public struct InventorySlotData
    {
        public int itemId;
        public int itemNum;
    }

    [Serializable]
    public class InventoryData
    {
        public InventoryData(int capacity)
        {
            slotDataList = new List<InventorySlotData>(capacity);
        }


        public List<InventorySlotData> slotDataList;
    }
}
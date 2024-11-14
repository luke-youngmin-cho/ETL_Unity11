using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Practices.MVC_Example.UI
{
    public class InventorySlot : MonoBehaviour
    {
        public int index { get; set; }
        public Sprite itemIcon
        {
            get => _itemIcon.sprite;
            set => _itemIcon.sprite = value;
        }

        public int itemId
        {
            get => _itemId;
            set => _itemId = value;
        }

        public int itemNum
        {
            get => _itemNum;
            set 
            {
                if (value < 2)
                    _itemNumText.text = string.Empty;
                else
                    _itemNumText.text = value.ToString();

                _itemNum = value;
            }
        }


        [SerializeField] Image _itemIcon;
        [SerializeField] TMP_Text _itemNumText;
        private int _itemId;
        private int _itemNum;
    }
}
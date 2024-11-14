using System.Collections.Generic;
using UnityEngine;

namespace Practices.MVC_Example.Database
{
    [CreateAssetMenu(fileName = "ItemSpecRepository", menuName = "Scriptable Objects/ItemSpecRepository")]
    public class ItemSpecRepository : ScriptableObject
    {
        [field: SerializeField] public List<ItemSpec> specs;


        public ItemSpec Get(int itemId)
        {
            int index = specs.FindIndex(spec => spec.id == itemId);

            if (index < 0)
            {
                throw new System.Exception($"[{nameof(ItemSpecRepository)}] : Failed to get item spec. wrong id {itemId}.");
            }
            else
            {
                return specs[index];
            }
        }
    }
}
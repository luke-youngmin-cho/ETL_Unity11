using UnityEngine;

namespace Practices.MVC_Example.Database
{
    [CreateAssetMenu(fileName = "ItemSpec", menuName = "Scriptable Objects/ItemSpec")]
    public class ItemSpec : ScriptableObject
    {
        [field:SerializeField] public int id { get; private set; }
        [field:SerializeField] public string description { get; private set; }
        [field:SerializeField] public int price { get; private set; }
        [field:SerializeField] public Sprite icon { get; private set; }
        [field:SerializeField] public GameObject prefab { get; private set; }
    }
}
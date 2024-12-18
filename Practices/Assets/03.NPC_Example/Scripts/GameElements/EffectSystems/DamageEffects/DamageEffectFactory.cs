using System.Collections.Generic;
using UnityEngine;

namespace Practices.NPC_Example.GameElements.EffectSystems.DamageEffects
{
    public class DamageEffectFactory
    {
        public DamageEffectFactory()
        {
            _effectTable = new Dictionary<string, DamageEffect>();
        }

        public DamageEffect Create(string effectName, Vector3 position, float damageAmount)
        {
            DamageEffect concreteInstance = GameObject.Instantiate(GetPrefab(effectName));
            concreteInstance.Show(damageAmount);
            concreteInstance.transform.position = position;
            return concreteInstance;
        }


        Dictionary<string, DamageEffect> _effectTable;


        DamageEffect GetPrefab(string effectName)
        {
            DamageEffect prefab;

            if (_effectTable.TryGetValue(effectName, out prefab) == false)
            {
                prefab = Resources.Load<DamageEffect>($"DamageEffects/{effectName}");

                if (prefab == false)
                {
                    throw new System.Exception($"[{nameof(DamageEffectFactory)}] : Failed to load asset \"{effectName}\"");
                }
            }

            return prefab;
        }
    }
}
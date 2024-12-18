using System.Collections;
using TMPro;
using UnityEngine;

namespace Practices.NPC_Example.GameElements.EffectSystems.DamageEffects
{
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageEffect : MonoBehaviour
    {
        TextMeshPro _damageAmount;
        float _fadeoutDuration = 0.8f;
        float _moveUpDistance = 0.5f;
        bool _isFading;


        public void Show(float damageAmount)
        {
            _damageAmount = GetComponent<TextMeshPro>();
            _damageAmount.text = ((int)damageAmount).ToString();

            if (_isFading)
                StopAllCoroutines();

            _isFading = true;
            StartCoroutine(C_FadeOut());
            StartCoroutine(C_AlignToScreen());
        }


        IEnumerator C_FadeOut()
        {
            Color tmpColor = _damageAmount.color;
            tmpColor.a = 1;
            _damageAmount.color = tmpColor;
            float timeMark = Time.time;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + Vector3.up * _moveUpDistance;

            while (Time.time - timeMark < _fadeoutDuration)
            {
                float t = (Time.time - timeMark) / _fadeoutDuration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                tmpColor.a = Mathf.Lerp(1f, 0f, t);
                _damageAmount.color = tmpColor;
                yield return null;
            }

            _isFading = false;
            Destroy(gameObject);
        }

        IEnumerator C_AlignToScreen()
        {
            Camera camera = Camera.main;

            while (_isFading)
            {
                Vector3 screenPoint = camera.WorldToScreenPoint(transform.position);
                Ray ray = camera.ScreenPointToRay(screenPoint);
                transform.rotation = Quaternion.LookRotation(ray.direction);
                yield return null;
            }
        }
    }
}
using System.Collections;
using UnityEngine;

public class CoroutineTester : MonoBehaviour
{
    bool _isBusy;

    private void Update()
    {
        if (_isBusy)
            return;

        _isBusy = true;
        IEnumerator enumerator = C_Count();
        StartCoroutine(enumerator);
    }

    IEnumerator C_Count()
    {
        Debug.Log(1);
        yield return null;
        Debug.Log(2);
        _isBusy = false;
        yield break;
    }
}

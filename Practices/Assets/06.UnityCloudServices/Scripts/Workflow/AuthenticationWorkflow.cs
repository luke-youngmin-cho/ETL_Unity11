using Practices.UGUI_Management.UI;
using Practices.UnityCloudServices.Services;
using Practices.UnityCloudServices.UI;
using System;
using System.Collections;
using Unity.Services.Core;
using UnityEngine;

namespace Practices.UnityCloudServices
{
    public class AuthenticationWorkflow : MonoBehaviour
    {
        AuthenticationFacade _authenticationFacade;

        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity service initialized.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _authenticationFacade = new AuthenticationFacade();
            StartCoroutine(Workflow());
        }

        IEnumerator Workflow()
        {
            yield return new WaitUntil(() => UnityServices.State == ServicesInitializationState.Initialized);
            UI_Manager.instance.Resolve<UI_Authentication>().Show();
            yield break;
        }
    }
}
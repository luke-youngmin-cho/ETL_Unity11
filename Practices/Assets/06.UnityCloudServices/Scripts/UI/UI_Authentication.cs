using Practices.UGUI_Management.UI;
using Practices.UGUI_Management.Utilities;
using Practices.UnityCloudServices.Services;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Practices.UnityCloudServices.UI
{
    public class UI_Authentication : UI_Screen
    {
        [Resolve] TMP_InputField _id;
        [Resolve] TMP_InputField _password;
        [Resolve] Button _google;
        [Resolve] Button _facebook;
        [Resolve] Button _apple;
        [Resolve] Button _login;
        [Resolve] Button _register;
        UsernameAuthInterface _usernameAuthInterface;

        protected override void Awake()
        {
            base.Awake();

            _usernameAuthInterface = new UsernameAuthInterface();

            _id.onValueChanged.AddListener(value =>
            {
                _login.interactable = value.Length > 2 && _password.text.Length > 2;
            });

            _password.onValueChanged.AddListener(value =>
            {
                _login.interactable = value.Length > 2 && _id.text.Length > 2;
            });

            _login.onClick.AddListener(async () =>
            {
                _login.interactable = false;

                (bool success, string message) result = await _usernameAuthInterface.SignInWithUsernamePasswordAsync(_id.text, _password.text);
                await Awaitable.MainThreadAsync();
                UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

                if (result.success)
                {
                    confirmWindow.Show(result.message);
                }
                else
                {
                    confirmWindow.Show(result.message);
                }
            });

            _register.onClick.AddListener(() =>
            {
                UI_Manager.instance.Resolve<UI_Registration>().Show();
            });
        }
    }
}
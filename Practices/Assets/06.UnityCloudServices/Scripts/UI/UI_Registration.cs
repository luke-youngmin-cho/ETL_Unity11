using Practices.UGUI_Management.UI;
using Practices.UGUI_Management.Utilities;
using Practices.UnityCloudServices.Services;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Scripting;
using UnityEngine;

namespace Practices.UnityCloudServices.UI
{
    public class UI_Registration : UI_Popup
    {
        [Resolve] TMP_InputField _id;
        [Resolve] TMP_InputField _password;
        [Resolve] Button _submit;
        UsernameAuthInterface _usernameAuthInterface;


        protected override void Awake()
        {
            base.Awake();
            
            _usernameAuthInterface = new UsernameAuthInterface();

            _id.onValueChanged.AddListener(value =>
            {
                _submit.interactable = value.Length > 2 && _password.text.Length > 2;
            });

            _password.onValueChanged.AddListener(value =>
            {
                _submit.interactable = value.Length > 2 && _id.text.Length > 2;
            });

            _submit.onClick.AddListener(async () =>
            {
                _submit.interactable = false;

                (bool success, string message) result = await _usernameAuthInterface.SignUpWithUsernamePasswordAsync(_id.text, _password.text);
                await Awaitable.MainThreadAsync();
                UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

                if (result.success)
                {
                    confirmWindow.Show(result.message, Hide);
                    await Awaitable.WaitForSecondsAsync(2f);
                    confirmWindow.Hide();
                    Hide();
                }
                else
                {
                    confirmWindow.Show(result.message);
                }
            });
        }
    }
}
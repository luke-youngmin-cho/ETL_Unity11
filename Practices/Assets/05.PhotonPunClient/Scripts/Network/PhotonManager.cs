using Photon.Pun;
using UnityEngine;

namespace Practices.PhotonPunClient.Network
{
    public class PhotonManager : MonoBehaviourPunCallbacks
    {
        public static PhotonManager instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new GameObject(nameof(PhotonManager)).AddComponent<PhotonManager>();
                }

                return s_instance;
            }
        }

        static PhotonManager s_instance;


        private void Awake()
        {
            if (s_instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                s_instance = this;
            }

            if (PhotonNetwork.IsConnected == false)
            {
#if UNITY_EDITOR
                PhotonNetwork.LogLevel = PunLogLevel.Full;
                Application.runInBackground = true;
#endif
                PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues(Random.Range(0, 999999999).ToString());
                PhotonNetwork.NickName = Random.Range(0, 999999999).ToString();
                bool isConnected = PhotonNetwork.ConnectUsingSettings();
                Debug.Assert(isConnected, $"[{nameof(PhotonManager)}] Failed to connect to photon pun server.");
            }

            DontDestroyOnLoad(gameObject);
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            
            PhotonNetwork.AutomaticallySyncScene = true; // 현재 속해있는 방의 방장이 씬을 전환하면 따라서 전환하는 옵션
            // PhotonNetwork.NickName
            Debug.Log($"[{nameof(PhotonManager)}] Connected to master server.");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log($"[{nameof(PhotonManager)}] Joined lobby.");
        }
    }
}


using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Practices.PhotonPunClient.Network
{
    public class ClientCharacterController : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable
    {
        public int ownerActorNr => _photonView.OwnerActorNr;
        public int photonViewId => _photonView.ViewID;
        public bool isInitialized { get; private set; }

        PhotonView _photonView;
        NavMeshAgent _agent;
        InputActions _inputActions;
        [SerializeField] LayerMask _groundMask;
        Vector3 _positionCached;
        Quaternion _rotationCached;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _agent = GetComponent<NavMeshAgent>();
        }

        private void FixedUpdate()
        {
            transform.position = _positionCached;
            transform.rotation = _rotationCached;
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { PlayerInGamePlayPropertyKey.IS_CHARACTER_SPAWNED, true }
            });

            isInitialized = true;

            if (_photonView.IsMine)
            {
                _agent.enabled = true;
                _inputActions = new InputActions();
                _inputActions.Player.MouseRight.performed += OnRightClick;
                _inputActions.Enable();
            }
            else
            {
                _agent.enabled = false;
            }

            Debug.Log("Instantiated");
        }

        void OnRightClick(InputAction.CallbackContext context)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Debug.DrawRay(ray.origin, ray.direction);

            if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, _groundMask))
            {
                Debug.Log("Hit");
                _agent.SetDestination(hit.point);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                _positionCached = (Vector3)stream.ReceiveNext();
                _rotationCached = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using ExitGames.Client.Photon.StructWrapping;

namespace Practices.PhotonPunClient
{
    public class GamePlayWorkflow : MonoBehaviour
    {
        WaitForSeconds _waitFor1Seconds = new WaitForSeconds(1);


        private void Start()
        {
            StartCoroutine(C_Workflow());
        }

        IEnumerator C_Workflow()
        {
            SpawnPlayerCharacterRandomly();
            yield return StartCoroutine(C_WaitUntilAllPlayerCharactersAreSpawned());
            // TODO: Player input enable
        }

        void SpawnPlayerCharacterRandomly()
        {
            Vector2 xz = Random.insideUnitCircle * 5f;
            Vector3 randomPosition = new Vector3(xz.x, 0f, xz.y);
            PhotonNetwork.Instantiate("PhotonNetworkObjects/PlayerCharacter",
                                      randomPosition,
                                      Quaternion.identity);
        }

        IEnumerator C_WaitUntilAllPlayerCharactersAreSpawned()
        {
            while (true)
            {
                bool allReady = true;

                foreach (Player player in PhotonNetwork.PlayerListOthers)
                {
                    if (player.CustomProperties.TryGetValue(PlayerInGamePlayPropertyKey.IS_CHARACTER_SPAWNED, out bool isCharacterSpawned))
                    {
                        if (isCharacterSpawned == false)
                        {
                            allReady = false;
                            break;
                        }
                    }
                    else
                    {
                        allReady = false;
                        break;
                    }
                }

                if (allReady)
                    break;

                yield return _waitFor1Seconds;
            }
        }
    }
}
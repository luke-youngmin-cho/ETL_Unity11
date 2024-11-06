using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System;

namespace ARP.GoogleMaps
{
    public class GoogleMapInterface : MonoBehaviour
    {
        private const string BASE_URL = "https://maps.googleapis.com/maps/api/staticmap?";
        private const string API_KEY = "AIzaSyAkZQSzVvCNeR0O_6GPQJ6L4BJl16UYiBg";
        private Texture2D _cachedTexture;


        public void LoadMap(float latitude, float longitude, float zoom, Vector2 size, Action<Texture2D> onComplete)
        {
            StartCoroutine(C_LoadMap(latitude, longitude, zoom, size, onComplete));
        }

        IEnumerator C_LoadMap(float latitude, float longitude, float zoom, Vector2 size, Action<Texture2D> onComplete)
        {
            string url =
                BASE_URL +
                "center=" + latitude + "," + longitude +
                "&zoom=" + zoom.ToString() +
                "&size=" + size.x.ToString() + "x" + size.y.ToString() +
                "&key=" + API_KEY;

            Debug.Log($"[{nameof(GoogleMapInterface)}] : Request map texture ... {url}");

            url = UnityWebRequest.UnEscapeURL(url); //Url에 대한  Web 요청
            UnityWebRequest req = UnityWebRequestTexture.GetTexture(url); //Texture에 대한 요청 

            yield return req.SendWebRequest();  //요청 전송

            _cachedTexture = DownloadHandlerTexture.GetContent(req); // 받은 Texture를 RAW 이미지에 적용
            onComplete.Invoke(_cachedTexture);
        }
    }
}
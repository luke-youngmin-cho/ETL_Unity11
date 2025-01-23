using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class GoogleOAuthExample : MonoBehaviour
{
    // 구글 OAuth용 Client 정보 (예시)
    private const string CLIENT_ID = "Client id";
    private const string CLIENT_SECRET = "Scret key";
    private const string AUTHORIZATION_ENDPOINT = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TOKEN_ENDPOINT = "https://www.googleapis.com/oauth2/v4/token";
    private const string USERINFO_ENDPOINT = "https://www.googleapis.com/oauth2/v3/userinfo";

    private void LogMessage(string msg)
    {
        Debug.Log(msg);
    }

    // 예시로, Unity 시작 시 버튼 클릭 없이 자동으로 OAuth 플로우를 시작하려면
    // Start() 또는 OnGUI() 등에서 함수를 호출할 수 있습니다.
    void Start()
    {
        // 필요 시 주석 해제 후 테스트
        //StartOAuthFlow();
    }

    /// <summary>
    /// 외부에서 이 함수를 호출하면 OAuth 인증 과정을 시작합니다.
    /// </summary>
    public async void StartOAuthFlow()
    {
        // 1. 상태 토큰, PKCE용 code_verifier/challenge 생성
        string state = RandomDataBase64Url(32);
        string codeVerifier = RandomDataBase64Url(32);
        string codeChallenge = Base64UrlencodeNoPadding(Sha256(codeVerifier));
        const string codeChallengeMethod = "S256";

        // 2. 리디렉션 받을 로컬주소, 포트 생성 (HttpListener)
        int port = GetRandomUnusedPort();
        string redirectUri = $"http://127.0.0.1:{port}/";
        LogMessage($"Redirect URI: {redirectUri}");

        HttpListener http = new HttpListener();
        http.Prefixes.Add(redirectUri);
        http.Start();
        LogMessage("Listening on local HttpListener...");

        // 3. Authorization Request URL 생성
        //    (여기서는 scope=openID+profile 정도만, 필요하면 email, calendar 등 추가)
        string authorizationRequest = string.Format(
            "{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
            AUTHORIZATION_ENDPOINT,
            Uri.EscapeDataString(redirectUri),
            CLIENT_ID,
            state,
            codeChallenge,
            codeChallengeMethod
        );

        // 4. 외부 브라우저 열기
        Application.OpenURL(authorizationRequest);

        // 5. HttpListener로 브라우저 리다이렉트 응답 대기 (비동기)
        var context = await http.GetContextAsync();

        // 6. 브라우저에 간단히 응답 보내고 로컬 서버 종료
        string responseString = "<html><body>Authentication completed. You can close this tab.</body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.ContentLength64 = buffer.Length;
        using (var responseOutput = context.Response.OutputStream)
        {
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
        }
        http.Stop();

        // 7. 쿼리스트링에서 code, state 파라미터 확인
        var query = context.Request.QueryString;
        if (query.Get("error") != null)
        {
            LogMessage($"OAuth authorization error: {query.Get("error")}");
            return;
        }
        string code = query.Get("code");
        string incomingState = query.Get("state");

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(incomingState))
        {
            LogMessage("Malformed authorization response.");
            return;
        }
        if (!incomingState.Equals(state))
        {
            LogMessage($"Invalid state parameter. Expected={state}, Received={incomingState}");
            return;
        }

        LogMessage($"Authorization Code: {code}");

        // 8. Authorization Code를 사용하여 토큰 요청
        await PerformCodeExchange(code, codeVerifier, redirectUri);
    }

    /// <summary>
    /// Authorization Code -> Access Token 교환
    /// </summary>
    private async Task PerformCodeExchange(string code, string codeVerifier, string redirectUri)
    {
        LogMessage("Exchanging code for tokens...");

        // POST 바디 구성
        string tokenRequestBody = string.Format(
            "code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
            code,
            Uri.EscapeDataString(redirectUri),
            CLIENT_ID,
            codeVerifier,
            CLIENT_SECRET
        );

        // HttpWebRequest로 요청
        HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(TOKEN_ENDPOINT);
        tokenRequest.Method = "POST";
        tokenRequest.ContentType = "application/x-www-form-urlencoded";

        byte[] bodyBytes = Encoding.ASCII.GetBytes(tokenRequestBody);
        tokenRequest.ContentLength = bodyBytes.Length;

        using (var stream = await tokenRequest.GetRequestStreamAsync())
        {
            await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
        }

        // 응답 처리
        try
        {
            var tokenResponse = await tokenRequest.GetResponseAsync();
            using (var reader = new StreamReader(tokenResponse.GetResponseStream()))
            {
                string responseText = await reader.ReadToEndAsync();
                LogMessage($"Token response:\n{responseText}");

                // JSON 파싱
                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
                if (tokenData.ContainsKey("access_token"))
                {
                    string accessToken = tokenData["access_token"];
                    // 필요 시 refresh_token, id_token 등도 활용

                    // 9. Access Token으로 UserInfo 엔드포인트 요청
                    await RequestUserInfo(accessToken);
                }
            }
        }
        catch (WebException ex)
        {
            LogMessage($"Token request failed: {ex.Message}");
            if (ex.Response != null)
            {
                using (var errorReader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    var errorResponse = await errorReader.ReadToEndAsync();
                    LogMessage($"Error detail: {errorResponse}");
                }
            }
        }
    }

    /// <summary>
    /// Access Token을 사용해 구글 UserInfo API 호출
    /// </summary>
    private async Task RequestUserInfo(string accessToken)
    {
        LogMessage("Requesting UserInfo...");

        var userinfoRequest = (HttpWebRequest)WebRequest.Create(USERINFO_ENDPOINT);
        userinfoRequest.Method = "GET";
        userinfoRequest.Headers.Add($"Authorization: Bearer {accessToken}");

        try
        {
            var response = await userinfoRequest.GetResponseAsync();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = await reader.ReadToEndAsync();
                LogMessage($"UserInfo response:\n{json}");
            }
        }
        catch (WebException ex)
        {
            LogMessage($"UserInfo request failed: {ex.Message}");
        }
    }

    #region ---- Utility Methods ----

    /// <summary>
    /// 사용되지 않은 임의 포트를 반환
    /// </summary>
    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// PKCE 등에 사용될 임의의 바이트 배열을 Base64 URL Safe 문자열로 변환
    /// </summary>
    private string RandomDataBase64Url(uint length)
    {
        var rng = new RNGCryptoServiceProvider();
        byte[] bytes = new byte[length];
        rng.GetBytes(bytes);
        return Base64UrlencodeNoPadding(bytes);
    }

    /// <summary>
    /// SHA256 해시
    /// </summary>
    private byte[] Sha256(string inputString)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(inputString);
        using (var sha = new SHA256Managed())
        {
            return sha.ComputeHash(bytes);
        }
    }

    /// <summary>
    /// Base64 URL Safe 인코딩 (Padding 제거)
    /// </summary>
    private string Base64UrlencodeNoPadding(byte[] buffer)
    {
        string base64 = Convert.ToBase64String(buffer);

        // URL-safe 변환
        base64 = base64.Replace("+", "-").Replace("/", "_");

        // 패딩 제거
        base64 = base64.Replace("=", "");

        return base64;
    }

    #endregion
}

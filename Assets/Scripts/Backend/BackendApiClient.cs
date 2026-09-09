using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class MatchTokenResponse
{
    public string match_token;
    public string expires_at;
}

[Serializable]
public class ScoreSubmissionPayload
{
    public string match_token;
    public int score;
    public float client_duration;
    public string checksum;
}

public class BackendApiClient : Singleton<BackendApiClient>
{
    [SerializeField] private string baseApiUrl = "https://api.yourgame.com/api";
    [SerializeField] private string clientGameSalt = "YOUR_SECRET_CLIENT_CHECKSUM_KEY";

    private string _bearerToken;

    public void SetAuthToken(string token)
    {
        _bearerToken = token;
    }

    public IEnumerator RequestMatchToken(int? tournamentId, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{baseApiUrl}/match/token";
        string jsonPayload = tournamentId.HasValue ? $"{{\"tournament_id\":{tournamentId.Value}}}" : "{}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {_bearerToken}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<MatchTokenResponse>(req.downloadHandler.text);
                onSuccess?.Invoke(response.match_token);
            }
            else
            {
                onError?.Invoke(req.error);
            }
        }
    }

    public IEnumerator SubmitScore(string matchToken, int score, float durationSeconds, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{baseApiUrl}/match/submit";
        string rawSignature = matchToken + score + clientGameSalt;
        string checksum = ComputeSha256(rawSignature);

        var payload = new ScoreSubmissionPayload
        {
            match_token = matchToken,
            score = score,
            client_duration = durationSeconds,
            checksum = checksum
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {_bearerToken}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(req.error);
            }
        }
    }

    private string ComputeSha256(string rawData)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}

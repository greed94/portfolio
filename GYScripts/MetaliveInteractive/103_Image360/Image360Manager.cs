using System.Collections;
using Cysharp.Threading.Tasks;
using Metalive;
using UnityEngine;
using UnityEngine.Networking;

public class Image360Manager : MonoBehaviour
{
    private string imageUrl;
    private Texture2D texture;
    public Material material;

    void Start()
    {
        imageUrl = Setting.Video.url;
        // 이미지를 다운로드하기 위한 UnityWebRequest 객체를 생성합니다.
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);

        // 이미지 다운로드를 시작합니다.
        GetTexture(www).Forget();
    }

    private async UniTask GetTexture(UnityWebRequest www)
    {
        try
        {
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Texture 변수에 이미지를 저장합니다.
                texture = DownloadHandlerTexture.GetContent(www);

                // Material에 Texture를 할당합니다.
                material.mainTexture = texture;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void OnDisable()
    {
        material.mainTexture = null;
    }
}
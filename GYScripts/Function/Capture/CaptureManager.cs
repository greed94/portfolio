using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Metalive;
using UnityEngine;

public class CaptureManager : MonoBehaviour, IPopupCallback
{
    [SerializeField]
    private Canvas defalutCanvas;
    [SerializeField]
    private Canvas joystickCanvas;
    [SerializeField]
    private Canvas chatCanvas;
    [SerializeField]
    private Canvas emotionCanvas;
    private bool chatCanvasBool;
    private bool emotionCanvasBool;

    private void OnEnable()
    {
        Popup.AddCallback(this);
    }

    private int i = 0;

    public void ScreenShotBtn()
    {
        if (Setting.World.code == 4057 || Setting.World.code == 4066)
        {
            var hash = new PopupHash()
            {
                title = "스크린 캡처 기능을 지원하지 않습니다.",
                message = " ",
                confirm = "확인"
            };
            Popup.Message(PopupType.General, hash);
            return;
        }

        if (i == 0)
        {
            i++;
        }
        // TaskScreenShot().Forget();
        try
        {
            TakeScreenshotAndSave().Forget();
        }
        catch (System.Exception)
        {
            var hash = new PopupHash
            {
                title = "",
                message = "스크린 캡처를 실패하였습니다.",
                confirm = "",
                yes = "",
                no = "",
            };

            Popup.Message(PopupType.Floating, PopupStatusType.Caution, hash);
        }
    }

    private async UniTask TakeScreenshotAndSave()
    {
        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);

        // 권한 미선택 or 미승인
        if (permission == NativeGallery.Permission.ShouldAsk || permission == NativeGallery.Permission.Denied)
        {
            var hash = new PopupHash()
            {
                title = "권한 설정 필요",
                message = "스크린 캡처 사용을 위해\n‘사진 접근 권한’을 모든 사진으로 허용해야 합니다.",
                yes = "설정",
                no = "취소"
            };

            Popup.Message(PopupType.Choice, PopupCallbackType.Callback, "Capture", hash);
            return;
        }

        // 권한 승인
        chatCanvasBool = false;
        emotionCanvasBool = false;
        defalutCanvas.enabled = false;
        joystickCanvas.enabled = false;

        if (chatCanvas)
        {
            chatCanvas.enabled = false;
            chatCanvasBool = true;
        }

        if (emotionCanvas)
        {
            if (emotionCanvas.enabled)
            {
                emotionCanvas.enabled = false;
                emotionCanvasBool = true;
            }
            else
                emotionCanvasBool = false;
        }

        await UniTask.WaitForEndOfFrame(this);
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        var fileName = "Screenshot_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "_metalive.png";

        // Save the screenshot to Gallery/Photos
        NativeGallery.SaveImageToGallery(ss, "metalive", fileName, (success, path) => Debug.Log("Media save result: " + success + " " + path));

        if (chatCanvasBool)
            chatCanvas.enabled = true;

        if (emotionCanvasBool)
            emotionCanvas.enabled = true;

        defalutCanvas.enabled = true;
        joystickCanvas.enabled = true;

        var popup = new PopupHash
        {
            title = "",
            message = "스크린 캡처가 완료되었습니다.",
            confirm = "",
            yes = "",
            no = "",
        };

        Popup.Message(PopupType.Floating, PopupStatusType.Caution, popup);

        // To avoid memory leaks
        Destroy(ss);
    }

    public void OnPopupUpdate(PopupRedirect redirect)
    {
        if (redirect.code.Equals("Capture"))
        {
            if (redirect.type == PopupRedirectType.Yes)
            {
                if (NativeGallery.CanOpenSettings())
                {
                    NativeGallery.OpenSettings();
                }
            }
            else
            {
                //Debug.Log("닫기");
            }
        }
    }

    private void OnDisable()
    {
        Popup.RemoveCallback(this);
    }
}

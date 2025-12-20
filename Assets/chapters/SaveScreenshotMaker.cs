using System.Collections;
using UnityEngine;

public class SaveScreenshotMaker : MonoBehaviour
{
    public CanvasGroup pauseRoot;

    public IEnumerator TakeScreenshot(System.Action<Texture2D> callback)
    {
        pauseRoot.alpha = 0f;
        pauseRoot.blocksRaycasts = false;

        yield return new WaitForEndOfFrame();

        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

        pauseRoot.alpha = 1f;
        pauseRoot.blocksRaycasts = true;

        callback?.Invoke(tex);
    }
}

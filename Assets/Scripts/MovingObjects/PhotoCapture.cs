using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCapture : MonoBehaviour
{
    private Texture2D screenCapture;
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private DetectiveCamera detectiveCamera;
    [SerializeField] private PlayerCamera playerCamera;

    private void Start()
    {
        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && CameraManager.Instance.CurrentCameraMode == CameraMode.Detective)
        {
            StartCoroutine(CapturePhoto());
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            HidePhoto();
        }
    }

    IEnumerator CapturePhoto()
    {
        playerCamera.HighlightVisibleObjects();
        yield return new WaitForEndOfFrame();

        Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);
        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply();
        ShowPhoto();
        detectiveCamera.ChangeCamera();
    }

    void ShowPhoto()
    {
        photoDisplayArea.gameObject.SetActive(true);
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0, 0, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        photoDisplayArea.sprite = photoSprite;

    }


    void HidePhoto()
    {
        photoDisplayArea.gameObject.SetActive(false);

    }
}
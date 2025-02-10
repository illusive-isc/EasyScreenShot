using System.IO;
using UnityEngine;

namespace jp.illusive_isc
{
    public class IllEasyScreenShot : MonoBehaviour
    {
        public int width = 1920;
        public int height = 1080;

        private int tmpWidth;
        private int tmpHeight;

        [SerializeField]
        public string folderPath = "Screenshots";

        [SerializeField, ColorUsage(true, false)]
        public Color background;
        [SerializeField]
        public string fileString = "screenshot";
        public Texture2D previewTexture;  // プレビュー用のテクスチャ


        [SerializeField]
        public int resolutionsIndex = 3;
        [SerializeField]
        public int aspectRatioIndex = 4;
        [SerializeField]
        public int aspectIndex = 0;
        [SerializeField]
        public int backgroundIndex = 0;
        [SerializeField]
        public int mode = 0;
        [SerializeField]
        public int cameraIndex = 0;
        [SerializeField]
        public int projectionIndex = 0;
        [SerializeField]
        public bool doImage = false;
        [SerializeField]
        public float size = 1.0f;
        [SerializeField]
        public int FoV = 30;

        private RenderTexture rt;

        [ContextMenu("Save Camera Image")]
        public void GetImage()
        {
            if (doImage) return;
            doImage = true;

            try
            {
                // RenderTextureのサイズが変更された場合に作り直す
                if (rt == null || rt.width != width || rt.height != height)
                {
                    if (rt != null)
                    {
                        rt.Release();
                        Destroy(rt);
                    }

                    rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                }

                switch (backgroundIndex)
                {
                    case 0:
                        Camera.allCameras[cameraIndex].clearFlags = CameraClearFlags.Skybox;
                        break;
                    case 1:
                        Camera.allCameras[cameraIndex].clearFlags = CameraClearFlags.Color;
                        Camera.allCameras[cameraIndex].backgroundColor = background;
                        break;
                    case 2:
                        Camera.allCameras[cameraIndex].clearFlags = CameraClearFlags.SolidColor;
                        Camera.allCameras[cameraIndex].backgroundColor = Color.clear;
                        break;
                    default:
                        break;
                }
                Camera.allCameras[cameraIndex].orthographic = projectionIndex == 0;
                Camera.allCameras[cameraIndex].orthographicSize = 0.5f / size;
                Camera.allCameras[cameraIndex].fieldOfView = FoV;


                Camera.allCameras[cameraIndex].targetTexture = rt;

                Camera.allCameras[cameraIndex].Render();

                if (previewTexture == null || previewTexture.width != width || previewTexture.height != height)
                {
                    previewTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
                }

                RenderTexture.active = rt;
                previewTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                previewTexture.Apply();

                Camera.allCameras[cameraIndex].targetTexture = null;
                RenderTexture.active = null;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Screenshot capture failed: " + e.Message);
            }
            finally
            {
                doImage = false;
            }
        }

        public void cameraMoveDef()
        {
            var camera = Camera.allCameras[cameraIndex].transform;
            camera.position = new(0, 0.8f, 1f);
            camera.rotation = Quaternion.Euler(new(0, 180, 0));
        }
        // 画像をファイルとして保存
        public void SaveToFile()
        {
            if (previewTexture != null)
            {
                byte[] bytes = previewTexture.EncodeToPNG();
                string filePath = Path.Combine(folderPath, fileString + ".png");

                try
                {
                    // フォルダが存在しない場合は作成
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    File.WriteAllBytes(filePath, bytes);
                    Debug.Log("Saved Camera Screenshot: " + filePath);

                    // エクスプローラーで選択状態にする
                    string absoluteFilePath = Path.GetFullPath(filePath);
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + absoluteFilePath + "\"");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to save screenshot: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("No screenshot to save.");
            }
        }

        void Start()
        {
            tmpWidth = width;
            tmpHeight = height;
            InvokeRepeating(nameof(RepeatedFunction), 2f, 3f); // 2秒後に開始し、3秒ごとに実行
        }

        void RepeatedFunction()
        {
            if (tmpWidth != width || tmpHeight != height)
            {
                tmpWidth = width;
                tmpHeight = height;
                GetImage();
            }
        }

        private void OnDestroy()
        {
            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
            }
        }
    }
}

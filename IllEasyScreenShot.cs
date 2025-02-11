using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
namespace jp.illusive_isc
{
    public class IllEasyScreenShot : MonoBehaviour
    {
        public int width = 1920;
        public int height = 1080;

        private int tmpWidth;
        private int tmpHeight;

        public string folderPath = "Assets//Screenshots";

        [SerializeField, ColorUsage(true, false)]
        public Color background;
        public string fileString = "screenshot";
        public Texture2D previewTexture;  // プレビュー用のテクスチャ


        public int resolutionsIndex = 3;
        public int aspectRatioIndex = 4;
        public int aspectIndex = 0;
        public int backgroundIndex = 0;
        public int mode = 0;
        public int cameraIndex = 0;
        public int projectionIndex = 0;
        public bool doImage = false;
        public float size = 1.0f;
        public int FoV = 30;


        [ContextMenu("Save Camera Image")]
        public void GetImage()
        {

            if (doImage) return;
            doImage = true;

            try
            {
                RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                Camera copiedCamera = Instantiate(Camera.allCameras[cameraIndex]);


                switch (backgroundIndex)
                {
                    case 0:
                        copiedCamera.clearFlags = CameraClearFlags.Skybox;
                        break;
                    case 1:
                        copiedCamera.clearFlags = CameraClearFlags.Color;
                        copiedCamera.backgroundColor = background;
                        break;
                    case 2:
                        copiedCamera.clearFlags = CameraClearFlags.SolidColor;
                        copiedCamera.backgroundColor = Color.clear;
                        break;
                    default:
                        break;
                }
                copiedCamera.orthographic = projectionIndex == 0;
                copiedCamera.orthographicSize = 0.5f / size;
                copiedCamera.fieldOfView = FoV;


                copiedCamera.targetTexture = rt;

                copiedCamera.Render();

                if (previewTexture == null || previewTexture.width != width || previewTexture.height != height)
                {
                    previewTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
                }

                RenderTexture.active = rt;
                previewTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                previewTexture.Apply();


                if (Application.isPlaying)
                {
                    RenderTexture.active = null;
                    rt.Release();
                    Destroy(copiedCamera.gameObject);
                }
                else
                {
                    RenderTexture.active = null;
                    rt.Release();
                    DestroyImmediate(copiedCamera.gameObject);
                }

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

        public void CameraMoveDef()
        {
            Camera.allCameras[cameraIndex].transform.SetPositionAndRotation(new(0, 1f, 1f), Quaternion.Euler(new(0, 180, 0)));
        }
        // 画像をファイルとして保存
        public void SaveToFile()
        {
            if (previewTexture != null)
            {
                byte[] bytes = previewTexture.EncodeToPNG();


                string aaa = ExtractTextInBrackets(fileString.Replace("<プロジェクト名>", GetProjectName()));
                string filePath = Path.Combine(folderPath, aaa + ".png");

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
        string GetProjectName()
        {
            string projectPath = Application.dataPath;
            string projectRoot = projectPath.Substring(0, projectPath.LastIndexOf('/'));
            string projectName = projectRoot.Substring(projectRoot.LastIndexOf('/') + 1);

            return projectName;
        }
        string ExtractTextInBrackets(string input)
        {
            // 正規表現で < と > の間の文字を取り出す
            Match match = Regex.Match(input, @"(.*)<([^>]+)>(.*)");

            if (match.Success)
            {
                DateTime now = DateTime.Now;
                string extractedText = match.Groups[2].Value;  // < > の中身を返す
                extractedText = extractedText.Replace("月", now.ToString("MM"));
                extractedText = extractedText.Replace("日", now.ToString("dd"));
                extractedText = extractedText.Replace("時", now.ToString("HH"));
                extractedText = extractedText.Replace("分", now.ToString("mm"));
                extractedText = extractedText.Replace("秒", now.ToString("ss"));
                return match.Groups[1].Value + extractedText + match.Groups[3].Value;
            }

            return input;  // < > が見つからない場合は null を返す
        }
    }
}

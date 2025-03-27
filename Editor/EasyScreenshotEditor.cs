using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.illusive_isc
{
    [CustomEditor(typeof(IllEasyScreenShot))]
    public class EasyScreenShotEditor : Editor
    {
        [MenuItem("GameObject/illusive_tools/Attach IllEasyScreenShot", false, 10)]
        private static void AttachIllEasyScreenShot()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject selectedObject = Selection.activeGameObject;

                // 既にアタッチされていないかチェック
                if (selectedObject.GetComponent<IllEasyScreenShot>() == null)
                {
                    selectedObject.AddComponent<IllEasyScreenShot>();
                    Debug.Log(
                        "IllEasyScreenShot コンポーネントが "
                            + selectedObject.name
                            + " に追加されました。"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "IllEasyScreenShot はすでに "
                            + selectedObject.name
                            + " にアタッチされています。"
                    );
                }
            }
            else
            {
                Debug.LogWarning("オブジェクトが選択されていません。");
            }
        }

        [MenuItem("GameObject/Illusive ISC/Attach IllEasyScreenShot", true)]
        private static bool ValidateAttachIllEasyScreenShot()
        {
            return Selection.activeGameObject != null;
        }

        private readonly int[] resolutions =
        {
            1280, // HD
            1920, // フルHD
            2048, // 2K
            3840, // 4K
            7680, // 8K
        };
        private readonly string[] resolutionsString =
        {
            "HD", // HD
            "フルHD", // フルHD
            "2K", // 2K
            "4K", // 4K
            "8K", // 8K
        };
        private readonly float[] aspect = { 1, 5f / 4f, 4f / 3f, 3f / 2f, 16f / 9f, 21f / 9f };
        private readonly string[] aspectRatioString =
        {
            " 1 : 1",
            " 5 : 4",
            " 4 : 3",
            " 3 : 2",
            "16 : 9",
            "21 : 9",
        };
        private readonly string[] modeString = { "プリセット", "手動設定" };
        private readonly string[] aspectString = { "横", "縦" };
        private readonly string[] backgroundMode =
        {
            "スカイボックス",
            "背景色",
            "透過",
            "背景画像",
        };
        private readonly string[] projectionString = { "正射", "透視" };

        private (int width, int height) GetResolution(bool orientation)
        {
            return orientation
                ? (
                    (int)
                        Math.Round(
                            resolutions[EasyScreenShot.resolutionsIndex]
                                / aspect[EasyScreenShot.aspectRatioIndex]
                        ),
                    resolutions[EasyScreenShot.resolutionsIndex]
                )
                : (
                    resolutions[EasyScreenShot.resolutionsIndex],
                    (int)
                        Math.Round(
                            resolutions[EasyScreenShot.resolutionsIndex]
                                / aspect[EasyScreenShot.aspectRatioIndex]
                        )
                );
        }

        private IllEasyScreenShot EasyScreenShot;

        private SerializedProperty background;
        private SerializedProperty bgTexture;
        private SerializedProperty useLensBlur;
        private SerializedProperty size;
        private SerializedProperty MaxBlur;
        private SerializedProperty BlurSize;
        private SerializedProperty FoV;
        private SerializedProperty FoVtoggle;
        private SerializedProperty ScreenshotMode;

        private double lastTime;

        private void OnEnable()
        {
            background = serializedObject.FindProperty("background");
            bgTexture = serializedObject.FindProperty("bgTexture");
            useLensBlur = serializedObject.FindProperty("useLensBlur");
            size = serializedObject.FindProperty("size");
            MaxBlur = serializedObject.FindProperty("MaxBlur");
            BlurSize = serializedObject.FindProperty("BlurSize");
            FoV = serializedObject.FindProperty("FoV");
            FoVtoggle = serializedObject.FindProperty("FoVtoggle");
            ScreenshotMode = serializedObject.FindProperty("ScreenshotMode");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUIStyle labelStyle = new(GUI.skin.label) { fontStyle = FontStyle.BoldAndItalic };
            EasyScreenShot = (IllEasyScreenShot)target;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("カ メ ラ　", labelStyle);
            if (GUILayout.Button("キャラ正面へ"))
                EasyScreenShot.CameraMoveDef();
            GUILayout.FlexibleSpace();

            List<string> cameraNames = new();
            foreach (var camera in Camera.allCameras)
                cameraNames.Add(camera.name);

            var tmpCameraIndex = EditorGUILayout.Popup(
                EasyScreenShot.cameraIndex,
                cameraNames.ToArray(),
                GUILayout.Width(186)
            );
            if (EasyScreenShot.cameraIndex != tmpCameraIndex)
            {
                EasyScreenShot.cameraIndex = tmpCameraIndex;
                EasyScreenShot.GetImage();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("投 　影", labelStyle);
            GUILayout.FlexibleSpace();
            var tmpProjectionIndex = EditorGUILayout.Popup(
                EasyScreenShot.projectionIndex,
                projectionString,
                GUILayout.Width(186)
            );
            if (EasyScreenShot.projectionIndex != tmpProjectionIndex)
            {
                EasyScreenShot.projectionIndex = tmpProjectionIndex;
                EasyScreenShot.GetImage();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (EasyScreenShot.projectionIndex == 0)
            {
                GUILayout.Label("拡 大 率", labelStyle);
                GUILayout.FlexibleSpace();
                size.floatValue = EditorGUILayout.Slider(
                    size.floatValue,
                    0f,
                    10f,
                    GUILayout.Width(186)
                );
            }
            else
            {
                GUILayout.Label("F　o　V（同期モード", labelStyle);
                EditorGUILayout.PropertyField(FoVtoggle, new GUIContent(""), GUILayout.Width(10));
                GUILayout.Label("）", labelStyle);
                GUILayout.FlexibleSpace();
                if (FoVtoggle.boolValue)
                {
                    GUI.enabled = false;
                    FoV.floatValue = Camera.allCameras[EasyScreenShot.cameraIndex].fieldOfView;
                }
                FoV.floatValue = EditorGUILayout.Slider(
                    FoV.floatValue,
                    0f,
                    180f,
                    GUILayout.Width(186)
                );
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("背 　景", labelStyle);
            GUILayout.FlexibleSpace();

            var tmpBackgroundIndex = EditorGUILayout.Popup(
                EasyScreenShot.backgroundIndex,
                backgroundMode,
                GUILayout.Width(EasyScreenShot.backgroundIndex == 1 ? 82 : 186)
            );
            if (EasyScreenShot.backgroundIndex != tmpBackgroundIndex)
            {
                EasyScreenShot.backgroundIndex = tmpBackgroundIndex;
                EasyScreenShot.GetImage();
            }
            if (EasyScreenShot.backgroundIndex == 1)
            {
                EditorGUILayout.PropertyField(background, GUIContent.none, GUILayout.Width(101));
                EasyScreenShot.GetImage();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("モ ー ド", labelStyle);
            GUILayout.FlexibleSpace();
            EasyScreenShot.mode = EditorGUILayout.Popup(
                EasyScreenShot.mode,
                modeString,
                GUILayout.Width(186)
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("解像度　", labelStyle);
            GUILayout.FlexibleSpace();
            if (EasyScreenShot.mode == 0)
            {
                EasyScreenShot.resolutionsIndex = EditorGUILayout.Popup(
                    EasyScreenShot.resolutionsIndex,
                    resolutionsString,
                    GUILayout.Width(70)
                );
                EasyScreenShot.aspectRatioIndex = EditorGUILayout.Popup(
                    EasyScreenShot.aspectRatioIndex,
                    aspectRatioString,
                    GUILayout.Width(60)
                );
                EasyScreenShot.aspectIndex = EditorGUILayout.Popup(
                    EasyScreenShot.aspectIndex,
                    aspectString,
                    GUILayout.Width(50)
                );
                var (width, height) = GetResolution(EasyScreenShot.aspectIndex != 0);
                EasyScreenShot.width = width;
                EasyScreenShot.height = height;
            }
            else
            {
                GUILayout.Label("縦");
                EasyScreenShot.width = EditorGUILayout.IntField(EasyScreenShot.width);
                GUILayout.Label("横");
                EasyScreenShot.height = EditorGUILayout.IntField(EasyScreenShot.height);
            }

            EditorGUILayout.EndHorizontal();
            if (EasyScreenShot.backgroundIndex == 3)
            {
                serializedObject.Update();
                GUILayout.Label("背景画像", labelStyle);
                bgTexture.objectReferenceValue = EditorGUILayout.ObjectField(
                    "",
                    bgTexture.objectReferenceValue,
                    typeof(Texture2D),
                    false
                );
                GUILayout.Label("ブラーオプション", EditorStyles.boldLabel);

                useLensBlur.boolValue = EditorGUILayout.Toggle(
                    "レンズブラーを使用",
                    useLensBlur.boolValue
                );

                GUILayout.Label("レンズブラー", labelStyle);
                GUILayout.FlexibleSpace();
                if (useLensBlur.boolValue)
                    EditorGUILayout.PropertyField(MaxBlur, new GUIContent("強度"));
                EditorGUILayout.PropertyField(BlurSize, new GUIContent("拡散率"));
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("フォルダ 　", GUILayout.Width(60));
            EditorGUILayout.LabelField(EasyScreenShot.folderPath);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("選択", GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel(
                    "Select Folder",
                    EasyScreenShot.folderPath,
                    ""
                );
                if (!string.IsNullOrEmpty(path))
                    EasyScreenShot.folderPath = path;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ファイル名", GUILayout.Width(60));
            EasyScreenShot.fileString = EditorGUILayout.TextField(EasyScreenShot.fileString);
            EditorGUILayout.LabelField(".png", GUILayout.Width(45));
            EditorGUILayout.EndHorizontal();
            if (EasyScreenShot.previewTexture != null)
            {
                float width = EditorGUIUtility.currentViewWidth - 20;
                float height =
                    EasyScreenShot.previewTexture.height
                    * (width / EasyScreenShot.previewTexture.width);
                GUILayout.Box(
                    EasyScreenShot.previewTexture,
                    GUILayout.Width(width),
                    GUILayout.Height(height)
                );
            }
            GUI.enabled = !EasyScreenShot.doImage;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("リアルタイム変更", labelStyle, GUILayout.Width(85));
            EditorGUILayout.PropertyField(ScreenshotMode, new GUIContent(""), GUILayout.Width(15));
            serializedObject.ApplyModifiedProperties();
            if (ScreenshotMode.boolValue)
            {
                if (EditorApplication.timeSinceStartup - lastTime >= 0.5)
                {
                    EasyScreenShot.GetImage();
                    lastTime = EditorApplication.timeSinceStartup;
                }
                GUI.enabled = false;
            }
            if (GUILayout.Button("Screenshot"))
                EasyScreenShot.GetImage();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Saveしてフォルダ表示"))
                EasyScreenShot.SaveToFile(true);
            if (GUILayout.Button("Save"))
                EasyScreenShot.SaveToFile(false);
            EditorGUILayout.EndHorizontal();
        }
    }
}

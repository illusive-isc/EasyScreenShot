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

        private void OnEnable()
        {
            background = serializedObject.FindProperty("background");
            bgTexture = serializedObject.FindProperty("bgTexture");
            useLensBlur = serializedObject.FindProperty("useLensBlur");
            size = serializedObject.FindProperty("size");
            MaxBlur = serializedObject.FindProperty("MaxBlur");
            BlurSize = serializedObject.FindProperty("BlurSize");
            FoV = serializedObject.FindProperty("FoV");
        }

        public override void OnInspectorGUI()
        {
            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.BoldAndItalic;
            EasyScreenShot = (IllEasyScreenShot)target;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("カ メ ラ　", labelStyle);
            if (GUILayout.Button("キャラ正面へ"))
                EasyScreenShot.CameraMoveDef();
            GUILayout.FlexibleSpace();

            List<string> cameraNames = new();
            foreach (var camera in Camera.allCameras)
                cameraNames.Add(camera.name);
            EasyScreenShot.cameraIndex = EditorGUILayout.Popup(
                EasyScreenShot.cameraIndex,
                cameraNames.ToArray(),
                GUILayout.Width(186)
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("投 　影", labelStyle);
            GUILayout.FlexibleSpace();
            EasyScreenShot.projectionIndex = EditorGUILayout.Popup(
                EasyScreenShot.projectionIndex,
                projectionString,
                GUILayout.Width(186)
            );
            EditorGUILayout.EndHorizontal();

            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            if (EasyScreenShot.projectionIndex == 0)
            {
                GUILayout.Label("拡 大 率", labelStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(size, GUIContent.none, GUILayout.Width(186));
            }
            else
            {
                GUILayout.Label("F　o　V", labelStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(FoV, GUIContent.none, GUILayout.Width(186));
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("背 　景", labelStyle);
            GUILayout.FlexibleSpace();
            if (EasyScreenShot.backgroundIndex != 1)
            {
                EasyScreenShot.backgroundIndex = EditorGUILayout.Popup(
                    EasyScreenShot.backgroundIndex,
                    backgroundMode,
                    GUILayout.Width(186)
                );
            }
            else
            {
                EasyScreenShot.backgroundIndex = EditorGUILayout.Popup(
                    EasyScreenShot.backgroundIndex,
                    backgroundMode,
                    GUILayout.Width(82)
                );
                EditorGUILayout.PropertyField(background, GUIContent.none, GUILayout.Width(101));
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
                var resolution = GetResolution(EasyScreenShot.aspectIndex != 0);
                EasyScreenShot.width = resolution.width;
                EasyScreenShot.height = resolution.height;
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

                // bool型の選択ボックス（チェックボックス）
                useLensBlur.boolValue = EditorGUILayout.Toggle(
                    "レンズブラーを使用",
                    useLensBlur.boolValue
                );

                // チェックボックスの状態に基づいて表示を変更

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
            if (GUILayout.Button("Screenshot"))
                EasyScreenShot.GetImage();
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Saveしてフォルダ表示"))
                EasyScreenShot.SaveToFile(true);
            if (GUILayout.Button("Save"))
                EasyScreenShot.SaveToFile(false);
            EditorGUILayout.EndHorizontal();
        }
    }
}

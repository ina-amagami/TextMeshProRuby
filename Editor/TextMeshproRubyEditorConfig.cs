using UnityEngine;
using UnityEditor;
using System.IO;

namespace TMP_Ruby.Editor
{
    public class TextMeshProRubyEditorConfig : ScriptableObject
    {
        [Header("ルビタグ付与対象の正規表現")]
        public string KanjiRegex = @"[\u3040-\u309F\u30A0-\u30FFA-Za-z0-9ａ-ｚＡ-Ｚ０-９ｦ-ﾟ\s、。！？「」（）『』【】\r\n\t]";

        [Header("AIルビ振りを有効化")] public bool EnableRubyFromAI = true;

        [Header("OpenAI API Settings")] public string OpenAI_Model = "gpt-4o";
        public string OpenAI_APIKey;
        [Range(0, 1f)] public float OpenAI_Temperature = 0f;

        private const string AssetFileName = nameof(TextMeshProRubyEditorConfig) + ".asset";
        private const string AssetFolder = "Assets/Editor Default Resources";
        private const string AssetPath = AssetFolder + "/" + AssetFileName;

        private static TextMeshProRubyEditorConfig _instance;

        public static TextMeshProRubyEditorConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadOrCreateAsset();
                }

                return _instance;
            }
        }

        private static void LoadOrCreateAsset()
        {
            _instance = AssetDatabase.LoadAssetAtPath<TextMeshProRubyEditorConfig>(AssetPath);
            if (_instance != null)
                return;

            if (_instance != null)
            {
                return;
            }

            _instance = CreateInstance<TextMeshProRubyEditorConfig>();
            if (!Directory.Exists(AssetFolder))
            {
                Directory.CreateDirectory(AssetFolder);
            }

            AssetDatabase.CreateAsset(_instance,
                Path.Combine(AssetFolder, nameof(TextMeshProRubyEditorConfig) + ".asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created new TextMeshProRubyEditorConfig in Editor Default Resources.");
        }
    }
}
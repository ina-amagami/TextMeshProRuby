using UnityEditor;
using UnityEngine;

namespace TMP_Ruby.Editor
{
    static class TextMeshProRubySettingsProvider
    {
        private static SerializedObject _serializedObject;

        [SettingsProvider]
        public static SettingsProvider CreateTextMeshProRubySettingsProvider()
        {
            var provider = new SettingsProvider("Project/TMP Ruby Settings", SettingsScope.Project)
            {
                label = "TMP Ruby Settings",

                guiHandler = (searchContext) =>
                {
                    var config = TextMeshProRubyEditorConfig.Instance;

                    if (_serializedObject == null || _serializedObject.targetObject != config)
                    {
                        _serializedObject = new SerializedObject(config);
                    }

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField("Config Asset", config, typeof(TextMeshProRubyEditorConfig), false);
                    EditorGUI.EndDisabledGroup();

                    _serializedObject.Update();

                    EditorGUILayout.PropertyField(_serializedObject.FindProperty("KanjiRegex"));

                    EditorGUILayout.Separator();
                    EditorGUILayout.HelpBox("AI機能はエディタ専用です。ランタイムには含まれません", MessageType.Info);

                    var enableAIProp = _serializedObject.FindProperty("EnableRubyFromAI");
                    EditorGUILayout.PropertyField(enableAIProp);

                    EditorGUI.BeginDisabledGroup(!enableAIProp.boolValue);

                    var modelProp = _serializedObject.FindProperty("OpenAI_Model");
                    EditorGUILayout.PropertyField(modelProp);

                    var apiKeyProp = _serializedObject.FindProperty("OpenAI_APIKey");
                    EditorGUILayout.PropertyField(apiKeyProp);

                    EditorGUILayout.PropertyField(_serializedObject.FindProperty("OpenAI_Temperature"));

                    EditorGUI.EndDisabledGroup();

                    if (enableAIProp.boolValue)
                    {
                        if (string.IsNullOrEmpty(modelProp.stringValue))
                        {
                            EditorGUILayout.HelpBox("Modelが入力されていません", MessageType.Error);
                        }

                        if (string.IsNullOrEmpty(apiKeyProp.stringValue))
                        {
                            EditorGUILayout.HelpBox("APIキーが入力されていません", MessageType.Error);
                        }
                    }

                    _serializedObject.ApplyModifiedProperties();
                },

                keywords = new[] {"OpenAI", "TMP", "Ruby", "API", "Key", "Kanji"}
            };

            return provider;
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace TMP_Ruby.Editor
{
    [CustomEditor(typeof(TextMeshProRuby))]
    public class TextMeshProRubyEditor : UnityEditor.Editor
    {
        TextMeshProRuby tmProRuby;
        SerializedObject so;
        SerializedProperty fixedLineHeightProp;
        SerializedProperty autoMarginTopProp;

        private void OnEnable()
        {
            tmProRuby = target as TextMeshProRuby;
            so = new SerializedObject(target);
            fixedLineHeightProp = so.FindProperty("fixedLineHeight");
            autoMarginTopProp = so.FindProperty("autoMarginTop");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            so.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(fixedLineHeightProp);

            if (GUILayout.Button("漢字に<r>タグ付与"))
            {
                tmProRuby.Text = Utility.AddRubyTagToKanji(tmProRuby.Text);
                GUI.FocusControl(null);
                EditorUtility.SetDirty(tmProRuby);
            }

            if (TextMeshProRubyEditorConfig.Instance.EnableRubyFromAI && GUILayout.Button("AIルビ振り"))
            {
                tmProRuby.Text = Utility.CreateRubyTagByOpenAI(tmProRuby.Text);
                GUI.FocusControl(null);
                EditorUtility.SetDirty(tmProRuby);
            }

            if (fixedLineHeightProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(autoMarginTopProp);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterFullObjectHierarchyUndo(tmProRuby.gameObject, "TextMeshProRuby");

                so.ApplyModifiedProperties();
                if (tmProRuby.enabled)
                {
                    tmProRuby.Apply();
                }
            }
        }
    }
}
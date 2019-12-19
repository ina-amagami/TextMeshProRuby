/*
TextMeshProRuby

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(TMP_Text))]
public class TextMeshProRuby : MonoBehaviour
{
	[SerializeField, HideInInspector] private TMP_Text tmpText;

	[TextArea(5, 10)]
	[Tooltip("ルビは <r=もじ>文字</r> もしくは <r=\"もじ\">文字</r>")]
	[SerializeField] private string text;

	/// <summary>
	/// ルビタグを含んだテキスト
	/// </summary>
	public string Text
	{
		get => text;
		set
		{
			text = value;
			if (enabled)
			{
				Apply();
			}
		}
	}

	[Tooltip("行間を固定します")]
	[SerializeField, HideInInspector] private bool fixedLineHeight;

	/// <summary>
	/// 行間を固定する
	/// </summary>
	public bool FixedLineHeight
	{
		get => fixedLineHeight;
		set
		{
			bool isChanged = fixedLineHeight != value;
			fixedLineHeight = value;
			if (isChanged && enabled)
			{
				Apply();
			}
		}
	}

	[Tooltip("1行目のルビ有無によって自動でMarginTopを追加します")]
	[SerializeField, HideInInspector] private bool autoMarginTop = true;

	/// <summary>
	/// 1行目のルビ有無によって自動でMarginTopを追加する
	/// </summary>
	public bool AutoMarginTop
	{
		get => autoMarginTop;
		set
		{
			bool isChanged = autoMarginTop != value;
			autoMarginTop = value;
			if (isChanged && enabled)
			{
				Apply();
			}
		}
	}

	private void OnEnable()
	{
		Apply();
	}

	public void Apply()
	{
		if (!tmpText)
		{
			tmpText = GetComponent<TMP_Text>();
		}
		tmpText.SetTextAndExpandRuby(Text, fixedLineHeight, autoMarginTop);
	}

#if UNITY_EDITOR
	private void Reset()
	{
		tmpText = GetComponent<TMP_Text>();
		Text = tmpText.text;
	}

	private void OnValidate()
	{
		// Copy & PasteComponent対応
		var newTMPText = GetComponent<TMP_Text>();
		if (tmpText != newTMPText)
		{
			tmpText = newTMPText;
			Text = tmpText.text;
			return;
		}

		if (enabled)
		{
			Apply();
		}
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextMeshProRuby))]
public class TextMeshProRubyEditor : Editor
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
#endif

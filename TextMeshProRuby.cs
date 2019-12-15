/*
TextMeshProRuby

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextMeshProRuby : MonoBehaviour
{
	[TextArea(5, 10)]
	[SerializeField] private string text;
	[SerializeField, HideInInspector] private TMP_Text tmpText;

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
		tmpText.SetTextAndExpandRuby(Text);
	}

#if UNITY_EDITOR
	private void Reset()
	{
		if (!tmpText)
		{
			tmpText = GetComponent<TMP_Text>();
			Text = tmpText.text;
		}
	}

	private void OnValidate()
	{
		if (enabled)
		{
			Apply();
		}
	}
#endif
}

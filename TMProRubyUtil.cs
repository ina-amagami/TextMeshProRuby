/*
TextMeshProRuby

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/
using System.Text;
using System.Text.RegularExpressions;
using TMPro;

public static class TMProRubyUtil
{
	/// <summary>
	/// <ruby=もじ>文字</ruby> もしくは <ruby="もじ">文字</ruby>
	/// </summary>
	private static readonly Regex TagRegex = new Regex("<r=\"?(?<ruby>.*?)\"?>(?<kanji>.*?)</r>", RegexOptions.IgnoreCase);

	/// <summary>
	/// 展開後の開始タグ
	/// </summary>
	private const string StartTag = "<voffset=1em><size=50%>";

	/// <summary>
	/// 展開後の終了タグ
	/// </summary>
	private const string EndTag = "</size></voffset>";

	/// <summary>
	/// GCAlloc対策
	/// </summary>
	private static readonly StringBuilder builder = new StringBuilder(StringBuilderCapacity);
	private const int StringBuilderCapacity = 1024;

	/// <summary>
	/// ルビタグを展開してセット
	/// </summary>
	public static void SetTextAndExpandRuby(this TMP_Text tmpText, string text)
	{
		tmpText.text = GetExpandText(text);
	}

	/// <summary>
	/// 文字列に含まれるルビタグを展開して取得
	/// </summary>
	/// <returns>ルビタグ展開後の文字列</returns>
	public static string GetExpandText(string text)
	{
		var match = TagRegex.Match(text);
		while (match.Success)
		{
			if (match.Groups.Count > 2)
			{
				builder.Length = 0;

				string ruby = match.Groups["ruby"].Value;
				int rL = ruby.Length;
				string kanji = match.Groups["kanji"].Value;
				int kL2 = kanji.Length * 2;

				// 手前に付ける空白
				float space = kL2 < rL ? (rL - kL2) * 0.25f : 0f;
				if (space < 0 || space > 0)
				{
					builder.Append($"<space={space.ToString("F2")}em>");
				}

				// 漢字 - 文字数分だけ左に移動 - 開始タグ - ルビ - 終了タグ
				space = -(kL2 * 0.25f + rL * 0.25f);
				builder.Append($"{kanji}<space={space.ToString("F2")}em>{StartTag}{ruby}{EndTag}");

				// 後ろに付ける空白
				space = kL2 > rL ? (kL2 - rL) * 0.25f : 0f;
				if (space < 0 || space > 0)
				{
					builder.Append($"<space={space.ToString("F2")}em>");
				}

				text = text.Replace(match.Groups[0].Value, builder.ToString());
			}
			match = match.NextMatch();
		}
		return text;
	}
}

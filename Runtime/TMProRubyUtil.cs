/*
TextMeshProRuby

Copyright (c) 2019-2025 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using System.Text;
using System.Text.RegularExpressions;
using TMPro;

namespace TMP_Ruby
{
    public static class TMProRubyUtil
    {
        private static readonly Regex TagRegex =
            new Regex("<r=\"?(?<ruby>.*?)\"?>(?<kanji>.*?)</r>", RegexOptions.IgnoreCase);

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
        public static void SetTextAndExpandRuby(
            this TMP_Text tmpText,
            string text,
            bool fixedLineHeight = false,
            bool autoMarginTop = true)
        {
            // 1行目にルビがあるか調べる
            var isFirstLineRuby = false;
            if (fixedLineHeight && autoMarginTop)
            {
                var firstNewLineIndex = text.IndexOf('\n');
                var firstLine = firstNewLineIndex > 1 ? text.Substring(0, firstNewLineIndex + 1) : text;
                isFirstLineRuby = TagRegex.IsMatch(firstLine);
            }

            text = GetExpandText(text);

            if (fixedLineHeight)
            {
                // 行間を固定
                var lineHeight = tmpText.font.faceInfo.lineHeight / tmpText.font.faceInfo.pointSize;
                text = $"<line-height={lineHeight:F3}em>{text}";

                // 1行目にルビがある時はMarginTopで位置調整
                if (autoMarginTop)
                {
                    var margin = tmpText.margin;
                    margin.y = isFirstLineRuby ? -(tmpText.fontSize * 0.55f) : 0;
                    margin.y *= tmpText.isOrthographic ? 1 : 0.1f;
                    tmpText.margin = margin;
                }
            }

            tmpText.text = text;
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

                    var ruby = match.Groups["ruby"].Value;
                    var rL = ruby.Length;
                    var kanji = match.Groups["kanji"].Value;
                    var kL2 = kanji.Length * 2;

                    // 手前に付ける空白
                    var space = kL2 < rL ? (rL - kL2) * 0.25f : 0f;
                    if (space < 0 || space > 0)
                    {
                        builder.Append($"<space={space:F2}em>");
                    }

                    // 漢字 - 文字数分だけ左に移動 - 開始タグ - ルビ - 終了タグ
                    space = -(kL2 * 0.25f + rL * 0.25f);
                    builder.Append($"{kanji}<space={space:F2}em>{StartTag}{ruby}{EndTag}");

                    // 後ろに付ける空白
                    space = kL2 > rL ? (kL2 - rL) * 0.25f : 0f;
                    if (space < 0 || space > 0)
                    {
                        builder.Append($"<space={space:F2}em>");
                    }

                    text = text.Replace(match.Groups[0].Value, builder.ToString());
                }

                match = match.NextMatch();
            }

            return text;
        }

        /// <summary>
        /// TextMeshPro以外などの表示用にルビタグを除外したテキストを取得
        /// </summary>
        /// <returns>ルビタグ削除の文字列</returns>
        public static string RemoveRubyTag(string text)
        {
            var match = TagRegex.Match(text);
            while (match.Success)
            {
                if (match.Groups.Count > 2)
                {
                    text = text.Replace(match.Groups[0].Value, match.Groups["kanji"].Value);
                }

                match = match.NextMatch();
            }

            return text;
        }
    }
}
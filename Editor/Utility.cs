using UnityEngine;
using System.IO;
using System.Net.Http;
using System.Text;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TMP_Ruby.Editor
{
    public static class Utility
    {
        /// <summary>
        /// 文章の漢字部分にrタグだけ付与する（ひらがなは付与しない）
        /// </summary>
        public static string AddRubyTagToKanji(string srcText)
        {
            var tagRegex = new Regex(@"<r=[^>]*?>.*?<\/r>", RegexOptions.Singleline);
            var matches = tagRegex.Matches(srcText);

            var protectedRanges = new List<(int start, int end)>();
            foreach (Match match in matches)
            {
                protectedRanges.Add((match.Index, match.Index + match.Length));
            }

            var result = new StringBuilder();
            for (var i = 0; i < srcText.Length;)
            {
                // 現在の位置が保護範囲か？
                var inProtected = protectedRanges.Exists(r => i >= r.start && i < r.end);
                if (inProtected)
                {
                    // 保護範囲にある → その範囲はそのまま追加
                    var range = protectedRanges.Find(r => i >= r.start && i < r.end);
                    result.Append(srcText.Substring(range.start, range.end - range.start));
                    i = range.end;
                }
                else
                {
                    // 保護範囲外 → ルビ対象文字列を探す
                    var match = MatchRubyTarget(srcText, i);
                    if (match.length > 0)
                    {
                        result.Append($"<r=>{match.value}</r>");
                        i += match.length;
                    }
                    else
                    {
                        result.Append(srcText[i]);
                        i++;
                    }
                }
            }

            return result.ToString();
        }

        // ルビを付ける対象文字列のマッチ処理
        private static (string value, int length) MatchRubyTarget(string text, int start)
        {
            var i = start;
            while (i < text.Length)
            {
                if (Regex.IsMatch(text[i].ToString(),
                        TextMeshProRubyEditorConfig.Instance.KanjiRegex))
                {
                    break;
                }

                i++;
            }

            return i > start ? (text.Substring(start, i - start), i - start) : ("", 0);
        }

        /// <summary>
        /// OpenAIのAPIで、rタグに対してひらがなを付与する
        /// </summary>
        public static string CreateRubyTagByOpenAI(string srcText)
        {
            var config = TextMeshProRubyEditorConfig.Instance;
            var model = config.OpenAI_Model;
            var apiKey = config.OpenAI_APIKey;
            var temperature = config.OpenAI_Temperature;

            var promptPath = Path.Combine(GetScriptDirectory(), "Prompt.txt");
            if (!File.Exists(promptPath))
            {
                Debug.LogError("Prompt.txt が見つかりません: " + promptPath);
                return srcText;
            }

            var prompt = File.ReadAllText(promptPath, Encoding.UTF8)
                .Replace("{srcText}", AddRubyTagToKanji(srcText));

            try
            {
                var result = PostToOpenAISync(prompt, model, apiKey, temperature);
                return string.IsNullOrEmpty(result) ? srcText : result;
            }
            catch (Exception ex)
            {
                Debug.LogError("OpenAI API 呼び出し失敗: " + ex.Message);
                return srcText;
            }
        }

        private static string GetScriptDirectory(
            [System.Runtime.CompilerServices.CallerFilePath]
            string filePath = null)
        {
            return Path.GetDirectoryName(filePath);
        }

        private static string PostToOpenAISync(string prompt, string model, string apiKey, float temperature)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new ChatCompletionRequest
            {
                model = model,
                messages = new[]
                {
                    new Message {role = "user", content = prompt}
                },
                temperature = temperature
            };

            var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");

            var response = client.PostAsync("https://api.openai.com/v1/chat/completions", content).GetAwaiter()
                .GetResult();
            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API error: {response.StatusCode}\n{responseBody}");
            }

            var parsed = JsonUtility.FromJson<ChatCompletionResponse>(responseBody);
            return parsed?.choices?[0]?.message?.content ?? "";
        }

        [Serializable]
        internal class Message
        {
            public string role;
            public string content;
        }

        [Serializable]
        internal class ChatCompletionRequest
        {
            public string model;
            public Message[] messages;
            public float temperature;

            public override string ToString()
            {
                return JsonUtility.ToJson(this);
            }
        }

        [Serializable]
        internal class Choice
        {
            public Message message;
        }

        [Serializable]
        internal class ChatCompletionResponse
        {
            public Choice[] choices;
        }
    }
}
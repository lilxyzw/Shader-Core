using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace jp.lilxyzw.shadercore
{
    public static class HLSLMinifier
    {
        public static string Minify(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            // コメント削除
            source = RemoveComments(source);

            // 行単位処理
            var sb = new StringBuilder();
            using var sr = new StringReader(source);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;
                // 空白圧縮
                trimmed = Regex.Replace(trimmed, @"\s+", " ");
                // 演算子・記号周辺の空白削除
                trimmed = Regex.Replace(
                    trimmed,
                    @"\s*([{}()\[\];,+\-*/%=<>!&|^?:])\s*",
                    "$1");
                sb.AppendLine(trimmed);
            }

            string result = sb.ToString();

            return result.Trim();
        }

        private static string RemoveComments(string source)
        {
            var sb = new StringBuilder();

            bool inString = false;
            bool inLineComment = false;
            bool inBlockComment = false;

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';

                if (inLineComment)
                {
                    if (c == '\n')
                    {
                        inLineComment = false;
                        sb.Append(c);
                    }
                    continue;
                }

                if (inBlockComment)
                {
                    if (c == '*' && next == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }
                    continue;
                }

                if (!inString)
                {
                    if (c == '/' && next == '/')
                    {
                        inLineComment = true;
                        i++;
                        continue;
                    }

                    if (c == '/' && next == '*')
                    {
                        inBlockComment = true;
                        i++;
                        continue;
                    }
                }

                if (c == '"' && (i == 0 || source[i - 1] != '\\'))
                {
                    inString = !inString;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}

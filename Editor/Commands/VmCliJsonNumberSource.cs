using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VMUnityAutomation.Editor;

namespace VMUnityPipeline.Editor.Commands
{
    internal sealed class VmCliJsonNumberSource
    {
        private readonly string json;
        private int lineStart;
        private int lineNumber = 1;

        internal VmCliJsonNumberSource(string json) { this.json = json; }

        internal object Read(JToken token)
        {
            var location = (IJsonLineInfo)token;
            if (!location.HasLineInfo() || location.LineNumber < lineNumber)
                throw new JsonReaderException("JSON numeric token has invalid source coordinates.");
            while (lineNumber < location.LineNumber)
            {
                while (lineStart < json.Length && json[lineStart] != '\r' && json[lineStart] != '\n') lineStart++;
                if (lineStart == json.Length) throw new JsonReaderException("JSON numeric source line is absent.");
                char newline = json[lineStart++];
                if (newline == '\r' && lineStart < json.Length && json[lineStart] == '\n') lineStart++;
                lineNumber++;
            }
            int end = lineStart + location.LinePosition;
            if (end > json.Length) throw new JsonReaderException("JSON numeric source position is absent.");
            int start = end;
            while (start > lineStart && IsNumberCharacter(json[start - 1])) start--;
            string lexeme = json.Substring(start, end - start);
            return VmJsonNumber.Parse(lexeme);
        }

        private static bool IsNumberCharacter(char value) =>
            value >= '0' && value <= '9' || value == '-' || value == '+' ||
            value == '.' || value == 'e' || value == 'E';
    }
}

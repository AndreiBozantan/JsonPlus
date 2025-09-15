namespace JsonPlus;

using System.Text;

public partial class JsonCodec
{
    public static string Debug(JsonValue value)
    {
        var sb = new StringBuilder();
        DebugAppendValue(value, sb, 0);
        return sb.ToString();
    }

    private static void DebugAppendValue(JsonValue value, StringBuilder sb, int indent)
    {
        AppendLeadingTrivia(value.Kind.ToString(), value.LeadingTrivia, sb);
        switch (value.Kind)
        {
            case JsonValueKind.Null:
                sb.Append("null");
                break;
            case JsonValueKind.Boolean:
                sb.Append(value.GetBoolean() ? "true" : "false");
                break;
            case JsonValueKind.Number:
                sb.Append(value.GetRawValue());
                break;
            case JsonValueKind.String:
                sb.Append(value.GetRawValue());
                break;
            case JsonValueKind.Array:
                DebugAppendArray((JsonArray)value, sb, indent);
                break;
            case JsonValueKind.Object:
                DebugAppendObject((JsonObject)value, sb, indent);
                break;
        }
        AppendTrailingTrivia($"/{value.Kind}", value.TrailingTrivia, sb);
    }

    private static void AppendIndent(StringBuilder sb, int indent)
    {
        sb.Append(' ', indent * 2);
    }

    private static void AppendLeadingTrivia(string type, JsonTriviaCollection trivia, StringBuilder sb)
    {
        sb.Append("<<");
        foreach (var t in trivia)
        {
            sb.Append($"{t.Value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}");
        }
        sb.Append('|');
    }

    private static void AppendTrailingTrivia(string type, JsonTriviaCollection trivia, StringBuilder sb)
    {
        sb.Append('|');
        foreach (var t in trivia)
        {
            sb.Append($"{t.Value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}");
        }
        sb.Append(">>");
    }

    private static void DebugAppendArray(JsonArray array, StringBuilder sb, int indent)
    {
        sb.AppendLine();
        for (int i = 0; i < array.Count; i++)
        {
            AppendIndent(sb, indent + 1);
            DebugAppendValue(array[i], sb, indent + 1);
            sb.AppendLine();
        }
        AppendIndent(sb, indent);
    }

    private static void DebugAppendObject(JsonObject obj, StringBuilder sb, int indent)
    {
        sb.AppendLine();
        for (int i = 0; i < obj.Count; i++)
        {
            var prop = obj[i];
            AppendIndent(sb, indent + 1);
            AppendLeadingTrivia(">", prop.Key.LeadingTrivia, sb);
            sb.Append(prop.Key.RawValue);
            AppendTrailingTrivia("|", prop.Key.TrailingTrivia, sb);
            DebugAppendValue(prop.Value, sb, indent + 1);
            sb.AppendLine();
        }
        AppendIndent(sb, indent);
    }
}
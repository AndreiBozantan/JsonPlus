namespace JsonPlus;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public record struct JsonPosition(int Index = 0, int Line = 0, int Column = 0, int Depth = 0);

[Serializable]
public class JsonParsingException(string message, string token, JsonPosition position) : Exception(message)
{
    public JsonParsingException(string message, string token) : this(message, token, new JsonPosition()) { }

    public JsonPosition Position { get; } = position;

    public override string ToString() => $"{Message} (found token: `{token}` at line: {Position.Line}, column: {Position.Column}, index: {Position.Index})";
}

public record JsonDecodingOptions(bool AllowTrailingCommas, bool AllowSingleLineComments, bool AllowMultiLineComments, int MaxNestingDepth);

/// <summary>
/// A Concrete Syntax Tree JSON parser that produces a <see cref="JsonValue" />, including comments and whitespace as trivia.
/// </summary>
public partial class JsonCodec
{
    private static readonly ThreadLocal<JsonCodec> Instance = new(() => new JsonCodec());

    private readonly JsonReader Reader = new();

    private JsonTriviaCollection CurrentTrivia = [];

    private JsonDecodingOptions DecodingOptions = null!;

    public static readonly JsonDecodingOptions StrictDecodingOptions = new(AllowMultiLineComments: false, AllowSingleLineComments: false, AllowTrailingCommas: false, MaxNestingDepth: 100);

    public static readonly JsonDecodingOptions RelaxedDecodingOptions = new(AllowMultiLineComments: true, AllowSingleLineComments: true, AllowTrailingCommas: true, MaxNestingDepth: 1000);

    public static JsonValue Decode(string json)
    {
        var codec = Instance.Value ?? throw new InvalidOperationException("Failed to initialize JsonCodec instance");
        return codec.DecodeValue(json, RelaxedDecodingOptions);
    }

    public static JsonValue Decode(string json, JsonDecodingOptions options)
    {
        var codec = Instance.Value ?? throw new InvalidOperationException("Failed to initialize JsonCodec instance");
        return codec.DecodeValue(json, options);
    }

    private JsonValue DecodeValue(string json, JsonDecodingOptions options)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(options);
        DecodingOptions = options;
        Reader.Init(json, options);
        CurrentTrivia.Clear();
        ParseWhiteSpaceAndCommentsTrivia(CurrentTrivia, false);
        var value = ParseValue();
        value.TrailingTrivia.AddRange(CurrentTrivia);
        CheckEndOfInput();
        return value;
    }

    private void CheckEndOfInput()
    {
        if (Reader.Current != -1)
        {
            throw CreateParsingException("Expected end of input");
        }
        if (Reader.Depth > 0)
        {
            throw CreateParsingException("Unclosed structure at end of input");
        }
    }

    private JsonParsingException CreateParsingException(string message)
    {
        return Reader.CreateParsingException(message);
    }

    private JsonValue ParseValue()
    {
        var value = Reader.Current switch
        {
            '[' => ParseArray() as JsonValue,
            '{' => ParseObject(),
            '"' => ParseString(),
            'n' => ParseNull(),
            't' => ParseBoolean(true, "true"),
            'f' => ParseBoolean(false, "false"),
            '-' or (>= '0' and <= '9') => ParseNumber(),
            -1 => throw CreateParsingException("Unexpected end of input"),
            _ => throw CreateParsingException($"Unexpected character '{Reader.CurrentChar}'"),
        };
        return value;
    }

    private JsonNull ParseNull()
    {
        Reader.ReadLiteralToken("null");
        var value = new JsonNull();
        ParsePrimitiveValueTrivia(value);
        return value;
    }

    private JsonBoolean ParseBoolean(bool expectedValue, string expectedLiteral)
    {
        Reader.ReadLiteralToken(expectedLiteral);
        var value = new JsonBoolean(expectedValue);
        ParsePrimitiveValueTrivia(value);
        return value;
    }

    private JsonNumber ParseNumber()
    {
        var rawValue = Reader.ReadNumberToken();
        var value = new JsonNumber(rawValue);
        ParsePrimitiveValueTrivia(value);
        return value;
    }

    private JsonString ParseString()
    {
        Reader.ReadStringToken(out var strValue, out var rawValue);
        var value = new JsonString(strValue, rawValue);
        ParsePrimitiveValueTrivia(value);
        return value;
    }

    private JsonArray ParseArray()
    {
        var array = new JsonArray();
        Reader.StartArray();
        ParseLeadingTrivia(array, JsonTriviaKind.ArrayStart, "[");
        while (true)
        {
            if (Reader.Current == ']')
            {
                break;
            }
            var value = ParseValue();
            array.AddParsedValue(value);
            if (Reader.Current == ']')
            {
                break;
            }
            if (Reader.Current != ',')
            {
                throw CreateParsingException("Expected ',' or ']' in array");
            }
            ParseTrailingTrivia(value, JsonTriviaKind.Comma, ",");
        }
        if (!DecodingOptions.AllowTrailingCommas && array.Count > 0)
        {
            if (array[^1].TrailingTrivia.Any(t => t.Kind == JsonTriviaKind.Comma))
            {
                throw CreateParsingException("Trailing commas are not allowed in objects");
            }
        }
        ParseTrailingTrivia(array, JsonTriviaKind.ArrayEnd, "]");
        Reader.EndArray();
        return array;
    }

    private JsonObject ParseObject()
    {
        var obj = new JsonObject();
        Reader.StartObject();
        ParseLeadingTrivia(obj, JsonTriviaKind.ObjectStart, "{");
        while (true)
        {
            if (Reader.Current == '}')
            {
                break;
            }
            var key = ParseKey();
            var value = ParseValue();
            var property = new JsonProperty(key, value);
            obj.AddParsedProperty(property);
            if (Reader.Current == '}')
            {
                break;
            }
            if (Reader.Current != ',')
            {
                throw CreateParsingException("Expected ',' or '}' in object");
            }
            ParseTrailingTrivia(value, JsonTriviaKind.Comma, ",");
        }
        if (!DecodingOptions.AllowTrailingCommas && obj.Count > 0)
        {
            if (obj[^1].Value.TrailingTrivia.Any(t => t.Kind == JsonTriviaKind.Comma))
            {
                throw CreateParsingException("Trailing commas are not allowed in objects");
            }
        }
        ParseTrailingTrivia(obj, JsonTriviaKind.ObjectEnd, "}");
        Reader.EndObject();
        return obj;
    }

    private JsonString ParseKey()
    {
        if (Reader.Current != '"')
        {
            throw CreateParsingException("Expected string as key in object");
        }
        var key = ParseString();
        if (Reader.Current != ':')
        {
            throw CreateParsingException("Expected ':' after object key");
        }
        Reader.Read(); // consume ':'
        key.TrailingTrivia.Add(new JsonTrivia(JsonTriviaKind.Colon, ":"));
        ParseWhiteSpaceAndCommentsTrivia(CurrentTrivia, true);
        // if there is a newline before the value, all trivia belongs to the key's trailing trivia
        if (CurrentTrivia[^1].Kind == JsonTriviaKind.NewLine)
        {
            key.TrailingTrivia.AddRange(CurrentTrivia);
            CurrentTrivia.Clear();
        }
        ParseWhiteSpaceAndCommentsTrivia(CurrentTrivia, false); // parse trivia after newline
        return key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void ParseWhiteSpaceAndCommentsTrivia(JsonTriviaCollection trivia, bool stopAfterNewLine)
    {
        var stop = false;
        while (!stop)
        {
            switch (Reader.Current)
            {
                case '/': trivia.Add(Reader.ReadCommentToken()); break;
                case ' ': trivia.Add(Reader.ReadWhitespacesToken()); break;
                case '\t': trivia.Add(Reader.ReadTabsToken()); break;
                case '\n': trivia.Add(Reader.ReadNewLineToken()); stop = stopAfterNewLine; break;
                case '\r': trivia.Add(Reader.ReadCarriageReturnToken()); stop = stopAfterNewLine; break;
                default: return;
            }
        }
    }

    private void ParsePrimitiveValueTrivia(JsonValue value)
    {
        var tmp = value.LeadingTrivia;
        value.LeadingTrivia = CurrentTrivia;
        ParseWhiteSpaceAndCommentsTrivia(value.TrailingTrivia, true);
        CurrentTrivia = tmp;
        ParseWhiteSpaceAndCommentsTrivia(CurrentTrivia, false);
    }

    private void ParseLeadingTrivia(JsonValue value, JsonTriviaKind currentTriviaTokenKind, string currentTriviaTokenValue)
    {
        var tmp = value.LeadingTrivia;
        value.LeadingTrivia = CurrentTrivia;
        value.LeadingTrivia.Add(new JsonTrivia(currentTriviaTokenKind, currentTriviaTokenValue));
        Reader.Read(); // consume the current token
        ParseWhiteSpaceAndCommentsTrivia(value.LeadingTrivia, true);
        CurrentTrivia = tmp;
        ParseWhiteSpaceAndCommentsTrivia(CurrentTrivia, false);
    }

    private void ParseTrailingTrivia(JsonValue value, JsonTriviaKind currentTriviaTokenKind, string currentTriviaTokenValue)
    {
        var tmp = value.TrailingTrivia;
        if (value.TrailingTrivia.Count == 0)
        {
            value.TrailingTrivia = CurrentTrivia;
        }
        else
        {
            value.TrailingTrivia.AddRange(CurrentTrivia);
            tmp = [];
        }
        value.TrailingTrivia.Add(new JsonTrivia(currentTriviaTokenKind, currentTriviaTokenValue));
        Reader.Read(); // consume the current token
        ParseWhiteSpaceAndCommentsTrivia(value.TrailingTrivia, true);
        CurrentTrivia = tmp;
        ParseWhiteSpaceAndCommentsTrivia(CurrentTrivia, false);
    }
}
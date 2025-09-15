namespace JsonPlus;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public enum JsonValueKind
{
    Null,
    Boolean,
    Number,
    String,
    Array,
    Object,
}

public enum JsonTriviaKind
{
    Whitespace,
    NewLine,
    SingleLineComment,
    MultiLineComment,
    ArrayStart,
    ArrayEnd,
    ObjectStart,
    ObjectEnd,
    Colon,
    Comma,
}

public record struct JsonTrivia(JsonTriviaKind Kind, string Value);

public class JsonTriviaCollection : List<JsonTrivia>
{
    public JsonTriviaCollection() : base(4) { }

    public JsonTriviaCollection(IEnumerable<JsonTrivia> collection) : base(collection) { }
}

public interface IJsonValue
{
    public JsonValueKind Kind { get; }

    public bool IsPrimitive { get; }

    public JsonTriviaCollection LeadingTrivia { get; set; }

    public JsonTriviaCollection TrailingTrivia { get; set; }

    public JsonArray GetArrayValue() => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public JsonObject GetObjectValue() => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public JsonString GetStringValue() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public JsonNumber GetNumberValue() => throw new InvalidOperationException($"Value is not a number (actual type: {Kind})");

    public JsonBoolean GetBooleanValue() => throw new InvalidOperationException($"Value is not a boolean (actual type: {Kind})");

    public JsonNull GetNullValue() => throw new InvalidOperationException($"Value is not null (actual type: {Kind})");

    public string GetString() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public double GetDouble() => throw new InvalidOperationException($"Value is not a number (actual type: {Kind})");

    public bool GetBoolean() => throw new InvalidOperationException($"Value is not a boolean (actual type: {Kind})");

    public JsonProperty GetProperty(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public IJsonValue GetPropertyValue(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public IJsonValue SetProperty(string key, IJsonValue value) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public IJsonValue AddProperty(string key, IJsonValue value) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public IJsonValue InsertProperty(int index, JsonProperty property) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public IJsonValue RemoveProperty(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public IJsonValue GetItem(int index) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public IJsonValue SetItem(int index, IJsonValue newValue) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public IJsonValue AddItem(IJsonValue item) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public IJsonValue InsertItem(int index, IJsonValue newValue) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public IJsonValue RemoveItem(IJsonValue item) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public IJsonValue RemoveItemAt(int index) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");
}

public sealed record JsonProperty(JsonString Key, IJsonValue Value)
{
    public JsonProperty(string key, IJsonValue value) : this(new JsonString(key), value) { }

    public IJsonValue Value { get; set; } = Value;

    public IJsonValue GetValue() => Value;

    public JsonProperty SetValue(IJsonValue newValue)
    {
        Value = newValue;
        return this;
    }
}

public sealed record JsonString : IJsonValue, IEquatable<string>, IEquatable<JsonString>
{
    public string Value { get; private set; }

    public string RawValue { get; private set; }

    public JsonString(string value, string rawValue) => (Value, RawValue) = (value, rawValue);

    public JsonString(string value) : this(value, JsonCodec.EncodeString(value)) { }

    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public JsonValueKind Kind => JsonValueKind.String;

    public JsonString GetStringValue() => this;

    public string GetString() => Value;

    public bool Equals(string? other) => Value.Equals(other);

    public bool Equals(JsonString? other) => other is not null && Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public bool IsPrimitive => true;
}

public sealed record JsonNumber(string RawValue): IJsonValue, IEquatable<double>, IEquatable<JsonNumber>
{
    private double? Value;

    public JsonNumber(double value) : this(value.ToString(System.Globalization.CultureInfo.InvariantCulture)) { }

    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public JsonValueKind Kind => JsonValueKind.Number;

    public JsonNumber GetNumberValue() => this;

    public double GetDouble() => Value ??= double.Parse(RawValue, System.Globalization.CultureInfo.InvariantCulture);

    public bool Equals(double other) => Value.Equals(other);

    public bool Equals(JsonNumber? other) => other is not null && Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public bool IsPrimitive => true;
}

public sealed record JsonBoolean(bool Value): IJsonValue, IEquatable<bool>, IEquatable<JsonBoolean>
{
    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public JsonValueKind Kind => JsonValueKind.Boolean;

    public JsonBoolean GetBooleanValue() => this;

    public bool GetBoolean() => Value;

    public bool Equals(bool other) => Value.Equals(other);

    public bool Equals(JsonBoolean? other) => other is not null && Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public bool IsPrimitive => true;
}

public sealed record JsonNull: IJsonValue, IEquatable<JsonNull>
{
    public JsonNull() { }

    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public JsonValueKind Kind => JsonValueKind.Null;

    public JsonNull GetNullValue() => this;

    public bool Equals(JsonNull? other) => other is not null;

    public override int GetHashCode() => 0;

    public bool IsPrimitive => true;
}

public class JsonArray : IJsonValue, IList<IJsonValue>, IEquatable<JsonArray>
{
    private List<IJsonValue> Items { get; } = new List<IJsonValue>(8);

    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public JsonValueKind Kind => JsonValueKind.Array;

    public JsonArray GetArrayValue() => this;

    public bool IsPrimitive => false;

    public int Count => Items.Count;

    public bool IsReadOnly => false;

    public IJsonValue this[int index] { get => Items[index]; set => Items[index] = value; }

    public int IndexOf(IJsonValue item) => Items.IndexOf(item);

    public void Insert(int index, IJsonValue item) => Items.Insert(index, item);

    public void RemoveAt(int index) => Items.RemoveAt(index);

    public void Clear() => Items.Clear();

    public bool Contains(IJsonValue item) => Items.Contains(item);

    public void CopyTo(IJsonValue[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public bool Remove(IJsonValue item) => Items.Remove(item);

    public IEnumerator<IJsonValue> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public IJsonValue GetItem(int index) => Items[index];

    public IJsonValue SetItem(int index, IJsonValue item)
    {
        Items[index] = item;
        return this;
    }

    public IJsonValue InsertItem(int index, IJsonValue item)
    {
        Insert(index, item);
        return this;
    }

    public IJsonValue RemoveItemAt(int index)
    {
        RemoveAt(index);
        return this;
    }

    public IJsonValue RemoveItem(IJsonValue item)
    {
        Remove(item);
        return this;
    }

    public IJsonValue AddItem(IJsonValue item)
    {
        Add(item);
        return this;
    }

    public void Add(IJsonValue item)
    {
        if (Items.Count > 0)
        {
            var lastItem = Items[^1];
            if (item.LeadingTrivia.Count == 0)
            {
                item.LeadingTrivia.AddRange(lastItem.LeadingTrivia);
            }
            var commaIndex = lastItem.TrailingTrivia.FindIndex(t => t.Kind == JsonTriviaKind.Comma);
            if (commaIndex == -1)
            {
                lastItem.TrailingTrivia.Insert(0, new JsonTrivia(JsonTriviaKind.Comma, ","));
            }
            var newLineIndex = lastItem.TrailingTrivia.FindIndex(t => t.Kind == JsonTriviaKind.NewLine);
            if (newLineIndex != -1 && item.TrailingTrivia.Count == 0)
            {
                item.TrailingTrivia.Add(lastItem.TrailingTrivia[newLineIndex]);
            }
        }
        Items.Add(item);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as JsonArray);
    }

    public bool Equals(JsonArray? other)
    {
        if (other is null || other.Count != Count)
        {
            return false;
        }
        for (int i = 0; i < Count; i++)
        {
            if (!Items[i].Equals(other.Items[i]))
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }

    /// <summary>
    /// Used during parsing to add items without modifying trivia.
    /// </summary>
    internal void AddParsedValue(IJsonValue item)
    {
        Items.Add(item);
    }
}

public class JsonObject: IJsonValue, IDictionary<string, IJsonValue>, IList<JsonProperty>, IEquatable<JsonObject>
{
    private readonly List<JsonProperty> Sequence = new(8);

    private readonly Dictionary<string, JsonProperty> Items = new(8);

    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public JsonValueKind Kind => JsonValueKind.Object;

    public JsonObject GetObjectValue() => this;

    public bool IsPrimitive => false;

    public ICollection<string> Keys => Items.Keys;

    public ICollection<IJsonValue> Values => [.. Items.Values.Select(it => it.Value)];

    public int Count => Items.Count;

    public bool IsReadOnly => false;

    public JsonProperty this[int index]
    {
        get => Sequence[index];
        set => Sequence[index]= value;
    }

    public IJsonValue this[string key]
    {
        get => Items[key].Value;
        set
        {
            if (Items.TryGetValue(key, out var prop))
            {
                var oldValue = prop.Value;
                prop.Value = value;
                if (prop.Value.LeadingTrivia.Count == 0)
                {
                    prop.Value.LeadingTrivia.AddRange(oldValue.LeadingTrivia);
                }
                if (prop.Value.TrailingTrivia.Count == 0)
                {
                    prop.Value.TrailingTrivia.AddRange(oldValue.TrailingTrivia);
                }
            }
            else
            {
                Add(key, value);
            }
        }
    }

    public JsonProperty GetProperty(string key)
    {
        if (Items.TryGetValue(key, out var prop))
        {
            return prop;
        }
        throw new KeyNotFoundException($"The given key '{key}' was not present in the JsonObject.");
    }

    public IJsonValue GetPropertyValue(string key)
    {
        if (Items.TryGetValue(key, out var prop))
        {
            return prop.Value;
        }
        throw new KeyNotFoundException($"The given key '{key}' was not present in the JsonObject.");
    }

    public IJsonValue SetProperty(string key, IJsonValue value)
    {
        this[key] = value;
        return this;
    }

    public IJsonValue AddProperty(string key, IJsonValue value)
    {
        Add(key, value);
        return this;
    }

    public IJsonValue InsertProperty(int index, JsonProperty property)
    {
        Insert(index, property);
        return this;
    }

    public IJsonValue RemoveProperty(string key)
    {
        Remove(key);
        return this;
    }

    /// <summary>
    /// Used  during parsing to add items without modifying trivia.
    /// </summary>
    internal void AddParsedProperty(JsonProperty prop)
    {
        Items[prop.Key.Value] = prop;
        Sequence.Add(prop);
    }

    public void Add(KeyValuePair<string, IJsonValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Add(string key, IJsonValue value)
    {
        if (Items.ContainsKey(key))
        {
            throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
        }
        Add(new JsonProperty(key, value));
    }

    public void Add(JsonProperty prop)
    {
        if (Sequence.Count > 0)
        {
            CopyTrivia(Sequence[^1], prop, true);
        }
        Items[prop.Key.Value] = prop;
        Sequence.Add(prop);
    }

    public void Insert(int index, JsonProperty prop)
    {
        if (index >= 0 && index <= Sequence.Count)
        {
            CopyTrivia(Sequence[index == 0 ? index : index - 1], prop, isLast: index == Sequence.Count);
        }
        Sequence.Insert(index, prop);
        Items[prop.Key.Value] = prop;
    }

    public bool ContainsKey(string key)
    {
        return Items.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        if (Items.Remove(key))
        {
            var index = Sequence.FindIndex(p => p.Key.Value == key);
            if (index < 0)
            {
                throw new InvalidOperationException("Inconsistent state: key exists in dictionary but not in sequence");
            }
            Sequence.RemoveAt(index);
            return true;
        }
        return false;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IJsonValue value)
    {
        if (Items.TryGetValue(key, out var prop))
        {
            value = prop.Value;
            return true;
        }
        value = null;
        return false;
    }

    public void Clear()
    {
        Items.Clear();
        Sequence.Clear();
    }

    public bool Contains(KeyValuePair<string, IJsonValue> item)
    {
        if (Items.TryGetValue(item.Key, out var prop))
        {
            return EqualityComparer<IJsonValue>.Default.Equals(prop.Value, item.Value);
        }
        return false;
    }

    public void CopyTo(KeyValuePair<string, IJsonValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, IJsonValue>>)Items).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, IJsonValue> item)
    {
        if (Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    public IEnumerator<KeyValuePair<string, IJsonValue>> GetEnumerator()
    {
        return Items.Select(kv => new KeyValuePair<string, IJsonValue>(kv.Key, kv.Value.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(JsonProperty item)
    {
        return Sequence.IndexOf(item);
    }

    public void RemoveAt(int index)
    {
        Items.Remove(Sequence[index].Key.Value);
        Sequence.RemoveAt(index);
    }

    public bool Contains(JsonProperty prop)
    {
        if (!Items.TryGetValue(prop.Key.Value, out var item))
        {
            return false;
        }
        if (!EqualityComparer<IJsonValue>.Default.Equals(item.Value, prop.Value))
        {
            return false;
        }
        return true;
    }

    public void CopyTo(JsonProperty[] array, int arrayIndex)
    {
        Sequence.CopyTo(array, arrayIndex);
    }

    public bool Remove(JsonProperty item)
    {
        Items.Remove(item.Key.Value);
        return Sequence.Remove(item);
    }

    IEnumerator<JsonProperty> IEnumerable<JsonProperty>.GetEnumerator()
    {
        return Sequence.GetEnumerator();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as JsonObject);
    }

    public bool Equals(JsonObject? other)
    {
        if (other is null || other.Count != Count)
        {
            return false;
        }
        for (int i = 0; i < Sequence.Count; i++)
        {
            if (!Sequence[i].Key.Equals(other.Sequence[i].Key) || !Sequence[i].Value.Equals(other.Sequence[i].Value))
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        return Sequence.GetHashCode();
    }

    private static void CopyTrivia(JsonProperty src, JsonProperty dst, bool isLast)
    {
        if (dst.Key.LeadingTrivia.Count == 0)
        {
            dst.Key.LeadingTrivia.AddRange(src.Key.LeadingTrivia);
        }
        if (dst.Key.TrailingTrivia.Count == 0)
        {
            dst.Key.TrailingTrivia.AddRange(src.Key.TrailingTrivia);
        }
        var srcCommaIndex = src.Value.TrailingTrivia.FindIndex(t => t.Kind == JsonTriviaKind.Comma);
        if (srcCommaIndex == -1)
        {
            // add comma to src, assuming that src is the element before dst which is being inserted
            src.Value.TrailingTrivia.Insert(0, new JsonTrivia(JsonTriviaKind.Comma, ","));
        }
        if (dst.Value.LeadingTrivia.Count == 0)
        {
            // copy leading value trivia from src to dst
            dst.Value.LeadingTrivia.AddRange(src.Value.LeadingTrivia);
        }
        if (dst.Value.TrailingTrivia.Count == 0)
        {
            // add newline to dst trivia if src has it
            var newLineIndex = src.Value.TrailingTrivia.FindIndex(t => t.Kind == JsonTriviaKind.NewLine);
            if (newLineIndex != -1)
            {
                dst.Value.TrailingTrivia.Add(src.Value.TrailingTrivia[newLineIndex]);
            }
        }
        if (!isLast)
        {
            // add comma to dst if it is not the last element
            var dstCommaIndex = dst.Value.TrailingTrivia.FindIndex(t => t.Kind == JsonTriviaKind.Comma);
            if (dstCommaIndex == -1)
            {
                dst.Value.TrailingTrivia.Insert(0, new JsonTrivia(JsonTriviaKind.Comma, ","));
            }
        }
    }
}

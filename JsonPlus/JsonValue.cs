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

public abstract class JsonValue
{
    public JsonTriviaCollection LeadingTrivia { get; set; } = [];

    public JsonTriviaCollection TrailingTrivia { get; set; } = [];

    public virtual JsonValueKind Kind { get; }

    public virtual bool IsPrimitive { get; }

    public virtual string GetRawValue() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public virtual string GetString() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public virtual double GetDouble() => throw new InvalidOperationException($"Value is not a number (actual type: {Kind})");

    public virtual bool GetBoolean() => throw new InvalidOperationException($"Value is not a boolean (actual type: {Kind})");

    public virtual JsonArray GetArrayValue() => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonObject GetObjectValue() => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonString GetStringValue() => throw new InvalidOperationException($"Value is not a string (actual type: {Kind})");

    public virtual JsonNumber GetNumberValue() => throw new InvalidOperationException($"Value is not a number (actual type: {Kind})");

    public virtual JsonBoolean GetBooleanValue() => throw new InvalidOperationException($"Value is not a boolean (actual type: {Kind})");

    public virtual JsonNull GetNullValue() => throw new InvalidOperationException($"Value is not null (actual type: {Kind})");

    public virtual JsonProperty GetProperty(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue GetPropertyValue(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue SetProperty(string key, JsonValue value) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue AddProperty(string key, JsonValue value) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue InsertProperty(int index, JsonProperty property) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue RemoveProperty(string key) => throw new InvalidOperationException($"Value is not an object (actual type: {Kind})");

    public virtual JsonValue GetItem(int index) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue SetItem(int index, JsonValue newValue) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue AddItem(JsonValue item) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue InsertItem(int index, JsonValue newValue) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue RemoveItem(JsonValue item) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");

    public virtual JsonValue RemoveItemAt(int index) => throw new InvalidOperationException($"Value is not an array (actual type: {Kind})");
}

public sealed class JsonProperty
{
    public JsonString Key { get; set; }

    public JsonValue Value { get; set; }

    public JsonProperty(JsonString key, JsonValue value) => (Key, Value) = (key, value);

    public JsonProperty(string key, JsonValue value) => (Key, Value) = (new JsonString(key), value);

    public JsonValue GetValue() => Value;

    public JsonProperty SetValue(JsonValue newValue)
    {
        Value = newValue;
        return this;
    }
}

public sealed class JsonString : JsonValue, IEquatable<string>, IEquatable<JsonString>
{
    public string Value { get; private set; }

    public string RawValue { get; private set; }

    public JsonString(string value, string rawValue) => (Value, RawValue) = (value, rawValue);

    public JsonString(string value) : this(value, JsonCodec.EncodeString(value)) { }

    public override JsonValueKind Kind => JsonValueKind.String;

    public override bool IsPrimitive => true;

    public override string GetString() => Value;

    public override string GetRawValue() => RawValue;

    public override JsonString GetStringValue() => this;

    public override string ToString() => Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(string? other) => Value.Equals(other);

    public bool Equals(JsonString? other) => other is not null && Value.Equals(other.Value);
}

public sealed class JsonNumber: JsonValue, IEquatable<double>, IEquatable<JsonNumber>
{
    private double? Value;

    private readonly string RawValue;

    public JsonNumber(string rawValue) => RawValue = rawValue;

    public JsonNumber(double value) => (Value, RawValue) = (value, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public override JsonValueKind Kind => JsonValueKind.Number;

    public override bool IsPrimitive => true;

    public override string GetRawValue() => RawValue;

    public override double GetDouble() => Value ??= double.Parse(RawValue, System.Globalization.CultureInfo.InvariantCulture);

    public override JsonNumber GetNumberValue() => this;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(double other) => Value.Equals(other);

    public bool Equals(JsonNumber? other) => other is not null && Value.Equals(other.Value);
}

public sealed class JsonBoolean: JsonValue, IEquatable<bool>, IEquatable<JsonBoolean>
{
    public JsonBoolean(bool value) => Value = value;

    public bool Value { get; }

    public override JsonValueKind Kind => JsonValueKind.Boolean;

    public override bool IsPrimitive => true;

    public override bool GetBoolean() => Value;

    public override JsonBoolean GetBooleanValue() => this;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(bool other) => Value.Equals(other);

    public bool Equals(JsonBoolean? other) => other is not null && Value.Equals(other.Value);
}

public sealed class JsonNull: JsonValue, IEquatable<JsonNull>
{
    public JsonNull() { }

    public override JsonValueKind Kind => JsonValueKind.Null;

    public override bool IsPrimitive => true;

    public override JsonNull GetNullValue() => this;

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => Equals(obj as JsonString);

    public bool Equals(JsonNull? other) => other is not null;
}

public class JsonArray : JsonValue, IList<JsonValue>, IEquatable<JsonArray>
{
    private List<JsonValue> Items { get; } = new List<JsonValue>(8);

    public override JsonValueKind Kind => JsonValueKind.Array;

    public override bool IsPrimitive => false;

    public override JsonArray GetArrayValue() => this;

    public int Count => Items.Count;

    public bool IsReadOnly => false;

    public JsonValue this[int index] { get => Items[index]; set => Items[index] = value; }

    public int IndexOf(JsonValue item) => Items.IndexOf(item);

    public void Insert(int index, JsonValue item) => Items.Insert(index, item);

    public void RemoveAt(int index) => Items.RemoveAt(index);

    public void Clear() => Items.Clear();

    public bool Contains(JsonValue item) => Items.Contains(item);

    public void CopyTo(JsonValue[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public bool Remove(JsonValue item) => Items.Remove(item);

    public IEnumerator<JsonValue> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public override JsonValue GetItem(int index) => Items[index];

    public override JsonValue SetItem(int index, JsonValue item)
    {
        Items[index] = item;
        return this;
    }

    public override JsonValue InsertItem(int index, JsonValue item)
    {
        Insert(index, item);
        return this;
    }

    public override JsonValue RemoveItemAt(int index)
    {
        RemoveAt(index);
        return this;
    }

    public override JsonValue RemoveItem(JsonValue item)
    {
        Remove(item);
        return this;
    }

    public override JsonValue AddItem(JsonValue item)
    {
        Add(item);
        return this;
    }

    public void Add(JsonValue item)
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

    public override int GetHashCode()
    {
        return Items.GetHashCode();
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

    /// <summary>
    /// Used during parsing to add items without modifying trivia.
    /// </summary>
    internal void AddParsedValue(JsonValue item)
    {
        Items.Add(item);
    }
}

public class JsonObject: JsonValue, IDictionary<string, JsonValue>, IList<JsonProperty>, IEquatable<JsonObject>
{
    private readonly List<JsonProperty> Sequence = new(8);

    private readonly Dictionary<string, JsonProperty> Items = new(8);

    public override JsonValueKind Kind => JsonValueKind.Object;

    public override bool IsPrimitive => false;

    public override JsonObject GetObjectValue() => this;

    public ICollection<string> Keys => Items.Keys;

    public ICollection<JsonValue> Values => [.. Items.Values.Select(it => it.Value)];

    public int Count => Items.Count;

    public bool IsReadOnly => false;

    public JsonProperty this[int index]
    {
        get => Sequence[index];
        set => Sequence[index]= value;
    }

    public JsonValue this[string key]
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

    public override JsonProperty GetProperty(string key)
    {
        if (Items.TryGetValue(key, out var prop))
        {
            return prop;
        }
        throw new KeyNotFoundException($"The given key '{key}' was not present in the JsonObject.");
    }

    public override JsonValue GetPropertyValue(string key)
    {
        if (Items.TryGetValue(key, out var prop))
        {
            return prop.Value;
        }
        throw new KeyNotFoundException($"The given key '{key}' was not present in the JsonObject.");
    }

    public override JsonValue SetProperty(string key, JsonValue value)
    {
        this[key] = value;
        return this;
    }

    public override JsonValue AddProperty(string key, JsonValue value)
    {
        Add(key, value);
        return this;
    }

    public override JsonValue InsertProperty(int index, JsonProperty property)
    {
        Insert(index, property);
        return this;
    }

    public override JsonValue RemoveProperty(string key)
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

    public void Add(KeyValuePair<string, JsonValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Add(string key, JsonValue value)
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

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonValue value)
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

    public bool Contains(KeyValuePair<string, JsonValue> item)
    {
        if (Items.TryGetValue(item.Key, out var prop))
        {
            return EqualityComparer<JsonValue>.Default.Equals(prop.Value, item.Value);
        }
        return false;
    }

    public void CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, JsonValue>>)Items).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, JsonValue> item)
    {
        if (Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
    {
        return Items.Select(kv => new KeyValuePair<string, JsonValue>(kv.Key, kv.Value.Value)).GetEnumerator();
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
        if (!EqualityComparer<JsonValue>.Default.Equals(item.Value, prop.Value))
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

    public override int GetHashCode()
    {
        return Sequence.GetHashCode();
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

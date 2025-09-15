namespace JsonPlus.Tests;

[TestClass]
public class PrimitiveValuesTests
{
    [TestMethod]
    public void DecodeNullReturnsJsonNull()
    {
        // Test parsing null values
        var result = JsonCodec.Decode("null");
        Assert.AreEqual(JsonValueKind.Null, result.Kind);
    }

    [TestMethod]
    public void DecodeTrueValueReturnsTrue()
    {
        var result = JsonCodec.Decode("true");
        Assert.AreEqual(JsonValueKind.Boolean, result.Kind);
        Assert.AreEqual(true, result.GetBoolean());
    }

    [TestMethod]
    public void DecodeFalseValueReturnsFalse()
    {
        var result = JsonCodec.Decode("false");
        Assert.AreEqual(JsonValueKind.Boolean, result.Kind);
        Assert.AreEqual(false, result.GetBoolean());
    }

    [TestMethod]
    public void DecodeValidIntegerReturnsJsonNumber()
    {
        var result = JsonCodec.Decode("42");
        Assert.AreEqual(JsonValueKind.Number, result.Kind);
        Assert.AreEqual(42.0, result.GetDouble());
    }

    [TestMethod]
    public void DecodeStringReturnsJsonString()
    {
        var result = JsonCodec.Decode(""" "hello" """);
        Assert.AreEqual(JsonValueKind.String, result.Kind);
        Assert.AreEqual("hello", result.GetString());
    }

    [TestMethod]
    public void DecodeLargeNumbersHandlesEdgeCases()
    {
        var result = JsonCodec.Decode("1.7976931348623157E+308");
        Assert.AreEqual(JsonValueKind.Number, result.Kind);
    }

    [TestMethod]
    [DataRow("null", JsonValueKind.Null)]
    [DataRow("true", JsonValueKind.Boolean)]
    [DataRow("42", JsonValueKind.Number)]
    [DataRow("3.1415", JsonValueKind.Number)]
    [DataRow("1.2e5", JsonValueKind.Number)]
    [DataRow("[]", JsonValueKind.Array)]
    [DataRow("{}", JsonValueKind.Object)]
    [DataRow(""" "text" """, JsonValueKind.String)]
    public void DecodeValueReturnsCorrectKind(string json, JsonValueKind expected)
    {
        var result = JsonCodec.Decode(json);
        Assert.AreEqual(expected, result.Kind);
    }
}
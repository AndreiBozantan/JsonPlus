namespace JsonPlus.Tests;

[TestClass]
public class ConstructedValuesTests
{
    [TestMethod]
    public void DecodeSimpleArrayReturnsJsonArray()
    {
        var result = JsonCodec.Decode("[1, 2, 3]");
        Assert.AreEqual(JsonValueKind.Array, result.Kind);
        var array = result.GetArrayValue();
        Assert.AreEqual(3, array.Count);
    }

    [TestMethod]
    public void DecodeSimpleObjectReturnsJsonObject()
    {
        var result = JsonCodec.Decode("""{"key": "value"}""");
        Assert.AreEqual(JsonValueKind.Object, result.Kind);
        var obj = result.GetObjectValue();
        Assert.AreEqual(1, obj.Count);
    }
    [TestMethod]
    public void DecodeHandlesNesting()
    {
        var json = """{"array": [{"nested": true}]}""";
        var result = JsonCodec.Decode(json);
        // Assert nested structure is parsed correctly
        var obj = result.GetObjectValue();
        var array = obj["array"].GetArrayValue();
        var nestedObj = array[0].GetObjectValue();
        Assert.IsTrue(nestedObj["nested"].GetBoolean());
    }

    [TestMethod]
    public void DecodeEmptyArrayReturnsEmptyJsonArray()
    {
        var result = JsonCodec.Decode("[]");
        var array = result.GetArrayValue();
        Assert.AreEqual(0, array.Count);
    }
}
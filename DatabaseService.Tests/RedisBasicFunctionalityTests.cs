using DatabaseService;

namespace Tests;

[TestClass]
public class RedisBasicFunctionalityTests
{
    [TestMethod]
    public void BasicRedisInsertTest()
    {
        var db = RedisHandler.Database;
        db.StringSet("foo", "bar");

        string? retrievedData = db.StringGet("foo");
        Assert.IsNotNull(retrievedData);
        Assert.AreEqual(retrievedData, "bar");
    }

    [TestMethod]
    public void BasicRedisBatchTest()
    {
        var db = RedisHandler.Database;
        var batch = db.CreateBatch();

        batch.StringSetAsync("batch_foo", "batch_bar");
        batch.StringSetAsync("some", "thing");
        batch.StringSetAsync("batch_car", "batch_truck");

        batch.Execute();

        string? retrievedData = db.StringGet("batch_car");
        Assert.IsNotNull(retrievedData);
        Assert.AreEqual(retrievedData, "batch_truck");
        Console.WriteLine("tests are running???");
    }

    [TestMethod]
    public void ExampleFalseTest()
    {
        Assert.IsTrue(1 == 1);
    }
}
namespace Tests;

public class Tests
{
    [Test]
    public void TestFail()
    {
        Assert.Fail();
    }
    
    [Test]
    public void TestPass()
    {
        Assert.Pass();
    }
}
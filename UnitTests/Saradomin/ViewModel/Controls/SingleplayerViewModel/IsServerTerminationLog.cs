namespace UnitTests.Saradomin.ViewModel.Controls.SingleplayerViewModel;

[TestFixture]
public class IsServerTerminationLogTests
{
    [Test]
    [TestCase("[23:43:26]: [SystemTermination] Server successfully terminated!", true)]
    [TestCase("[23:00:26]: [SystemTermination] Server successfully terminated!", true)]
    [TestCase("[23:43:26]: lol said [SystemTermination] Server successfully terminated!", false)]
    [TestCase("[23:00:26]: [SystemTermination] Server successfully terminated! weee exploit", false)]
    [TestCase("Gotcha [23:00:26]: [SystemTermination] Server successfully terminated!", false)]
    public void ServerTerminationLogTests(string log, bool expectedOutcome)
    {
        // Act
        var result = global::Saradomin.ViewModel.Controls.SingleplayerViewModel.IsServerTerminationLog(log);

        // Assert
        Assert.That(result, Is.EqualTo(expectedOutcome), $"Failed for log: {log}");
    }
}
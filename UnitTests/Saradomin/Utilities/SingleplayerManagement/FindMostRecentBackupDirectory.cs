namespace UnitTests.Saradomin.Utilities.SingleplayerManagement;

[TestFixture]
public class BackupManagerTests
{
    [Test]
    public void WithValidDirectories_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230101_120000",
            "/backups/20230102_120000",
            "/backups/20230103_120000"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230103_120000".Equals(mostRecent));
    }

    [Test]
    public void WithDifferentYears_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20210101_120000",
            "/backups/20220101_120000",
            "/backups/20230101_120000"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230101_120000".Equals(mostRecent));
    }

    [Test]
    public void WithDifferentMonths_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230101_120000",
            "/backups/20230201_120000",
            "/backups/20230301_120000"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230301_120000".Equals(mostRecent));
    }

    [Test]
    public void WithDifferentDays_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230301_120000",
            "/backups/20230302_120000",
            "/backups/20230303_120000"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230303_120000".Equals(mostRecent));
    }

    [Test]
    public void WithDifferentHours_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230301_110000",
            "/backups/20230301_120000",
            "/backups/20230301_130000"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230301_130000".Equals(mostRecent));
    }

    [Test]
    public void WithDifferentMinutes_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230301_120100",
            "/backups/20230301_120200",
            "/backups/20230301_120300"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230301_120300".Equals(mostRecent));
    }

    [Test]
    public void WithDifferentSeconds_ReturnsMostRecent()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230301_120000",
            "/backups/20230301_120015",
            "/backups/20230301_120030"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230301_120030".Equals(mostRecent));
    }

    [Test]
    public void WithMixedDateTimes_ReturnsMostRecent_Case1()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20210101_235959",
            "/backups/20201231_235959",
            "/backups/20220228_235959"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20220228_235959".Equals(mostRecent));
    }

    [Test]
    public void WithMixedDateTimes_ReturnsMostRecent_Case2()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20211001_000001",
            "/backups/20211001_000002",
            "/backups/20210930_235959"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20211001_000002".Equals(mostRecent));
    }

    [Test]
    public void WithMixedDateTimes_ReturnsMostRecent_Case3()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20211231_235959",
            "/backups/20220101_000000",
            "/backups/20211231_235958"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20220101_000000".Equals(mostRecent));
    }

    [Test]
    public void WithMixedDateTimes_ReturnsMostRecent_Case4()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230530_123456",
            "/backups/20230529_123456",
            "/backups/20220530_123457"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230530_123456".Equals(mostRecent));
    }

    [Test]
    public void WithMixedDateTimes_ReturnsMostRecent_Case5()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20230102_010101",
            "/backups/20230101_222222",
            "/backups/20221231_235959"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20230102_010101".Equals(mostRecent));
    }

    [Test]
    public void WithMixedDateTimes_ReturnsMostRecent_Case6()
    {
        // Arrange
        var directories = new List<string>
        {
            "/backups/20240101_120000",
            "/backups/20231231_235959",
            "/backups/20231231_235958"
        };

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That("20240101_120000".Equals(mostRecent));
    }

    [Test]
    public void WithNoDirectories_ReturnsNull()
    {
        // Arrange
        var directories = new List<string>();

        // Act
        var mostRecent =
            global::Saradomin.Utilities.SingleplayerManagement.FindMostRecentBackupDirectory(
                directories
            );

        // Assert
        Assert.That(mostRecent, Is.EqualTo(null));
    }
}

using Xunit;
using FluentAssertions;
using FinancesControl.API.Accounts;
using FinancesControl.API.Common;

namespace FinancesControl.Tests.Accounts.Models;

public class AccountTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() => new Account(1, invalidName, "user-123"));
    }

    [Fact]
    public void Constructor_WithNameTooShort_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() => new Account(1, "A", "user-123"));
    }

    [Fact]
    public void Constructor_WithNameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('A', 51);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() => new Account(1, longName, "user-123"));
    }

    [Fact]
    public void Constructor_WithValidData_CreatesAccount()
    {
        // Act
        var account = new Account(1, "Valid Name", "user-123");

        // Assert
        account.Id.Should().Be(1);
        account.Name.Should().Be("Valid Name");
        account.UserId.Should().Be("user-123");
        account.IsActive.Should().BeTrue();
        account.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region Rename Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var account = new Account(1, "Original Name", "user-123");

        // Act & Assert
        Assert.Throws<DomainValidationException>(() => account.Rename(invalidName));
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        // Arrange
        var account = new Account(1, "Original Name", "user-123");

        // Act
        account.Rename("New Name");

        // Assert
        account.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_UpdatesTimestamp()
    {
        // Arrange
        var account = new Account(1, "Original Name", "user-123");
        var originalUpdatedAt = account.UpdatedAt ?? DateTime.MinValue;

        // Act
        account.Rename("New Name");

        // Assert
        account.UpdatedAt.Should().NotBe(originalUpdatedAt);
        account.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_BecomesActive()
    {
        // Arrange
        var account = new Account(1, "Test Account", "user-123");
        account.Deactivate();

        // Act
        account.Activate();

        // Assert
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_UpdatesTimestamp()
    {
        // Arrange
        var account = new Account(1, "Test Account", "user-123");
        account.Deactivate();
        var originalUpdatedAt = account.UpdatedAt ?? DateTime.MinValue;

        // Act
        account.Activate();

        // Assert
        account.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_BecomesInactive()
    {
        // Arrange
        var account = new Account(1, "Test Account", "user-123");

        // Act
        account.Deactivate();

        // Assert
        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_UpdatesTimestamp()
    {
        // Arrange
        var account = new Account(1, "Test Account", "user-123");
        var originalUpdatedAt = account.UpdatedAt ?? DateTime.MinValue;

        // Act
        account.Deactivate();

        // Assert
        account.UpdatedAt.Should().NotBe(originalUpdatedAt);
        account.UpdatedAt.Should().NotBeNull();
    }

    #endregion
}

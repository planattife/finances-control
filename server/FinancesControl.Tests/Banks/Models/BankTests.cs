using System;
using Xunit;
using FluentAssertions;
using FinancesControl.API.Banks;
using FinancesControl.API.Enums;

namespace FinancesControl.Tests.Banks.Models;

public class BankTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Bank(invalidName, BankAccountType.Check, 1, "123"));
    }

    [Fact]
    public void Constructor_WithNameTooShort_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Bank("A", BankAccountType.Check, 1, "123"));
    }

    [Fact]
    public void Constructor_WithNameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('A', 51);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Bank(longName, BankAccountType.Check, 1, "123"));
    }

    [Fact]
    public void Constructor_WithValidData_CreatesBank()
    {
        // Act
        var bank = new Bank("Valid Bank", BankAccountType.Check, 1, "123");

        // Assert
        bank.Id.Should().Be(1);
        bank.Name.Should().Be("Valid Bank");
        bank.AccountType.Should().Be(BankAccountType.Check);
        bank.IsActive.Should().BeTrue();
        bank.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Constructor_WithSavingsAccount_CreatesSavingsBank()
    {
        // Act
        var bank = new Bank("Savings Bank", BankAccountType.Savings, 1, "123");

        // Assert
        bank.AccountType.Should().Be(BankAccountType.Savings);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_BecomesActive()
    {
        // Arrange
        var bank = new Bank("Test Bank", BankAccountType.Check, 1, "123");
        bank.Deactivate();

        // Act
        bank.Activate();

        // Assert
        bank.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_UpdatesTimestamp()
    {
        // Arrange
        var bank = new Bank("Test Bank", BankAccountType.Check, 1, "123");
        bank.Deactivate();
        var originalUpdatedAt = bank.UpdatedAt ?? DateTime.MinValue;

        // Act
        bank.Activate();

        // Assert
        bank.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_BecomesInactive()
    {
        // Arrange
        var bank = new Bank("Test Bank", BankAccountType.Check, 1, "123");

        // Act
        bank.Deactivate();

        // Assert
        bank.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_UpdatesTimestamp()
    {
        // Arrange
        var bank = new Bank("Test Bank", BankAccountType.Check, 1, "123");
        var originalUpdatedAt = bank.UpdatedAt ?? DateTime.MinValue;

        // Act
        bank.Deactivate();

        // Assert
        bank.UpdatedAt.Should().NotBe(originalUpdatedAt);
        bank.UpdatedAt.Should().NotBeNull();
    }

    #endregion
}

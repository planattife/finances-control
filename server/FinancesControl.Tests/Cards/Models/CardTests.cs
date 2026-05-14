using Xunit;
using FluentAssertions;
using FinancesControl.API.Cards;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.Tests.Cards.Models;

public class CardTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidData_CreatesCard()
    {
        // Act
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1);

        // Assert
        card.Id.Should().Be(1);
        card.CardNumber.Should().Be("4111111111111111");
        card.CardNetwork.Should().Be(CardNetwork.Visa);
        card.BankId.Should().Be(1);
        card.AccountId.Should().Be(1);
        card.CardType.Should().Be(CardType.Debit);
        card.IsActive.Should().BeTrue();
        card.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Constructor_WithCardName_CreatesCardWithName()
    {
        // Act
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1, "My Debit Card");

        // Assert
        card.CardName.Should().Be("My Debit Card");
    }

    [Fact]
    public void Constructor_WithoutCardName_CreatesCardWithNullName()
    {
        // Act
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1);

        // Assert
        card.CardName.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithDifferentNetworks_CreatesCard()
    {
        // Arrange
        var networks = new[] { CardNetwork.Mastercard, CardNetwork.Elo, CardNetwork.AmericanExpress, CardNetwork.Hipercard };

        // Act & Assert
        foreach (var network in networks)
        {
            var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), network, 1, 1, CardType.Debit, 1);
            card.CardNetwork.Should().Be(network);
        }
    }

    #endregion

    #region UpdateCardName Tests

    [Fact]
    public void UpdateCardName_WithValidName_UpdatesName()
    {
        // Arrange
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1, "Old Name");

        // Act
        card.UpdateCardName("New Name");

        // Assert
        card.CardName.Should().Be("New Name");
    }

    [Fact]
    public void UpdateCardName_CanUpdateMultipleTimes()
    {
        // Arrange
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1);

        // Act
        card.UpdateCardName("First Name");
        card.UpdateCardName("Second Name");

        // Assert
        card.CardName.Should().Be("Second Name");
    }

    #endregion

    #region SetIsActive Tests

    [Fact]
    public void SetIsActive_ToFalse_BecomesInactive()
    {
        // Arrange
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1);

        // Act
        card.SetIsActive(false);

        // Assert
        card.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetIsActive_ToTrue_BecomesActive()
    {
        // Arrange
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1);
        card.SetIsActive(false);

        // Act
        card.SetIsActive(true);

        // Assert
        card.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetIsActive_UpdatesTimestamp()
    {
        // Arrange
        var card = new Card("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, 1, CardType.Debit, 1);
        var originalUpdatedAt = card.UpdatedAt;

        // Act
        card.SetIsActive(false);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
        card.UpdatedAt.Should().NotBeNull();
    }

    #endregion
}

public class CreditCardTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidData_CreatesCreditCard()
    {
        // Act
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20, CardType.Credit, 1);

        // Assert
        creditCard.Id.Should().Be(1);
        creditCard.CardNumber.Should().Be("5555555555554444");
        creditCard.CardNetwork.Should().Be(CardNetwork.Mastercard);
        creditCard.BankId.Should().Be(1);
        creditCard.AccountId.Should().Be(1);
        creditCard.CardType.Should().Be(CardType.Credit);
        creditCard.ClosingDay.Should().Be(10);
        creditCard.DueDateDay.Should().Be(20);
        creditCard.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCardName_CreatesCreditCardWithName()
    {
        // Act
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20, CardType.Credit, 1, "My Credit Card");

        // Assert
        creditCard.CardName.Should().Be("My Credit Card");
    }

    #endregion

    #region UpdateClosingDay Tests

    [Fact]
    public void UpdateClosingDay_WithValidDay_UpdatesClosingDay()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Act
        creditCard.UpdateClosingDay(15);

        // Assert
        creditCard.ClosingDay.Should().Be(15);
    }

    [Fact]
    public void UpdateClosingDay_UpdatesTimestamp()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);
        var originalUpdatedAt = creditCard.UpdatedAt;

        // Act
        creditCard.UpdateClosingDay(15);

        // Assert
        creditCard.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateClosingDay_CanUpdateMultipleTimes()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Act
        creditCard.UpdateClosingDay(15);
        creditCard.UpdateClosingDay(25);

        // Assert
        creditCard.ClosingDay.Should().Be(25);
    }

    #endregion

    #region UpdateDueDateDay Tests

    [Fact]
    public void UpdateDueDateDay_WithValidDay_UpdatesDueDateDay()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Act
        creditCard.UpdateDueDateDay(25);

        // Assert
        creditCard.DueDateDay.Should().Be(25);
    }

    [Fact]
    public void UpdateDueDateDay_UpdatesTimestamp()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);
        var originalUpdatedAt = creditCard.UpdatedAt;

        // Act
        creditCard.UpdateDueDateDay(25);

        // Assert
        creditCard.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDueDateDay_CanUpdateMultipleTimes()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Act
        creditCard.UpdateDueDateDay(25);
        creditCard.UpdateDueDateDay(5);

        // Assert
        creditCard.DueDateDay.Should().Be(5);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void CreditCard_InheritsFromCard()
    {
        // Arrange & Act
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Assert
        creditCard.Should().BeAssignableTo<Card>();
    }

    [Fact]
    public void CreditCard_InheritsSetIsActiveMethod()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Act
        creditCard.SetIsActive(false);

        // Assert
        creditCard.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CreditCard_InheritsUpdateCardNameMethod()
    {
        // Arrange
        var creditCard = new CreditCard("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 1, 10, 20);

        // Act
        creditCard.UpdateCardName("My Credit Card");

        // Assert
        creditCard.CardName.Should().Be("My Credit Card");
    }

    #endregion
}

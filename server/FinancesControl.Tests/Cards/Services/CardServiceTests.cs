using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Cards;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Cards.Repository;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Common;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.Tests.Cards.Services;

public class CardServiceTests
{
    private readonly ICardRepository _repository;
    private readonly CardService _service;

    public CardServiceTests()
    {
        _repository = Substitute.For<ICardRepository>();
        _service = new CardService(_repository);
    }

    #region CreateDebitCard Tests

    [Fact]
    public void CreateDebitCard_WithValidRequest_ReturnsNewCardId()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _repository.GetAll().Returns(new List<Card>());
        _repository.Add(Arg.Any<Card>()).Returns(1);

        // Act
        var result = _service.CreateDebitCard(request);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Card>());
    }

    [Fact]
    public void CreateDebitCard_GeneratesAutoIncrementingId()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        var existingCards = new List<Card>
        {
            new("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, 1),
            new("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, CardType.Debit, 2)
        };
        _repository.GetAll().Returns(existingCards);
        _repository.Add(Arg.Any<Card>()).Returns(3);

        // Act
        var result = _service.CreateDebitCard(request);

        // Assert
        result.Should().Be(3);
        _repository.Received(1).Add(Arg.Is<Card>(c => c.Id == 3));
    }

    [Fact]
    public void CreateDebitCard_FirstCardGetsId1()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _repository.GetAll().Returns(new List<Card>());
        _repository.Add(Arg.Any<Card>()).Returns(1);

        // Act
        var result = _service.CreateDebitCard(request);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void CreateDebitCard_NewCardIsActive()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _repository.GetAll().Returns(new List<Card>());
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateDebitCard(request);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateDebitCard_WithCardName()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, "My Debit Card");
        _repository.GetAll().Returns(new List<Card>());
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateDebitCard(request);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard.CardName.Should().Be("My Debit Card");
    }

    #endregion

    #region CreateCreditCard Tests

    [Fact]
    public void CreateCreditCard_WithValidRequest_ReturnsNewCardId()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _repository.GetAll().Returns(new List<Card>());
        _repository.Add(Arg.Any<Card>()).Returns(1);

        // Act
        var result = _service.CreateCreditCard(request);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Card>());
    }

    [Fact]
    public void CreateCreditCard_CreatesCreditCardType()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _repository.GetAll().Returns(new List<Card>());
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateCreditCard(request);

        // Assert
        capturedCard.Should().BeOfType<CreditCard>();
        (capturedCard as CreditCard).ClosingDay.Should().Be(10);
        (capturedCard as CreditCard).DueDateDay.Should().Be(20);
    }

    [Fact]
    public void CreateCreditCard_WithCardName()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, "My Credit Card");
        _repository.GetAll().Returns(new List<Card>());
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateCreditCard(request);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard.CardName.Should().Be("My Credit Card");
    }

    #endregion

    #region GetById Tests

    [Fact]
    public void GetById_WithValidId_ReturnsCardDetail()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId, "My Card");
        _repository.GetById(cardId).Returns(card);

        // Act
        var result = _service.GetById(cardId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.CardType.Should().Be(CardType.Debit);
        result.CardNumber.Should().Be("****1111");
        result.CardName.Should().Be("My Card");
    }

    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var cardId = 999;
        _repository.GetById(cardId).Returns((Card?)null);

        // Act
        var result = _service.GetById(cardId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_InactiveCard_ReturnsInactiveStatus()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        card.SetIsActive(false);
        _repository.GetById(cardId).Returns(card);

        // Act
        var result = _service.GetById(cardId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Inactive");
    }

    #endregion

    #region GetCreditById Tests

    [Fact]
    public void GetCreditById_WithValidId_ReturnsCreditCardDetail()
    {
        // Arrange
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, CardType.Credit, cardId, "My Credit");
        _repository.GetById(cardId).Returns(creditCard);

        // Act
        var result = _service.GetCreditById(cardId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.CardType.Should().Be(CardType.Credit);
        result.CardNumber.Should().Be("****4444");
        result.NextClosingDay.Should().NotBe(default(DateTime));
        result.NextDueDateDay.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void GetCreditById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var cardId = 999;
        _repository.GetById(cardId).Returns((Card?)null);

        // Act
        var result = _service.GetCreditById(cardId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCreditById_WithDebitCard_ReturnsNull()
    {
        // Arrange
        var cardId = 1;
        var debitCard = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        _repository.GetById(cardId).Returns(debitCard);

        // Act
        var result = _service.GetCreditById(cardId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCreditById_ComputesNextClosingDay()
    {
        // Arrange
        var cardId = 1;
        var closingDay = 15;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, closingDay, 20, CardType.Credit, cardId);
        _repository.GetById(cardId).Returns(creditCard);

        // Act
        var result = _service.GetCreditById(cardId);

        // Assert
        result.Should().NotBeNull();
        var nextClosing = DateHelper.GetNextOccurrence(closingDay);
        result.NextClosingDay.Should().Be(nextClosing);
    }

    [Fact]
    public void GetCreditById_ComputesNextDueDay()
    {
        // Arrange
        var cardId = 1;
        var dueDay = 25;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, dueDay, CardType.Credit, cardId);
        _repository.GetById(cardId).Returns(creditCard);

        // Act
        var result = _service.GetCreditById(cardId);

        // Assert
        result.Should().NotBeNull();
        var nextDue = DateHelper.GetNextOccurrence(dueDay);
        result.NextDueDateDay.Should().Be(nextDue);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_WithMultipleCards_ReturnsAllCards()
    {
        // Arrange
        var cards = new List<Card>
        {
            new("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, 1),
            new("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, CardType.Debit, 2)
        };
        _repository.GetAll().Returns(cards);

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetAll_WithNoCards_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAll().Returns(new List<Card>());

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateCardName Tests

    [Fact]
    public void UpdateCardName_WithValidIdAndName_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId, "Old Name");
        var newName = "New Name";
        _repository.GetById(cardId).Returns(card);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        var result = _service.UpdateCardName(cardId, newName);

        // Assert
        result.Should().BeTrue();
        card.CardName.Should().Be("New Name");
        _repository.Received(1).Update(card);
    }

    [Fact]
    public void UpdateCardName_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var cardId = 999;
        var newName = "New Name";
        _repository.GetById(cardId).Returns((Card?)null);

        // Act
        var result = _service.UpdateCardName(cardId, newName);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>());
    }

    #endregion

    #region UpdateCreditCardDates Tests

    [Fact]
    public void UpdateCreditCardDates_WithValidId_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, CardType.Credit, cardId);
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId).Returns(creditCard);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, request);

        // Assert
        result.Should().BeTrue();
        creditCard.ClosingDay.Should().Be(15);
        creditCard.DueDateDay.Should().Be(25);
        _repository.Received(1).Update(creditCard);
    }

    [Fact]
    public void UpdateCreditCardDates_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var cardId = 999;
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId).Returns((Card?)null);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, request);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>());
    }

    [Fact]
    public void UpdateCreditCardDates_WithDebitCard_ReturnsFalse()
    {
        // Arrange
        var cardId = 1;
        var debitCard = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId).Returns(debitCard);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, request);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>());
    }

    [Fact]
    public void UpdateCreditCardDates_UpdatesTimestamp()
    {
        // Arrange
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, CardType.Credit, cardId);
        var originalUpdatedAt = creditCard.UpdatedAt;
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId).Returns(creditCard);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        _service.UpdateCreditCardDates(cardId, request);

        // Assert
        creditCard.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        card.SetIsActive(false);
        _repository.GetById(cardId).Returns(card);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        var result = _service.Activate(cardId);

        // Assert
        result.Should().BeTrue();
        card.IsActive.Should().BeTrue();
        _repository.Received(1).Update(card);
    }

    [Fact]
    public void Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var cardId = 999;
        _repository.GetById(cardId).Returns((Card?)null);

        // Act
        var result = _service.Activate(cardId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>());
    }

    [Fact]
    public void Activate_UpdatesTimestamp()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        card.SetIsActive(false);
        var originalUpdatedAt = card.UpdatedAt;
        _repository.GetById(cardId).Returns(card);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        _service.Activate(cardId);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        _repository.GetById(cardId).Returns(card);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        var result = _service.Deactivate(cardId);

        // Assert
        result.Should().BeTrue();
        card.IsActive.Should().BeFalse();
        _repository.Received(1).Update(card);
    }

    [Fact]
    public void Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var cardId = 999;
        _repository.GetById(cardId).Returns((Card?)null);

        // Act
        var result = _service.Deactivate(cardId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>());
    }

    [Fact]
    public void Deactivate_UpdatesTimestamp()
    {
        // Arrange
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, CardType.Debit, cardId);
        var originalUpdatedAt = card.UpdatedAt;
        _repository.GetById(cardId).Returns(card);
        _repository.Update(Arg.Any<Card>()).Returns(true);

        // Act
        _service.Deactivate(cardId);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion
}

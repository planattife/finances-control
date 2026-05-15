using System.Security.Claims;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Accounts.Services;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Cards;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Cards.Repository;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Common;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;
using Microsoft.AspNetCore.Http;

namespace FinancesControl.Tests.Cards.Services;

public class CardServiceTests
{
    private readonly ICardRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBankService _bankService;
    private readonly IAccountService _accountService;
    private readonly CardService _service;
    private const string UserId = "test-user-id";
    private const long AccountId = 1;

    public CardServiceTests()
    {
        _repository = Substitute.For<ICardRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _bankService = Substitute.For<IBankService>();
        _accountService = Substitute.For<IAccountService>();
        _service = new CardService(_repository, _httpContextAccessor, _bankService, _accountService);
    }

    private void SetupAuthorizedUser()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(principal);
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }

    private void SetupAccountExists()
    {
        var accountResponse = new AccountDetailResponse(
            AccountId,
            "Test Account",
            "Active",
            DateTime.UtcNow,
            null
        );
        _accountService.GetById(AccountId, UserId).Returns(accountResponse);
    }

    #region CreateDebitCard Tests

    [Fact]
    public void CreateDebitCard_WithValidRequest_ReturnsNewCardId()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _bankService.ValidateBankForUser(1, UserId).Returns(true);
        _repository.Add(Arg.Any<Card>()).Returns(1);

        // Act
        var result = _service.CreateDebitCard(request, AccountId, UserId);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Card>());
        _bankService.Received(1).ValidateBankForUser(1, UserId);
    }

    [Fact]
    public void CreateDebitCard_AccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.CreateDebitCard(request, AccountId, UserId));
    }

    [Fact]
    public void CreateDebitCard_NewCardIsActive()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _bankService.ValidateBankForUser(1, UserId).Returns(true);
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateDebitCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard.IsActive.Should().BeTrue();
        capturedCard.AccountId.Should().Be(AccountId);
    }

    [Fact]
    public void CreateDebitCard_WithCardName()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, "My Debit Card");
        _bankService.ValidateBankForUser(1, UserId).Returns(true);
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateDebitCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard.CardName.Should().Be("My Debit Card");
    }

    [Fact]
    public void CreateDebitCard_WithInactiveBankForUser_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _bankService.ValidateBankForUser(1, UserId).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _service.CreateDebitCard(request, AccountId, UserId));
        exception.Message.Should().Be("Bank is not valid.");
    }

    #endregion

    #region CreateCreditCard Tests

    [Fact]
    public void CreateCreditCard_WithValidRequest_ReturnsNewCardId()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _bankService.ValidateBankForUser(1, UserId).Returns(true);
        _repository.Add(Arg.Any<Card>()).Returns(1);

        // Act
        var result = _service.CreateCreditCard(request, AccountId, UserId);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Card>());
        _bankService.Received(1).ValidateBankForUser(1, UserId);
    }

    [Fact]
    public void CreateCreditCard_AccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.CreateCreditCard(request, AccountId, UserId));
    }

    [Fact]
    public void CreateCreditCard_CreatesCreditCardType()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _bankService.ValidateBankForUser(1, UserId).Returns(true);
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateCreditCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().BeOfType<CreditCard>();
        (capturedCard as CreditCard).ClosingDay.Should().Be(10);
        (capturedCard as CreditCard).DueDateDay.Should().Be(20);
        capturedCard.AccountId.Should().Be(AccountId);
    }

    [Fact]
    public void CreateCreditCard_WithCardName()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, "My Credit Card");
        _bankService.ValidateBankForUser(1, UserId).Returns(true);
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return 1;
            });

        // Act
        _service.CreateCreditCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard.CardName.Should().Be("My Credit Card");
    }

    [Fact]
    public void CreateCreditCard_WithInactiveBankForUser_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _bankService.ValidateBankForUser(1, UserId).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _service.CreateCreditCard(request, AccountId, UserId));
        exception.Message.Should().Be("Bank is not valid.");
        _repository.DidNotReceive().Add(Arg.Any<Card>());
    }

    #endregion

    #region GetById Tests

    [Fact]
    public void GetById_WithValidId_ReturnsCardDetail()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId, "My Card");
        _repository.GetById(cardId, AccountId).Returns(card);

        // Act
        var result = _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.CardType.Should().Be(CardType.Debit);
        result.CardNumber.Should().Be("****1111");
        result.CardName.Should().Be("My Card");
    }

    [Fact]
    public void GetById_AccountNotFound_ReturnsNull()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var cardId = 1;

        // Act
        var result = _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_WithInvalidCardId_ReturnsNull()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns((Card?)null);

        // Act
        var result = _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_InactiveCard_ReturnsInactiveStatus()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        card.SetIsActive(false);
        _repository.GetById(cardId, AccountId).Returns(card);

        // Act
        var result = _service.GetById(cardId, AccountId, UserId);

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
        SetupAccountExists();
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, 10, 20, CardType.Credit, cardId, "My Credit");
        _repository.GetById(cardId, AccountId).Returns(creditCard);

        // Act
        var result = _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.CardType.Should().Be(CardType.Credit);
        result.CardNumber.Should().Be("****4444");
        result.NextClosingDay.Should().NotBe(default(DateTime));
        result.NextDueDateDay.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void GetCreditById_AccountNotFound_ReturnsNull()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var cardId = 1;

        // Act
        var result = _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCreditById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns((Card?)null);

        // Act
        var result = _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCreditById_WithDebitCard_ReturnsNull()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var debitCard = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        _repository.GetById(cardId, AccountId).Returns(debitCard);

        // Act
        var result = _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCreditById_ComputesNextClosingDay()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var closingDay = 15;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, closingDay, 20, CardType.Credit, cardId);
        _repository.GetById(cardId, AccountId).Returns(creditCard);

        // Act
        var result = _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        var nextClosing = DateHelper.GetNextOccurrence(closingDay);
        result.NextClosingDay.Should().Be(nextClosing);
    }

    [Fact]
    public void GetCreditById_ComputesNextDueDay()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var dueDay = 25;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, 10, dueDay, CardType.Credit, cardId);
        _repository.GetById(cardId, AccountId).Returns(creditCard);

        // Act
        var result = _service.GetCreditById(cardId, AccountId, UserId);

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
        SetupAccountExists();
        var cards = new List<Card>
        {
            new("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, 1),
            new("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, CardType.Debit, 2)
        };
        _repository.GetAll(AccountId).Returns(cards);

        // Act
        var result = _service.GetAll(AccountId, UserId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetAll_AccountNotFound_ReturnsEmptyList()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);

        // Act
        var result = _service.GetAll(AccountId, UserId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WithNoCards_ReturnsEmptyList()
    {
        // Arrange
        SetupAccountExists();
        _repository.GetAll(AccountId).Returns(new List<Card>());

        // Act
        var result = _service.GetAll(AccountId, UserId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateCardName Tests

    [Fact]
    public void UpdateCardName_WithValidIdAndName_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId, "Old Name");
        var newName = "New Name";
        _repository.GetById(cardId, AccountId).Returns(card);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        var result = _service.UpdateCardName(cardId, AccountId, UserId, newName);

        // Assert
        result.Should().BeTrue();
        card.CardName.Should().Be("New Name");
        _repository.Received(1).Update(card, AccountId);
    }

    [Fact]
    public void UpdateCardName_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var cardId = 1;
        var newName = "New Name";

        // Act
        var result = _service.UpdateCardName(cardId, AccountId, UserId, newName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateCardName_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        var newName = "New Name";
        _repository.GetById(cardId, AccountId).Returns((Card?)null);

        // Act
        var result = _service.UpdateCardName(cardId, AccountId, UserId, newName);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    #endregion

    #region UpdateCreditCardDates Tests

    [Fact]
    public void UpdateCreditCardDates_WithValidId_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, 10, 20, CardType.Credit, cardId);
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(creditCard);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeTrue();
        creditCard.ClosingDay.Should().Be(15);
        creditCard.DueDateDay.Should().Be(25);
        _repository.Received(1).Update(creditCard, AccountId);
    }

    [Fact]
    public void UpdateCreditCardDates_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var cardId = 1;
        var request = new UpdateCreditCardRequest(15, 25);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateCreditCardDates_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns((Card?)null);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public void UpdateCreditCardDates_WithDebitCard_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var debitCard = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(debitCard);

        // Act
        var result = _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public void UpdateCreditCardDates_UpdatesTimestamp()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, 10, 20, CardType.Credit, cardId);
        var originalUpdatedAt = creditCard.UpdatedAt;
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(creditCard);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        creditCard.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        card.SetIsActive(false);
        _repository.GetById(cardId, AccountId).Returns(card);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        var result = _service.Activate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeTrue();
        card.IsActive.Should().BeTrue();
        _repository.Received(1).Update(card, AccountId);
    }

    [Fact]
    public void Activate_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var cardId = 1;

        // Act
        var result = _service.Activate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns((Card?)null);

        // Act
        var result = _service.Activate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public void Activate_UpdatesTimestamp()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        card.SetIsActive(false);
        var originalUpdatedAt = card.UpdatedAt;
        _repository.GetById(cardId, AccountId).Returns(card);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        _service.Activate(cardId, AccountId, UserId);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        _repository.GetById(cardId, AccountId).Returns(card);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        var result = _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeTrue();
        card.IsActive.Should().BeFalse();
        _repository.Received(1).Update(card, AccountId);
    }

    [Fact]
    public void Deactivate_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.GetById(AccountId, UserId).Returns((AccountDetailResponse?)null);
        var cardId = 1;

        // Act
        var result = _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns((Card?)null);

        // Act
        var result = _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public void Deactivate_UpdatesTimestamp()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        var originalUpdatedAt = card.UpdatedAt;
        _repository.GetById(cardId, AccountId).Returns(card);
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(true);

        // Act
        _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion
}

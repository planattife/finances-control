using System.Security.Claims;
using Xunit;
using NSubstitute;
using FluentAssertions;
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
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(true));
    }

    #region CreateDebitCard Tests

    [Fact]
    public async Task CreateDebitCard_WithValidRequest_ReturnsNewCardId()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(true));
        _repository.Add(Arg.Any<Card>()).Returns(Task.FromResult(1L));

        // Act
        var result = await _service.CreateDebitCard(request, AccountId, UserId);

        // Assert
        result.Should().Be(1);
        await _repository.Received(1).Add(Arg.Any<Card>());
        await _bankService.Received(1).ValidateBankForUser(1, UserId);
    }

    [Fact]
    public async Task CreateDebitCard_AccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateDebitCard(request, AccountId, UserId));
    }

    [Fact]
    public async Task CreateDebitCard_NewCardIsActive()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(true));
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateDebitCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard!.IsActive.Should().BeTrue();
        capturedCard!.AccountId.Should().Be(AccountId);
    }

    [Fact]
    public async Task CreateDebitCard_WithCardName()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, "My Debit Card");
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(true));
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateDebitCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard!.CardName.Should().Be("My Debit Card");
    }

    [Fact]
    public async Task CreateDebitCard_WithInactiveBankForUser_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(false));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateDebitCard(request, AccountId, UserId));
        exception.Message.Should().Be("Bank is not valid.");
    }

    #endregion

    #region CreateCreditCard Tests

    [Fact]
    public async Task CreateCreditCard_WithValidRequest_ReturnsNewCardId()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(true));
        _repository.Add(Arg.Any<Card>()).Returns(Task.FromResult(1L));

        // Act
        var result = await _service.CreateCreditCard(request, AccountId, UserId);

        // Assert
        result.Should().Be(1);
        await _repository.Received(1).Add(Arg.Any<Card>());
        await _bankService.Received(1).ValidateBankForUser(1, UserId);
    }

    [Fact]
    public async Task CreateCreditCard_AccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateCreditCard(request, AccountId, UserId));
    }

    [Fact]
    public async Task CreateCreditCard_CreatesCreditCardType()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(true));
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateCreditCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().BeOfType<CreditCard>();
        var capturedCreditCard = (capturedCard as CreditCard)!;
        capturedCreditCard.ClosingDay.Should().Be(10);
        capturedCreditCard.DueDateDay.Should().Be(20);
        capturedCreditCard.AccountId.Should().Be(AccountId);
    }

    [Fact]
    public async Task CreateCreditCard_WithCardName()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, "My Credit Card");
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(true));
        Card? capturedCard = null;
        _repository.Add(Arg.Any<Card>())
            .Returns(x =>
            {
                capturedCard = x.Arg<Card>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateCreditCard(request, AccountId, UserId);

        // Assert
        capturedCard.Should().NotBeNull();
        capturedCard!.CardName.Should().Be("My Credit Card");
    }

    [Fact]
    public async Task CreateCreditCard_WithInactiveBankForUser_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupAuthorizedUser();
        SetupAccountExists();
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _bankService.ValidateBankForUser(1, UserId).Returns(Task.FromResult(false));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateCreditCard(request, AccountId, UserId));
        exception.Message.Should().Be("Bank is not valid.");
        await _repository.DidNotReceive().Add(Arg.Any<Card>());
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsCardDetail()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = CreateCard(cardId, "My Card");
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));

        // Act
        var result = await _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(200);
        result.Errors.Should().BeEmpty();
        result.Data!.Should().NotBeNull();
        result.Data!.Id.Should().Be(cardId);
        result.Data!.CardType.Should().Be(CardType.Debit);
        result.Data!.CardNumber.Should().Be("****1111");
        result.Data!.CardName.Should().Be("My Card");
        result.Data!.Bank.Id.Should().Be(1);
        result.Data!.Bank.Name.Should().Be("Test Bank");
    }

    [Fact]
    public async Task GetById_AccountNotFound_ReturnsNull()
    {
        // Arrange
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var cardId = 1;

        // Act
        var result = await _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Account not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidCardId_ReturnsNull()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)null));

        // Act
        var result = await _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Card not found.");
    }

    [Fact]
    public async Task GetById_InactiveCard_ReturnsInactiveStatus()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = CreateCard(cardId);
        card.SetIsActive(false);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));

        // Act
        var result = await _service.GetById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(200);
        result.Data!.Should().NotBeNull();
        result.Data!.Status.Should().Be("Inactive");
    }

    #endregion

    #region GetCreditById Tests

    [Fact]
    public async Task GetCreditById_WithValidId_ReturnsCreditCardDetail()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var creditCard = CreateCreditCard(cardId, "My Credit");
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)creditCard));

        // Act
        var result = await _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(200);
        result.Errors.Should().BeEmpty();
        result.Data!.Should().NotBeNull();
        result.Data!.Id.Should().Be(cardId);
        result.Data!.CardType.Should().Be(CardType.Credit);
        result.Data!.CardNumber.Should().Be("****4444");
        result.Data!.Bank.Id.Should().Be(1);
        result.Data!.Bank.Name.Should().Be("Test Bank");
        result.Data!.NextClosingDay.Should().NotBe(default(DateTime));
        result.Data!.NextDueDateDay.Should().NotBe(default(DateTime));
    }

    [Fact]
    public async Task GetCreditById_AccountNotFound_ReturnsNull()
    {
        // Arrange
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var cardId = 1;

        // Act
        var result = await _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Account not found.");
    }

    [Fact]
    public async Task GetCreditById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)null));

        // Act
        var result = await _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Credit card not found.");
    }

    [Fact]
    public async Task GetCreditById_WithDebitCard_ReturnsNull()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var debitCard = CreateCard(cardId);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)debitCard));

        // Act
        var result = await _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Credit card not found.");
    }

    [Fact]
    public async Task GetCreditById_ComputesNextClosingDay()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var closingDay = 15;
        var creditCard = CreateCreditCard(cardId, closingDay: closingDay, dueDateDay: 20);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)creditCard));

        // Act
        var result = await _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        var nextClosing = DateHelper.GetNextOccurrence(closingDay);
        result.HttpStatusCode.Should().Be(200);
        result.Data!.Should().NotBeNull();
        result.Data!.NextClosingDay.Should().Be(nextClosing);
    }

    [Fact]
    public async Task GetCreditById_ComputesNextDueDay()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var dueDay = 25;
        var creditCard = CreateCreditCard(cardId, closingDay: 10, dueDateDay: dueDay);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)creditCard));

        // Act
        var result = await _service.GetCreditById(cardId, AccountId, UserId);

        // Assert
        result.Should().NotBeNull();
        var nextDue = DateHelper.GetNextOccurrence(dueDay);
        result.HttpStatusCode.Should().Be(200);
        result.Data!.Should().NotBeNull();
        result.Data!.NextDueDateDay.Should().Be(nextDue);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMultipleCards_ReturnsAllCards()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        SetupAccountExists();
        var cards = new List<Card>
        {
            CreateCard(1),
            CreateCard(2, cardNumber: "****4444", cardNetwork: CardNetwork.Mastercard)
        };
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(cards));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(cards.Count));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(200);
        result.Data!.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPrevPage.Should().BeFalse();
        result.Data[0].Bank!.Id.Should().Be(1);
        result.Data[0].Bank!.Name.Should().Be("Test Bank");
    }

    [Fact]
    public async Task GetAll_AccountNotFound_ReturnsEmptyList()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Account not found.");
        await _repository.DidNotReceive().GetAll(Arg.Any<long>(), Arg.Any<PaginationParams>());
        await _repository.DidNotReceive().CountByAccountId(Arg.Any<long>());
    }

    [Fact]
    public async Task GetAll_WithNoCards_ReturnsEmptyList()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(new List<Card>()));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(0));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        result.Should().NotBeNull();
        result!.HttpStatusCode.Should().Be(200);
        result.Data!.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WithPaginationParams_PassesPaginationParamsToRepository()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var cards = CreateCards(5, 6);
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(cards));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        await _repository.Received(1).GetAll(AccountId, paginationParams);
        await _repository.Received(1).CountByAccountId(AccountId);
        result.Data!.Should().HaveCount(5);
        result.Data[0].Id.Should().Be(6);
        result.Data[4].Id.Should().Be(10);
    }

    [Fact]
    public async Task GetAll_WithSecondPage_ReturnsCorrectPaginationMetadata()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateCards(5, 6)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeTrue();
        result.Data!.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAll_WithLastPage_ReturnsHasNextFalseAndHasPrevTrue()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 3 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateCards(2, 11)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(3);
        result.HasNextPage.Should().BeFalse();
        result.HasPrevPage.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithFirstPage_ReturnsHasPrevFalse()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 1 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateCards(5, 1)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        result.CurrentPage.Should().Be(1);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeFalse();
        result.Data!.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAll_WithInvalidPaginationParams_UsesDefaults()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 0, PageNumber = -1 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateCards(10, 1)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(AccountId, UserId, paginationParams);

        // Assert
        paginationParams.PageSize.Should().Be(10);
        paginationParams.PageNumber.Should().Be(1);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeFalse();
    }

    #endregion

    #region UpdateCardName Tests

    [Fact]
    public async Task UpdateCardName_WithValidIdAndName_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId, "Old Name");
        var newName = "New Name";
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        var result = await _service.UpdateCardName(cardId, AccountId, UserId, newName);

        // Assert
        result.Should().BeTrue();
        card.CardName.Should().Be("New Name");
        await _repository.Received(1).Update(card, AccountId);
    }

    [Fact]
    public async Task UpdateCardName_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var cardId = 1;
        var newName = "New Name";

        // Act
        var result = await _service.UpdateCardName(cardId, AccountId, UserId, newName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCardName_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        var newName = "New Name";
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)null));

        // Act
        var result = await _service.UpdateCardName(cardId, AccountId, UserId, newName);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    #endregion

    #region UpdateCreditCardDates Tests

    [Fact]
    public async Task UpdateCreditCardDates_WithValidId_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, 10, 20, CardType.Credit, cardId);
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)creditCard));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        var result = await _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeTrue();
        creditCard.ClosingDay.Should().Be(15);
        creditCard.DueDateDay.Should().Be(25);
        await _repository.Received(1).Update(creditCard, AccountId);
    }

    [Fact]
    public async Task UpdateCreditCardDates_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var cardId = 1;
        var request = new UpdateCreditCardRequest(15, 25);

        // Act
        var result = await _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCreditCardDates_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)null));

        // Act
        var result = await _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public async Task UpdateCreditCardDates_WithDebitCard_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var debitCard = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)debitCard));

        // Act
        var result = await _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public async Task UpdateCreditCardDates_UpdatesTimestamp()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var creditCard = new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, 10, 20, CardType.Credit, cardId);
        var originalUpdatedAt = creditCard.UpdatedAt;
        var request = new UpdateCreditCardRequest(15, 25);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)creditCard));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        await _service.UpdateCreditCardDates(cardId, AccountId, UserId, request);

        // Assert
        creditCard.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        card.SetIsActive(false);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Activate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeTrue();
        card.IsActive.Should().BeTrue();
        await _repository.Received(1).Update(card, AccountId);
    }

    [Fact]
    public async Task Activate_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var cardId = 1;

        // Act
        var result = await _service.Activate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)null));

        // Act
        var result = await _service.Activate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public async Task Activate_UpdatesTimestamp()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        card.SetIsActive(false);
        var originalUpdatedAt = card.UpdatedAt;
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        await _service.Activate(cardId, AccountId, UserId);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeTrue();
        card.IsActive.Should().BeFalse();
        await _repository.Received(1).Update(card, AccountId);
    }

    [Fact]
    public async Task Deactivate_AccountNotFound_ReturnsFalse()
    {
        // Arrange
        _accountService.ExistsAsync(AccountId, UserId).Returns(Task.FromResult(false));
        var cardId = 1;

        // Act
        var result = await _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 999;
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)null));

        // Act
        var result = await _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().Update(Arg.Any<Card>(), Arg.Any<long>());
    }

    [Fact]
    public async Task Deactivate_UpdatesTimestamp()
    {
        // Arrange
        SetupAccountExists();
        var cardId = 1;
        var card = new Card("****1111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, AccountId, CardType.Debit, cardId);
        var originalUpdatedAt = card.UpdatedAt;
        _repository.GetById(cardId, AccountId).Returns(Task.FromResult((Card?)card));
        _repository.Update(Arg.Any<Card>(), AccountId).Returns(Task.FromResult(true));

        // Act
        await _service.Deactivate(cardId, AccountId, UserId);

        // Assert
        card.UpdatedAt.Should().NotBe(originalUpdatedAt ?? DateTime.MinValue);
    }

    #endregion

    private static List<Card> CreateCards(int count, int startId)
    {
        return Enumerable.Range(startId, count)
            .Select(id => CreateCard(id))
            .ToList();
    }

    private static Card CreateCard(
        long id,
        string? cardName = null,
        CardNetwork cardNetwork = CardNetwork.Visa,
        string cardNumber = "****1111")
    {
        return new Card(cardNumber, DateTime.UtcNow.AddYears(5), cardNetwork, 1, AccountId, CardType.Debit, id, cardName, "Test Bank");
    }

    private static CreditCard CreateCreditCard(
        long id,
        string? cardName = null,
        int closingDay = 10,
        int dueDateDay = 20)
    {
        return new CreditCard("****4444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, AccountId, closingDay, dueDateDay, CardType.Credit, id, cardName, "Test Bank");
    }
}

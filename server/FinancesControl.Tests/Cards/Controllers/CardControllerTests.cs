using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using FinancesControl.API.Cards.Controllers;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Enums;
using System.Security.Claims;

namespace FinancesControl.Tests.Cards.Controllers;

public class CardControllerTests
{
    private readonly ICardService _cardService;
    private readonly CardController _controller;
    private const long AccountId = 1;
    private const string UserId = "test-user-id";

    public CardControllerTests()
    {
        _cardService = Substitute.For<ICardService>();
        _controller = new CardController(_cardService);
        SetupAuthorizedUser();
    }

    #region CreateDebitCard Tests

    [Fact]
    public async Task CreateDebitCard_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        long cardId = 1;
        _cardService.CreateDebitCard(request, AccountId, UserId).Returns(cardId);

        // Act
        var result = await _controller.CreateDebitCard(AccountId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues["accountId"].Should().Be(AccountId);
        createdResult.RouteValues["cardId"].Should().Be(cardId);
        _cardService.Received(1).CreateDebitCard(request, AccountId, UserId);
    }

    [Fact]
    public async Task CreateDebitCard_WithCardName_Returns201Created()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, "My Debit Card");
        _cardService.CreateDebitCard(request, AccountId, UserId).Returns(1);

        // Act
        var result = await _controller.CreateDebitCard(AccountId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _cardService.Received(1).CreateDebitCard(request, AccountId, UserId);
    }

    [Fact]
    public async Task CreateDebitCard_AccountNotFound_Returns404NotFound()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _cardService.When(x => x.CreateDebitCard(request, AccountId, UserId)).Do(x => throw new InvalidOperationException("Account not found"));

        // Act
        var result = await _controller.CreateDebitCard(AccountId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region CreateCreditCard Tests

    [Fact]
    public async Task CreateCreditCard_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        long cardId = 1;
        _cardService.CreateCreditCard(request, AccountId, UserId).Returns(cardId);

        // Act
        var result = await _controller.CreateCreditCard(AccountId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.ActionName.Should().Be(nameof(_controller.GetCreditById));
        createdResult.RouteValues["accountId"].Should().Be(AccountId);
        createdResult.RouteValues["cardId"].Should().Be(cardId);
        _cardService.Received(1).CreateCreditCard(request, AccountId, UserId);
    }

    [Fact]
    public async Task CreateCreditCard_WithCardName_Returns201Created()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, "My Credit Card");
        _cardService.CreateCreditCard(request, AccountId, UserId).Returns(1);

        // Act
        var result = await _controller.CreateCreditCard(AccountId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _cardService.Received(1).CreateCreditCard(request, AccountId, UserId);
    }

    [Fact]
    public async Task CreateCreditCard_AccountNotFound_Returns404NotFound()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _cardService.When(x => x.CreateCreditCard(request, AccountId, UserId)).Do(x => throw new InvalidOperationException("Account not found"));

        // Act
        var result = await _controller.CreateCreditCard(AccountId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_Returns200Ok()
    {
        // Arrange
        var cardId = 1;
        var response = new CardDetailResponse(
            cardId,
            "****1111",
            CardType.Debit,
            CardNetwork.Visa,
            DateTime.UtcNow.AddYears(5),
            1,
            "My Debit Card",
            "Active",
            DateTime.UtcNow,
            null
        );
        _cardService.GetById(cardId, AccountId, UserId).Returns(response);

        // Act
        var result = await _controller.GetById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(response);
        _cardService.Received(1).GetById(cardId, AccountId, UserId);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.GetById(cardId, AccountId, UserId).Returns((CardDetailResponse?)null);

        // Act
        var result = await _controller.GetById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).GetById(cardId, AccountId, UserId);
    }

    #endregion

    #region GetCreditById Tests

    [Fact]
    public async Task GetCreditById_WithValidId_Returns200Ok()
    {
        // Arrange
        var cardId = 1;
        var nextClosing = DateTime.UtcNow.AddDays(5);
        var nextDue = DateTime.UtcNow.AddDays(15);
        var response = new CreditCardDetailResponse(
            cardId,
            "****4444",
            CardType.Credit,
            CardNetwork.Mastercard,
            DateTime.UtcNow.AddYears(5),
            1,
            "My Credit Card",
            nextClosing,
            nextDue,
            "Active",
            DateTime.UtcNow,
            null
        );
        _cardService.GetCreditById(cardId, AccountId, UserId).Returns(response);

        // Act
        var result = await _controller.GetCreditById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(response);
        _cardService.Received(1).GetCreditById(cardId, AccountId, UserId);
    }

    [Fact]
    public async Task GetCreditById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.GetCreditById(cardId, AccountId, UserId).Returns((CreditCardDetailResponse?)null);

        // Act
        var result = await _controller.GetCreditById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).GetCreditById(cardId, AccountId, UserId);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithMultipleCards_Returns200Ok()
    {
        // Arrange
        var cards = new List<CardListItemResponse>
        {
            new(1, "****1111", CardType.Debit, CardNetwork.Visa, 1, "Debit"),
            new(2, "****4444", CardType.Credit, CardNetwork.Mastercard, 1, "Credit")
        };
        _cardService.GetAll(AccountId, UserId).Returns(cards);

        // Act
        var result = await _controller.Get(AccountId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedList = okResult.Value as List<CardListItemResponse>;
        returnedList.Should().HaveCount(2);
        returnedList[0].Id.Should().Be(1);
        returnedList[0].CardType.Should().Be(CardType.Debit);
        returnedList[1].Id.Should().Be(2);
        returnedList[1].CardType.Should().Be(CardType.Credit);
        _cardService.Received(1).GetAll(AccountId, UserId);
    }

    [Fact]
    public async Task Get_WithNoCards_Returns200OkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<CardListItemResponse>();
        _cardService.GetAll(AccountId, UserId).Returns(emptyList);

        // Act
        var result = await _controller.Get(AccountId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedList = okResult.Value as List<CardListItemResponse>;
        returnedList.Should().BeEmpty();
    }

    #endregion

    #region UpdateCardName Tests

    [Fact]
    public async Task UpdateCardName_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        var newName = "Updated Card Name";
        _cardService.UpdateCardName(cardId, AccountId, UserId, newName).Returns(true);

        // Act
        var result = await _controller.UpdateCardName(AccountId, cardId, newName);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).UpdateCardName(cardId, AccountId, UserId, newName);
    }

    [Fact]
    public async Task UpdateCardName_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        var newName = "Updated Card Name";
        _cardService.UpdateCardName(cardId, AccountId, UserId, newName).Returns(false);

        // Act
        var result = await _controller.UpdateCardName(AccountId, cardId, newName);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).UpdateCardName(cardId, AccountId, UserId, newName);
    }

    #endregion

    #region UpdateCreditCardDates Tests

    [Fact]
    public async Task UpdateCreditCardDates_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        var request = new UpdateCreditCardRequest(15, 25);
        _cardService.UpdateCreditCardDates(cardId, AccountId, UserId, request).Returns(true);

        // Act
        var result = await _controller.UpdateCreditCardDates(AccountId, cardId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).UpdateCreditCardDates(cardId, AccountId, UserId, request);
    }

    [Fact]
    public async Task UpdateCreditCardDates_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        var request = new UpdateCreditCardRequest(15, 25);
        _cardService.UpdateCreditCardDates(cardId, AccountId, UserId, request).Returns(false);

        // Act
        var result = await _controller.UpdateCreditCardDates(AccountId, cardId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).UpdateCreditCardDates(cardId, AccountId, UserId, request);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        _cardService.Activate(cardId, AccountId, UserId).Returns(true);

        // Act
        var result = await _controller.Activate(AccountId, cardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).Activate(cardId, AccountId, UserId);
    }

    [Fact]
    public async Task Activate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.Activate(cardId, AccountId, UserId).Returns(false);

        // Act
        var result = await _controller.Activate(AccountId, cardId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).Activate(cardId, AccountId, UserId);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        _cardService.Deactivate(cardId, AccountId, UserId).Returns(true);

        // Act
        var result = await _controller.Deactivate(AccountId, cardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).Deactivate(cardId, AccountId, UserId);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.Deactivate(cardId, AccountId, UserId).Returns(false);

        // Act
        var result = await _controller.Deactivate(AccountId, cardId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).Deactivate(cardId, AccountId, UserId);
    }

    #endregion

    #region Helper Methods

    private void SetupAuthorizedUser()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, UserId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(principal);
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    #endregion
}

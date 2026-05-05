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
        _cardService.CreateDebitCard(request).Returns(cardId);

        // Act
        var result = await _controller.CreateDebitCard(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues["cardId"].Should().Be(cardId);
        _cardService.Received(1).CreateDebitCard(request);
    }

    [Fact]
    public async Task CreateDebitCard_WithCardName_Returns201Created()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1, "My Debit Card");
        _cardService.CreateDebitCard(request).Returns(1);

        // Act
        var result = await _controller.CreateDebitCard(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _cardService.Received(1).CreateDebitCard(request);
    }

    #endregion

    #region CreateCreditCard Tests

    [Fact]
    public async Task CreateCreditCard_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        long cardId = 1;
        _cardService.CreateCreditCard(request).Returns(cardId);

        // Act
        var result = await _controller.CreateCreditCard(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.ActionName.Should().Be(nameof(_controller.GetCreditById));
        createdResult.RouteValues["cardId"].Should().Be(cardId);
        _cardService.Received(1).CreateCreditCard(request);
    }

    [Fact]
    public async Task CreateCreditCard_WithCardName_Returns201Created()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20, "My Credit Card");
        _cardService.CreateCreditCard(request).Returns(1);

        // Act
        var result = await _controller.CreateCreditCard(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _cardService.Received(1).CreateCreditCard(request);
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
        _cardService.GetById(cardId).Returns(response);

        // Act
        var result = await _controller.GetById(cardId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(response);
        _cardService.Received(1).GetById(cardId);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.GetById(cardId).Returns((CardDetailResponse?)null);

        // Act
        var result = await _controller.GetById(cardId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).GetById(cardId);
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
        _cardService.GetCreditById(cardId).Returns(response);

        // Act
        var result = await _controller.GetCreditById(cardId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(response);
        _cardService.Received(1).GetCreditById(cardId);
    }

    [Fact]
    public async Task GetCreditById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.GetCreditById(cardId).Returns((CreditCardDetailResponse?)null);

        // Act
        var result = await _controller.GetCreditById(cardId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).GetCreditById(cardId);
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
        _cardService.GetAll().Returns(cards);

        // Act
        var result = await _controller.Get();

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
        _cardService.Received(1).GetAll();
    }

    [Fact]
    public async Task Get_WithNoCards_Returns200OkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<CardListItemResponse>();
        _cardService.GetAll().Returns(emptyList);

        // Act
        var result = await _controller.Get();

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
        _cardService.UpdateCardName(cardId, newName).Returns(true);

        // Act
        var result = await _controller.UpdateCardName(cardId, newName);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).UpdateCardName(cardId, newName);
    }

    [Fact]
    public async Task UpdateCardName_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        var newName = "Updated Card Name";
        _cardService.UpdateCardName(cardId, newName).Returns(false);

        // Act
        var result = await _controller.UpdateCardName(cardId, newName);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).UpdateCardName(cardId, newName);
    }

    #endregion

    #region UpdateCreditCardDates Tests

    [Fact]
    public async Task UpdateCreditCardDates_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        var request = new UpdateCreditCardRequest(15, 25);
        _cardService.UpdateCreditCardDates(cardId, request).Returns(true);

        // Act
        var result = await _controller.UpdateCreditCardDates(cardId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).UpdateCreditCardDates(cardId, request);
    }

    [Fact]
    public async Task UpdateCreditCardDates_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        var request = new UpdateCreditCardRequest(15, 25);
        _cardService.UpdateCreditCardDates(cardId, request).Returns(false);

        // Act
        var result = await _controller.UpdateCreditCardDates(cardId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).UpdateCreditCardDates(cardId, request);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        _cardService.Activate(cardId).Returns(true);

        // Act
        var result = await _controller.Activate(cardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).Activate(cardId);
    }

    [Fact]
    public async Task Activate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.Activate(cardId).Returns(false);

        // Act
        var result = await _controller.Activate(cardId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).Activate(cardId);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var cardId = 1;
        _cardService.Deactivate(cardId).Returns(true);

        // Act
        var result = await _controller.Deactivate(cardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cardService.Received(1).Deactivate(cardId);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        _cardService.Deactivate(cardId).Returns(false);

        // Act
        var result = await _controller.Deactivate(cardId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _cardService.Received(1).Deactivate(cardId);
    }

    #endregion

    #region Helper Methods

    private void SetupAuthorizedUser()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #endregion
}

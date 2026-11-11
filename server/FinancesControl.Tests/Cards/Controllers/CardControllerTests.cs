using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using FinancesControl.API.Cards.Controllers;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Common;
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
        var createdResult = (result as CreatedAtActionResult)!;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues!["accountId"].Should().Be(AccountId);
        createdResult.RouteValues!["cardId"].Should().Be(cardId);
        await _cardService.Received(1).CreateDebitCard(request, AccountId, UserId);
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
        await _cardService.Received(1).CreateDebitCard(request, AccountId, UserId);
    }

    [Fact]
    public async Task CreateDebitCard_AccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateDebitCardRequest("4111111111111111", DateTime.UtcNow.AddYears(5), CardNetwork.Visa, 1);
        _cardService.When(x => x.CreateDebitCard(request, AccountId, UserId)).Do(x => throw new ResourceNotFoundException("Account not found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _controller.CreateDebitCard(AccountId, request));
        Assert.Equal("Account not found", ex.Message);
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
        var createdResult = (result as CreatedAtActionResult)!;
        createdResult.ActionName.Should().Be(nameof(_controller.GetCreditById));
        createdResult.RouteValues!["accountId"].Should().Be(AccountId);
        createdResult.RouteValues!["cardId"].Should().Be(cardId);
        await _cardService.Received(1).CreateCreditCard(request, AccountId, UserId);
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
        await _cardService.Received(1).CreateCreditCard(request, AccountId, UserId);
    }

    [Fact]
    public async Task CreateCreditCard_AccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateCreditCardRequest("5555555555554444", DateTime.UtcNow.AddYears(5), CardNetwork.Mastercard, 1, 10, 20);
        _cardService.When(x => x.CreateCreditCard(request, AccountId, UserId)).Do(x => throw new ResourceNotFoundException("Account not found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _controller.CreateCreditCard(AccountId, request));
        Assert.Equal("Account not found", ex.Message);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_Returns200Ok()
    {
        // Arrange
        var cardId = 1;
        var detail = new CardDetailResponse(
            cardId,
            "****1111",
            CardType.Debit,
            CardNetwork.Visa,
            DateTime.UtcNow.AddYears(5),
            new CardBankResponse(1, "Test Bank"),
            "My Debit Card",
            "Active",
            DateTime.UtcNow,
            null
        );
        var response = new SingleResponse<CardDetailResponse> { Data = detail };
        response.SetHttpStatusCode(200);
        _cardService.GetById(cardId, AccountId, UserId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (result.Result as ObjectResult)!;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(response);
        await _cardService.Received(1).GetById(cardId, AccountId, UserId);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        var response = new SingleResponse<CardDetailResponse>();
        response.AddError("Card not found.");
        response.SetHttpStatusCode(404);
        _cardService.GetById(cardId, AccountId, UserId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (result.Result as ObjectResult)!;
        objectResult.StatusCode.Should().Be(404);
        objectResult.Value.Should().Be(response);
        await _cardService.Received(1).GetById(cardId, AccountId, UserId);
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
        var detail = new CreditCardDetailResponse(
            cardId,
            "****4444",
            CardType.Credit,
            CardNetwork.Mastercard,
            DateTime.UtcNow.AddYears(5),
            new CardBankResponse(1, "Test Bank"),
            "My Credit Card",
            nextClosing,
            nextDue,
            "Active",
            DateTime.UtcNow,
            null
        );
        var response = new SingleResponse<CreditCardDetailResponse> { Data = detail };
        response.SetHttpStatusCode(200);
        _cardService.GetCreditById(cardId, AccountId, UserId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetCreditById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (result.Result as ObjectResult)!;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(response);
        await _cardService.Received(1).GetCreditById(cardId, AccountId, UserId);
    }

    [Fact]
    public async Task GetCreditById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var cardId = 999;
        var response = new SingleResponse<CreditCardDetailResponse>();
        response.AddError("Credit card not found.");
        response.SetHttpStatusCode(404);
        _cardService.GetCreditById(cardId, AccountId, UserId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetCreditById(AccountId, cardId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (result.Result as ObjectResult)!;
        objectResult.StatusCode.Should().Be(404);
        objectResult.Value.Should().Be(response);
        await _cardService.Received(1).GetCreditById(cardId, AccountId, UserId);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithMultipleCards_Returns200Ok()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        var cards = new List<CardListItemResponse>
        {
            new(1, "****1111", CardType.Debit, CardNetwork.Visa, new CardBankResponse(1, "Test Bank"), "Debit", "Active"),
            new(2, "****4444", CardType.Credit, CardNetwork.Mastercard, new CardBankResponse(1, "Test Bank"), "Credit", "Inactive")
        };
        var response = new PaginatedResponse<CardListItemResponse> { Data = cards };
        response.SetPagination(cards.Count, 1, 1, false, false);
        response.SetHttpStatusCode(200);
        _cardService.GetAll(AccountId, UserId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(AccountId, paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (result.Result as OkObjectResult)!;
        okResult.StatusCode.Should().Be(200);
        var returnedResponse = (okResult.Value as PaginatedResponse<CardListItemResponse>)!;
        returnedResponse.Data!.Should().HaveCount(2);
        returnedResponse.Data[0].Id.Should().Be(1);
        returnedResponse.Data[0].CardType.Should().Be(CardType.Debit);
        returnedResponse.Data[0].Bank!.Id.Should().Be(1);
        returnedResponse.Data[0].Bank!.Name.Should().Be("Test Bank");
        returnedResponse.Data[0].Status.Should().Be("Active");
        returnedResponse.Data[1].Id.Should().Be(2);
        returnedResponse.Data[1].CardType.Should().Be(CardType.Credit);
        returnedResponse.Data[1].Status.Should().Be("Inactive");
        await _cardService.Received(1).GetAll(AccountId, UserId, paginationParams);
    }

    [Fact]
    public async Task Get_WithNoCards_Returns200OkWithEmptyList()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        var emptyList = new List<CardListItemResponse>();
        var response = new PaginatedResponse<CardListItemResponse> { Data = emptyList };
        response.SetPagination(0, 0, 1, false, false);
        response.SetHttpStatusCode(200);
        _cardService.GetAll(AccountId, UserId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(AccountId, paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (result.Result as OkObjectResult)!;
        okResult.StatusCode.Should().Be(200);
        var returnedResponse = (okResult.Value as PaginatedResponse<CardListItemResponse>)!;
        returnedResponse.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_WithPaginationParams_PassesPaginationParamsToService()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<CardListItemResponse>
        {
            Data = new List<CardListItemResponse>
            {
                new(6, "****1111", CardType.Debit, CardNetwork.Visa, new CardBankResponse(1, "Test Bank"), "Debit", "Active")
            }
        };
        response.SetPagination(12, 3, 2, true, true);
        response.SetHttpStatusCode(200);
        _cardService.GetAll(AccountId, UserId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(AccountId, paginationParams);

        // Assert
        var okResult = (result.Result as OkObjectResult)!;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(response);
        await _cardService.Received(1).GetAll(AccountId, UserId, paginationParams);
    }

    [Fact]
    public async Task Get_WithPaginationResponse_ReturnsPaginationMetadata()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<CardListItemResponse>
        {
            Data = new List<CardListItemResponse>
            {
                new(6, "****1111", CardType.Debit, CardNetwork.Visa, new CardBankResponse(1, "Test Bank"), "Debit", "Active")
            }
        };
        response.SetPagination(12, 3, 2, true, true);
        response.SetHttpStatusCode(200);
        _cardService.GetAll(AccountId, UserId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(AccountId, paginationParams);

        // Assert
        var okResult = (result.Result as OkObjectResult)!;
        okResult.Should().NotBeNull();
        var returnedResponse = (okResult.Value as PaginatedResponse<CardListItemResponse>)!;
        returnedResponse.Should().NotBeNull();
        returnedResponse!.TotalCount.Should().Be(12);
        returnedResponse.TotalPages.Should().Be(3);
        returnedResponse.CurrentPage.Should().Be(2);
        returnedResponse.HasNextPage.Should().BeTrue();
        returnedResponse.HasPrevPage.Should().BeTrue();
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
        await _cardService.Received(1).UpdateCardName(cardId, AccountId, UserId, newName);
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
        await _cardService.Received(1).UpdateCardName(cardId, AccountId, UserId, newName);
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
        await _cardService.Received(1).UpdateCreditCardDates(cardId, AccountId, UserId, request);
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
        await _cardService.Received(1).UpdateCreditCardDates(cardId, AccountId, UserId, request);
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
        await _cardService.Received(1).Activate(cardId, AccountId, UserId);
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
        await _cardService.Received(1).Activate(cardId, AccountId, UserId);
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
        await _cardService.Received(1).Deactivate(cardId, AccountId, UserId);
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
        await _cardService.Received(1).Deactivate(cardId, AccountId, UserId);
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

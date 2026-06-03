using System;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using FinancesControl.API.Banks.Controllers;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Enums;
using FinancesControl.API.Common;
using System.Security.Claims;

namespace FinancesControl.Tests.Banks.Controllers;

public class BankControllerTests
{
    private readonly IBankService _bankService;
    private readonly BankController _controller;

    public BankControllerTests()
    {
        _bankService = Substitute.For<IBankService>();
        _controller = new BankController(_bankService);
        SetupAuthorizedUser();
    }

    #region CreateBank Tests

    [Fact]
    public async Task CreateBank_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateBankRequest("Test Bank", BankAccountType.Check);
        long bankId = 1;
        _bankService.CreateBank(request).Returns(Task.FromResult(bankId));

        // Act
        var result = await _controller.CreateBank(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues!["bankId"].Should().Be(bankId);
        _bankService.Received(1).CreateBank(request);
    }

    [Fact]
    public async Task CreateBank_WithValidSavingsAccount_Returns201Created()
    {
        // Arrange
        var request = new CreateBankRequest("Savings Bank", BankAccountType.Savings);
        var bankId = 1;
        _bankService.CreateBank(request).Returns(Task.FromResult((long)bankId));

        // Act
        var result = await _controller.CreateBank(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _bankService.Received(1).CreateBank(request);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_Returns200Ok()
    {
        // Arrange
        var bankId = 1;
        var userId = "test-user-id";
        var response = new SingleResponse<BankDetailResponse>
        {
            Data = new BankDetailResponse(
                bankId,
                "Test Bank",
                BankAccountType.Check,
                "Active",
                DateTime.UtcNow,
                null
            )
        };
        response.SetHttpStatusCode(200);
        _bankService.GetById(bankId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(bankId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(response);
        _bankService.Received(1).GetById(bankId, userId);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        var userId = "test-user-id";
        var response = new SingleResponse<BankDetailResponse>();
        response.Data = null;
        response.AddError("Bank not found.");
        response.SetHttpStatusCode(404);
        _bankService.GetById(bankId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(bankId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(404);
        _bankService.Received(1).GetById(bankId, userId);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithMultipleBanks_Returns200Ok()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams();
        var banks = new List<BankListItemResponse>
        {
            new(1, "Bank 1", BankAccountType.Check),
            new(2, "Bank 2", BankAccountType.Savings)
        };
        var response = new PaginatedResponse<BankListItemResponse>
        {
            Data = banks,
            TotalCount = 2,
            TotalPages = 1,
            CurrentPage = 1,
            HasNextPage = false,
            HasPrevPage = false
        };
        _bankService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedResponse = okResult.Value as PaginatedResponse<BankListItemResponse>;
        returnedResponse.Data.Should().HaveCount(2);
        returnedResponse.Data[0].Id.Should().Be(1);
        returnedResponse.Data[0].Name.Should().Be("Bank 1");
        returnedResponse.Data[1].Id.Should().Be(2);
        returnedResponse.Data[1].Name.Should().Be("Bank 2");
        _bankService.Received(1).GetAll(userId, paginationParams);
    }

    [Fact]
    public async Task Get_WithNoBanks_Returns200OkWithEmptyList()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams();
        var response = new PaginatedResponse<BankListItemResponse>
        {
            Data = new List<BankListItemResponse>(),
            TotalCount = 0,
            TotalPages = 0,
            CurrentPage = 1,
            HasNextPage = false,
            HasPrevPage = false
        };
        _bankService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedResponse = okResult.Value as PaginatedResponse<BankListItemResponse>;
        returnedResponse.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_WithPaginationParams_PassesPaginationParamsToService()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<BankListItemResponse>
        {
            Data = new List<BankListItemResponse>
            {
                new(6, "Bank 6", BankAccountType.Check)
            }
        };
        response.SetPagination(12, 3, 2, true, true);
        response.SetHttpStatusCode(200);
        _bankService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().Be(response);
        _bankService.Received(1).GetAll(userId, paginationParams);
    }

    [Fact]
    public async Task Get_WithPaginationResponse_ReturnsPaginationMetadata()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<BankListItemResponse>
        {
            Data = new List<BankListItemResponse>
            {
                new(6, "Bank 6", BankAccountType.Check)
            }
        };
        response.SetPagination(12, 3, 2, true, true);
        response.SetHttpStatusCode(200);
        _bankService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedResponse = okResult.Value as PaginatedResponse<BankListItemResponse>;
        returnedResponse.Should().NotBeNull();
        returnedResponse.TotalCount.Should().Be(12);
        returnedResponse.TotalPages.Should().Be(3);
        returnedResponse.CurrentPage.Should().Be(2);
        returnedResponse.HasNextPage.Should().BeTrue();
        returnedResponse.HasPrevPage.Should().BeTrue();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_Returns204NoContent()
    {
        // Arrange
        var bankId = 1;
        var userId = "test-user-id";
        var request = new UpdateBankRequest("Updated Bank", BankAccountType.Savings);
        _bankService.Update(bankId, request, userId).Returns(true);

        // Act
        var result = await _controller.Update(bankId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bankService.Received(1).Update(bankId, request, userId);
    }

    [Fact]
    public async Task Update_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        var userId = "test-user-id";
        var request = new UpdateBankRequest("Updated Bank", BankAccountType.Savings);
        _bankService.Update(bankId, request, userId).Returns(false);

        // Act
        var result = await _controller.Update(bankId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).Update(bankId, request, userId);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var bankId = 1;
        var userId = "test-user-id";
        _bankService.Activate(bankId, userId).Returns(true);

        // Act
        var result = await _controller.Activate(bankId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bankService.Received(1).Activate(bankId, userId);
    }

    [Fact]
    public async Task Activate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        var userId = "test-user-id";
        _bankService.Activate(bankId, userId).Returns(false);

        // Act
        var result = await _controller.Activate(bankId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).Activate(bankId, userId);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var bankId = 1;
        var userId = "test-user-id";
        _bankService.Deactivate(bankId, userId).Returns(true);

        // Act
        var result = await _controller.Deactivate(bankId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bankService.Received(1).Deactivate(bankId, userId);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        var userId = "test-user-id";
        _bankService.Deactivate(bankId, userId).Returns(false);

        // Act
        var result = await _controller.Deactivate(bankId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).Deactivate(bankId, userId);
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

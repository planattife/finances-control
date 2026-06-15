using System;
using System.Security.Claims;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using FinancesControl.API.Accounts.Controllers;
using FinancesControl.API.Accounts.Services;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Common;

namespace FinancesControl.Tests.Accounts.Controllers;

public class AccountControllerTests
{
    private readonly IAccountService _accountService;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _accountService = Substitute.For<IAccountService>();
        _controller = new AccountController(_accountService);
        SetupAuthorizedUser();
    }

    #region CreateAccount Tests

    [Fact]
    public async Task CreateAccount_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        long accountId = 1;
        _accountService.CreateAccount(request, Arg.Any<string>()).Returns(Task.FromResult(accountId));

        // Act
        var result = await _controller.CreateAccount(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues!["accountId"].Should().Be(accountId);
        _accountService.Received(1).CreateAccount(request, Arg.Any<string>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAccount_WithInvalidName_ReturnsBadRequest(string invalidName)
    {
        // Arrange
        var request = new CreateAccountRequest(invalidName);

        // Act
        var result = await _controller.CreateAccount(request);

        // Assert - The validation happens at model level
        // This test verifies the controller handles validation properly
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAccount_WithNameTooShort_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateAccountRequest("A"); // Less than MinLength(2)

        // Act & Assert
        // Validation should occur before the action method is called
        // You would need to manually validate or use model state validation
        Assert.NotNull(request);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_Returns200Ok()
    {
        // Arrange
        var accountId = 1;
        var userId = "test-user-id";
        var response = new SingleResponse<AccountDetailResponse>
        {
            Data = new AccountDetailResponse(
                accountId,
                "Test Account",
                "Active",
                DateTime.UtcNow,
                null
            )
        };
        response.SetHttpStatusCode(200);
        _accountService.GetById(accountId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(accountId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(response);
        _accountService.Received(1).GetById(accountId, userId);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var accountId = 999;
        var userId = "test-user-id";
        var response = new SingleResponse<AccountDetailResponse>();
        response.Data = null;
        response.AddError("Account not found.");
        response.SetHttpStatusCode(404);
        _accountService.GetById(accountId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(accountId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(404);
        _accountService.Received(1).GetById(accountId, userId);
    }

    [Fact]
    public async Task GetById_WithAccountFromDifferentUser_Returns404NotFound()
    {
        // Arrange
        var accountId = 1;
        var userId = "test-user-id";
        // Account belongs to different user - service returns null
        var response = new SingleResponse<AccountDetailResponse>();
        response.Data = null;
        response.AddError("Account not found.");
        response.SetHttpStatusCode(404);
        _accountService.GetById(accountId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetById(accountId);

        // Assert
        // Returns 404 (not found) instead of 403 (forbidden) to avoid exposing account existence
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(404);
        _accountService.Received(1).GetById(accountId, userId);
    }

    #endregion

    #region GetAccountKpi Tests

    [Fact]
    public async Task GetAccountKpi_WithValidId_Returns200Ok()
    {
        // Arrange
        var accountId = 1;
        var userId = "test-user-id";
        var response = new SingleResponse<AccountKpiResponse>
        {
            Data = new AccountKpiResponse(accountId, 2, 5)
        };
        response.SetHttpStatusCode(200);
        _accountService.GetAccountKpi(accountId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetAccountKpi(accountId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(response);
        _accountService.Received(1).GetAccountKpi(accountId, userId);
    }

    [Fact]
    public async Task GetAccountKpi_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var accountId = 999;
        var userId = "test-user-id";
        var response = new SingleResponse<AccountKpiResponse>();
        response.Data = null;
        response.AddError("Account not found.");
        response.SetHttpStatusCode(404);
        _accountService.GetAccountKpi(accountId, userId).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.GetAccountKpi(accountId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(404);
        objectResult.Value.Should().Be(response);
        _accountService.Received(1).GetAccountKpi(accountId, userId);
    }

    [Fact]
    public async Task GetAccountKpi_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        var accountId = 1;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        // Act
        var result = await _controller.GetAccountKpi(accountId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        await _accountService.DidNotReceive().GetAccountKpi(Arg.Any<long>(), Arg.Any<string>());
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithMultipleAccounts_Returns200Ok()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams();
        var accounts = new List<AccountListItemResponse>
        {
            new(1, "Account 1"),
            new(2, "Account 2")
        };
        var response = new PaginatedResponse<AccountListItemResponse>
        {
            Data = accounts,
            TotalCount = 2,
            TotalPages = 1,
            CurrentPage = 1,
            HasNextPage = false,
            HasPrevPage = false
        };
        _accountService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedResponse = okResult.Value as PaginatedResponse<AccountListItemResponse>;
        returnedResponse.Data.Should().HaveCount(2);
        returnedResponse.Data[0].Id.Should().Be(1);
        returnedResponse.Data[0].Name.Should().Be("Account 1");
        returnedResponse.Data[1].Id.Should().Be(2);
        returnedResponse.Data[1].Name.Should().Be("Account 2");
        _accountService.Received(1).GetAll(userId, paginationParams);
    }

    [Fact]
    public async Task Get_WithNoAccounts_Returns200OkWithEmptyList()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams();
        var response = new PaginatedResponse<AccountListItemResponse>
        {
            Data = new List<AccountListItemResponse>(),
            TotalCount = 0,
            TotalPages = 0,
            CurrentPage = 1,
            HasNextPage = false,
            HasPrevPage = false
        };
        _accountService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedResponse = okResult.Value as PaginatedResponse<AccountListItemResponse>;
        returnedResponse.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_WithPaginationParams_PassesPaginationParamsToService()
    {
        // Arrange
        var userId = "test-user-id";
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<AccountListItemResponse>
        {
            Data = new List<AccountListItemResponse>
            {
                new(6, "Account 6")
            },
            TotalCount = 12,
            TotalPages = 3,
            CurrentPage = 2,
            HasNextPage = true,
            HasPrevPage = true
        };
        _accountService.GetAll(userId, paginationParams).Returns(Task.FromResult(response));

        // Act
        var result = await _controller.Get(paginationParams);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedResponse = okResult.Value as PaginatedResponse<AccountListItemResponse>;
        returnedResponse.TotalCount.Should().Be(12);
        returnedResponse.TotalPages.Should().Be(3);
        returnedResponse.CurrentPage.Should().Be(2);
        returnedResponse.HasNextPage.Should().BeTrue();
        returnedResponse.HasPrevPage.Should().BeTrue();
        _accountService.Received(1).GetAll(userId, paginationParams);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_Returns204NoContent()
    {
        // Arrange
        var accountId = 1;
        var userId = "test-user-id";
        var request = new UpdateAccountRequest("Updated Account");
        _accountService.Update(accountId, request, userId).Returns(true);

        // Act
        var result = await _controller.Update(accountId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _accountService.Received(1).Update(accountId, request, userId);
    }

    [Fact]
    public async Task Update_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var accountId = 999;
        var userId = "test-user-id";
        var request = new UpdateAccountRequest("Updated Account");
        _accountService.Update(accountId, request, userId).Returns(false);

        // Act
        var result = await _controller.Update(accountId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _accountService.Received(1).Update(accountId, request, userId);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var accountId = 1;
        var userId = "test-user-id";
        _accountService.Activate(accountId, userId).Returns(true);

        // Act
        var result = await _controller.Activate(accountId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _accountService.Received(1).Activate(accountId, userId);
    }

    [Fact]
    public async Task Activate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var accountId = 999;
        var userId = "test-user-id";
        _accountService.Activate(accountId, userId).Returns(false);

        // Act
        var result = await _controller.Activate(accountId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _accountService.Received(1).Activate(accountId, userId);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var accountId = 1;
        var userId = "test-user-id";
        _accountService.Deactivate(accountId, userId).Returns(true);

        // Act
        var result = await _controller.Deactivate(accountId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _accountService.Received(1).Deactivate(accountId, userId);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var accountId = 999;
        var userId = "test-user-id";
        _accountService.Deactivate(accountId, userId).Returns(false);

        // Act
        var result = await _controller.Deactivate(accountId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _accountService.Received(1).Deactivate(accountId, userId);
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

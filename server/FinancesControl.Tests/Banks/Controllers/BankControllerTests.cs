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
        var bankId = 1;
        _bankService.CreateBank(request).Returns(bankId);

        // Act
        var result = await _controller.CreateBank(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues["bankId"].Should().Be(bankId);
        _bankService.Received(1).CreateBank(request);
    }

    [Fact]
    public async Task CreateBank_WithValidSavingsAccount_Returns201Created()
    {
        // Arrange
        var request = new CreateBankRequest("Savings Bank", BankAccountType.Savings);
        var bankId = 1;
        _bankService.CreateBank(request).Returns(bankId);

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
        var response = new BankDetailResponse(
            bankId,
            "Test Bank",
            BankAccountType.Check,
            "Active",
            DateTime.UtcNow,
            null
        );
        _bankService.GetById(bankId).Returns(response);

        // Act
        var result = await _controller.GetById(bankId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(response);
        _bankService.Received(1).GetById(bankId);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        _bankService.GetById(bankId).Returns((BankDetailResponse?)null);

        // Act
        var result = await _controller.GetById(bankId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).GetById(bankId);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithMultipleBanks_Returns200Ok()
    {
        // Arrange
        var banks = new List<BankListItemResponse>
        {
            new(1, "Bank 1", BankAccountType.Check),
            new(2, "Bank 2", BankAccountType.Savings)
        };
        _bankService.GetAll().Returns(banks);

        // Act
        var result = await _controller.Get();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedList = okResult.Value as List<BankListItemResponse>;
        returnedList.Should().HaveCount(2);
        returnedList[0].Id.Should().Be(1);
        returnedList[0].Name.Should().Be("Bank 1");
        returnedList[1].Id.Should().Be(2);
        returnedList[1].Name.Should().Be("Bank 2");
        _bankService.Received(1).GetAll();
    }

    [Fact]
    public async Task Get_WithNoBanks_Returns200OkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<BankListItemResponse>();
        _bankService.GetAll().Returns(emptyList);

        // Act
        var result = await _controller.Get();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        var returnedList = okResult.Value as List<BankListItemResponse>;
        returnedList.Should().BeEmpty();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_Returns204NoContent()
    {
        // Arrange
        var bankId = 1;
        var request = new UpdateBankRequest("Updated Bank", BankAccountType.Savings);
        _bankService.Update(bankId, request).Returns(true);

        // Act
        var result = await _controller.Update(bankId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bankService.Received(1).Update(bankId, request);
    }

    [Fact]
    public async Task Update_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        var request = new UpdateBankRequest("Updated Bank", BankAccountType.Savings);
        _bankService.Update(bankId, request).Returns(false);

        // Act
        var result = await _controller.Update(bankId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).Update(bankId, request);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var bankId = 1;
        _bankService.Activate(bankId).Returns(true);

        // Act
        var result = await _controller.Activate(bankId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bankService.Received(1).Activate(bankId);
    }

    [Fact]
    public async Task Activate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        _bankService.Activate(bankId).Returns(false);

        // Act
        var result = await _controller.Activate(bankId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).Activate(bankId);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_Returns204NoContent()
    {
        // Arrange
        var bankId = 1;
        _bankService.Deactivate(bankId).Returns(true);

        // Act
        var result = await _controller.Deactivate(bankId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bankService.Received(1).Deactivate(bankId);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var bankId = 999;
        _bankService.Deactivate(bankId).Returns(false);

        // Act
        var result = await _controller.Deactivate(bankId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _bankService.Received(1).Deactivate(bankId);
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

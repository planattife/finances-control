using System;
using System.Security.Claims;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using FinancesControl.API.TransactionCategories.Controllers;
using FinancesControl.API.TransactionCategories.Services;
using FinancesControl.API.TransactionCategories.DTOs;

namespace FinancesControl.Tests.TransactionCategories.Controllers;

public class TransactionCategoryControllerTests
{
    private readonly ITransactionCategoryService _service;
    private readonly TransactionCategoryController _controller;
    private const string TestUserId = "test-user-123";

    public TransactionCategoryControllerTests()
    {
        _service = Substitute.For<ITransactionCategoryService>();
        _controller = new TransactionCategoryController(_service);
        SetupControllerUser();
    }

    private void SetupControllerUser()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateTransactionCategoryRequest("Food");
        long transactionCategoryId = 10;
        _service.CreateCategory(request, TestUserId).Returns(transactionCategoryId);

        var result = await _controller.Create(request);

        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created.ActionName.Should().Be(nameof(_controller.GetById));
        created.RouteValues["categoryId"].Should().Be(transactionCategoryId);
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsOk()
    {
        var detail = new TransactionCategoryDetailResponse(1, "Food", "Active", DateTime.UtcNow, null);
        _service.GetById(1, TestUserId).Returns(detail);

        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNotFound()
    {
        _service.GetById(999, TestUserId).Returns((TransactionCategoryDetailResponse?)null);
        var result = await _controller.GetById(999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Get_ReturnsList()
    {
        var list = new List<TransactionCategoryListItemResponse>
        {
            new(1, "Food"),
            new(2, "Bills")
        };
        _service.GetAll(TestUserId).Returns(list);
        var result = await _controller.Get();
        var ok = result.Result as OkObjectResult;
        ok.Value.Should().Be(list);
    }

    [Fact]
    public async Task Rename_ValidId_ReturnsNoContent()
    {
        var req = new RenameTransactionCategoryRequest("NewName");
        _service.Rename(2, req, TestUserId).Returns(true);
        var result = await _controller.Rename(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Rename_InvalidId_ReturnsNotFound()
    {
        var req = new RenameTransactionCategoryRequest("NewName");
        _service.Rename(999, req, TestUserId).Returns(false);
        var result = await _controller.Rename(999, req);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activate_ValidId_ReturnsNoContent()
    {
        _service.Activate(3, TestUserId).Returns(true);
        var result = await _controller.Activate(3);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Deactivate_ValidId_ReturnsNoContent()
    {
        _service.Deactivate(3, TestUserId).Returns(true);
        var result = await _controller.Deactivate(3);
        result.Should().BeOfType<NoContentResult>();
    }
}

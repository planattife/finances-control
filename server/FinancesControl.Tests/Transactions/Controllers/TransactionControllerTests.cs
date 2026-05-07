using System;
using System.Security.Claims;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using FinancesControl.API.Transactions.Controllers;
using FinancesControl.API.Transactions.Services;
using FinancesControl.API.Transactions.DTOs;
using System.Collections.Generic;

namespace FinancesControl.Tests.Transactions.Controllers;

public class TransactionControllerTests
{
    private readonly ITransactionService _service;
    private readonly TransactionController _controller;
    private const string TestUserId = "test-user-123";

    public TransactionControllerTests()
    {
        _service = Substitute.For<ITransactionService>();
        _controller = new TransactionController(_service);
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
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = API.Enums.PaymentMethod.Cash, CategoryId = 1, TransactionType = API.Enums.TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        long transactionId = 10;
        _service.CreateTransaction(request, TestUserId).Returns(transactionId);

        var result = await _controller.Create(request);

        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created.ActionName.Should().Be(nameof(_controller.GetById));
        created.RouteValues["transactionId"].Should().Be(transactionId);
    }

    [Fact]
    public async Task Create_WithInvalidAmount_ReturnsException()
    {
        var request = new CreateTransactionRequest { Amount = -1, PaymentMethod = API.Enums.PaymentMethod.Cash, CategoryId = 1, TransactionType = API.Enums.TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        _service.When(s => s.CreateTransaction(request, TestUserId)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.Create(request));
    }

    [Fact]
    public async Task Create_WithInvalidCategoryId_ReturnsException()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = API.Enums.PaymentMethod.Cash, CategoryId = 0, TransactionType = API.Enums.TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        _service.When(s => s.CreateTransaction(request, TestUserId)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.Create(request));
    }


    [Fact]
    public async Task GetById_ValidId_ReturnsOk()
    {
        var detail = new TransactionDetailResponse { Id = 1, Amount = 100, PaymentMethod = "Cash", TransactionType = "Expense", TransactionDate = DateTime.UtcNow, CategoryId = 1, Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _service.GetById(1).Returns(detail);

        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok.Value.Should().Be(detail);
    }


    [Fact]
    public async Task GetById_InvalidId_ReturnsNotFound()
    {
        _service.GetById(999).Returns((TransactionDetailResponse?)null);
        var result = await _controller.GetById(999);
        result.Result.Should().BeOfType<NotFoundResult>();
    }


    [Fact]
    public async Task Get_ReturnsList()
    {
        var list = new List<TransactionListItemResponse>
        {
            new() { Id = 1, Amount = 100, PaymentMethod = "Cash", TransactionType = "Expense", TransactionDate = DateTime.UtcNow, CategoryId = 1, Status = "Active" },
            new() { Id = 2, Amount = 200, PaymentMethod = "Credit", TransactionType = "Income", TransactionDate = DateTime.UtcNow, CategoryId = 2, Status = "Inactive" }
        };
        _service.GetAll().Returns(list);
        var result = await _controller.Get();
        var ok = result.Result as OkObjectResult;
        ok.Value.Should().Be(list);
    }

    [Fact]
    public async Task Get_WhenEmpty_ReturnsEmptyList()
    {
        _service.GetAll().Returns(new List<TransactionListItemResponse>());
        var result = await _controller.Get();
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var list = ok.Value as IEnumerable<TransactionListItemResponse>;
        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }


    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        _service.Delete(2).Returns(true);
        var result = await _controller.Delete(2);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        _service.Delete(999).Returns(false);
        var result = await _controller.Delete(999);
        result.Should().BeOfType<NotFoundResult>();
    }


    // ===== Unified Update Tests =====

    [Fact]
    public async Task Update_WithAmountOnly_ReturnsNoContent()
    {
        var req = new UpdateTransactionRequest { NewAmount = 150 };
        _service.Update(2, req, TestUserId).Returns(true);
        var result = await _controller.Update(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_WithCategoryOnly_ReturnsNoContent()
    {
        var req = new UpdateTransactionRequest { NewCategoryId = 7 };
        _service.Update(2, req, TestUserId).Returns(true);
        var result = await _controller.Update(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_WithDescriptionOnly_ReturnsNoContent()
    {
        var req = new UpdateTransactionRequest { NewDescription = "Updated transaction description", HasDescriptionUpdate = true };
        _service.Update(2, req, TestUserId).Returns(true);
        var result = await _controller.Update(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_WithAllFields_ReturnsNoContent()
    {
        var req = new UpdateTransactionRequest 
        { 
            NewAmount = 200, 
            NewCategoryId = 3, 
            NewDescription = "Complete update",
            HasDescriptionUpdate = true
        };
        _service.Update(2, req, TestUserId).Returns(true);
        var result = await _controller.Update(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsNotFound()
    {
        var req = new UpdateTransactionRequest { NewAmount = 150 };
        _service.Update(999, req, TestUserId).Returns(false);
        var result = await _controller.Update(999, req);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_WithNegativeAmount_ReturnsException()
    {
        var req = new UpdateTransactionRequest { NewAmount = -50 };
        _service.When(s => s.Update(2, req, TestUserId)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.Update(2, req));
    }

    [Fact]
    public async Task Update_WithInvalidCategoryId_ReturnsException()
    {
        var req = new UpdateTransactionRequest { NewCategoryId = 0 };
        _service.When(s => s.Update(2, req, TestUserId)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.Update(2, req));
    }

    [Fact]
    public async Task Create_WithDescription_ReturnsCreated()
    {
        var request = new CreateTransactionRequest 
        { 
            Amount = 100, 
            PaymentMethod = API.Enums.PaymentMethod.Cash, 
            CategoryId = 1, 
            TransactionType = API.Enums.TransactionType.Expense, 
            TransactionDate = DateTime.UtcNow,
            Description = "Test purchase"
        };
        long transactionId = 10;
        _service.CreateTransaction(request, TestUserId).Returns(transactionId);

        var result = await _controller.Create(request);

        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created.ActionName.Should().Be(nameof(_controller.GetById));
        created.RouteValues["transactionId"].Should().Be(transactionId);
    }

    [Fact]
    public async Task GetById_WithDescription_ReturnsOk()
    {
        var detail = new TransactionDetailResponse 
        { 
            Id = 1, 
            Amount = 100, 
            PaymentMethod = "Cash", 
            TransactionType = "Expense", 
            TransactionDate = DateTime.UtcNow, 
            CategoryId = 1, 
            Status = "Active", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow,
            Description = "Test transaction"
        };
        _service.GetById(1).Returns(detail);

        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        var response = ok.Value as TransactionDetailResponse;
        response.Description.Should().Be("Test transaction");
    }
}

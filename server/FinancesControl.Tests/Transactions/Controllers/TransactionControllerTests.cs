using System;
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

    public TransactionControllerTests()
    {
        _service = Substitute.For<ITransactionService>();
        _controller = new TransactionController(_service);
    }


    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = API.Enums.PaymentMethod.Cash, CategoryId = 1, TransactionType = API.Enums.TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        long transactionId = 10;
        _service.CreateTransaction(request).Returns(transactionId);

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
        _service.When(s => s.CreateTransaction(request)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.Create(request));
    }

    [Fact]
    public async Task Create_WithInvalidCategoryId_ReturnsException()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = API.Enums.PaymentMethod.Cash, CategoryId = 0, TransactionType = API.Enums.TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        _service.When(s => s.CreateTransaction(request)).Do(x => { throw new ArgumentException(); });
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


    [Fact]
    public async Task UpdateAmount_ValidId_ReturnsNoContent()
    {
        var req = new UpdateTransactionAmountRequest { NewAmount = 123 };
        _service.UpdateAmount(2, req).Returns(true);
        var result = await _controller.UpdateAmount(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateAmount_NegativeAmount_ReturnsException()
    {
        var req = new UpdateTransactionAmountRequest { NewAmount = -100 };
        _service.When(s => s.UpdateAmount(2, req)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.UpdateAmount(2, req));
    }

    [Fact]
    public async Task UpdateAmount_InvalidId_ReturnsNotFound()
    {
        var req = new UpdateTransactionAmountRequest { NewAmount = 123 };
        _service.UpdateAmount(999, req).Returns(false);
        var result = await _controller.UpdateAmount(999, req);
        result.Should().BeOfType<NotFoundResult>();
    }


    [Fact]
    public async Task UpdateCategory_ValidId_ReturnsNoContent()
    {
        var req = new UpdateTransactionCategoryRequest { NewCategoryId = 5 };
        _service.UpdateCategory(2, req).Returns(true);
        var result = await _controller.UpdateCategory(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateCategory_InvalidId_ReturnsNotFound()
    {
        var req = new UpdateTransactionCategoryRequest { NewCategoryId = 5 };
        _service.UpdateCategory(999, req).Returns(false);
        var result = await _controller.UpdateCategory(999, req);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateCategory_InvalidCategoryId_ReturnsException()
    {
        var req = new UpdateTransactionCategoryRequest { NewCategoryId = 0 };
        _service.When(s => s.UpdateCategory(2, req)).Do(x => { throw new ArgumentException(); });
        await Assert.ThrowsAsync<ArgumentException>(async () => await _controller.UpdateCategory(2, req));
    }
}

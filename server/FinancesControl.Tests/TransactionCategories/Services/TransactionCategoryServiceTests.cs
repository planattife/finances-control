using System.Collections.Generic;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.TransactionCategories.DTOs;
using FinancesControl.API.TransactionCategories.Repository;
using FinancesControl.API.TransactionCategories.Services;
using FinancesControl.API.Models;

namespace FinancesControl.Tests.TransactionCategories.Services;

public class TransactionCategoryServiceTests
{
    private readonly ITransactionCategoryRepository _repository;
    private readonly TransactionCategoryService _service;
    private const string TestUserId = "test-user-123";

    public TransactionCategoryServiceTests()
    {
        _repository = Substitute.For<ITransactionCategoryRepository>();
        _service = new TransactionCategoryService(_repository);
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsNewId()
    {
        var request = new CreateTransactionCategoryRequest("Food");
        _repository.Add(Arg.Any<TransactionCategory>()).Returns(Task.FromResult(1L));

        var result = await _service.CreateCategory(request, TestUserId);

        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<FinancesControl.API.Models.TransactionCategory>());
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _repository.GetById(999, TestUserId).Returns(Task.FromResult((FinancesControl.API.Models.TransactionCategory?)null));
        var result = await _service.GetById(999, TestUserId);
        result.Should().NotBeNull();
        result.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Category not found.");
    }

    [Fact]
    public async Task GetById_Exists_ReturnsDetail()
    {
        var model = new TransactionCategory("Bills", 5, TestUserId);
        _repository.GetById(5, TestUserId).Returns(Task.FromResult((TransactionCategory?)model));
        var result = await _service.GetById(5, TestUserId);
        result.Should().NotBeNull();
        result.HttpStatusCode.Should().Be(200);
        result.Errors.Should().BeEmpty();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(5);
        result.Data.Name.Should().Be("Bills");
        result.Data.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Rename_ValidCategory_UpdatesName()
    {
        var cat = new TransactionCategory("Old", 3, TestUserId);
        _repository.GetById(3, TestUserId).Returns(Task.FromResult((TransactionCategory?)cat));
        _repository.Update(Arg.Any<FinancesControl.API.Models.TransactionCategory>()).Returns(Task.FromResult(true));
        var request = new RenameTransactionCategoryRequest("New");

        var result = await _service.Rename(3, request, TestUserId);

        result.Should().BeTrue();
        _repository.Received(1).Update(Arg.Is<FinancesControl.API.Models.TransactionCategory>(c => c.Name == "New"));
    }

    [Fact]
    public async Task Activate_ExistingCategory_ReturnsTrue()
    {
        var cat = new TransactionCategory("Test", 4, TestUserId);
        cat.Deactivate();
        _repository.GetById(4, TestUserId).Returns(Task.FromResult((TransactionCategory?)cat));
        _repository.Update(cat).Returns(Task.FromResult(true));

        var result = await _service.Activate(4, TestUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_DefaultCategory_ReturnsFalse()
    {
        // Return null to simulate that the category doesn't exist for this user (it's a default one)
        _repository.GetById(4, TestUserId).Returns(Task.FromResult((TransactionCategory?)null));

        var result = await _service.Activate(4, TestUserId);

        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<FinancesControl.API.Models.TransactionCategory>());
    }

    [Fact]
    public async Task Deactivate_ExistingCategory_ReturnsTrue()
    {
        var cat = new TransactionCategory("Test", 5, TestUserId);
        _repository.GetById(5, TestUserId).Returns(Task.FromResult((TransactionCategory?)cat));
        _repository.Update(cat).Returns(Task.FromResult(true));

        var result = await _service.Deactivate(5, TestUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Deactivate_DefaultCategory_ReturnsFalse()
    {
        // Return null to simulate that the category doesn't exist for this user (it's a default one)
        _repository.GetById(5, TestUserId).Returns(Task.FromResult((TransactionCategory?)null));

        var result = await _service.Deactivate(5, TestUserId);

        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<FinancesControl.API.Models.TransactionCategory>());
    }
}

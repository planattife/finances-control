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
    public void CreateCategory_WithValidRequest_ReturnsNewId()
    {
        var request = new CreateTransactionCategoryRequest("Food");
        _repository.Add(Arg.Any<TransactionCategory>()).Returns(1);

        var result = _service.CreateCategory(request, TestUserId);

        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<FinancesControl.API.Models.TransactionCategory>());
    }

    [Fact]
    public void GetById_NotFound_ReturnsNull()
    {
        _repository.GetById(999, TestUserId).Returns((FinancesControl.API.Models.TransactionCategory?)null);
        var result = _service.GetById(999, TestUserId);
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_Exists_ReturnsDetail()
    {
        var model = new TransactionCategory("Bills", 5, TestUserId);
        _repository.GetById(5, TestUserId).Returns(model);
        var result = _service.GetById(5, TestUserId);
        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.Name.Should().Be("Bills");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public void Rename_ValidCategory_UpdatesName()
    {
        var cat = new TransactionCategory("Old", 3, TestUserId);
        _repository.GetById(3, TestUserId).Returns(cat);
        _repository.Update(Arg.Any<FinancesControl.API.Models.TransactionCategory>()).Returns(true);
        var request = new RenameTransactionCategoryRequest("New");

        var result = _service.Rename(3, request, TestUserId);

        result.Should().BeTrue();
        _repository.Received(1).Update(Arg.Is<FinancesControl.API.Models.TransactionCategory>(c => c.Name == "New"));
    }

    [Fact]
    public void Activate_ExistingCategory_ReturnsTrue()
    {
        var cat = new TransactionCategory("Test", 4, TestUserId);
        cat.Deactivate();
        _repository.GetById(4, TestUserId).Returns(cat);
        _repository.Update(cat).Returns(true);

        var result = _service.Activate(4, TestUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public void Activate_DefaultCategory_ReturnsFalse()
    {
        // Return null to simulate that the category doesn't exist for this user (it's a default one)
        _repository.GetById(4, TestUserId).Returns((TransactionCategory?)null);

        var result = _service.Activate(4, TestUserId);

        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<FinancesControl.API.Models.TransactionCategory>());
    }

    [Fact]
    public void Deactivate_ExistingCategory_ReturnsTrue()
    {
        var cat = new TransactionCategory("Test", 5, TestUserId);
        _repository.GetById(5, TestUserId).Returns(cat);
        _repository.Update(cat).Returns(true);

        var result = _service.Deactivate(5, TestUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_DefaultCategory_ReturnsFalse()
    {
        // Return null to simulate that the category doesn't exist for this user (it's a default one)
        _repository.GetById(5, TestUserId).Returns((TransactionCategory?)null);

        var result = _service.Deactivate(5, TestUserId);

        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<FinancesControl.API.Models.TransactionCategory>());
    }
}

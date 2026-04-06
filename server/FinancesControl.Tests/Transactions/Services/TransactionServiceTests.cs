using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Transactions.Repository;
using FinancesControl.API.Transactions.Services;
using FinancesControl.API.Models;
using FinancesControl.API.Enums;

namespace FinancesControl.Tests.Transactions.Services;

public class TransactionServiceTests
{
    private readonly ITransactionRepository _repository;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _repository = Substitute.For<ITransactionRepository>();
        _service = new TransactionService(_repository);
    }


    [Fact]
    public void CreateTransaction_WithValidRequest_ReturnsNewId()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1 };
        _repository.GetAll().Returns(new List<Transaction>());
        _repository.Add(Arg.Any<Transaction>()).Returns(1);
        var result = _service.CreateTransaction(request);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Transaction>());
    }

    [Fact]
    public void CreateTransaction_WithNegativeAmount_Throws()
    {
        var request = new CreateTransactionRequest { Amount = -10, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        _repository.GetAll().Returns(new List<Transaction>());
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithZeroCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Cash, CategoryId = 0, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        _repository.GetAll().Returns(new List<Transaction>());
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithUnsupportedPaymentMethod_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = (PaymentMethod)999, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow };
        _repository.GetAll().Returns(new List<Transaction>());
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithCardPaymentWithoutCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, CardId = null };
        _repository.GetAll().Returns(new List<Transaction>());
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithGeneralPaymentWithoutBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, BankId = null };
        _repository.GetAll().Returns(new List<Transaction>());
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }


    [Fact]
    public void GetById_NotFound_ReturnsNull()
    {
        _repository.GetById(999).Returns((Transaction?)null);
        var result = _service.GetById(999);
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_DeletedTransaction_ReturnsInactiveStatus()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        t.Deactivate();
        _repository.GetById(5).Returns(t);
        var result = _service.GetById(5);
        result.Status.Should().Be("Inactive");
    }

    [Fact]
    public void GetById_Exists_ReturnsDetail()
    {
        var model = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        _repository.GetById(5).Returns(model);
        var result = _service.GetById(5);
        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.Amount.Should().Be(100);
        result.Status.Should().Be("Active");
    }


    [Fact]
    public void Delete_ExistingTransaction_ReturnsTrue()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        _repository.GetById(5).Returns(t);
        _repository.Update(t).Returns(true);
        var result = _service.Delete(5);
        result.Should().BeTrue();
    }

    [Fact]
    public void Delete_NonexistentTransaction_ReturnsFalse()
    {
        _repository.GetById(999).Returns((Transaction?)null);
        var result = _service.Delete(999);
        result.Should().BeFalse();
    }


    [Fact]
    public void UpdateAmount_ValidTransaction_UpdatesAmount()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        _repository.GetById(5).Returns(t);
        _repository.Update(t).Returns(true);
        var req = new UpdateTransactionAmountRequest { NewAmount = 200 };
        var result = _service.UpdateAmount(5, req);
        result.Should().BeTrue();
        t.Amount.Should().Be(200);
    }

    [Fact]
    public void UpdateAmount_NonexistentTransaction_ReturnsFalse()
    {
        _repository.GetById(999).Returns((Transaction?)null);
        var req = new UpdateTransactionAmountRequest { NewAmount = 200 };
        var result = _service.UpdateAmount(999, req);
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateAmount_NegativeAmount_Throws()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        _repository.GetById(5).Returns(t);
        var req = new UpdateTransactionAmountRequest { NewAmount = -100 };
        Assert.Throws<ArgumentException>(() => _service.UpdateAmount(5, req));
    }


    [Fact]
    public void UpdateCategory_ValidTransaction_UpdatesCategory()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        _repository.GetById(5).Returns(t);
        _repository.Update(t).Returns(true);
        var req = new UpdateTransactionCategoryRequest { NewCategoryId = 7 };
        var result = _service.UpdateCategory(5, req);
        result.Should().BeTrue();
        t.CategoryId.Should().Be(7);
    }

    [Fact]
    public void UpdateCategory_NonexistentTransaction_ReturnsFalse()
    {
        _repository.GetById(999).Returns((Transaction?)null);
        var req = new UpdateTransactionCategoryRequest { NewCategoryId = 7 };
        var result = _service.UpdateCategory(999, req);
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateCategory_ZeroCategoryId_Throws()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 5);
        _repository.GetById(5).Returns(t);
        var req = new UpdateTransactionCategoryRequest { NewCategoryId = 0 };
        Assert.Throws<ArgumentException>(() => _service.UpdateCategory(5, req));
    }
}

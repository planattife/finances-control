using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Transactions.Repository;
using FinancesControl.API.Transactions.Services;
using FinancesControl.API.Models;
using FinancesControl.API.Enums;
using FinancesControl.API.Accounts.Repository;
using FinancesControl.API.TransactionCategories.Repository;
using FinancesControl.API.Cards.Repository;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Accounts;
using FinancesControl.API.Banks;
using FinancesControl.API.Enums;

namespace FinancesControl.Tests.Transactions.Services;

public class TransactionServiceTests
{
    private readonly ITransactionRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionCategoryRepository _categoryRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IBankRepository _bankRepository;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _repository = Substitute.For<ITransactionRepository>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _categoryRepository = Substitute.For<ITransactionCategoryRepository>();
        _cardRepository = Substitute.For<ICardRepository>();
        _bankRepository = Substitute.For<IBankRepository>();
        _service = new TransactionService(_repository, _accountRepository, _categoryRepository, _cardRepository, _bankRepository);
    }


    // ===== Foreign Key Validation Tests =====

    [Fact]
    public void CreateTransaction_WithNonexistentAccountId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 999 };
        _accountRepository.GetById(999).Returns((Account?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Account with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.AccountId));
    }

    [Fact]
    public void CreateTransaction_WithNonexistentCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 999, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1 };
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(999).Returns((TransactionCategory?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Transaction category with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.CategoryId));
    }

    [Fact]
    public void CreateTransaction_WithCardPaymentAndNonexistentCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1, CardId = 999 };
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        _cardRepository.GetById(999).Returns((Card?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Card with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.CardId));
    }

    [Fact]
    public void CreateTransaction_WithDebitPaymentAndNonexistentCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Debit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1, CardId = 999 };
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        _cardRepository.GetById(999).Returns((Card?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Card with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.CardId));
    }

    [Fact]
    public void CreateTransaction_WithPixPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1, BankId = 999 };
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        _bankRepository.GetById(999).Returns((Bank?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.BankId));
    }

    [Fact]
    public void CreateTransaction_WithBankSlipPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.BankSlip, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1, BankId = 999 };
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        _bankRepository.GetById(999).Returns((Bank?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.BankId));
    }

    [Fact]
    public void CreateTransaction_WithBankTransferPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.BankTransfer, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1, BankId = 999 };
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        _bankRepository.GetById(999).Returns((Bank?)null);
        
        var ex = Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.BankId));
    }

    [Fact]
    public void CreateTransaction_WithValidRequest_ReturnsNewId()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1 };
        
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        _repository.Add(Arg.Any<Transaction>()).Returns(1);
        
        var result = _service.CreateTransaction(request);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Transaction>());
    }

    [Fact]
    public void CreateTransaction_WithNegativeAmount_Throws()
    {
        var request = new CreateTransactionRequest { Amount = -10, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1 };
        
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithZeroCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Cash, CategoryId = 0, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1 };
        
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithUnsupportedPaymentMethod_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = (PaymentMethod)999, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 1 };
        
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithCardPaymentWithoutCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, CardId = null, AccountId = 1 };
        
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        
        Assert.Throws<ArgumentException>(() => _service.CreateTransaction(request));
    }

    [Fact]
    public void CreateTransaction_WithGeneralPaymentWithoutBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, BankId = null, AccountId = 1 };
        
        _accountRepository.GetById(1).Returns(new Account(1, "Test Account", "user123"));
        _categoryRepository.GetById(1).Returns(new TransactionCategory("Test Category", 1));
        
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

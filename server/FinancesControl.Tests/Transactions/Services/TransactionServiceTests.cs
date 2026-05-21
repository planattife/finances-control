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
using Xunit;

namespace FinancesControl.Tests.Transactions.Services;

public class TransactionServiceTests
{
    private readonly ITransactionRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionCategoryRepository _categoryRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IBankRepository _bankRepository;
    private readonly TransactionService _service;
    private const string TestUserId = "test-user-123";
    private const long AccountId = 1;

    public TransactionServiceTests()
    {
        _repository = Substitute.For<ITransactionRepository>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _categoryRepository = Substitute.For<ITransactionCategoryRepository>();
        _cardRepository = Substitute.For<ICardRepository>();
        _bankRepository = Substitute.For<IBankRepository>();
        _service = new TransactionService(_repository, _accountRepository, _categoryRepository, _cardRepository, _bankRepository);
    }

    private void SetupAccountExists() =>
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", TestUserId)));

    [Fact]
    public async Task CreateTransaction_WithUrlAccountIdMismatchRequestAccountId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 2 };
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Invalid account");
    }


    // ===== Foreign Key Validation Tests =====

    [Fact]
    public async Task CreateTransaction_WithNonexistentAccountId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 999 };
        _accountRepository.GetById(999, TestUserId).Returns(Task.FromResult((Account?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, 999, TestUserId));
        ex.Message.Should().Contain("Invalid Account.");
    }

    [Fact]
    public async Task CreateTransaction_WithNonexistentCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 999, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(999, TestUserId).Returns(Task.FromResult((TransactionCategory?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Transaction category with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.CategoryId));
    }

    [Fact]
    public async Task CreateTransaction_WithCardPaymentAndNonexistentCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 999 };
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _cardRepository.GetById(999, AccountId).Returns(Task.FromResult((Card?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Card with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.CardId));
    }

    [Fact]
    public async Task CreateTransaction_WithDebitPaymentAndNonexistentCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Debit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 999 };
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _cardRepository.GetById(999, AccountId).Returns(Task.FromResult((Card?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Card with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.CardId));
    }

    [Fact]
    public async Task CreateTransaction_WithPixPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, BankId = 999 };
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _bankRepository.GetById(999, TestUserId).Returns(Task.FromResult((Bank?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.BankId));
    }

    [Fact]
    public async Task CreateTransaction_WithBankSlipPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.BankSlip, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, BankId = 999 };
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _bankRepository.GetById(999, TestUserId).Returns(Task.FromResult((Bank?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.BankId));
    }

    [Fact]
    public async Task CreateTransaction_WithBankTransferPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.BankTransfer, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, BankId = 999 };
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _bankRepository.GetById(999, TestUserId).Returns(Task.FromResult((Bank?)null));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
        ex.ParamName.Should().Be(nameof(request.BankId));
    }

    [Fact]
    public async Task CreateTransaction_WithValidRequest_ReturnsNewId()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _repository.Add(Arg.Any<Transaction>()).Returns(Task.FromResult(1L));
        
        var result = await _service.CreateTransaction(request, AccountId, TestUserId);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Transaction>());
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_Throws()
    {
        var request = new CreateTransactionRequest { Amount = -10, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithZeroCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Cash, CategoryId = 0, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithUnsupportedPaymentMethod_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = (PaymentMethod)999, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithCardPaymentWithoutCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, CardId = null, AccountId = AccountId };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithGeneralPaymentWithoutBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, BankId = null, AccountId = AccountId };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithCreditPaymentAndInactiveCard_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 1 };
        var inactiveCard = new Card("1234567890123456", DateTime.Now.AddYears(2), CardNetwork.Visa, 1, AccountId, CardType.Credit, 1, "My Credit Card");
        inactiveCard.SetIsActive(false);
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _cardRepository.GetById(1, AccountId).Returns(Task.FromResult((Card?)inactiveCard));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("is not active and cannot be used for transactions");
        ex.ParamName.Should().Be(nameof(request.CardId));
    }

    [Fact]
    public async Task CreateTransaction_WithDebitPaymentAndInactiveCard_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Debit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 2 };
        var inactiveCard = new Card("9876543210987654", DateTime.Now.AddYears(2), CardNetwork.Mastercard, 1, AccountId, CardType.Debit, 2, "My Debit Card");
        inactiveCard.SetIsActive(false);
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _cardRepository.GetById(2, AccountId).Returns(Task.FromResult((Card?)inactiveCard));
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("is not active and cannot be used for transactions");
        ex.ParamName.Should().Be(nameof(request.CardId));
    }

    [Fact]
    public async Task CreateTransaction_WithCreditPaymentAndActiveCard_Succeeds()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 1 };
        var activeCard = new Card("1234567890123456", DateTime.Now.AddYears(2), CardNetwork.Visa, 1, AccountId, CardType.Credit, 1, "My Credit Card");
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _cardRepository.GetById(1, AccountId).Returns(Task.FromResult((Card?)activeCard));
        _repository.Add(Arg.Any<CardTransaction>()).Returns(Task.FromResult(1L));
        
        var result = await _service.CreateTransaction(request, AccountId, TestUserId);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<CardTransaction>());
    }

    [Fact]
    public async Task CreateTransaction_WithDebitPaymentAndActiveCard_Succeeds()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Debit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 2 };
        var activeCard = new Card("9876543210987654", DateTime.Now.AddYears(2), CardNetwork.Mastercard, 1, AccountId, CardType.Debit, 2, "My Debit Card");
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _cardRepository.GetById(2, AccountId).Returns(Task.FromResult((Card?)activeCard));
        _repository.Add(Arg.Any<CardTransaction>()).Returns(Task.FromResult(1L));
        
        var result = await _service.CreateTransaction(request, AccountId, TestUserId);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<CardTransaction>());
    }


    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        SetupAccountExists();
        _repository.GetById(999, AccountId).Returns(Task.FromResult((Transaction?)null));
        var result = await _service.GetById(999, AccountId, TestUserId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_DeletedTransaction_ReturnsInactiveStatus()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        t.Deactivate();
        SetupAccountExists();
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        var result = await _service.GetById(5, AccountId, TestUserId);
        result.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task GetById_Exists_ReturnsDetail()
    {
        SetupAccountExists();
        var model = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)model));
        var result = await _service.GetById(5, AccountId, TestUserId);
        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.Amount.Should().Be(100);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetAll_WithMultipleTransactions_ReturnsAllForAccount()
    {
        var transactions = new List<Transaction>
        {
            new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 1),
            new Transaction(200, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 2),
        };
        SetupAccountExists();
        _repository.GetAll(AccountId).Returns(Task.FromResult(transactions));
        
        var result = await _service.GetAll(AccountId, TestUserId);
        result.Should().HaveCount(2);
        result[0].Amount.Should().Be(100);
        result[1].Amount.Should().Be(200);
    }

    [Fact]
    public async Task GetAll_WithNoTransactions_ReturnsEmptyList()
    {
        SetupAccountExists();
        _repository.GetAll(AccountId).Returns(Task.FromResult(new List<Transaction>()));
        
        var result = await _service.GetAll(AccountId, TestUserId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_ExistingTransaction_ReturnsTrue()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        SetupAccountExists();
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        _repository.Update(t).Returns(Task.FromResult(true));
        var result = await _service.Delete(5, AccountId, TestUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonexistentTransaction_ReturnsFalse()
    {
        SetupAccountExists();
        _repository.GetById(999, AccountId).Returns(Task.FromResult((Transaction?)null));
        var result = await _service.Delete(999, AccountId, TestUserId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithAmountOnly_UpdatesAmount()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        _repository.Update(t).Returns(Task.FromResult(true));
        var req = new UpdateTransactionRequest { NewAmount = 250 };
        var result = await _service.Update(5, AccountId, req, TestUserId);
        result.Should().BeTrue();
        t.Amount.Should().Be(250);
    }

    [Fact]
    public async Task Update_WithCategoryOnly_UpdatesCategory()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        _categoryRepository.GetById(3, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Updated Category", 3, TestUserId)));
        _repository.Update(t).Returns(Task.FromResult(true));
        var req = new UpdateTransactionRequest { NewCategoryId = 3 };
        var result = await _service.Update(5, AccountId, req, TestUserId);
        result.Should().BeTrue();
        t.CategoryId.Should().Be(3);
    }

    [Fact]
    public async Task Update_WithDescriptionOnly_UpdatesDescription()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        _repository.Update(t).Returns(Task.FromResult(true));
        var req = new UpdateTransactionRequest { NewDescription = "Test expense", HasDescriptionUpdate = true };
        var result = await _service.Update(5, AccountId, req, TestUserId);
        result.Should().BeTrue();
        t.Description.Should().Be("Test expense");
    }

    [Fact]
    public async Task Update_WithAllFields_UpdatesAll()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        _categoryRepository.GetById(2, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Updated Category", 2, TestUserId)));
        _repository.Update(t).Returns(Task.FromResult(true));
        var req = new UpdateTransactionRequest { NewAmount = 300, NewCategoryId = 2, NewDescription = "Updated transaction", HasDescriptionUpdate = true };
        var result = await _service.Update(5, AccountId, req, TestUserId);
        result.Should().BeTrue();
        t.Amount.Should().Be(300);
        t.CategoryId.Should().Be(2);
        t.Description.Should().Be("Updated transaction");
    }

    [Fact]
    public async Task Update_NonexistentTransaction_ReturnsFalse()
    {
        SetupAccountExists();
        _repository.GetById(999, AccountId).Returns(Task.FromResult((Transaction?)null));
        var req = new UpdateTransactionRequest { NewAmount = 200 };
        var result = await _service.Update(999, AccountId, req, TestUserId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithNegativeAmount_Throws()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        var req = new UpdateTransactionRequest { NewAmount = -50 };
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Update(5, AccountId, req, TestUserId));
    }

    [Fact]
    public async Task Update_WithNullDescription_ClearsDescription()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5, "Original description");
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        _repository.Update(t).Returns(Task.FromResult(true));
        var req = new UpdateTransactionRequest { NewDescription = null, HasDescriptionUpdate = true };
        var result = await _service.Update(5, AccountId, req, TestUserId);
        result.Should().BeTrue();
        t.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreateTransaction_WithDescription_StoresDescription()
    {
        var request = new CreateTransactionRequest 
        { 
            Amount = 100, 
            PaymentMethod = PaymentMethod.Cash, 
            CategoryId = 1, 
            TransactionType = TransactionType.Expense, 
            TransactionDate = DateTime.UtcNow, 
            AccountId = AccountId,
            Description = "Test purchase"
        };
        
        _accountRepository.GetById(AccountId, TestUserId).Returns(Task.FromResult((Account?)new Account(AccountId, "Test Account", "user123")));
        _categoryRepository.GetById(1, TestUserId).Returns(Task.FromResult((TransactionCategory?)new TransactionCategory("Test Category", 1, TestUserId)));
        _repository.Add(Arg.Any<Transaction>()).Returns(Task.FromResult(1L));
        
        var result = await _service.CreateTransaction(request, AccountId, TestUserId);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Is<Transaction>(t => t.Description == "Test purchase"));
    }

    [Fact]
    public async Task GetById_ReturnsDescriptionInDetail()
    {
        SetupAccountExists();
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5, "Test description");
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        var result = await _service.GetById(5, AccountId, TestUserId);
        result.Should().NotBeNull();
        result.Description.Should().Be("Test description");
    }
}

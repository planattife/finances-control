using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Transactions.Repository;
using FinancesControl.API.Transactions.Services;
using FinancesControl.API.Models;
using FinancesControl.API.Enums;
using FinancesControl.API.Accounts.Services;
using FinancesControl.API.TransactionCategories.Services;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Common;
using FinancesControl.API.Accounts;
using FinancesControl.API.Banks;
using Xunit;

namespace FinancesControl.Tests.Transactions.Services;

public class TransactionServiceTests
{
    private readonly ITransactionRepository _repository;
    private readonly IAccountService _accountService;
    private readonly ITransactionCategoryService _categoryService;
    private readonly ICardService _cardService;
    private readonly IBankService _bankService;
    private readonly TransactionService _service;
    private const string TestUserId = "test-user-123";
    private const long AccountId = 1;

    public TransactionServiceTests()
    {
        _repository = Substitute.For<ITransactionRepository>();
        _accountService = Substitute.For<IAccountService>();
        _categoryService = Substitute.For<ITransactionCategoryService>();
        _cardService = Substitute.For<ICardService>();
        _bankService = Substitute.For<IBankService>();
        _service = new TransactionService(_repository, _accountService, _categoryService, _cardService, _bankService);
    }

    private void SetupAccountExists() =>
        _accountService.ExistsAsync(AccountId, TestUserId).Returns(Task.FromResult(true));

    private static SingleResponse<CardDetailResponse> CreateCardResponse(long cardId, CardType cardType, string status = "Active") =>
        new()
        {
            Data = new CardDetailResponse(
                cardId,
                "1234567890123456",
                cardType,
                CardNetwork.Visa,
                DateTime.UtcNow.AddYears(1),
                new CardBankResponse(1, "Test Bank"),
                "Test Card",
                status,
                DateTime.UtcNow,
                null)
        };

    [Fact]
    public async Task CreateTransaction_WithUrlAccountIdMismatchRequestAccountId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 2 };
        
        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Invalid account");
    }


    // ===== Foreign Key Validation Tests =====

    [Fact]
    public async Task CreateTransaction_WithNonexistentAccountId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = 999 };
        _accountService.ExistsAsync(999, TestUserId).Returns(Task.FromResult(false));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, 999, TestUserId));
        ex.Message.Should().Contain("Account not found.");
    }

    [Fact]
    public async Task CreateTransaction_WithNonexistentCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 999, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        SetupAccountExists();
        _categoryService.ExistsAsync(999).Returns(Task.FromResult(false));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Transaction category with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateTransaction_WithCardPaymentAndNonexistentCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 999 };
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _cardService.GetById(999, AccountId, TestUserId).Returns(Task.FromResult(new SingleResponse<CardDetailResponse>()));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Card with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateTransaction_WithDebitPaymentAndNonexistentCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Debit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 999 };
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _cardService.GetById(999, AccountId, TestUserId).Returns(Task.FromResult(new SingleResponse<CardDetailResponse>()));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Card with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateTransaction_WithPixPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, BankId = 999 };
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _bankService.ExistsAsync(999, TestUserId).Returns(Task.FromResult(false));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateTransaction_WithBankSlipPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.BankSlip, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, BankId = 999 };
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _bankService.ExistsAsync(999, TestUserId).Returns(Task.FromResult(false));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateTransaction_WithBankTransferPaymentAndNonexistentBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.BankTransfer, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, BankId = 999 };
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _bankService.ExistsAsync(999, TestUserId).Returns(Task.FromResult(false));
        
        var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("Bank with ID 999 does not exist");
    }

    [Fact]
    public async Task CreateTransaction_WithValidRequest_ReturnsNewId()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _repository.Add(Arg.Any<Transaction>()).Returns(Task.FromResult(1L));
        
        var result = await _service.CreateTransaction(request, AccountId, TestUserId);
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Transaction>());
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_Throws()
    {
        var request = new CreateTransactionRequest { Amount = -10, PaymentMethod = PaymentMethod.Cash, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithZeroCategoryId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Cash, CategoryId = 0, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        SetupAccountExists();
        
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithUnsupportedPaymentMethod_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = (PaymentMethod)999, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithCardPaymentWithoutCardId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, CardId = null, AccountId = AccountId };
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithGeneralPaymentWithoutBankId_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 10, PaymentMethod = PaymentMethod.Pix, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, BankId = null, AccountId = AccountId };
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    }

    [Fact]
    public async Task CreateTransaction_WithCreditPaymentAndInactiveCard_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 1 };
        var inactiveCard = new Card("1234567890123456", DateTime.Now.AddYears(2), CardNetwork.Visa, 1, AccountId, CardType.Credit, 1, "My Credit Card");
        inactiveCard.SetIsActive(false);
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _cardService.GetById(1, AccountId, TestUserId).Returns(Task.FromResult(CreateCardResponse(1, CardType.Credit, "Inactive")));
        
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("is not active and cannot be used for transactions");
    }

    [Fact]
    public async Task CreateTransaction_WithDebitPaymentAndInactiveCard_Throws()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Debit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 2 };
        var inactiveCard = new Card("9876543210987654", DateTime.Now.AddYears(2), CardNetwork.Mastercard, 1, AccountId, CardType.Debit, 2, "My Debit Card");
        inactiveCard.SetIsActive(false);
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _cardService.GetById(2, AccountId, TestUserId).Returns(Task.FromResult(CreateCardResponse(2, CardType.Debit, "Inactive")));
        
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
        ex.Message.Should().Contain("is not active and cannot be used for transactions");
    }

    [Fact]
    public async Task CreateTransaction_WithCreditPaymentAndActiveCard_Succeeds()
    {
        var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Credit, CategoryId = 1, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId, CardId = 1 };
        var activeCard = new Card("1234567890123456", DateTime.Now.AddYears(2), CardNetwork.Visa, 1, AccountId, CardType.Credit, 1, "My Credit Card");
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _cardService.GetById(1, AccountId, TestUserId).Returns(Task.FromResult(CreateCardResponse(1, CardType.Credit)));
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
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
        _cardService.GetById(2, AccountId, TestUserId).Returns(Task.FromResult(CreateCardResponse(2, CardType.Debit)));
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
        result.Data.Should().BeNull();
        result.HttpStatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetById_DeletedTransaction_ReturnsInactiveStatus()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5);
        t.Deactivate();
        SetupAccountExists();
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)t));
        var result = await _service.GetById(5, AccountId, TestUserId);
        result.Data.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task GetById_Exists_ReturnsDetail()
    {
        SetupAccountExists();
        var model = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 5, categoryName: "Food");
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)model));
        var result = await _service.GetById(5, AccountId, TestUserId);
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(5);
        result.Data.Amount.Should().Be(100);
        result.Data.Category.CategoryId.Should().Be(1);
        result.Data.Category.CategoryName.Should().Be("Food");
        result.Data.Card.Should().BeNull();
        result.Data.Bank.Should().BeNull();
        result.Data.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetById_CardTransaction_ReturnsCardObject()
    {
        SetupAccountExists();
        var model = new CardTransaction(100, PaymentMethod.Credit, TransactionType.Expense, DateTime.UtcNow, 1, 10, AccountId, 5, cardName: "Rewards Card");
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)model));

        var result = await _service.GetById(5, AccountId, TestUserId);

        result.Data.Should().NotBeNull();
        result.Data.Card.Should().NotBeNull();
        result.Data.Card.CardId.Should().Be(10);
        result.Data.Card.CardName.Should().Be("Rewards Card");
        result.Data.Bank.Should().BeNull();
    }

    [Fact]
    public async Task GetById_GeneralTransaction_ReturnsBankObject()
    {
        SetupAccountExists();
        var model = new GeneralTransaction(100, PaymentMethod.Pix, TransactionType.Expense, DateTime.UtcNow, 1, 20, AccountId, 5, bankName: "Main Bank");
        _repository.GetById(5, AccountId).Returns(Task.FromResult((Transaction?)model));

        var result = await _service.GetById(5, AccountId, TestUserId);

        result.Data.Should().NotBeNull();
        result.Data.Bank.Should().NotBeNull();
        result.Data.Bank.BankId.Should().Be(20);
        result.Data.Bank.BankName.Should().Be("Main Bank");
        result.Data.Card.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_WithMultipleTransactions_ReturnsAllForAccount()
    {
        var paginationParams = new PaginationParams();
        var transactions = new List<Transaction>
        {
            new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, 1, categoryName: "Food"),
            new Transaction(200, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 2, AccountId, 2, categoryName: "Transport"),
        };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(transactions));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(transactions.Count));
        
        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);
        result.Data.Should().HaveCount(2);
        result.Data[0].Amount.Should().Be(100);
        result.Data[0].Category.CategoryId.Should().Be(1);
        result.Data[0].Category.CategoryName.Should().Be("Food");
        result.Data[1].Amount.Should().Be(200);
        result.Data[1].Category.CategoryId.Should().Be(2);
        result.Data[1].Category.CategoryName.Should().Be("Transport");
        result.TotalCount.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPrevPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_WithNoTransactions_ReturnsEmptyList()
    {
        var paginationParams = new PaginationParams();
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(new List<Transaction>()));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(0));
        
        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WithPaginationParams_PassesPaginationParamsToRepository()
    {
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var transactions = CreateTransactions(5, 6);
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(transactions));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);

        await _repository.Received(1).GetAll(AccountId, paginationParams);
        await _repository.Received(1).CountByAccountId(AccountId);
        result.Data.Should().HaveCount(5);
        result.Data[0].Id.Should().Be(6);
        result.Data[4].Id.Should().Be(10);
    }

    [Fact]
    public async Task GetAll_WithSecondPage_ReturnsCorrectPaginationMetadata()
    {
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateTransactions(5, 6)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);

        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeTrue();
        result.Data.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAll_WithLastPage_ReturnsHasNextFalseAndHasPrevTrue()
    {
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 3 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateTransactions(2, 11)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);

        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(3);
        result.HasNextPage.Should().BeFalse();
        result.HasPrevPage.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithFirstPage_ReturnsHasPrevFalse()
    {
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 1 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateTransactions(5, 1)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);

        result.CurrentPage.Should().Be(1);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeFalse();
        result.Data.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAll_WithInvalidPaginationParams_UsesDefaults()
    {
        var paginationParams = new PaginationParams { PageSize = 0, PageNumber = -1 };
        SetupAccountExists();
        _repository.GetAll(AccountId, paginationParams).Returns(Task.FromResult(CreateTransactions(10, 1)));
        _repository.CountByAccountId(AccountId).Returns(Task.FromResult(12));

        var result = await _service.GetAll(AccountId, TestUserId, paginationParams);

        paginationParams.PageSize.Should().Be(10);
        paginationParams.PageNumber.Should().Be(1);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeFalse();
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
        t.IsActive.Should().BeFalse();
        await _repository.Received(1).Update(Arg.Is<Transaction>(transaction => !transaction.IsActive));
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
        _categoryService.ExistsAsync(3).Returns(Task.FromResult(true));
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
        _categoryService.ExistsAsync(2).Returns(Task.FromResult(true));
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
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.Update(5, AccountId, req, TestUserId));
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
        
        SetupAccountExists();
        _categoryService.ExistsAsync(1).Returns(Task.FromResult(true));
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
        result.Data.Should().NotBeNull();
        result.Data.Description.Should().Be("Test description");
    }

    private static List<Transaction> CreateTransactions(int count, int startId)
    {
        return Enumerable.Range(startId, count)
            .Select(id => new Transaction(100 + id, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, AccountId, id, categoryName: $"Category {id}"))
            .ToList();
    }
}

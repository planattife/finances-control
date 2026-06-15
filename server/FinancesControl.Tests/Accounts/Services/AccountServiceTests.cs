using System;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Accounts;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Accounts.Repository;
using FinancesControl.API.Accounts.Services;
using FinancesControl.API.Common;

namespace FinancesControl.Tests.Accounts.Services;

public class AccountServiceTests
{
    private readonly IAccountRepository _repository;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _repository = Substitute.For<IAccountRepository>();
        _service = new AccountService(_repository);
    }

    #region CreateAccount Tests

    [Fact]
    public async Task CreateAccount_WithValidRequest_ReturnsNewAccountId()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        var userId = "user-123";
        _repository.Add(Arg.Any<Account>()).Returns(Task.FromResult(1L));

        // Act
        var result = await _service.CreateAccount(request, userId);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Account>());
    }

    [Fact]
    public async Task CreateAccount_NewAccountIsActive()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        var userId = "user-123";
        Account? capturedAccount = null;
        _repository.Add(Arg.Any<Account>())
            .Returns(x =>
            {
                capturedAccount = x.Arg<Account>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateAccount(request, userId);

        // Assert
        capturedAccount.Should().NotBeNull();
        capturedAccount.IsActive.Should().BeTrue();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsAccountDetail()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)account));

        // Act
        var result = await _service.GetById(accountId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(accountId);
        result.Data.Name.Should().Be("Test Account");
        result.Data.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var accountId = 999;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.GetById(accountId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Account not found.");
    }

    [Fact]
    public async Task GetById_WithDifferentUser_ReturnsNull()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        var otherUserId = "user-456";
        var account = new Account(accountId, "Test Account", otherUserId);
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.GetById(accountId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Account not found.");
    }

    [Fact]
    public async Task GetById_InactiveAccount_ReturnsInactiveStatus()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        account.Deactivate();
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)account));

        // Act
        var result = await _service.GetById(accountId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Status.Should().Be("Inactive");
    }

    #endregion

    #region GetAccountKpi Tests

    [Fact]
    public async Task GetAccountKpi_WithValidId_ReturnsAccountKpi()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var accountKpi = new AccountKpiResponse(accountId, 2, 5);
        _repository.GetAccountKpi(accountId, userId).Returns(Task.FromResult((AccountKpiResponse?)accountKpi));

        // Act
        var result = await _service.GetAccountKpi(accountId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.AccountId.Should().Be(accountId);
        result.Data.CardsCount.Should().Be(2);
        result.Data.TransactionsCount.Should().Be(5);
        result.HttpStatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetAccountKpi_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var accountId = 999;
        var userId = "user-123";
        _repository.GetAccountKpi(accountId, userId).Returns(Task.FromResult((AccountKpiResponse?)null));

        // Act
        var result = await _service.GetAccountKpi(accountId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Account not found.");
    }

    [Fact]
    public async Task GetAccountKpi_WithValidId_PassesAccountIdAndUserIdToRepository()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var accountKpi = new AccountKpiResponse(accountId, 0, 0);
        _repository.GetAccountKpi(accountId, userId).Returns(Task.FromResult((AccountKpiResponse?)accountKpi));

        // Act
        await _service.GetAccountKpi(accountId, userId);

        // Assert
        await _repository.Received(1).GetAccountKpi(accountId, userId);
    }

    [Fact]
    public async Task GetAccountKpi_WithZeroCounts_ReturnsZeroCounts()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var accountKpi = new AccountKpiResponse(accountId, 0, 0);
        _repository.GetAccountKpi(accountId, userId).Returns(Task.FromResult((AccountKpiResponse?)accountKpi));

        // Act
        var result = await _service.GetAccountKpi(accountId, userId);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.CardsCount.Should().Be(0);
        result.Data.TransactionsCount.Should().Be(0);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMultipleAccounts_ReturnsAllAccounts()
    {
        // Arrange
        var userId = "user-123";
        var paginationParams = new PaginationParams();
        var accounts = new List<Account>
        {
            new(1, "Account 1", userId),
            new(2, "Account 2", userId),
            new(3, "Account 3", userId)
        };
        _repository.GetAll(userId, paginationParams).Returns(Task.FromResult(accounts));
        _repository.CountByUserId(userId).Returns(Task.FromResult(accounts.Count));

        // Act
        var result = await _service.GetAll(userId, paginationParams);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoAccounts_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var paginationParams = new PaginationParams();
        _repository.GetAll(userId, paginationParams).Returns(Task.FromResult(new List<Account>()));
        _repository.CountByUserId(userId).Returns(Task.FromResult(0));

        // Act
        var result = await _service.GetAll(userId, paginationParams);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithPaginationParams_PassesPaginationParamsToRepository()
    {
        // Arrange
        var userId = "user-123";
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var accounts = CreateAccounts(5, 6, userId);
        _repository.GetAll(userId, paginationParams).Returns(Task.FromResult(accounts));
        _repository.CountByUserId(userId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(userId, paginationParams);

        // Assert
        result.Should().NotBeNull();
        await _repository.Received(1).GetAll(userId, paginationParams);
        await _repository.Received(1).CountByUserId(userId);
    }

    [Fact]
    public async Task GetAll_WithPaginationMetadata_ReturnsCorrectPagination()
    {
        // Arrange
        var userId = "user-123";
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        _repository.GetAll(userId, paginationParams).Returns(Task.FromResult(CreateAccounts(5, 6, userId)));
        _repository.CountByUserId(userId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(userId, paginationParams);

        // Assert
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeTrue();
        result.HttpStatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetAll_WithLastPage_ReturnsNoNextPage()
    {
        // Arrange
        var userId = "user-123";
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 3 };
        _repository.GetAll(userId, paginationParams).Returns(Task.FromResult(CreateAccounts(2, 11, userId)));
        _repository.CountByUserId(userId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(userId, paginationParams);

        // Assert
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(3);
        result.HasNextPage.Should().BeFalse();
        result.HasPrevPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_WithInvalidPaginationParams_UsesDefaults()
    {
        // Arrange
        var userId = "user-123";
        var paginationParams = new PaginationParams { PageSize = 0, PageNumber = -1 };
        _repository.GetAll(userId, paginationParams).Returns(Task.FromResult(CreateAccounts(10, 1, userId)));
        _repository.CountByUserId(userId).Returns(Task.FromResult(12));

        // Act
        var result = await _service.GetAll(userId, paginationParams);

        // Assert
        paginationParams.PageSize.Should().Be(10);
        paginationParams.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.HasNextPage.Should().BeTrue();
        result.HasPrevPage.Should().BeFalse();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidIdAndName_ReturnsTrue()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Old Name", userId);
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)account));
        _repository.Update(Arg.Any<Account>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Update(accountId, request, userId);

        // Assert
        result.Should().BeTrue();
        account.Name.Should().Be("New Name");
        _repository.Received(1).Update(account);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var accountId = 999;
        var userId = "user-123";
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.Update(accountId, request, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public async Task Update_WithDifferentUser_ReturnsFalse()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        var otherUserId = "user-456";
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.Update(accountId, request, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public async Task Update_UpdatesAccountTimestamp()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Old Name", userId);
        DateTime originalUpdatedAt = account.UpdatedAt ?? DateTime.MinValue;
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)account));
        _repository.Update(Arg.Any<Account>()).Returns(Task.FromResult(true));

        // Act
        await _service.Update(accountId, request, userId);

        // Assert
        account.UpdatedAt.Should().NotBe(originalUpdatedAt);
        account.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        account.Deactivate();
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)account));
        _repository.Update(Arg.Any<Account>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Activate(accountId, userId);

        // Assert
        result.Should().BeTrue();
        account.IsActive.Should().BeTrue();
        _repository.Received(1).Update(account);
    }

    [Fact]
    public async Task Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var accountId = 999;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.Activate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public async Task Activate_WithDifferentUser_ReturnsFalse()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.Activate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)account));
        _repository.Update(Arg.Any<Account>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Deactivate(accountId, userId);

        // Assert
        result.Should().BeTrue();
        account.IsActive.Should().BeFalse();
        _repository.Received(1).Update(account);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var accountId = 999;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.Deactivate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public async Task Deactivate_WithDifferentUser_ReturnsFalse()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns(Task.FromResult((Account?)null));

        // Act
        var result = await _service.Deactivate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    #endregion

    private static List<Account> CreateAccounts(int count, int startId, string userId)
    {
        return Enumerable.Range(startId, count)
            .Select(id => new Account(id, $"Account {id}", userId))
            .ToList();
    }
}

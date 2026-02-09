using System;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Accounts;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Accounts.Repository;
using FinancesControl.API.Accounts.Services;

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
    public void CreateAccount_WithValidRequest_ReturnsNewAccountId()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        var userId = "user-123";
        _repository.GetAll().Returns(new List<Account>());
        _repository.Add(Arg.Any<Account>()).Returns(1);

        // Act
        var result = _service.CreateAccount(request, userId);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Account>());
    }

    [Fact]
    public void CreateAccount_GeneratesAutoIncrementingId()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        var userId = "user-123";
        var existingAccounts = new List<Account>
        {
            new(1, "Existing Account 1", "user-123"),
            new(2, "Existing Account 2", "user-123"),
            new(3, "Existing Account 3", "user-123")
        };
        _repository.GetAll().Returns(existingAccounts);
        _repository.Add(Arg.Any<Account>()).Returns(4);

        // Act
        var result = _service.CreateAccount(request, userId);

        // Assert
        result.Should().Be(4);
        _repository.Received(1).Add(Arg.Is<Account>(a => a.Id == 4));
    }

    [Fact]
    public void CreateAccount_FirstAccountGetsId1()
    {
        // Arrange
        var request = new CreateAccountRequest("First Account");
        var userId = "user-123";
        _repository.GetAll().Returns(new List<Account>());
        _repository.Add(Arg.Any<Account>()).Returns(1);

        // Act
        var result = _service.CreateAccount(request, userId);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void CreateAccount_NewAccountIsActive()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        var userId = "user-123";
        _repository.GetAll().Returns(new List<Account>());
        Account? capturedAccount = null;
        _repository.Add(Arg.Any<Account>())
            .Returns(x =>
            {
                capturedAccount = x.Arg<Account>();
                return 1;
            });

        // Act
        _service.CreateAccount(request, userId);

        // Assert
        capturedAccount.Should().NotBeNull();
        capturedAccount.IsActive.Should().BeTrue();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public void GetById_WithValidId_ReturnsAccountDetail()
    {
        // Arrange
        var accountId = 1;
        var account = new Account(accountId, "Test Account", "user-123");
        _repository.GetById(accountId).Returns(account);

        // Act
        var result = _service.GetById(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(accountId);
        result.Name.Should().Be("Test Account");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var accountId = 999;
        _repository.GetById(accountId).Returns((Account?)null);

        // Act
        var result = _service.GetById(accountId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_InactiveAccount_ReturnsInactiveStatus()
    {
        // Arrange
        var accountId = 1;
        var account = new Account(accountId, "Test Account", "user-123");
        account.Deactivate();
        _repository.GetById(accountId).Returns(account);

        // Act
        var result = _service.GetById(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Inactive");
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_WithMultipleAccounts_ReturnsAllAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new(1, "Account 1", "user-123"),
            new(2, "Account 2", "user-123"),
            new(3, "Account 3", "user-123")
        };
        _repository.GetAll().Returns(accounts);

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void GetAll_WithNoAccounts_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAll().Returns(new List<Account>());

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidIdAndName_ReturnsTrue()
    {
        // Arrange
        var accountId = 1;
        var account = new Account(accountId, "Old Name", "user-123");
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        var result = _service.Update(accountId, request);

        // Assert
        result.Should().BeTrue();
        account.Name.Should().Be("New Name");
        _repository.Received(1).Update(account);
    }

    [Fact]
    public void Update_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var accountId = 999;
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId).Returns((Account?)null);

        // Act
        var result = _service.Update(accountId, request);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public void Update_UpdatesAccountTimestamp()
    {
        // Arrange
        var accountId = 1;
        var account = new Account(accountId, "Old Name", "user-123");
        DateTime originalUpdatedAt = account.UpdatedAt ?? DateTime.MinValue;
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        _service.Update(accountId, request);

        // Assert
        account.UpdatedAt.Should().NotBe(originalUpdatedAt);
        account.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var accountId = 1;
        var account = new Account(accountId, "Test Account", "user-123");
        account.Deactivate();
        _repository.GetById(accountId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        var result = _service.Activate(accountId);

        // Assert
        result.Should().BeTrue();
        account.IsActive.Should().BeTrue();
        _repository.Received(1).Update(account);
    }

    [Fact]
    public void Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var accountId = 999;
        _repository.GetById(accountId).Returns((Account?)null);

        // Act
        var result = _service.Activate(accountId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var accountId = 1;
        var account = new Account(accountId, "Test Account", "user-123");
        _repository.GetById(accountId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        var result = _service.Deactivate(accountId);

        // Assert
        result.Should().BeTrue();
        account.IsActive.Should().BeFalse();
        _repository.Received(1).Update(account);
    }

    [Fact]
    public void Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var accountId = 999;
        _repository.GetById(accountId).Returns((Account?)null);

        // Act
        var result = _service.Deactivate(accountId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    #endregion
}

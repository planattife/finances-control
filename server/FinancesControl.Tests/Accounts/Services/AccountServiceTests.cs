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
        _repository.Add(Arg.Any<Account>()).Returns(1);

        // Act
        var result = _service.CreateAccount(request, userId);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Account>());
    }

    [Fact]
    public void CreateAccount_NewAccountIsActive()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");
        var userId = "user-123";
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
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        _repository.GetById(accountId, userId).Returns(account);

        // Act
        var result = _service.GetById(accountId, userId);

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
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.GetById(accountId, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_WithDifferentUser_ReturnsNull()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        var otherUserId = "user-456";
        var account = new Account(accountId, "Test Account", otherUserId);
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.GetById(accountId, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_InactiveAccount_ReturnsInactiveStatus()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        account.Deactivate();
        _repository.GetById(accountId, userId).Returns(account);

        // Act
        var result = _service.GetById(accountId, userId);

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
        var userId = "user-123";
        var accounts = new List<Account>
        {
            new(1, "Account 1", userId),
            new(2, "Account 2", userId),
            new(3, "Account 3", userId)
        };
        _repository.GetAll(userId).Returns(accounts);

        // Act
        var result = _service.GetAll(userId);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void GetAll_WithNoAccounts_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-123";
        _repository.GetAll(userId).Returns(new List<Account>());

        // Act
        var result = _service.GetAll(userId);

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
        var userId = "user-123";
        var account = new Account(accountId, "Old Name", userId);
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        var result = _service.Update(accountId, request, userId);

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
        var userId = "user-123";
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.Update(accountId, request, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public void Update_WithDifferentUser_ReturnsFalse()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        var otherUserId = "user-456";
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.Update(accountId, request, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public void Update_UpdatesAccountTimestamp()
    {
        // Arrange
        var accountId = 1;
        var userId = "user-123";
        var account = new Account(accountId, "Old Name", userId);
        DateTime originalUpdatedAt = account.UpdatedAt ?? DateTime.MinValue;
        var request = new UpdateAccountRequest("New Name");
        _repository.GetById(accountId, userId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        _service.Update(accountId, request, userId);

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
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        account.Deactivate();
        _repository.GetById(accountId, userId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        var result = _service.Activate(accountId, userId);

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
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.Activate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public void Activate_WithDifferentUser_ReturnsFalse()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.Activate(accountId, userId);

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
        var userId = "user-123";
        var account = new Account(accountId, "Test Account", userId);
        _repository.GetById(accountId, userId).Returns(account);
        _repository.Update(Arg.Any<Account>()).Returns(true);

        // Act
        var result = _service.Deactivate(accountId, userId);

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
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.Deactivate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    [Fact]
    public void Deactivate_WithDifferentUser_ReturnsFalse()
    {
        // Arrange - Account belongs to different user
        var accountId = 1;
        var userId = "user-123";
        _repository.GetById(accountId, userId).Returns((Account?)null);

        // Act
        var result = _service.Deactivate(accountId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Account>());
    }

    #endregion
}

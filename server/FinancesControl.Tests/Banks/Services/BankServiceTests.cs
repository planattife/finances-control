using System;
using System.Security.Claims;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Banks;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Enums;
using Microsoft.AspNetCore.Http;

namespace FinancesControl.Tests.Banks.Services;

public class BankServiceTests
{
    private readonly IBankRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly BankService _service;

    public BankServiceTests()
    {
        _repository = Substitute.For<IBankRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        SetupHttpContextAccessor();
        _service = new BankService(_repository, _httpContextAccessor);
    }

    #region CreateBank Tests

    [Fact]
    public async Task CreateBank_WithValidRequest_ReturnsNewBankId()
    {
        // Arrange
        var request = new CreateBankRequest("Test Bank", BankAccountType.Check);
        _repository.Add(Arg.Any<Bank>()).Returns(Task.FromResult(1L));

        // Act
        var result = await _service.CreateBank(request);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Bank>());
    }

    [Fact]
    public async Task CreateBank_NewBankIsActive()
    {
        // Arrange
        var request = new CreateBankRequest("Test Bank", BankAccountType.Check);
        Bank? capturedBank = null;
        _repository.Add(Arg.Any<Bank>())
            .Returns(x =>
            {
                capturedBank = x.Arg<Bank>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateBank(request);

        // Assert
        capturedBank.Should().NotBeNull();
        capturedBank.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateBank_WithCheckAccountType_CreatesCheckBank()
    {
        // Arrange
        var request = new CreateBankRequest("Check Bank", BankAccountType.Check);
        Bank? capturedBank = null;
        _repository.Add(Arg.Any<Bank>())
            .Returns(x =>
            {
                capturedBank = x.Arg<Bank>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateBank(request);

        // Assert
        capturedBank.Should().NotBeNull();
        capturedBank.AccountType.Should().Be(BankAccountType.Check);
    }

    [Fact]
    public async Task CreateBank_WithSavingsAccountType_CreatesSavingsBank()
    {
        // Arrange
        var request = new CreateBankRequest("Savings Bank", BankAccountType.Savings);
        Bank? capturedBank = null;
        _repository.Add(Arg.Any<Bank>())
            .Returns(x =>
            {
                capturedBank = x.Arg<Bank>();
                return Task.FromResult(1L);
            });

        // Act
        await _service.CreateBank(request);

        // Assert
        capturedBank.Should().NotBeNull();
        capturedBank.AccountType.Should().Be(BankAccountType.Savings);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsBankDetail()
    {
        // Arrange
        var bankId = 1;
        var userId = "mock-user-id";
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));

        // Act
        var result = await _service.GetById(bankId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(bankId);
        result.Data.Name.Should().Be("Test Bank");
        result.Data.AccountType.Should().Be(BankAccountType.Check);
        result.Data.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var bankId = 999;
        var userId = "mock-user-id";
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)null));

        // Act
        var result = await _service.GetById(bankId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.HttpStatusCode.Should().Be(404);
        result.Errors.Should().Contain("Bank not found.");
    }

    [Fact]
    public async Task GetById_InactiveBank_ReturnsInactiveStatus()
    {
        // Arrange
        var bankId = 1;
        var userId = "mock-user-id";
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        bank.Deactivate();
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));

        // Act
        var result = await _service.GetById(bankId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Status.Should().Be("Inactive");
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMultipleBanks_ReturnsAllBanks()
    {
        // Arrange
        var userId = "mock-user-id";
        var banks = new List<Bank>
        {
            new("Bank 1", BankAccountType.Check, 1, userId),
            new("Bank 2", BankAccountType.Savings, 2, userId),
            new("Bank 3", BankAccountType.Check, 3, userId)
        };
        _repository.GetAll(userId).Returns(Task.FromResult(banks));

        // Act
        var result = await _service.GetAll(userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data[0].AccountType.Should().Be(BankAccountType.Check);
        result.Data[1].AccountType.Should().Be(BankAccountType.Savings);
    }

    [Fact]
    public async Task GetAll_WithNoBanks_ReturnsEmptyList()
    {
        // Arrange
        var userId = "mock-user-id";
        _repository.GetAll(userId).Returns(Task.FromResult(new List<Bank>()));

        // Act
        var result = await _service.GetAll(userId);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidIdAndName_ReturnsTrue()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 1;
        var bank = new Bank("Old Name", BankAccountType.Check, bankId, userId);
        var request = new UpdateBankRequest("New Name", BankAccountType.Savings);
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));
        _repository.Update(Arg.Any<Bank>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Update(bankId, request, userId);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(Arg.Any<Bank>());
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 999;
        var request = new UpdateBankRequest("New Name", BankAccountType.Savings);
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)null));

        // Act
        var result = await _service.Update(bankId, request, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Bank>());
    }

    [Fact]
    public async Task Update_ChangesAccountType()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        var request = new UpdateBankRequest("Test Bank", BankAccountType.Savings);
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));
        _repository.Update(Arg.Any<Bank>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Update(bankId, request, userId);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(Arg.Any<Bank>());
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        bank.Deactivate();
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));
        _repository.Update(Arg.Any<Bank>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Activate(bankId, userId);

        // Assert
        result.Should().BeTrue();
        bank.IsActive.Should().BeTrue();
        _repository.Received(1).Update(bank);
    }

    [Fact]
    public async Task Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 999;
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)null));

        // Act
        var result = await _service.Activate(bankId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Bank>());
    }

    [Fact]
    public async Task Activate_UpdatesTimestamp()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        bank.Deactivate();
        var originalUpdatedAt = bank.UpdatedAt ?? DateTime.MinValue;
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));
        _repository.Update(Arg.Any<Bank>()).Returns(Task.FromResult(true));

        // Act
        await _service.Activate(bankId, userId);

        // Assert
        bank.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public async Task Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));
        _repository.Update(Arg.Any<Bank>()).Returns(Task.FromResult(true));

        // Act
        var result = await _service.Deactivate(bankId, userId);

        // Assert
        result.Should().BeTrue();
        bank.IsActive.Should().BeFalse();
        _repository.Received(1).Update(bank);
    }

    [Fact]
    public async Task Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 999;
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)null));

        // Act
        var result = await _service.Deactivate(bankId, userId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Bank>());
    }

    [Fact]
    public async Task Deactivate_UpdatesTimestamp()
    {
        // Arrange
        var userId = "mock-user-id";
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId, userId);
        var originalUpdatedAt = bank.UpdatedAt ?? DateTime.MinValue;
        _repository.GetById(bankId, userId).Returns(Task.FromResult((Bank?)bank));
        _repository.Update(Arg.Any<Bank>()).Returns(Task.FromResult(true));

        // Act
        await _service.Deactivate(bankId, userId);

        // Assert
        bank.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    #endregion

    #region Helper Methods

    private void SetupHttpContextAccessor()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }

    #endregion
}

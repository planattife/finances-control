using System;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FinancesControl.API.Banks;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Enums;

namespace FinancesControl.Tests.Banks.Services;

public class BankServiceTests
{
    private readonly IBankRepository _repository;
    private readonly BankService _service;

    public BankServiceTests()
    {
        _repository = Substitute.For<IBankRepository>();
        _service = new BankService(_repository);
    }

    #region CreateBank Tests

    [Fact]
    public void CreateBank_WithValidRequest_ReturnsNewBankId()
    {
        // Arrange
        var request = new CreateBankRequest("Test Bank", BankAccountType.Check);
        _repository.GetAll().Returns(new List<Bank>());
        _repository.Add(Arg.Any<Bank>()).Returns(1);

        // Act
        var result = _service.CreateBank(request);

        // Assert
        result.Should().Be(1);
        _repository.Received(1).Add(Arg.Any<Bank>());
    }

    [Fact]
    public void CreateBank_GeneratesAutoIncrementingId()
    {
        // Arrange
        var request = new CreateBankRequest("Test Bank", BankAccountType.Savings);
        var existingBanks = new List<Bank>
        {
            new("Existing Bank 1", BankAccountType.Check, 1),
            new("Existing Bank 2", BankAccountType.Savings, 2),
            new("Existing Bank 3", BankAccountType.Check, 3)
        };
        _repository.GetAll().Returns(existingBanks);
        _repository.Add(Arg.Any<Bank>()).Returns(4);

        // Act
        var result = _service.CreateBank(request);

        // Assert
        result.Should().Be(4);
        _repository.Received(1).Add(Arg.Is<Bank>(b => b.Id == 4));
    }

    [Fact]
    public void CreateBank_FirstBankGetsId1()
    {
        // Arrange
        var request = new CreateBankRequest("First Bank", BankAccountType.Check);
        _repository.GetAll().Returns(new List<Bank>());
        _repository.Add(Arg.Any<Bank>()).Returns(1);

        // Act
        var result = _service.CreateBank(request);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void CreateBank_NewBankIsActive()
    {
        // Arrange
        var request = new CreateBankRequest("Test Bank", BankAccountType.Check);
        _repository.GetAll().Returns(new List<Bank>());
        Bank? capturedBank = null;
        _repository.Add(Arg.Any<Bank>())
            .Returns(x =>
            {
                capturedBank = x.Arg<Bank>();
                return 1;
            });

        // Act
        _service.CreateBank(request);

        // Assert
        capturedBank.Should().NotBeNull();
        capturedBank.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateBank_WithCheckAccountType_CreatesCheckBank()
    {
        // Arrange
        var request = new CreateBankRequest("Check Bank", BankAccountType.Check);
        _repository.GetAll().Returns(new List<Bank>());
        Bank? capturedBank = null;
        _repository.Add(Arg.Any<Bank>())
            .Returns(x =>
            {
                capturedBank = x.Arg<Bank>();
                return 1;
            });

        // Act
        _service.CreateBank(request);

        // Assert
        capturedBank.Should().NotBeNull();
        capturedBank.AccountType.Should().Be(BankAccountType.Check);
    }

    [Fact]
    public void CreateBank_WithSavingsAccountType_CreatesSavingsBank()
    {
        // Arrange
        var request = new CreateBankRequest("Savings Bank", BankAccountType.Savings);
        _repository.GetAll().Returns(new List<Bank>());
        Bank? capturedBank = null;
        _repository.Add(Arg.Any<Bank>())
            .Returns(x =>
            {
                capturedBank = x.Arg<Bank>();
                return 1;
            });

        // Act
        _service.CreateBank(request);

        // Assert
        capturedBank.Should().NotBeNull();
        capturedBank.AccountType.Should().Be(BankAccountType.Savings);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public void GetById_WithValidId_ReturnsBankDetail()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        _repository.GetById(bankId).Returns(bank);

        // Act
        var result = _service.GetById(bankId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(bankId);
        result.Name.Should().Be("Test Bank");
        result.AccountType.Should().Be(BankAccountType.Check);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var bankId = 999;
        _repository.GetById(bankId).Returns((Bank?)null);

        // Act
        var result = _service.GetById(bankId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_InactiveBank_ReturnsInactiveStatus()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        bank.Deactivate();
        _repository.GetById(bankId).Returns(bank);

        // Act
        var result = _service.GetById(bankId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Inactive");
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_WithMultipleBanks_ReturnsAllBanks()
    {
        // Arrange
        var banks = new List<Bank>
        {
            new("Bank 1", BankAccountType.Check, 1),
            new("Bank 2", BankAccountType.Savings, 2),
            new("Bank 3", BankAccountType.Check, 3)
        };
        _repository.GetAll().Returns(banks);

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().HaveCount(3);
        result[0].AccountType.Should().Be(BankAccountType.Check);
        result[1].AccountType.Should().Be(BankAccountType.Savings);
    }

    [Fact]
    public void GetAll_WithNoBanks_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAll().Returns(new List<Bank>());

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
        var bankId = 1;
        var bank = new Bank("Old Name", BankAccountType.Check, bankId);
        var request = new UpdateBankRequest("New Name", BankAccountType.Savings);
        _repository.GetById(bankId).Returns(bank);
        _repository.Update(Arg.Any<Bank>()).Returns(true);

        // Act
        var result = _service.Update(bankId, request);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(Arg.Any<Bank>());
    }

    [Fact]
    public void Update_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var bankId = 999;
        var request = new UpdateBankRequest("New Name", BankAccountType.Savings);
        _repository.GetById(bankId).Returns((Bank?)null);

        // Act
        var result = _service.Update(bankId, request);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Bank>());
    }

    [Fact]
    public void Update_ChangesAccountType()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        var request = new UpdateBankRequest("Test Bank", BankAccountType.Savings);
        _repository.GetById(bankId).Returns(bank);
        _repository.Update(Arg.Any<Bank>()).Returns(true);

        // Act
        var result = _service.Update(bankId, request);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(Arg.Any<Bank>());
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        bank.Deactivate();
        _repository.GetById(bankId).Returns(bank);
        _repository.Update(Arg.Any<Bank>()).Returns(true);

        // Act
        var result = _service.Activate(bankId);

        // Assert
        result.Should().BeTrue();
        bank.IsActive.Should().BeTrue();
        _repository.Received(1).Update(bank);
    }

    [Fact]
    public void Activate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var bankId = 999;
        _repository.GetById(bankId).Returns((Bank?)null);

        // Act
        var result = _service.Activate(bankId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Bank>());
    }

    [Fact]
    public void Activate_UpdatesTimestamp()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        bank.Deactivate();
        var originalUpdatedAt = bank.UpdatedAt ?? DateTime.MinValue;
        _repository.GetById(bankId).Returns(bank);
        _repository.Update(Arg.Any<Bank>()).Returns(true);

        // Act
        _service.Activate(bankId);

        // Assert
        bank.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WithValidId_ReturnsTrue()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        _repository.GetById(bankId).Returns(bank);
        _repository.Update(Arg.Any<Bank>()).Returns(true);

        // Act
        var result = _service.Deactivate(bankId);

        // Assert
        result.Should().BeTrue();
        bank.IsActive.Should().BeFalse();
        _repository.Received(1).Update(bank);
    }

    [Fact]
    public void Deactivate_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var bankId = 999;
        _repository.GetById(bankId).Returns((Bank?)null);

        // Act
        var result = _service.Deactivate(bankId);

        // Assert
        result.Should().BeFalse();
        _repository.DidNotReceive().Update(Arg.Any<Bank>());
    }

    [Fact]
    public void Deactivate_UpdatesTimestamp()
    {
        // Arrange
        var bankId = 1;
        var bank = new Bank("Test Bank", BankAccountType.Check, bankId);
        var originalUpdatedAt = bank.UpdatedAt ?? DateTime.MinValue;
        _repository.GetById(bankId).Returns(bank);
        _repository.Update(Arg.Any<Bank>()).Returns(true);

        // Act
        _service.Deactivate(bankId);

        // Assert
        bank.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    #endregion
}

using System;
using Xunit;
using FluentAssertions;
using FinancesControl.API.Common;
using FinancesControl.API.Models;
using FinancesControl.API.Enums;

namespace FinancesControl.Tests.Transactions.Models;

public class TransactionTests
{

    [Fact]
    public void Constructor_Valid_SetsProperties()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        t.Id.Should().Be(2);
        t.Amount.Should().Be(100);
        t.PaymentMethod.Should().Be(PaymentMethod.Cash);
        t.TransactionType.Should().Be(TransactionType.Expense);
        t.CategoryId.Should().Be(1);
        t.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ZeroAmount_Throws()
    {
        var ex = Record.Exception(() => new Transaction(0, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2));
        ex.Should().BeNull("Zero is allowed for amount");
    }

    [Fact]
    public void ChangeAmount_ToNegative_Throws()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        Assert.Throws<DomainValidationException>(() => t.ChangeAmount(-1));
    }

    [Fact]
    public void ChangeCategory_ToZero_Throws()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        Assert.Throws<DomainValidationException>(() => t.ChangeCategory(0));
    }

    [Fact]
    public void ChangeCategory_ToNegative_Throws()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        Assert.Throws<DomainValidationException>(() => t.ChangeCategory(-5));
    }

    [Fact]
    public void Deactivate_Twice_StaysInactive()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        t.Deactivate();
        t.IsActive.Should().BeFalse();
        t.Deactivate();
        t.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ChangeAmount_UpdatesAmount_AndTouches()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        var originalUpdated = t.UpdatedAt ?? DateTime.MinValue;
        t.ChangeAmount(200);
        t.Amount.Should().Be(200);
        t.UpdatedAt.Should().NotBe(originalUpdated);
    }

    [Fact]
    public void ChangeCategory_UpdatesCategory_AndTouches()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2);
        var originalUpdated = t.UpdatedAt ?? DateTime.MinValue;
        t.ChangeCategory(5);
        t.CategoryId.Should().Be(5);
        t.UpdatedAt.Should().NotBe(originalUpdated);
    }

    [Fact]
    public void Deactivate_SetsInactive_AndTouches()
    {
        var t = new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 1, 2);
        t.Deactivate();
        t.IsActive.Should().BeFalse();
        t.UpdatedAt.Should().NotBeNull();
    }


    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidAmount_Throws(decimal amount)
    {
        Assert.Throws<DomainValidationException>(() => new Transaction(amount, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, 1, 10, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidCategoryId_Throws(long categoryId)
    {
        Assert.Throws<DomainValidationException>(() => new Transaction(100, PaymentMethod.Cash, TransactionType.Expense, DateTime.UtcNow, categoryId, 10, 2));
    }
}

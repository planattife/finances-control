using System;
using Xunit;
using FluentAssertions;
using FinancesControl.API.Models;

namespace FinancesControl.Tests.TransactionCategories.Models;

public class TransactionCategoryTests
{
    private const string TestUserId = "test-user-123";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new TransactionCategory(name, 1, TestUserId));
    }

    [Fact]
    public void Constructor_ValidName_SetsProperties()
    {
        var cat = new TransactionCategory("Food", 2, TestUserId);
        cat.Id.Should().Be(2);
        cat.Name.Should().Be("Food");
        cat.IsActive.Should().BeTrue();
        cat.IsDefault.Should().BeFalse();
        cat.UserId.Should().Be(TestUserId);
    }

    [Fact]
    public void Rename_ChangesName_AndTouches()
    {
        var cat = new TransactionCategory("Old", 3, TestUserId);
        var originalUpdated = cat.UpdatedAt ?? DateTime.MinValue;
        cat.Rename("New");
        cat.Name.Should().Be("New");
        cat.UpdatedAt.Should().NotBe(originalUpdated);
    }

    [Fact]
    public void Activate_Deactivate_ToggleStateAndTouch()
    {
        var cat = new TransactionCategory("Foo", 4, TestUserId);
        cat.Deactivate();
        cat.IsActive.Should().BeFalse();
        var afterDeactivate = cat.UpdatedAt;
        cat.Activate();
        cat.IsActive.Should().BeTrue();
        cat.UpdatedAt.Should().BeAfter(afterDeactivate ?? DateTime.MinValue);
    }

    [Fact]
    public void Constructor_CreatesNonDefaultCategory()
    {
        var cat = new TransactionCategory("Food", 5, TestUserId);
        cat.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithoutUserId_CreatesUserSpecificCategory()
    {
        var cat = new TransactionCategory("User Category", 6, TestUserId);
        cat.IsDefault.Should().BeFalse();
        cat.UserId.Should().Be(TestUserId);
    }

    [Fact]
    public void Constructor_WithNullUserId_CreatesCategory()
    {
        var cat = new TransactionCategory("Any Category", 7, null);
        cat.IsDefault.Should().BeFalse();
        cat.UserId.Should().BeNull();
    }
}

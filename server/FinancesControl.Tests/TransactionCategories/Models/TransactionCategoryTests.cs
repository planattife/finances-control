using System;
using Xunit;
using FluentAssertions;
using FinancesControl.API.Models;

namespace FinancesControl.Tests.TransactionCategories.Models;

public class TransactionCategoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new TransactionCategory(name, 1));
    }

    [Fact]
    public void Constructor_ValidName_SetsProperties()
    {
        var cat = new TransactionCategory("Food", 2);
        cat.Id.Should().Be(2);
        cat.Name.Should().Be("Food");
        cat.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Rename_ChangesName_AndTouches()
    {
        var cat = new TransactionCategory("Old", 3);
        var originalUpdated = cat.UpdatedAt ?? DateTime.MinValue;
        cat.Rename("New");
        cat.Name.Should().Be("New");
        cat.UpdatedAt.Should().NotBe(originalUpdated);
    }

    [Fact]
    public void Activate_Deactivate_ToggleStateAndTouch()
    {
        var cat = new TransactionCategory("Foo", 4);
        cat.Deactivate();
        cat.IsActive.Should().BeFalse();
        var afterDeactivate = cat.UpdatedAt;
        cat.Activate();
        cat.IsActive.Should().BeTrue();
        cat.UpdatedAt.Should().BeAfter(afterDeactivate ?? DateTime.MinValue);
    }
}

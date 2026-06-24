using System;
using System.Security.Claims;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using FinancesControl.API.Common;
using FinancesControl.API.TransactionCategories.Controllers;
using FinancesControl.API.TransactionCategories.Services;
using FinancesControl.API.TransactionCategories.DTOs;

namespace FinancesControl.Tests.TransactionCategories.Controllers;

public class TransactionCategoryControllerTests
{
    private readonly ITransactionCategoryService _service;
    private readonly TransactionCategoryController _controller;
    private const string TestUserId = "test-user-123";

    public TransactionCategoryControllerTests()
    {
        _service = Substitute.For<ITransactionCategoryService>();
        _controller = new TransactionCategoryController(_service);
        SetupControllerUser();
    }

    private void SetupControllerUser()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateTransactionCategoryRequest("Food");
        long transactionCategoryId = 10;
        _service.CreateCategory(request, TestUserId).Returns(transactionCategoryId);

        var result = await _controller.Create(request);

        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created.ActionName.Should().Be(nameof(_controller.GetById));
        created.RouteValues["categoryId"].Should().Be(transactionCategoryId);
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsOk()
    {
        var detail = new TransactionCategoryDetailResponse(1, "Food", "Active", DateTime.UtcNow, null);
        var response = new SingleResponse<TransactionCategoryDetailResponse> { Data = detail };
        response.SetHttpStatusCode(200);
        _service.GetById(1, TestUserId).Returns(Task.FromResult(response));

        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNotFound()
    {
        var response = new SingleResponse<TransactionCategoryDetailResponse>();
        response.AddError("Category not found.");
        response.SetHttpStatusCode(404);
        _service.GetById(999, TestUserId).Returns(Task.FromResult(response));

        var result = await _controller.GetById(999);
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(404);
        objectResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Get_ReturnsList()
    {
        var paginationParams = new PaginationParams();
        var list = new List<TransactionCategoryListItemResponse>
        {
            new(1, "Food", true),
            new(2, "Bills", false)
        };
        var response = new PaginatedResponse<TransactionCategoryListItemResponse> { Data = list };
        response.SetPagination(list.Count, 1, 1, false, false);
        response.SetHttpStatusCode(200);
        _service.GetAll(TestUserId, paginationParams).Returns(Task.FromResult(response));

        var result = await _controller.Get(paginationParams);
        var ok = result.Result as OkObjectResult;
        ok.Value.Should().Be(response);
        var returnedResponse = ok.Value as PaginatedResponse<TransactionCategoryListItemResponse>;
        returnedResponse.Data.Should().HaveCount(2);
        returnedResponse.Data[0].Name.Should().Be("Food");
        returnedResponse.Data[0].IsDefault.Should().BeTrue();
        returnedResponse.Data[1].Name.Should().Be("Bills");
        returnedResponse.Data[1].IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task Get_WithPaginationParams_PassesPaginationParamsToService()
    {
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<TransactionCategoryListItemResponse>
        {
            Data = new List<TransactionCategoryListItemResponse>()
        };
        response.SetPagination(12, 3, 2, true, true);
        response.SetHttpStatusCode(200);
        _service.GetAll(TestUserId, paginationParams).Returns(Task.FromResult(response));

        await _controller.Get(paginationParams);

        await _service.Received(1).GetAll(TestUserId, paginationParams);
    }

    [Fact]
    public async Task Get_WithPaginationResponse_ReturnsPaginationMetadata()
    {
        var paginationParams = new PaginationParams { PageSize = 5, PageNumber = 2 };
        var response = new PaginatedResponse<TransactionCategoryListItemResponse>
        {
            Data = new List<TransactionCategoryListItemResponse>
            {
                new(6, "Transport", false)
            }
        };
        response.SetPagination(12, 3, 2, true, true);
        response.SetHttpStatusCode(200);
        _service.GetAll(TestUserId, paginationParams).Returns(Task.FromResult(response));

        var result = await _controller.Get(paginationParams);

        var ok = result.Result as OkObjectResult;
        var returnedResponse = ok.Value as PaginatedResponse<TransactionCategoryListItemResponse>;
        returnedResponse.TotalCount.Should().Be(12);
        returnedResponse.TotalPages.Should().Be(3);
        returnedResponse.CurrentPage.Should().Be(2);
        returnedResponse.HasNextPage.Should().BeTrue();
        returnedResponse.HasPrevPage.Should().BeTrue();
    }

    [Fact]
    public async Task Rename_ValidId_ReturnsNoContent()
    {
        var req = new RenameTransactionCategoryRequest("NewName");
        _service.Rename(2, req, TestUserId).Returns(true);
        var result = await _controller.Rename(2, req);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Rename_InvalidId_ReturnsNotFound()
    {
        var req = new RenameTransactionCategoryRequest("NewName");
        _service.Rename(999, req, TestUserId).Returns(false);
        var result = await _controller.Rename(999, req);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activate_ValidId_ReturnsNoContent()
    {
        _service.Activate(3, TestUserId).Returns(true);
        var result = await _controller.Activate(3);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Deactivate_ValidId_ReturnsNoContent()
    {
        _service.Deactivate(3, TestUserId).Returns(true);
        var result = await _controller.Deactivate(3);
        result.Should().BeOfType<NoContentResult>();
    }
}

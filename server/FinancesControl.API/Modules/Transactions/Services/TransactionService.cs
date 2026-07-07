using FinancesControl.API.Accounts.Repository;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Cards.Services;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;
using FinancesControl.API.TransactionCategories.Services;
using FinancesControl.API.Transactions.Repository;
using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Common;
using FinancesControl.API.Accounts.Services;

namespace FinancesControl.API.Transactions.Services;

public class TransactionService : ITransactionService
{
	private readonly ITransactionRepository _repository;
	private readonly IAccountService _accountService;
	private readonly ITransactionCategoryService _categoryService;
	private readonly ICardService _cardService;
	private readonly IBankService _bankService;

	public TransactionService(
		ITransactionRepository repository,
		IAccountService accountService,
		ITransactionCategoryService categoryService,
		ICardService cardService,
		IBankService bankService)
	{
		_repository = repository;
		_accountService = accountService;
		_categoryService = categoryService;
		_cardService = cardService;
		_bankService = bankService;
	}

	public async Task<long> CreateTransaction(CreateTransactionRequest request, long accountId, string? userId)
	{
		// Validate that URL accountId matches request AccountId
		if (accountId != request.AccountId)
			throw new DomainValidationException("Invalid account. URL account ID does not match request account ID.");

		// Validate foreign key constraints before attempting insert
		await ValidateForeignKeys(request, accountId, userId);

		Transaction transaction;

		switch (request.PaymentMethod)
		{
			case PaymentMethod.Credit:
			case PaymentMethod.Debit:
				if (request.CardId == null || request.CardId <= 0)
					throw new DomainValidationException("Card must be provided for card transactions.");
				transaction = new CardTransaction(request.Amount, request.PaymentMethod, request.TransactionType,  request.TransactionDate, request.CategoryId, request.CardId.Value, request.AccountId, 0, request.Description);
				break;
			case PaymentMethod.Pix:
			case PaymentMethod.BankSlip:
			case PaymentMethod.BankTransfer:
				if (request.BankId == null || request.BankId <= 0)
					throw new DomainValidationException("Bank must be provided for general transactions.");
				transaction = new GeneralTransaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.BankId.Value, request.AccountId, 0, request.Description);
				break;
			case PaymentMethod.Cash:
				transaction = new Transaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.AccountId, 0, request.Description);
				break;
			default:
				throw new DomainValidationException("Unsupported payment method.");
		}
		return await _repository.Add(transaction);
	}

	private async Task ValidateForeignKeys(CreateTransactionRequest request, long accountId, string? userId)
    {
        // Validate Account exists
        await ValidateAccount(accountId, userId);
        var authenticatedUserId = userId!;

        // Validate Category exists
        if (request.CategoryId <= 0)
            throw new DomainValidationException("CategoryId must be greater than 0.");

        if (!await _categoryService.ExistsAsync(request.CategoryId))
            throw new ResourceNotFoundException($"Transaction category with ID {request.CategoryId} does not exist.");

        // Validate Card exists (if applicable)
        if (request.PaymentMethod == PaymentMethod.Credit || request.PaymentMethod == PaymentMethod.Debit)
        {
            if (request.CardId == null || request.CardId <= 0)
                throw new DomainValidationException("Card ID must be provided for credit/debit card transactions.");

			var cardResponse = await _cardService.GetById(request.CardId.Value, accountId, authenticatedUserId);
            if (cardResponse.Data == null)
                throw new ResourceNotFoundException($"Card with ID {request.CardId} does not exist.");

            if (cardResponse.Data.Status == "Inactive")
                throw new BusinessRuleException($"Card with ID {request.CardId} is not active and cannot be used for transactions.");

            if ((request.PaymentMethod == PaymentMethod.Credit && cardResponse.Data.CardType != CardType.Credit) ||
                (request.PaymentMethod == PaymentMethod.Debit && cardResponse.Data.CardType != CardType.Debit))
            {
                throw new BusinessRuleException($"Card with ID {request.CardId} does not match the specified payment method.");
            }
        }

        // Validate Bank exists (if applicable)
        if (request.PaymentMethod == PaymentMethod.Pix || request.PaymentMethod == PaymentMethod.BankSlip || request.PaymentMethod == PaymentMethod.BankTransfer)
        {
            if (request.BankId == null || request.BankId <= 0)
                throw new DomainValidationException("Bank ID must be provided for pix/bank slip/bank transfer transactions.");

            if (!await _bankService.ExistsAsync(request.BankId.Value, authenticatedUserId))
                throw new ResourceNotFoundException($"Bank with ID {request.BankId} does not exist.");
        }
    }

    private async Task ValidateAccount(long accountId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new AuthenticationContextException("User context is not available.");

        if (!await _accountService.ExistsAsync(accountId, userId))
            throw new ResourceNotFoundException("Account not found.");
    }

	public async Task<SingleResponse<TransactionDetailResponse>> GetById(long transactionId, long accountId, string? userId)
	{
		var response = new SingleResponse<TransactionDetailResponse>();

		try
		{
			await ValidateAccount(accountId, userId);
		}
		catch (ResourceNotFoundException ex)
		{
			response.AddError(ex.Message);
			response.SetHttpStatusCode(404);
			return response;
		}

		var t = await _repository.GetById(transactionId, accountId);
		if (t == null)
		{
			response.AddError("Transaction not found.");
			response.SetHttpStatusCode(404);
			return response;
		}

		var detail = new TransactionDetailResponse
		{
			Id = t.Id,
			Amount = t.Amount,
			PaymentMethod = t.PaymentMethod.ToString(),
			TransactionType = t.TransactionType.ToString(),
			TransactionDate = t.TransactionDate,
			Category = new TransactionCategoryResponse
			{
				CategoryId = t.CategoryId,
				CategoryName = t.CategoryName
			},
			Description = t.Description,
			Status = t.IsActive ? "Active" : "Inactive",
			CreatedAt = t.CreatedAt,
			UpdatedAt = t.UpdatedAt ?? null
		};
		if (t is CardTransaction ct)
		{
			detail.Card = new TransactionCardResponse
			{
				CardId = ct.CardId,
				CardName = ct.CardName ?? string.Empty
			};
		}
		if (t is GeneralTransaction gt)
		{
			detail.Bank = new TransactionBankResponse
			{
				BankId = gt.BankId,
				BankName = gt.BankName ?? string.Empty
			};
		}

		response.Data = detail;
		response.SetHttpStatusCode(200);
		return response;
	}

	public async Task<PaginatedResponse<TransactionListItemResponse>> GetAll(long accountId, string? userId, PaginationParams paginationParams)
	{
		var response = new PaginatedResponse<TransactionListItemResponse>();

		try
		{
			await ValidateAccount(accountId, userId);
		}
		catch (ResourceNotFoundException ex)
		{
			response.AddError(ex.Message);
			response.SetHttpStatusCode(404);
			return response;
		}

		var transactions = await _repository.GetAll(accountId, paginationParams);
		var totalCount = await _repository.CountByAccountId(accountId);
		var data = transactions
			.Select(t => new TransactionListItemResponse
			{
				Id = t.Id,
				Amount = t.Amount,
				PaymentMethod = t.PaymentMethod.ToString(),
				TransactionType = t.TransactionType.ToString(),
				TransactionDate = t.TransactionDate,
				Category = new TransactionCategoryResponse
				{
					CategoryId = t.CategoryId,
					CategoryName = t.CategoryName
				},
				Status = t.IsActive ? "Active" : "Inactive"
			})
			.ToList();

		response.Data = data;
		var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);
		var hasNextPage = paginationParams.PageNumber < totalPages;
		var hasPrevPage = paginationParams.PageNumber > 1;

		response.SetPagination(totalCount, totalPages, paginationParams.PageNumber, hasNextPage, hasPrevPage);
		response.SetHttpStatusCode(200);
		return response;
	}

	public async Task<bool> Delete(long transactionId, long accountId, string? userId)
	{
		try
		{
			await ValidateAccount(accountId, userId);
		}
		catch (ResourceNotFoundException)
		{
			return false;
		}

		var transaction = await _repository.GetById(transactionId, accountId);
		if (transaction == null) return false;
		transaction.Deactivate();
		return await _repository.Update(transaction);
	}

	public async Task<bool> Update(long transactionId, long accountId, UpdateTransactionRequest request, string? userId)
	{
		try
		{
			await ValidateAccount(accountId, userId);
		}
		catch (ResourceNotFoundException)
		{
			return false;
		}
		
		var transaction = await _repository.GetById(transactionId, accountId);
		if (transaction == null) return false;

		if (request.NewAmount.HasValue && request.NewAmount != 0)
		{
			transaction.ChangeAmount(request.NewAmount.Value);
		}

		if (request.NewCategoryId.HasValue && request.NewCategoryId > 0)
		{
			if (!await _categoryService.ExistsAsync(request.NewCategoryId.Value))
				throw new ResourceNotFoundException($"Transaction category with ID {request.NewCategoryId.Value} does not exist.");
			transaction.ChangeCategory(request.NewCategoryId.Value);
		}

		if (request.HasDescriptionUpdate)
		{
			transaction.ChangeDescription(request.NewDescription);
		}

		return await _repository.Update(transaction);
	}

	public async Task<bool> ExistsAsync(long transactionId, long accountId)
	{
		return await _repository.ExistsAsync(transactionId, accountId);
	}
}

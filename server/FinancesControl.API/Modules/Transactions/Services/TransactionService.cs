using FinancesControl.API.Accounts.Repository;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Cards.Repository;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;
using FinancesControl.API.TransactionCategories.Repository;
using FinancesControl.API.Transactions.Repository;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public class TransactionService : ITransactionService
{
	private readonly ITransactionRepository _repository;
	private readonly IAccountRepository _accountRepository;
	private readonly ITransactionCategoryRepository _categoryRepository;
	private readonly ICardRepository _cardRepository;
	private readonly IBankRepository _bankRepository;

	public TransactionService(
		ITransactionRepository repository,
		IAccountRepository accountRepository,
		ITransactionCategoryRepository categoryRepository,
		ICardRepository cardRepository,
		IBankRepository bankRepository)
	{
		_repository = repository;
		_accountRepository = accountRepository;
		_categoryRepository = categoryRepository;
		_cardRepository = cardRepository;
		_bankRepository = bankRepository;
	}

	public long CreateTransaction(CreateTransactionRequest request, long accountId, string? userId)
	{
		// Validate that URL accountId matches request AccountId
		if (accountId != request.AccountId)
			throw new ArgumentException("Invalid account. URL account ID does not match request account ID.");

		// Validate foreign key constraints before attempting insert
		ValidateForeignKeys(request, accountId, userId);

		Transaction transaction;

		switch (request.PaymentMethod)
		{
			case PaymentMethod.Credit:
			case PaymentMethod.Debit:
				if (request.CardId == null || request.CardId <= 0)
					throw new ArgumentException("Card must be provided for card transactions.");
				transaction = new CardTransaction(request.Amount, request.PaymentMethod, request.TransactionType,  request.TransactionDate, request.CategoryId, request.CardId.Value, request.AccountId, 0, request.Description);
				break;
			case PaymentMethod.Pix:
			case PaymentMethod.BankSlip:
			case PaymentMethod.BankTransfer:
				if (request.BankId == null || request.BankId <= 0)
					throw new ArgumentException("Bank must be provided for general transactions.");
				transaction = new GeneralTransaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.BankId.Value, request.AccountId, 0, request.Description);
				break;
			case PaymentMethod.Cash:
				transaction = new Transaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.AccountId, 0, request.Description);
				break;
			default:
				throw new ArgumentException("Unsupported payment method");
		}
		return _repository.Add(transaction);
	}

	private void ValidateForeignKeys(CreateTransactionRequest request, long accountId, string? userId)
    {
        // Validate Account exists
        ValidateAccount(accountId, userId);

        // Validate Category exists
        if (_categoryRepository.GetById(request.CategoryId, userId) == null)
            throw new ArgumentException($"Transaction category with ID {request.CategoryId} does not exist.", nameof(request.CategoryId));

        // Validate Card exists (if applicable)
        if (request.PaymentMethod == PaymentMethod.Credit || request.PaymentMethod == PaymentMethod.Debit)
        {
            if (request.CardId == null || request.CardId <= 0)
                throw new ArgumentException("Card ID must be provided for credit/debit card transactions.", nameof(request.CardId));

			var card = _cardRepository.GetById(request.CardId.Value, accountId);
            if (card == null) {
                throw new ArgumentException($"Card with ID {request.CardId} does not exist.", nameof(request.CardId));
			} else {
				if (card.AccountId != accountId)
					throw new ArgumentException($"Card with ID {request.CardId} does not exist in this Account.", nameof(request.CardId));

				if ((request.PaymentMethod == PaymentMethod.Credit && card.CardType != CardType.Credit) ||
					(request.PaymentMethod == PaymentMethod.Debit && card.CardType != CardType.Debit))
				{
					throw new ArgumentException($"Card with ID {request.CardId} does not match the specified payment method.", nameof(request.CardId));
				}
			}
        }

        // Validate Bank exists (if applicable)
        if (request.PaymentMethod == PaymentMethod.Pix || request.PaymentMethod == PaymentMethod.BankSlip || request.PaymentMethod == PaymentMethod.BankTransfer)
        {
            if (request.BankId == null || request.BankId <= 0)
                throw new ArgumentException("Bank ID must be provided for pix/bank slip/bank transfer transactions.", nameof(request.BankId));

            if (_bankRepository.GetById(request.BankId.Value, userId) == null)
                throw new ArgumentException($"Bank with ID {request.BankId} does not exist.", nameof(request.BankId));
        }
    }

    private void ValidateAccount(long accountId, string? userId)
    {
        if (userId != null && _accountRepository.GetById(accountId, userId) == null)
            throw new ArgumentException($"Invalid Account.");
    }

    public TransactionDetailResponse? GetById(long transactionId, long accountId, string? userId)
	{
		try
		{
			ValidateAccount(accountId, userId);
		}
		catch (Exception)
		{
			return null;
		}

		ValidateAccount(accountId, userId);
		
		var t = _repository.GetById(transactionId, accountId);
		if (t == null) return null;
		var detail = new TransactionDetailResponse
		{
			Id = t.Id,
			Amount = t.Amount,
			PaymentMethod = t.PaymentMethod.ToString(),
			TransactionType = t.TransactionType.ToString(),
			TransactionDate = t.TransactionDate,
			CategoryId = t.CategoryId,
			Description = t.Description,
			Status = t.IsActive ? "Active" : "Inactive",
			CreatedAt = t.CreatedAt,
			UpdatedAt = t.UpdatedAt ?? null
		};
		if (t is CardTransaction ct)
			detail.CardId = ct.CardId;
		if (t is GeneralTransaction gt)
			detail.BankId = gt.BankId;
		return detail;
	}

	public List<TransactionListItemResponse> GetAll(long accountId, string? userId)
	{
		try
		{
			ValidateAccount(accountId, userId);
		}
		catch (Exception)
		{
			return new List<TransactionListItemResponse>();
		}

		return _repository.GetAll(accountId)
			.Select(t => new TransactionListItemResponse
			{
				Id = t.Id,
				Amount = t.Amount,
				PaymentMethod = t.PaymentMethod.ToString(),
				TransactionType = t.TransactionType.ToString(),
				TransactionDate = t.TransactionDate,
				CategoryId = t.CategoryId,
				Status = t.IsActive ? "Active" : "Inactive"
			})
			.ToList();
	}

	public bool Delete(long transactionId, long accountId, string? userId)
	{
		try
		{
			ValidateAccount(accountId, userId);
		}
		catch (Exception)
		{
			return false;
		}

		var transaction = _repository.GetById(transactionId, accountId);
		if (transaction == null) return false;
		transaction.Deactivate();
		return _repository.Update(transaction);
	}

	public bool Update(long transactionId, long accountId, UpdateTransactionRequest request, string? userId)
	{
		try
		{
			ValidateAccount(accountId, userId);
		}
		catch (Exception)
		{
			return false;
		}
		
		var transaction = _repository.GetById(transactionId, accountId);
		if (transaction == null) return false;

		if (request.NewAmount.HasValue && request.NewAmount != 0)
		{
			transaction.ChangeAmount(request.NewAmount.Value);
		}

		if (request.NewCategoryId.HasValue && request.NewCategoryId > 0)
		{
			if (_categoryRepository.GetById(request.NewCategoryId.Value, userId) == null)
				throw new ArgumentException($"Transaction category with ID {request.NewCategoryId.Value} does not exist.", nameof(request.NewCategoryId));
			transaction.ChangeCategory(request.NewCategoryId.Value);
		}

		if (request.HasDescriptionUpdate)
		{
			transaction.ChangeDescription(request.NewDescription);
		}

		return _repository.Update(transaction);
	}
}

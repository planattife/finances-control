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

	public int CreateTransaction(CreateTransactionRequest request)
	{
		// Validate foreign key constraints before attempting insert
		ValidateForeignKeys(request);

		Transaction transaction;

		switch (request.PaymentMethod)
		{
			case PaymentMethod.Credit:
			case PaymentMethod.Debit:
				if (request.CardId == null || request.CardId <= 0)
					throw new ArgumentException("Card must be provided for card transactions.");
				transaction = new CardTransaction(request.Amount, request.PaymentMethod, request.TransactionType,  request.TransactionDate, request.CategoryId, request.CardId.Value, request.AccountId, 0);
				break;
			case PaymentMethod.Pix:
			case PaymentMethod.BankSlip:
			case PaymentMethod.BankTransfer:
				if (request.BankId == null || request.BankId <= 0)
					throw new ArgumentException("Bank must be provided for general transactions.");
				transaction = new GeneralTransaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.BankId.Value, request.AccountId, 0);
				break;
			case PaymentMethod.Cash:
				transaction = new Transaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.AccountId, 0);
				break;
			default:
				throw new ArgumentException("Unsupported payment method");
		}
		return _repository.Add(transaction);
	}

	private void ValidateForeignKeys(CreateTransactionRequest request)
	{
		// Validate Account exists
		if (_accountRepository.GetById(request.AccountId) == null)
			throw new ArgumentException($"Account with ID {request.AccountId} does not exist.", nameof(request.AccountId));

		// Validate Category exists
		if (_categoryRepository.GetById(request.CategoryId) == null)
			throw new ArgumentException($"Transaction category with ID {request.CategoryId} does not exist.", nameof(request.CategoryId));

		// Validate Card exists (if applicable)
		if (request.PaymentMethod == PaymentMethod.Credit || request.PaymentMethod == PaymentMethod.Debit)
		{
			if (request.CardId == null || request.CardId <= 0)
				throw new ArgumentException("Card ID must be provided for credit/debit card transactions.", nameof(request.CardId));

			if (_cardRepository.GetById(request.CardId.Value) == null)
				throw new ArgumentException($"Card with ID {request.CardId} does not exist.", nameof(request.CardId));
		}

		// Validate Bank exists (if applicable)
		if (request.PaymentMethod == PaymentMethod.Pix || request.PaymentMethod == PaymentMethod.BankSlip || request.PaymentMethod == PaymentMethod.BankTransfer)
		{
			if (request.BankId == null || request.BankId <= 0)
				throw new ArgumentException("Bank ID must be provided for pix/bank slip/bank transfer transactions.", nameof(request.BankId));

			if (_bankRepository.GetById(request.BankId.Value) == null)
				throw new ArgumentException($"Bank with ID {request.BankId} does not exist.", nameof(request.BankId));
		}
	}

	public TransactionDetailResponse? GetById(int transactionId)
	{
		var t = _repository.GetById(transactionId);
		if (t == null) return null;
		var detail = new TransactionDetailResponse
		{
			Id = t.Id,
			Amount = t.Amount,
			PaymentMethod = t.PaymentMethod.ToString(),
			TransactionType = t.TransactionType.ToString(),
			TransactionDate = t.TransactionDate,
			CategoryId = t.CategoryId,
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

	public List<TransactionListItemResponse> GetAll()
	{
		return _repository.GetAll()
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

	public bool Delete(int transactionId)
	{
		var transaction = _repository.GetById(transactionId);
		if (transaction == null) return false;
		transaction.Deactivate();
		return _repository.Update(transaction);
	}

	public bool UpdateAmount(int transactionId, UpdateTransactionAmountRequest request)
	{
		var transaction = _repository.GetById(transactionId);
		if (transaction == null) return false;
		transaction.ChangeAmount(request.NewAmount);
		return _repository.Update(transaction);
	}

	public bool UpdateCategory(int transactionId, UpdateTransactionCategoryRequest request)
	{
		var transaction = _repository.GetById(transactionId);
		if (transaction == null) return false;
		transaction.ChangeCategory(request.NewCategoryId);
		return _repository.Update(transaction);
	}
}

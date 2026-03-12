using FinancesControl.API.Enums;
using FinancesControl.API.Models;
using FinancesControl.API.Transactions.Repository;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public class TransactionService : ITransactionService
{
	private readonly ITransactionRepository _repository;

	public TransactionService(ITransactionRepository repository)
	{
		_repository = repository;
	}

	public int CreateTransaction(CreateTransactionRequest request)
	{
		Transaction transaction;
        var all = _repository.GetAll();
        int newId = all.Count == 0 ? 1 : all.Max(c => c.Id) + 1;

		switch (request.PaymentMethod)
		{
			case PaymentMethod.Credit:
			case PaymentMethod.Debit:
				if (request.CardId == null || request.CardId <= 0)
					throw new ArgumentException("Card must be provided for card transactions.");
				transaction = new CardTransaction(request.Amount, request.PaymentMethod, request.TransactionType,  request.TransactionDate, request.CategoryId, request.CardId.Value, newId);
				break;
			case PaymentMethod.Pix:
			case PaymentMethod.BankSlip:
			case PaymentMethod.BankTransfer:
				if (request.BankId == null || request.BankId <= 0)
					throw new ArgumentException("Bank must be provided for general transactions.");
				transaction = new GeneralTransaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, request.BankId.Value, newId);
				break;
			case PaymentMethod.Cash:
				transaction = new Transaction(request.Amount, request.PaymentMethod, request.TransactionType, request.TransactionDate, request.CategoryId, newId);
				break;
			default:
				throw new ArgumentException("Unsupported payment method");
		}
		return _repository.Add(transaction);
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

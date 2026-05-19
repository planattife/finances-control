using System.Collections.Generic;
using System.Linq;
using Dapper;
using FinancesControl.API.Cards.Repository.SqlCommands;
using FinancesControl.API.Data;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository;

public class CardRepository : ICardRepository
{
    private readonly AppDbConnectionFactory _connectionFactory;

    public CardRepository(AppDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> Add(Card card)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            var command = new CreateCardCommand();
            var (sql, parameters) = command.Build(card);
            
            var id = await connection.QuerySingleOrDefaultAsync<long>(sql, parameters, transaction);
            
            // If it's a credit card, also insert credit card details
            if (card is CreditCard creditCard)
            {
                var creditCommand = new CreateCreditCardDetailsCommand();
                var (creditSql, creditParams) = creditCommand.Build(creditCard, id);
                await connection.ExecuteAsync(creditSql, creditParams, transaction);
            }
            
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Card?> GetById(long cardId, long accountId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetCardByIdQuery();
        var (sql, parameters) = query.Build(cardId, accountId);
        
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        // Check if it's a credit card and get additional details
        if ((int)result.CardType == 1) // CardType.Credit
        {
            var creditQuery = new GetCreditCardByIdQuery();
            var (creditSql, creditParams) = creditQuery.Build(cardId, accountId);
            var creditResult = await connection.QuerySingleOrDefaultAsync<dynamic>(creditSql, creditParams);
            
            if (creditResult != null)
                return MapToCreditCard(creditResult);
        }

        return MapToCard(result);
    }

    public async Task<List<Card>> GetAll(long accountId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllCardsQuery();
        var (sql, parameters) = query.Build(accountId);
        
        var results = (await connection.QueryAsync<dynamic>(sql, parameters)).ToList();
        var cards = new List<Card>();
        
        foreach (var result in results)
        {
            cards.Add(MapToCard(result));
        }
        
        return cards;
    }

    public async Task<bool> Update(Card card, long accountId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        
        try
        {
            var command = new UpdateCardCommand();
            var (sql, parameters) = command.Build(card, accountId);
            
            var rowsAffected = await connection.ExecuteAsync(sql, parameters, transaction);
            
            // If it's a credit card, also update credit card details
            if (card is CreditCard creditCard)
            {
                var creditCommand = new UpdateCreditCardDetailsCommand();
                var (creditSql, creditParams) = creditCommand.Build(creditCard);
                await connection.ExecuteAsync(creditSql, creditParams, transaction);
            }
            
            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Card MapToCard(dynamic row)
    {
        return Card.FromDatabase(
            id: (long)row.Id,
            cardNumber: row.CardNumber,
            expirationDate: row.ExpirationDate,
            cardNetwork: (CardNetwork)row.CardNetwork,
            bankId: (long)row.BankId,
            accountId: (long)row.AccountId,
            cardType: (CardType)row.CardType,
            isActive: row.IsActive,
            cardName: row.CardName,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }

    private static CreditCard MapToCreditCard(dynamic row)
    {
        return CreditCard.FromDatabase(
            id: (long)row.Id,
            cardNumber: row.CardNumber,
            expirationDate: row.ExpirationDate,
            cardNetwork: (CardNetwork)row.CardNetwork,
            bankId: (long)row.BankId,
            accountId: (long)row.AccountId,
            closingDay: row.ClosingDay,
            dueDateDay: row.DueDateDay,
            isActive: row.IsActive,
            cardName: row.CardName,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }
}
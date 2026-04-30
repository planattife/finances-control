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

    public int Add(Card card)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            var command = new CreateCardCommand();
            var (sql, parameters) = command.Build(card);
            
            var id = connection.QuerySingleOrDefault<long>(sql, parameters, transaction);
            
            // If it's a credit card, also insert credit card details
            if (card is CreditCard creditCard)
            {
                var creditCommand = new CreateCreditCardDetailsCommand();
                var (creditSql, creditParams) = creditCommand.Build(creditCard, id);
                connection.Execute(creditSql, creditParams, transaction);
            }
            
            transaction.Commit();
            return (int)id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Card? GetById(int cardId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetCardByIdQuery();
        var (sql, parameters) = query.Build(cardId);
        
        var result = connection.QuerySingleOrDefault<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        // Check if it's a credit card and get additional details
        if ((int)result.CardType == 1) // CardType.Credit
        {
            var creditQuery = new GetCreditCardByIdQuery();
            var (creditSql, creditParams) = creditQuery.Build(cardId);
            var creditResult = connection.QuerySingleOrDefault<dynamic>(creditSql, creditParams);
            
            if (creditResult != null)
                return MapToCreditCard(creditResult);
        }

        return MapToCard(result);
    }

    public List<Card> GetAll()
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllCardsQuery();
        
        var results = connection.Query<dynamic>(query.Sql).ToList();
        var cards = new List<Card>();
        
        foreach (var result in results)
        {
            cards.Add(MapToCard(result));
        }
        
        return cards;
    }

    public bool Update(Card card)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        
        try
        {
            var command = new UpdateCardCommand();
            var (sql, parameters) = command.Build(card);
            
            var rowsAffected = connection.Execute(sql, parameters, transaction);
            
            // If it's a credit card, also update credit card details
            if (card is CreditCard creditCard)
            {
                var creditCommand = new UpdateCreditCardDetailsCommand();
                var (creditSql, creditParams) = creditCommand.Build(creditCard);
                connection.Execute(creditSql, creditParams, transaction);
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
            id: (int)row.Id,
            cardNumber: row.CardNumber,
            expirationDate: row.ExpirationDate,
            cardNetwork: (CardNetwork)row.CardNetwork,
            bankId: (int)row.BankId,
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
            id: (int)row.Id,
            cardNumber: row.CardNumber,
            expirationDate: row.ExpirationDate,
            cardNetwork: (CardNetwork)row.CardNetwork,
            bankId: (int)row.BankId,
            closingDay: row.ClosingDay,
            dueDateDay: row.DueDateDay,
            isActive: row.IsActive,
            cardName: row.CardName,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }
}
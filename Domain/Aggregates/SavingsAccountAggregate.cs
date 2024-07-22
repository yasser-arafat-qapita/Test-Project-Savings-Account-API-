using System.Data;
using Domain.Events;

namespace Domain.Aggregates;

public class SavingsAccountAggregate : AggregateRoot
{
    private string _accountId;
    private decimal _balance;

    public string AccountId => _accountId;
    public decimal Balance => _balance;
    
    public SavingsAccountAggregate()
    {
        
    }

    public SavingsAccountAggregate(string accountId, decimal balance)
    {
        if (string.IsNullOrEmpty(accountId))
            throw new ArgumentNullException(nameof(accountId));
        if (balance < 0)
            throw new InvalidDataException("You cannnot open a Savings Account with negative balance!");
        RaiseEvent(new SavingsAccountCreated
            {
                AccountId = accountId,
                Balance = balance
            });
    }

    public void Apply(SavingsAccountCreated created)
    {
        _accountId = created.AccountId;
        _balance = created.Balance;
    }

    public void UpdateSavingsAccount(decimal amount, string transactionType, long version)
    {
        if (amount == (decimal)0)
            throw new ArgumentException("For Credit/Debit Transaction amount cannot be zero!");
        if (!(transactionType.ToLowerInvariant().Equals("debit") ||
              transactionType.ToLowerInvariant().Equals("credit")))
            throw new ArgumentException($"Invalid TransactionType {transactionType} !");
        if (transactionType.ToLowerInvariant().Equals("debit"))
        {
            if (amount > _balance)
                throw new EvaluateException(
                    $"You don't have sufficient balance : {_balance} to do a Debit Transaction of {amount} !");
        }
        RaiseEvent(new SavingsAccountUpdated
        {
            AccountId = _accountId,
            Amount = amount,
            TransactionType = transactionType,
            Version = version
        });
    }

    public void Apply(SavingsAccountUpdated updated)
    {
        _accountId = updated.AccountId;
        if (updated.TransactionType.ToLowerInvariant().Equals("debit"))
        {
            _balance -= updated.Amount;
        }
        else if (updated.TransactionType.ToLowerInvariant().Equals("credit"))
        {
            _balance += updated.Amount;
        }
    }
}
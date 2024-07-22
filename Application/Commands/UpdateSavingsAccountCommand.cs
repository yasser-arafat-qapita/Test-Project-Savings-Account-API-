
using NServiceBus;

namespace Application.Commands;

public class UpdateSavingsAccountCommand: ICommand
{
    public string AccountId { get; set; }
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
}
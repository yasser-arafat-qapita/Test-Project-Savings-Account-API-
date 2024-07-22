using NServiceBus;

namespace Application.Commands;

public class CreateSavingsAccountCommand: ICommand
{
    public string AccountId { get; set; }
    public decimal Balance { get; set; }
}
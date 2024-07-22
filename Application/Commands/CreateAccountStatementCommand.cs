using NServiceBus;

namespace Application.Commands;

public class CreateAccountStatementCommand : ICommand
{
    public string AccountId { get; set; }
}
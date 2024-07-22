using Application.Commands;
using Application.Services;
using Hangfire;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Application.Handlers;

public class CreateAccountStatementHandler: IHandleMessages<CreateAccountStatementCommand>
{
    private readonly ILogger<CreateAccountStatementHandler> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    public CreateAccountStatementHandler(ILogger<CreateAccountStatementHandler> logger, IBackgroundJobClient backgroundJobClient)
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(CreateAccountStatementCommand message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Account Statement Generation Handler called!");
        _backgroundJobClient.Enqueue<CreateAccountStatementService>(service => service.GenerateAccountStatementAsync(message.AccountId));
        return Task.CompletedTask;
    }
}
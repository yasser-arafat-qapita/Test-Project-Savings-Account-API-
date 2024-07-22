using Application.Commands;
using Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NServiceBus;

namespace SavingsAccountAPI.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class SavingsAccountController : ControllerBase
{
    private readonly IMessageSession _messageSession;
    private readonly ILogger<SavingsAccountController> _logger;

    public SavingsAccountController(ILogger<SavingsAccountController> logger, IMessageSession messageSession)
    {
        _logger = logger;
        _messageSession = messageSession;
    }

    [HttpPost("Create", Name = "createSavingsAccount")]
    public async Task<IActionResult> CreateSavingsAccount([FromBody] CreateSavingsAccountView view)
    {
        if (view.Balance < 0)
        {
            _logger.LogWarning("Negative Balance Account Creation is not Possible!");
            return BadRequest();
        }
        var command = new CreateSavingsAccountCommand
        {
            AccountId = view.AccountId,
            Balance = view.Balance
        };
        await _messageSession.SendLocal(command);
        return Ok();
    }
    
    [HttpPost("Update", Name = "updateSavingsAccount")]
    public async Task<IActionResult> UpdateSavingsAccount([FromBody] UpdateSavingsAccountView view)
    {
        string[] validTransactionTypes = { "debit", "credit" };
        if (view.Amount == 0 || !validTransactionTypes.Contains(view.TransactionType.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid Request : " + view.ToJson().ToString());
            return BadRequest();
        }
        var command = new UpdateSavingsAccountCommand
        {
            AccountId = view.AccountId,
            Amount = view.Amount,
            TransactionType = view.TransactionType
        };
        await _messageSession.SendLocal(command);
        return Ok();
    }
    
    [HttpPost("GenerateAccountStatement", Name = "generateAccountStatement")]
    public async Task<IActionResult> GenerateAccountStatement([FromBody] CreateAccountStatementView view)
    {
  
        if (string.IsNullOrEmpty(view.AccountId))
        {
            _logger.LogWarning("Invalid Request : " + view.ToJson().ToString());
            return BadRequest();
        }
        var command = new CreateAccountStatementCommand
        {
            AccountId = view.AccountId,
        };
        await _messageSession.SendLocal(command);
        return Ok();
    }
}
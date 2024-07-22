using Domain.Aggregates;
using Domain.Events;
using Infrastructure.Extensions;
using Infrastructure.Repository;

namespace Application.Services;
using EventStore.Client;
using Hangfire;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class CreateAccountStatementService
{
    private readonly ILogger<CreateAccountStatementService> _logger;
    private readonly SavingsAccountRepository _savingsAccountRepository;

    public CreateAccountStatementService(EventStoreClient eventStoreClient, ILogger<CreateAccountStatementService> logger, 
        SavingsAccountRepository savingsAccountRepository)
    {
        _savingsAccountRepository = savingsAccountRepository;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task<string> GenerateAccountStatementAsync(string accountId)
    {
        var latestAggregate = await _savingsAccountRepository.GetByIdAsync(accountId);
        var events = latestAggregate.GetUncommitedChanges();
        var filePath = GenerateExcelReport(events, latestAggregate.AccountId);
        _logger.LogInformation($"Account statement generated at: {filePath}");
        return filePath;
    }
    private string GenerateExcelReport(IEnumerable<BaseDomainEvent> events, string accountId)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        string filePath = Path.Combine(desktopPath, $"AccountStatement_{Guid.NewGuid()}.xlsx");

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add($"Account Statement for : {accountId}");

            // Add headers
            worksheet.Cells[1, 1].Value = "EventId";
            worksheet.Cells[1, 2].Value = "Transaction";
            worksheet.Cells[1, 3].Value = "Amount";
            int counter = 0;
            foreach (var @event in events)
            {
                var eventType = @event.Type;
                if (eventType == nameof(SavingsAccountCreated))
                {
                    var eventData = (SavingsAccountCreated)@event;
                    worksheet.Cells[counter + 2, 1].Value = eventData.EventId;
                    worksheet.Cells[counter + 2, 2].Value = "Account Opened";
                    worksheet.Cells[counter + 2, 3].Value = eventData.Balance;
                }else if (eventType == nameof(SavingsAccountUpdated))
                {
                    var eventData = (SavingsAccountUpdated)@event;
                    worksheet.Cells[counter + 2, 1].Value = eventData.EventId;
                    worksheet.Cells[counter + 2, 2].Value = eventData.TransactionType;
                    worksheet.Cells[counter + 2, 3].Value = eventData.Amount;
                }
                else
                    throw new InvalidOperationException($"Unknown event type: {eventType}");
                counter++;
            }
            package.SaveAs(new FileInfo(filePath));
        }
        return filePath;
    }
}

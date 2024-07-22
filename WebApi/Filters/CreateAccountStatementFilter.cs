using Hangfire.Server;

namespace SavingsAccountAPI.Filters;
using Hangfire.Common;
using Microsoft.Extensions.Logging;

public class CreateAccountStatementFilter : JobFilterAttribute, IServerFilter
{
    private readonly ILogger<CreateAccountStatementFilter> _logger;

    public CreateAccountStatementFilter(ILogger<CreateAccountStatementFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        _logger.LogInformation($"Starting Account Statement Excel job: {filterContext.BackgroundJob.Id}");
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        if (filterContext.Exception != null)
        {
            _logger.LogError(filterContext.Exception, $"Job failed: {filterContext.BackgroundJob.Id}");
        }
        else
        {
            _logger.LogInformation($"Account Statement Excel Job completed: {filterContext.BackgroundJob.Id}");
        }
    }
}

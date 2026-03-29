using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Reports.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Audit;

[Authorize(Roles = "ADMIN")]
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;

    public IndexModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    public IReadOnlyCollection<AuditLogDto> Logs { get; private set; } = Array.Empty<AuditLogDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Logs = await _reportService.GetRecentAuditLogsAsync(cancellationToken);
    }
}

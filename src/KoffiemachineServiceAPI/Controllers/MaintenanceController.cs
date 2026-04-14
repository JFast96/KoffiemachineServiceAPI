using KoffiemachineServiceAPI.DTOs;
using KoffiemachineServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KoffiemachineServiceAPI.Controllers;

[ApiController]
[Route("api/machines/{machineId:guid}/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>
    /// Log a maintenance action for a specific machine.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MaintenanceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid machineId, [FromBody] CreateMaintenanceRequest request)
    {
        var action = await _maintenanceService.CreateAsync(machineId, request);
        return CreatedAtAction(nameof(GetByMachineId), new { machineId }, action);
    }

    /// <summary>
    /// Retrieve maintenance actions for a specific machine with optional paging and filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MaintenanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByMachineId(
        Guid machineId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? performedBy = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var result = await _maintenanceService.GetByMachineIdAsync(machineId, page, pageSize, performedBy);
        return Ok(result);
    }
}

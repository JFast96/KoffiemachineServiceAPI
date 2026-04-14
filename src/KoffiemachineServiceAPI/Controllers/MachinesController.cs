using KoffiemachineServiceAPI.DTOs;
using KoffiemachineServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KoffiemachineServiceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMachineService _machineService;

    public MachinesController(IMachineService machineService)
    {
        _machineService = machineService;
    }

    /// <summary>
    /// Register a new coffee machine.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MachineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMachineRequest request)
    {
        var machine = await _machineService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = machine.Id }, machine);
    }

    /// <summary>
    /// Retrieve all coffee machines with optional paging and status filter.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MachineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var result = await _machineService.GetAllAsync(page, pageSize, status);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific coffee machine by its id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MachineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var machine = await _machineService.GetByIdAsync(id);
        if (machine is null)
        {
            return NotFound(new { message = $"Machine with id '{id}' not found." });
        }
        return Ok(machine);
    }

    /// <summary>
    /// Update the status of a coffee machine (with optimistic concurrency control).
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(MachineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var machine = await _machineService.UpdateStatusAsync(id, request);
        return Ok(machine);
    }
}

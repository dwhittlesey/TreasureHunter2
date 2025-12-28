using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Application.Features.TreasureItems.Commands;
using TreasureHunter.Application.Features.TreasureItems.Queries;

namespace TreasureHunter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TreasureController : ControllerBase
{
    private readonly IMediator _mediator;

    public TreasureController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<TreasureItemDto>> CreateTreasure([FromBody] CreateTreasureItemCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        command.PlacedByUserId = userId;

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetNearbyTreasures), new { }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<TreasureItemDto>>> GetNearbyTreasures(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusMeters = 100.0)
    {
        var query = new GetNearbyTreasuresQuery
        {
            UserLatitude = latitude,
            UserLongitude = longitude,
            SearchRadiusMeters = radiusMeters
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/collect")]
    public async Task<ActionResult<UserInventoryDto>> CollectTreasure(Guid id, [FromBody] LocationDto location)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new CollectTreasureItemCommand
        {
            TreasureItemId = id,
            UserId = userId,
            UserLatitude = location.Latitude,
            UserLongitude = location.Longitude
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

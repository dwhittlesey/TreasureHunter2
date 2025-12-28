using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Application.Features.Users.Queries;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRepository<ApplicationUser> _userRepository;

    public UserController(IMediator mediator, IRepository<ApplicationUser> userRepository)
    {
        _mediator = mediator;
        _userRepository = userRepository;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            DisplayName = user.DisplayName,
            TotalPoints = user.TotalPoints
        });
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetInventory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetUserInventoryQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

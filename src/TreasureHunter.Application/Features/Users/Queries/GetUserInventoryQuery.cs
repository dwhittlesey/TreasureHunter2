using System.Collections.Generic;
using MediatR;
using TreasureHunter.Application.DTOs;

namespace TreasureHunter.Application.Features.Users.Queries;

public class GetUserInventoryQuery : IRequest<IEnumerable<UserInventoryDto>>
{
    public string UserId { get; set; } = string.Empty;
}

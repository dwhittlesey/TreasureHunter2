using System;
using MediatR;
using TreasureHunter.Application.DTOs;

namespace TreasureHunter.Application.Features.TreasureItems.Commands;

public class CreateTreasureItemCommand : IRequest<TreasureItemDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ItemTypeId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double DiscoveryRadiusMeters { get; set; } = 5.0;
    public string PlacedByUserId { get; set; } = string.Empty;
}

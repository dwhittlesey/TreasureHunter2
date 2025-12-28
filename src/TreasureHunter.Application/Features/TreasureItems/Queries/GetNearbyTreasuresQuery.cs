using System.Collections.Generic;
using MediatR;
using TreasureHunter.Application.DTOs;

namespace TreasureHunter.Application.Features.TreasureItems.Queries;

public class GetNearbyTreasuresQuery : IRequest<IEnumerable<TreasureItemDto>>
{
    public double UserLatitude { get; set; }
    public double UserLongitude { get; set; }
    public double SearchRadiusMeters { get; set; } = 100.0;
}

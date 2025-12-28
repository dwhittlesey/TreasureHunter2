using FluentValidation;
using TreasureHunter.Domain.Constants;

namespace TreasureHunter.Application.Features.TreasureItems.Commands;

public class CreateTreasureItemCommandValidator : AbstractValidator<CreateTreasureItemCommand>
{
    public CreateTreasureItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.ItemTypeId)
            .GreaterThan(0).WithMessage("ItemTypeId must be greater than 0");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.DiscoveryRadiusMeters)
            .InclusiveBetween(GameConstants.MinDiscoveryRadiusMeters, GameConstants.MaxDiscoveryRadiusMeters)
            .WithMessage($"Discovery radius must be between {GameConstants.MinDiscoveryRadiusMeters} and {GameConstants.MaxDiscoveryRadiusMeters} meters");

        RuleFor(x => x.PlacedByUserId)
            .NotEmpty().WithMessage("PlacedByUserId is required");
    }
}

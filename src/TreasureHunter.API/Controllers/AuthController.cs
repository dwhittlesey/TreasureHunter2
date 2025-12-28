using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TreasureHunter.API.Models;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;
using TreasureHunter.Infrastructure.Identity;

namespace TreasureHunter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtTokenService _tokenService;
    private readonly IRepository<ApplicationUser> _appUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        JwtTokenService tokenService,
        IRepository<ApplicationUser> appUserRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _appUserRepository = appUserRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Create ApplicationUser profile
        var appUser = new ApplicationUser
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            DisplayName = request.DisplayName ?? user.UserName,
            TotalPoints = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _appUserRepository.AddAsync(appUser);
        await _unitOfWork.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.UserName, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            return Unauthorized("Invalid credentials");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized("Invalid credentials");
        }

        // Update last active
        var appUser = await _appUserRepository.GetByIdAsync(user.Id);
        if (appUser != null)
        {
            appUser.LastActiveAt = DateTime.UtcNow;
            await _appUserRepository.UpdateAsync(appUser);
            await _unitOfWork.SaveChangesAsync();
        }

        var token = _tokenService.GenerateToken(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email
        });
    }
}

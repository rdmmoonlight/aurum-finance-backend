using Aurum.Api.Core.Extensions;
using Aurum.Api.Features.Authentication.Dtos;
using Aurum.Api.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Features.Authentication;

/// <summary>
/// New endpoints — the previous NestJS backend had no /auth routes at all
/// (Supabase handled login/register directly from the frontend). There is
/// no prior contract to mirror here, but responses are still returned raw
/// (no envelope), matching the flat-JSON convention every other endpoint in
/// this API uses for Nest-Compatibility.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ICurrentUserService currentUser)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _registerValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        var result = await _authService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        await _loginValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        var result = await _authService.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Returns the caller's own profile — proves the bearer token round-trips end to end.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthenticatedUserDto>> Me(CancellationToken ct)
    {
        var user = await _authService.GetCurrentUserAsync(_currentUser.UserId, ct);
        return Ok(user);
    }
}

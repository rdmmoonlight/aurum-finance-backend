using Aurum.Api.Core.Extensions;
using Aurum.Api.Features.Authentication.Dtos;
using Aurum.Api.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Features.Authentication;

/// <summary>
/// Every response is returned raw — no envelope — matching the flat-JSON
/// convention used across this API (see
/// Core/Middleware/ExceptionHandlingMiddleware.cs for the error shape).
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
    private readonly IValidator<ResendVerificationRequest> _resendVerificationValidator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<VerifyEmailRequest> verifyEmailValidator,
        IValidator<ResendVerificationRequest> resendVerificationValidator,
        ICurrentUserService currentUser)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _resendVerificationValidator = resendVerificationValidator;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _registerValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        var result = await _authService.RegisterAsync(request, ClientIp, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        await _loginValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        var result = await _authService.LoginAsync(request, ClientIp, ct);
        return Ok(result);
    }

    /// <summary>Exchanges a still-valid refresh token for a new access token, rotating the refresh token in the same call.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _refreshTokenValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        var result = await _authService.RefreshAsync(request.RefreshToken, ClientIp, ct);
        return Ok(result);
    }

    /// <summary>Revokes a single refresh token, ending that session. Client is expected to discard both tokens locally afterward.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _refreshTokenValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        await _authService.LogoutAsync(request.RefreshToken, ClientIp, ct);
        return NoContent();
    }

    /// <summary>Returns the caller's own profile — proves the bearer token round-trips end to end.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthenticatedUserDto>> Me(CancellationToken ct)
    {
        var user = await _authService.GetCurrentUserAsync(_currentUser.UserId, ct);
        return Ok(user);
    }

    /// <summary>Always returns 204, whether or not the email is registered — never reveals account existence.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _forgotPasswordValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        await _authService.ForgotPasswordAsync(request, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _resetPasswordValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        await _authService.ResetPasswordAsync(request, ct);
        return NoContent();
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        await _verifyEmailValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        await _authService.VerifyEmailAsync(request, ct);
        return NoContent();
    }

    /// <summary>Always returns 204, whether or not the email is registered or already verified — never reveals account state.</summary>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken ct)
    {
        await _resendVerificationValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        await _authService.ResendVerificationAsync(request, ct);
        return NoContent();
    }

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
}

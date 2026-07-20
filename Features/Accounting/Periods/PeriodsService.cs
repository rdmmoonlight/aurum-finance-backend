using Aurum.Api.Common.PeriodLock;
using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Accounting.Periods.Dtos;
using Aurum.Api.Features.Accounting.Periods.Entities;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Accounting.Periods;

public interface IPeriodsService
{
    Task<PeriodDto> CreateAsync(CreatePeriodRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<PeriodDto>> FindAllAsync(CancellationToken ct = default);

    Task<PeriodDto> FindOneAsync(Guid id, CancellationToken ct = default);

    Task<PeriodDto> UpdateAsync(Guid id, UpdatePeriodRequest request, CancellationToken ct = default);

    Task RemoveAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Ported 1:1 from NestJS's PeriodsService (backend/src/periods/periods.service.ts).
/// </summary>
public sealed class PeriodsService : IPeriodsService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public PeriodsService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PeriodDto> CreateAsync(CreatePeriodRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var clashes = await _db.Periods.AnyAsync(p => p.UserId == userId && p.My == request.My, ct);
        if (clashes)
        {
            throw new ConflictException($"Period {request.My} already exists.");
        }

        if (request.SeedCash + request.SeedBank <= 0)
        {
            throw new BadRequestException("Cash + Bank must be greater than 0.");
        }

        var period = new Period
        {
            UserId = userId,
            My = request.My,
            Status = ParseStatus(request.Status) ?? PeriodStatus.Open,
            SeedCash = decimal.Round(request.SeedCash, 2),
            SeedBank = decimal.Round(request.SeedBank, 2),
        };

        _db.Periods.Add(period);
        await _db.SaveChangesAsync(ct);

        return ToDto(period);
    }

    public async Task<IReadOnlyList<PeriodDto>> FindAllAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var periods = await _db.Periods
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.My)
            .ToListAsync(ct);

        return periods.Select(ToDto).ToList();
    }

    public async Task<PeriodDto> FindOneAsync(Guid id, CancellationToken ct = default)
    {
        var period = await FindOwnedOrThrowAsync(id, _currentUser.UserId, ct);
        return ToDto(period);
    }

    public async Task<PeriodDto> UpdateAsync(Guid id, UpdatePeriodRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var period = await FindOwnedOrThrowAsync(id, userId, ct);

        // Closed periods are view + delete only — no field may be edited,
        // not just status.
        PeriodLockPolicy.AssertEditable(period);

        if (request.My.IsSet)
        {
            period.My = request.My.Value!;
        }

        if (request.Status.IsSet)
        {
            period.Status = ParseStatus(request.Status.Value) ?? period.Status;
        }

        if (request.SeedCash.IsSet)
        {
            period.SeedCash = decimal.Round(request.SeedCash.Value, 2);
        }

        if (request.SeedBank.IsSet)
        {
            period.SeedBank = decimal.Round(request.SeedBank.Value, 2);
        }

        if (request.ClosedAt.IsSet)
        {
            period.ClosedAtUtc = request.ClosedAt.Value is null ? null : DateTime.Parse(request.ClosedAt.Value).ToUniversalTime();
        }

        period.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ToDto(period);
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var period = await FindOwnedOrThrowAsync(id, userId, ct);

        _db.Periods.Remove(period);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Period> FindOwnedOrThrowAsync(Guid id, Guid userId, CancellationToken ct)
    {
        return await _db.Periods.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, ct)
            ?? throw new NotFoundException("Period not found.");
    }

    private static PeriodStatus? ParseStatus(string? status) => status switch
    {
        null => null,
        "open" => PeriodStatus.Open,
        "closing" => PeriodStatus.Closing,
        "closed" => PeriodStatus.Closed,
        _ => throw new BadRequestException($"Unknown period status \"{status}\"."),
    };

    private static string StatusToString(PeriodStatus status) => status switch
    {
        PeriodStatus.Open => "open",
        PeriodStatus.Closing => "closing",
        PeriodStatus.Closed => "closed",
        _ => throw new InvalidOperationException($"Unhandled period status {status}."),
    };

    private static PeriodDto ToDto(Period period) => new()
    {
        Id = period.Id,
        UserId = period.UserId,
        My = period.My,
        Status = StatusToString(period.Status),
        SeedCash = period.SeedCash,
        SeedBank = period.SeedBank,
        ClosedAt = period.ClosedAtUtc,
        CreatedAt = period.CreatedAtUtc,
        UpdatedAt = period.UpdatedAtUtc,
    };
}

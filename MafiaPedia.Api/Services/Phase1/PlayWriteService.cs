using System.Text.Json;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase1;
using MafiaPedia.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace MafiaPedia.Api.Services.Phase1;

public class PlayWriteService : IPlayWriteService
{
    private readonly MafiaDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PlayWriteService> _logger;

    public PlayWriteService(MafiaDbContext context, IWebHostEnvironment env, ILogger<PlayWriteService> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    public async Task<bool> UpdatePlayAsync(int playId, UpdatePlayDto dto)
    {
        var play = await _context.Plays
            .Include(p => p.Playplayers)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null) return false;

        if (dto.Title != null) play.Title = dto.Title;
        if (dto.DateTime != null) play.DateTime = dto.DateTime;
        if (dto.PlayersCount != null) play.PlayersCount = dto.PlayersCount;
        if (dto.Desc != null) play.Desc = dto.Desc;
        if (dto.SenarioId != null) play.SenarioId = dto.SenarioId.Value;
        if (dto.WinnersideId != null) play.WinnersideId = dto.WinnersideId.Value;
        if (dto.EventId != null) play.EventId = dto.EventId.Value;
        if (dto.RoomId != null) play.RoomId = dto.RoomId.Value;
        if (dto.MasterId != null) play.MasterId = dto.MasterId.Value;
        if (dto.GuestCount != null) play.GuestCount = dto.GuestCount;
        if (dto.Link != null) play.Link = dto.Link;

        if (dto.PlayersJson != null)
        {
            var players = DeserializePlayers(dto.PlayersJson);
            if (players is null)
                throw new InvalidOperationException("فرمت JSON بازیکنان نامعتبر است");

            _context.Playplayers.RemoveRange(play.Playplayers);

            var newPlayers = players.Select(p => new Playplayer
            {
                PlayId = playId,
                PlayerId = p.PlayerId,
                RoleId = p.RoleId,
                Action = p.Action,
                Rank = p.Rank
            }).ToList();

            await _context.Playplayers.AddRangeAsync(newPlayers);
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "plays");
        Directory.CreateDirectory(uploadsDir);

        var picturePath = await ResolvePictureAsync(dto.Picture, dto.Link, play.Link, uploadsDir);
        if (picturePath != null)
        {
            DeleteOldPlayPicture(play.Picture);
            play.Picture = picturePath;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeletePlayAsync(int playId)
    {
        var play = await _context.Plays
            .Include(p => p.Playplayers)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null)
            return (false, "بازی یافت نشد");

        _context.Playplayers.RemoveRange(play.Playplayers);
        _context.Plays.Remove(play);

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<int> AddPlayAsync(CreatePlayDto dto)
    {
        var players = DeserializePlayers(dto.PlayersJson) ?? new List<CreatePlayPlayerDto>();

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "plays");
        Directory.CreateDirectory(uploadsDir);

        var picturePath = await ResolvePictureAsync(dto.Picture, dto.Link, null, uploadsDir);

        var play = new Play
        {
            Title = dto.Title,
            DateTime = dto.DateTime,
            PlayersCount = dto.PlayersCount,
            Desc = dto.Desc,
            SenarioId = dto.SenarioId,
            WinnersideId = dto.WinnersideId,
            EventId = dto.EventId,
            RoomId = dto.RoomId,
            MasterId = dto.MasterId,
            UserId = dto.UserId,
            GuestCount = dto.GuestCount,
            Link = dto.Link,
            Picture = picturePath
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.Plays.Add(play);
        await _context.SaveChangesAsync();

        foreach (var playerDto in players)
        {
            var playplayer = new Playplayer
            {
                PlayId = play.Id,
                PlayerId = playerDto.PlayerId,
                RoleId = playerDto.RoleId,
                Action = playerDto.Action,
                Rank = playerDto.Rank
            };
            _context.Playplayers.Add(playplayer);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return play.Id;
    }

    private async Task<string?> ResolvePictureAsync(IFormFile? uploadedPicture, string? incomingLink, string? existingLink, string uploadsDir)
    {
        if (uploadedPicture is not null)
            return await SaveUploadedPictureAsync(uploadedPicture, uploadsDir);

        if (!string.IsNullOrWhiteSpace(incomingLink) && incomingLink != existingLink)
        {
            var videoId = YoutubeThumbnailHelper.ExtractVideoId(incomingLink);
            if (videoId is not null)
            {
                var path = await YoutubeThumbnailHelper.DownloadThumbnailAsync(videoId, uploadsDir, _logger);
                if (path is not null)
                    return path;
            }
        }

        return null;
    }

    private async Task<string> SaveUploadedPictureAsync(IFormFile picture, string uploadsDir)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(picture.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Only jpg, jpeg, png, webp files are allowed.");

        if (picture.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("File size must be less than 2 MB.");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await picture.CopyToAsync(stream);
        }

        return $"/uploads/plays/{fileName}";
    }

    private void DeleteOldPlayPicture(string? picturePath)
    {
        if (string.IsNullOrEmpty(picturePath)) return;
        var fullPath = Path.Combine(_env.WebRootPath, picturePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private static List<CreatePlayPlayerDto>? DeserializePlayers(string? playersJson)
    {
        if (string.IsNullOrWhiteSpace(playersJson)) return null;
        return JsonSerializer.Deserialize<List<CreatePlayPlayerDto>>(playersJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}

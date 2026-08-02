using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Extensions;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Utilities;
using System.Net;
using System.Text.Json;

namespace MoodPlaylistApi.Data.Repositories
{
    public sealed class LibraryRepository(AppDbContext dc) : ILibraryRepository
    {
        public async Task<ApiResponse<List<AvailableMood>>> GetAvailableMoods()
        {
            var moods = await dc.Moods
            .AsNoTracking()
            .Select(m => m.GetAvailableMood())
            .ToListAsync();
            return ApiResponse<AvailableMood>.SuccessList(HttpStatusCode.OK, data: moods);
        }

        public async Task<Mood?> GetByIdAsync(Guid id) =>
            await dc.Moods.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        public async Task<List<Mood>> GetByIdsAsync(IReadOnlyCollection<Guid> ids) =>
            await dc.Moods.AsNoTracking().Where(m => ids.Contains(m.Id)).ToListAsync();

        //public async Task<ApiResponse<List<UserPlaylist>>> GetUserPlaylists(int pageNo = 1, int pageSize = 10, string sortDir = "asc", Guid? userId = null, Guid? moodId = null)
        //{
        //    var query = dc.Playlists
        //        .AsNoTracking()
        //        .AsQueryable();

        //    // Filter by user
        //    if (userId.HasValue)
        //        query = query.Where(p => p.UserId == userId.Value);

        //    // Filter by mood
        //    if (moodId.HasValue)
        //        query = query.Where(p => p.MoodId == moodId.Value);

        //    if (sortDir == "asc") query = query.OrderBy(p => p.CreatedAt);
        //    else query = query.OrderByDescending(p => p.CreatedAt);

        //    var userPlaylists = await query
        //        .Skip((pageNo - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(p => new UserPlaylist
        //        {
        //            Title = p.Title,
        //            CreatorName = p.User.Name,
        //            CreatorTag = p.User.PublicId,
        //            Mood = p.Mood != null ? p.Mood.GetAvailableMood() : null,
        //            Tracks = p.GetTracks()
        //        })
        //        .ToListAsync();

        //    return ApiResponse<UserPlaylist>.SuccessList(HttpStatusCode.OK, userPlaylists);
        //}

        // Get playlists (user-specific or public)
        public async Task<ApiResponse<List<UserPlaylist>>> GetPlaylists(
            int pageNo,
            int pageSize,
            string sortDir,
            Guid? ownerId,
            Guid? excludedOwnerId,
            Guid? moodId,
            string? creatorTag)
        {
            var query = dc.Playlists
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Mood)
                .AsQueryable();

            // Filter by user
            if (ownerId.HasValue)
                query = query.Where(p => p.UserId == ownerId.Value);

            if (excludedOwnerId.HasValue)
                query = query.Where(p => p.UserId != excludedOwnerId.Value);

            if (!string.IsNullOrWhiteSpace(creatorTag))
                query = query.Where(p => p.User.PublicId == creatorTag);

            // Filter by mood
            if (moodId.HasValue)
                query = query.Where(p => p.MoodId == moodId.Value);

            // Sorting
            query = sortDir.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt);

            // Pagination + projection
            var userPlaylists = await query
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new UserPlaylist
                {
                    Id = p.Id,
                    Title = p.Title,
                    CreatorName = p.User.Name,
                    CreatorTag = p.User.PublicId,
                    Mood = p.Mood != null ? p.Mood.GetAvailableMood() : null,
                    Tracks = p.GetTracks()
                })
                .ToListAsync();

            return ApiResponse<UserPlaylist>.SuccessList(HttpStatusCode.OK, userPlaylists);
        }

        public async Task<Guid?> GetOwnedPlaylistMoodId(Guid userId, Guid playlistId) =>
            await dc.Playlists
                .AsNoTracking()
                .Where(p => p.Id == playlistId && p.UserId == userId)
                .Select(p => p.MoodId)
                .FirstOrDefaultAsync();

        public async Task<ApiResponse<UserPlaylist>> CreatePlaylist(Guid userId, UpsertPlaylist req)
        {
            if (await dc.Playlists.AnyAsync(p =>
                    EF.Functions.ILike(p.Title, req.Title) && p.UserId == userId))
                return ApiResponse<UserPlaylist>.Error(HttpStatusCode.Conflict, "You already have a playlist with the same title!");

            Playlist playlist = new()
            {
                Title = req.Title,
                MoodId = req.MoodId,
                UserId = userId,
                Tracks = req.Tracks
            };
            await dc.Playlists.AddAsync(playlist);
            await dc.SaveChangesAsync();

            // Reload to include navigation properties
            var created = await dc.Playlists
                .Include(p => p.User)
                .Include(p => p.Mood)
                .FirstOrDefaultAsync(p => p.Id == playlist.Id);

            return ApiResponse<UserPlaylist>.Success(HttpStatusCode.OK, data: new()
            {
                Id = created!.Id,
                Title = created!.Title,
                CreatorName = created.User.Name,
                CreatorTag = created.User.PublicId,
                Mood = created.Mood?.GetAvailableMood(),
                Tracks = created.GetTracks()
            });
        }

        public async Task<ApiResponse<UserPlaylist>> UpdatePlaylist(Guid userId, Guid playlistId, UpsertPlaylist req)
        {
            // Ownership check
            var playlist = await dc.Playlists
                .Include(p => p.User)
                .Include(p => p.Mood)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist is null)
                return ApiResponse<UserPlaylist>.Error(HttpStatusCode.NotFound, "Playlist not found.");

            if (playlist.UserId != userId)
                return ApiResponse<UserPlaylist>.Error(HttpStatusCode.Forbidden, "You do not own this playlist.");

            // Title conflict check
            if (!string.IsNullOrWhiteSpace(req.Title) && req.Title != playlist.Title)
            {
                bool conflict = await dc.Playlists
                    .AnyAsync(p => p.UserId == userId && p.Title == req.Title && p.Id != playlistId);

                if (conflict)
                    return ApiResponse<UserPlaylist>.Error(HttpStatusCode.BadRequest, "You already have a playlist with this title.");
            }

            // If tracks are provided as JSON, append only new ones
            if (!string.IsNullOrWhiteSpace(req.Tracks))
            {
                await dc.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE ""Playlists""
                    SET ""Tracks"" = (
                        SELECT jsonb_agg(elem)
                        FROM (
                            SELECT elem
                            FROM jsonb_array_elements(""Tracks"") elem
                            UNION
                            SELECT new_elem
                            FROM jsonb_array_elements({req.Tracks}::jsonb) new_elem
                            WHERE NOT EXISTS (
                                SELECT 1
                                FROM jsonb_array_elements(""Tracks"") e
                                WHERE e->>'id' = new_elem->>'id'
                            )
                        ) combined
                    ),
                    ""Title"" = {req.Title ?? playlist.Title}
                    WHERE ""Id"" = {playlistId} AND ""UserId"" = {userId};
                ");
            }
            else if (!string.IsNullOrWhiteSpace(req.Title))
            {
                await dc.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE ""Playlists""
                    SET ""Title"" = {req.Title}
                    WHERE ""Id"" = {playlistId} AND ""UserId"" = {userId};
                ");
            }

            // Return updated playlist DTO
            var updated = await dc.Playlists
                .Include(p => p.User)
                .Include(p => p.Mood)
                .FirstAsync(p => p.Id == playlistId);

            return ApiResponse<UserPlaylist>.Success(HttpStatusCode.OK, "Playlist updated successfully.", new UserPlaylist
            {
                Id = updated.Id,
                Title = updated.Title,
                CreatorName = updated.User?.Name ?? string.Empty,
                CreatorTag = updated.User?.PublicId ?? string.Empty,
                Mood = updated.Mood?.GetAvailableMood(),
                Tracks = updated.GetTracks()
            });
        }

        /*public async Task<ApiResponse<UserPlaylist>> UpdatePlaylist(Guid userId, Guid playlistId, UpsertPlaylist req)
        {
            // 1. Get the playlist
            var playlist = await dc.Playlists
                .Include(p => p.User)
                .Include(p => p.Mood)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
                return ApiResponse<UserPlaylist>.Error(HttpStatusCode.NotFound, "Playlist not found.");

            // 2. Check ownership
            if (playlist.UserId != userId)
                return ApiResponse<UserPlaylist>.Error(HttpStatusCode.Forbidden, "You do not own this playlist.");

            // 3. Title conflict check (only if title is being updated)
            if (!string.IsNullOrWhiteSpace(req.Title) && req.Title != playlist.Title)
            {
                bool titleConflict = await dc.Playlists
                    .AnyAsync(p => p.UserId == userId && p.Title == req.Title && p.Id != playlistId);

                if (titleConflict)
                    return ApiResponse<UserPlaylist>.Error(HttpStatusCode.BadRequest, "You already have a playlist with this title.");

                playlist.Title = req.Title;
            }

            // 4. Deserialize requested tracks
            // Note: Playlist.Tracks is stored as JSON string. UpsertPlaylist.Tracks is expected to be a JSON string as well.
            List<Track> existingTracks = playlist.GetTracks();

            List<Track> requestedTracks = [];
            if (req.Tracks is not null)
            {
                // if req.Tracks is already a JSON string (likely), try to deserialize; otherwise, if it's a List<Track> represented as object,
                // the following will attempt to handle the common case where UpsertPlaylist.Tracks is a JSON string.
                requestedTracks = PlaylistExtensions.GetTracks(req.Tracks);
            }

            // 5. Deduplicate: skip tracks already in playlist
            var existingTrackIds = existingTracks.Select(t => t.Id).ToHashSet();
            List<Track> newTracks = [.. requestedTracks.Where(t => !existingTrackIds.Contains(t.Id))];

            if (newTracks.Count == 0 && string.IsNullOrWhiteSpace(req.Title))
            {
                // Nothing new to update
                return ApiResponse<UserPlaylist>.Success(HttpStatusCode.OK, "No changes made.", new()
                {
                    Title = playlist.Title,
                    CreatorName = playlist.User.Name,
                    CreatorTag = playlist.User.PublicId,
                    Mood = playlist.Mood?.GetAvailableMood(),
                    Tracks = [.. existingTracks]
                });
            }

            // 6. Add new tracks
            if (newTracks.Count > 0)
            {
                existingTracks.AddRange(newTracks);
                playlist.Tracks = PlaylistExtensions.SetTracks(existingTracks);
            }

            // 7. Update and save
            dc.Playlists.Update(playlist);
            await dc.SaveChangesAsync();

            // 8. Return updated playlist DTO
            return ApiResponse<UserPlaylist>.Success(HttpStatusCode.OK, "Playlist updated successfully.", new()
            {
                Title = playlist.Title,
                CreatorName = playlist.User.Name,
                CreatorTag = playlist.User.PublicId,
                Mood = playlist.Mood?.GetAvailableMood(),
                Tracks = [.. existingTracks]
            });
        }*/

        public async Task<ApiResponse<List<Track>>> AddTracksAsync(
            Guid userId,
            Guid playlistId,
            IReadOnlyCollection<Track> tracks)
        {
            var playlist = await dc.Playlists.FirstOrDefaultAsync(
                p => p.Id == playlistId && p.UserId == userId);
            if (playlist is null)
                return ApiResponse<List<Track>>.Error(HttpStatusCode.NotFound, "Playlist was not found in your library.");

            var existingTracks = playlist.GetTracks();
            var existingIds = existingTracks.Select(track => track.Id).ToHashSet(StringComparer.Ordinal);
            var addedTracks = tracks
                .Where(track => !string.IsNullOrWhiteSpace(track.Id) && existingIds.Add(track.Id))
                .ToList();

            if (addedTracks.Count == 0)
                return ApiResponse<List<Track>>.Error(HttpStatusCode.Conflict, "All selected tracks are already in this playlist.");

            existingTracks.AddRange(addedTracks);
            playlist.Tracks = PlaylistExtensions.SetTracks(existingTracks);
            await dc.SaveChangesAsync();

            return ApiResponse<List<Track>>.Success(
                HttpStatusCode.OK,
                $"Added {addedTracks.Count} track(s).",
                addedTracks);
        }

        //public async Task<ApiResponse<Track>> AddTrackAsync(Guid userId, Guid playlistId, Track track)
        //{
        //    // 1. Get the playlist
        //    var playlist = await dc.Playlists
        //        .FirstOrDefaultAsync(p => p.Id == playlistId);

        //    if (playlist is null)
        //        return ApiResponse<Track>.Error(HttpStatusCode.NotFound, "Playlist not found.");

        //    // 2. Check ownership
        //    if (playlist.UserId != userId)
        //        return ApiResponse<Track>.Error(HttpStatusCode.Forbidden, "You do not own this playlist.");

        //    // 3. Deserialize existing tracks
        //    List<Track> existingTracks = playlist.GetTracks();

        //    // 4. Check if track already exists in playlist
        //    bool trackExists = existingTracks.Any(t => t.Id == track.Id);
        //    if (trackExists)
        //        return ApiResponse<Track>.Error(HttpStatusCode.Conflict, "Track already exists in playlist.");

        //    // 5. Add track to playlist
        //    existingTracks.Add(track);
        //    playlist.Tracks = PlaylistExtensions.SetTracks(existingTracks);

        //    // 6. Update and save
        //    dc.Playlists.Update(playlist);
        //    await dc.SaveChangesAsync();

        //    // 7. Return success response
        //    return ApiResponse<Track>.Success(HttpStatusCode.OK, "Track added successfully.", track);
        //}

        public async Task<ApiResponse<string>> RemoveTrack(Guid userId, Guid playlistId, string trackId)
        {
            var ownsPlaylist = await dc.Playlists
                .AnyAsync(p => p.Id == playlistId && p.UserId == userId);

            if (!ownsPlaylist)
                return ApiResponse<string>.Error(HttpStatusCode.Forbidden, "You do not own this playlist.");

            // Use the async variant so the returned value can be awaited (avoids awaiting an int)
            var rows = await dc.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Playlists""
                SET ""Tracks"" = COALESCE((
                    SELECT jsonb_agg(elem)
                    FROM jsonb_array_elements(""Tracks"") elem
                    WHERE elem->>'id' <> {trackId}
                ), '[]'::jsonb)
                WHERE ""Id"" = {playlistId} AND ""UserId"" = {userId};
            ");

            if (rows == 0)
                return ApiResponse<string>.Error(HttpStatusCode.NotFound, "Track not found in playlist.");

            return ApiResponse<string>.Success(HttpStatusCode.OK, "Track removed successfully.", trackId);
        }

        /*public async Task<ApiResponse<string>> RemoveTrack(Guid userId, Guid playlistId, string trackId)
        {
            var playlist = await dc.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
                return ApiResponse<string>.Error(HttpStatusCode.NotFound, "Playlist not found.");

            if (playlist.UserId != userId)
                return ApiResponse<string>.Error(HttpStatusCode.Forbidden, "You do not own this playlist.");

            List<Track> tracks = playlist.GetTracks();

            var removed = tracks.RemoveAll(t => t.Id == trackId);
            if (removed == 0)
                return ApiResponse<string>.Error(HttpStatusCode.NotFound, "Track not found in playlist.");

            playlist.Tracks = PlaylistExtensions.SetTracks(tracks);
            dc.Playlists.Update(playlist);
            await dc.SaveChangesAsync();

            return ApiResponse<string>.Success(HttpStatusCode.OK, "Track removed successfully.", trackId);
        }*/

        // Optional: check if track already exists in user’s library
        public async Task<ApiResponse<bool>> ExistsAsync(Guid userId, Guid playlistId, string trackId)
        {
            var trackJson = JsonSerializer.Serialize(new[]
            {
                new Dictionary<string, string> { ["id"] = trackId }
            });
            var exists = await dc.Playlists
                .Where(p => p.Id == playlistId && p.UserId == userId)
                .AnyAsync(p => EF.Functions.JsonContains(p.Tracks, trackJson));

            return ApiResponse<bool>.Success(HttpStatusCode.OK, data:exists);
        }

        /*public async Task<ApiResponse<bool>> ExistsAsync(Guid userId, Guid playlistId, string trackId)
        {
            var playlist = await dc.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null)
                return ApiResponse<bool>.Error(HttpStatusCode.NotFound, "Playlist not found.");

            if (playlist.UserId != userId)
                return ApiResponse<bool>.Error(HttpStatusCode.Forbidden, "You do not own this playlist.");

            var tracks = playlist.GetTracks(); 

            bool exists = tracks.Any(t => t.Id == trackId);
            return ApiResponse<bool>.Success(HttpStatusCode.OK, data: exists);
        }*/
    }
}

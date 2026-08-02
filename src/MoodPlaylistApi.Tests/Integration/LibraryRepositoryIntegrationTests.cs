using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data.Repositories;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Models;
using System.Net;

namespace MoodPlaylistApi.Tests.Integration;

[Collection("PostgreSQL")]
[Trait("Category", "Integration")]
public sealed class LibraryRepositoryIntegrationTests(PostgreSqlFixture database)
{
    [Fact(DisplayName = "Database migrations create and seed the PostgreSQL schema")]
    public async Task Migrations_ContainerStarted_CreatesSchemaAndSeedsMoods()
    {
        await database.ResetDatabaseAsync();
        await using var context = database.CreateContext();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        var moods = await context.Moods.AsNoTracking().ToListAsync();

        Assert.Contains("20260609145510_Initial-Migration", appliedMigrations);
        Assert.Equal(8, moods.Count);
        Assert.Contains(moods, mood => mood.Name == "Happy" && mood.SeedGenres.Contains("pop"));
    }

    [Fact(DisplayName = "Library repository persists queries and filters playlists in PostgreSQL")]
    public async Task CreateAndGetPlaylists_ValidData_PersistsAndFiltersPlaylist()
    {
        await database.ResetDatabaseAsync();
        await using var context = database.CreateContext();
        var user = await AddUser(context, "ada", "ada@example.com");
        var otherUser = await AddUser(context, "grace", "grace@example.com");
        var mood = await context.Moods.FirstAsync(x => x.Name == "Happy");
        var repository = new LibraryRepository(context);

        var created = await repository.CreatePlaylist(user.Id, new UpsertPlaylist
        {
            Title = "Morning",
            MoodId = mood.Id,
            Tracks = "[]"
        });
        await repository.CreatePlaylist(otherUser.Id, new UpsertPlaylist
        {
            Title = "Evening",
            MoodId = mood.Id,
            Tracks = "[]"
        });
        var mine = await repository.GetPlaylists(1, 10, "asc", user.Id, null, mood.Id, null);
        var others = await repository.GetPlaylists(1, 10, "asc", null, user.Id, null, null);

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.Equal("ada", created.Data?.CreatorTag);
        Assert.Equal("Morning", Assert.Single(mine.Data!).Title);
        Assert.Equal("Evening", Assert.Single(others.Data!).Title);
    }

    [Fact(DisplayName = "Library repository uses PostgreSQL JSONB to add find and remove tracks")]
    public async Task TrackLifecycle_ValidOwnedPlaylist_UsesJsonbOperations()
    {
        await database.ResetDatabaseAsync();
        await using var context = database.CreateContext();
        var user = await AddUser(context, "ada", "ada@example.com");
        var playlist = new Playlist { Title = "Morning", UserId = user.Id, Tracks = "[]" };
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        var repository = new LibraryRepository(context);
        var track = new Track { Id = "track-1", Name = "Sunshine" };

        var added = await repository.AddTracksAsync(user.Id, playlist.Id, [track, track]);
        var existsAfterAdd = await repository.ExistsAsync(user.Id, playlist.Id, track.Id);
        var removed = await repository.RemoveTrack(user.Id, playlist.Id, track.Id);
        var existsAfterRemove = await repository.ExistsAsync(user.Id, playlist.Id, track.Id);

        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        Assert.Single(added.Data!);
        Assert.True(existsAfterAdd.Data);
        Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
        Assert.False(existsAfterRemove.Data);
    }

    private static async Task<User> AddUser(
        MoodPlaylistApi.Data.AppDbContext context,
        string publicId,
        string email)
    {
        var user = new User { Name = publicId, PublicId = publicId, Email = email };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}

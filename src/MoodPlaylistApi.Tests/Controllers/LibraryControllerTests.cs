using Microsoft.AspNetCore.Mvc;
using Moq;
using MoodPlaylistApi.Controllers;
using MoodPlaylistApi.Dtos;
using MoodPlaylistApi.Exceptions;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Models;
using MoodPlaylistApi.Services;
using MoodPlaylistApi.Tests.TestSupport;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Tests.Controllers;

public sealed class LibraryControllerTests
{
    [Fact(DisplayName = "Available moods returns the repository response")]
    public async Task GetAvailableMoods_RepositoryReturnsMoods_ReturnsMatchingResponse()
    {
        var response = ApiResponse<List<AvailableMood>>.Success(
            HttpStatusCode.OK,
            data: [new AvailableMood { Name = "Happy" }]);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetAvailableMoods()).ReturnsAsync(response);
        var controller = CreateController(repository);

        var result = await controller.GetAvailableMoods();

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.Verify(x => x.GetAvailableMoods(), Times.Once);
    }

    [Fact(DisplayName = "Mood tracks uses mood configuration to request Spotify recommendations")]
    public async Task GetAvailableMoodTracks_ValidMood_RequestsRecommendationsAndReturnsTracks()
    {
        var moodId = Guid.NewGuid();
        var mood = new Mood
        {
            Id = moodId,
            Name = "Happy",
            SeedGenres = "{\"genres\":[\"pop\",\"dance\"]}",
            AudioFeatures = "{\"target\":{\"energy\":0.8}}"
        };
        var tracks = new List<Track> { new() { Id = "track-1", Name = "Sunshine" } };
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetByIdAsync(moodId)).ReturnsAsync(mood);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        spotify.Setup(x => x.GetTracksByMoodRecommendations(
                It.Is<List<string>>(genres => genres.SequenceEqual(new[] { "pop", "dance" })),
                It.Is<Dictionary<string, Dictionary<string, double>>>(features =>
                    features["target"]["energy"] == 0.8)))
            .ReturnsAsync(tracks);
        var controller = CreateController(repository, spotify);

        var result = await controller.GetAvailableMoodTracks(moodId);
        await controller.GetAvailableMoodTracks(moodId);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        var response = Assert.IsType<ApiResponse<List<Track>>>(objectResult.Value);
        Assert.Same(tracks, response.Data);
        spotify.Verify(x => x.GetTracksByMoodRecommendations(
            It.IsAny<List<string>>(),
            It.IsAny<Dictionary<string, Dictionary<string, double>>>()), Times.Once);
    }

    [Fact(DisplayName = "Mood tracks throws when the mood does not exist")]
    public async Task GetAvailableMoodTracks_MoodDoesNotExist_ThrowsMoodNotFoundException()
    {
        var moodId = Guid.NewGuid();
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetByIdAsync(moodId)).ReturnsAsync((Mood?)null);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateController(repository, spotify);

        var exception = await Assert.ThrowsAsync<MoodNotFoundException>(
            () => controller.GetAvailableMoodTracks(moodId));

        Assert.Contains(moodId.ToString(), exception.Message);
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Mood tracks throws when the mood has no seed genres")]
    public async Task GetAvailableMoodTracks_MoodHasNoSeedGenres_ThrowsMoodGenreNotValidException()
    {
        var moodId = Guid.NewGuid();
        var mood = new Mood { Id = moodId, Name = "Empty", SeedGenres = "{\"genres\":[]}" };
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetByIdAsync(moodId)).ReturnsAsync(mood);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateController(repository, spotify);

        var exception = await Assert.ThrowsAsync<MoodGenreNotValidException>(
            () => controller.GetAvailableMoodTracks(moodId));

        Assert.Contains(moodId.ToString(), exception.Message);
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Recommendations combines selected moods and Spotify tracks")]
    public async Task GetRecommendations_MoodsAndTracksSelected_CombinesSeedsAndReturnsTracks()
    {
        var firstMoodId = Guid.NewGuid();
        var secondMoodId = Guid.NewGuid();
        var moods = new List<Mood>
        {
            new() { Id = firstMoodId, Name = "Happy", SeedGenres = "{\"genres\":[\"pop\",\"dance\"]}", AudioFeatures = "{\"energy\":{\"target\":0.8}}" },
            new() { Id = secondMoodId, Name = "Calm", SeedGenres = "{\"genres\":[\"ambient\",\"acoustic\"]}", AudioFeatures = "{\"energy\":{\"target\":0.4}}" }
        };
        var expectedTracks = new List<Track> { new() { Id = "recommended" } };
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetByIdsAsync(It.Is<IReadOnlyCollection<Guid>>(
                ids => ids.SequenceEqual(new[] { firstMoodId, secondMoodId }))))
            .ReturnsAsync(moods);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        spotify.Setup(x => x.GetRecommendations(
                It.Is<IReadOnlyCollection<string>>(genres => genres.SequenceEqual(new[] { "pop", "ambient", "dance", "acoustic" })),
                It.Is<IReadOnlyCollection<string>>(tracks => tracks.SequenceEqual(new[] { "track-1" })),
                It.Is<IReadOnlyDictionary<string, Dictionary<string, double>>>(features =>
                    Math.Abs(features["energy"]["target"] - 0.6) < 0.000001),
                25,
                "NG"))
            .ReturnsAsync(expectedTracks);
        var controller = CreateController(repository, spotify);
        var request = new RecommendationRequest
        {
            MoodIds = [firstMoodId, secondMoodId],
            TrackIds = ["spotify:track:track-1"],
            Limit = 25,
            Market = "NG"
        };

        var result = await controller.GetRecommendations(request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        var response = Assert.IsType<ApiResponse<List<Track>>>(objectResult.Value);
        Assert.Same(expectedTracks, response.Data);
        repository.VerifyAll();
        spotify.VerifyAll();
    }

    [Fact(DisplayName = "Recommendations requires at least one mood or track")]
    public async Task GetRecommendations_NoSeedsSelected_ThrowsRecommendationRequestException()
    {
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateController(repository, spotify);

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.GetRecommendations(new RecommendationRequest()));

        Assert.Contains("at least one", exception.Message);
        repository.VerifyNoOtherCalls();
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Recommendations rejects more than five combined seeds")]
    public async Task GetRecommendations_MoreThanFiveCombinedSeeds_ThrowsRecommendationRequestException()
    {
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateController(repository, spotify);
        var request = new RecommendationRequest
        {
            MoodIds = [Guid.NewGuid(), Guid.NewGuid()],
            TrackIds = ["one", "two", "three", "four"]
        };

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.GetRecommendations(request));

        Assert.Contains("at most five", exception.Message);
        repository.VerifyNoOtherCalls();
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Recommendations reports selected moods that do not exist")]
    public async Task GetRecommendations_MoodDoesNotExist_ThrowsRecommendationRequestException()
    {
        var existingMoodId = Guid.NewGuid();
        var missingMoodId = Guid.NewGuid();
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([new Mood { Id = existingMoodId, Name = "Happy" }]);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateController(repository, spotify);
        var request = new RecommendationRequest { MoodIds = [existingMoodId, missingMoodId] };

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.GetRecommendations(request));

        Assert.Contains(missingMoodId.ToString(), exception.Message);
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Recommendations rejects moods without usable genre seeds")]
    public async Task GetRecommendations_MoodsHaveNoGenres_ThrowsRecommendationRequestException()
    {
        var moodId = Guid.NewGuid();
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([new Mood { Id = moodId, Name = "Empty", SeedGenres = "{\"genres\":[]}" }]);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateController(repository, spotify);

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.GetRecommendations(new RecommendationRequest { MoodIds = [moodId] }));

        Assert.Contains("usable Spotify genre seeds", exception.Message);
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Track-only recommendations normalize Spotify URLs without loading moods")]
    public async Task GetRecommendations_TrackUrlOnly_NormalizesTrackAndSkipsMoodLookup()
    {
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        spotify.Setup(x => x.GetRecommendations(
                It.Is<IReadOnlyCollection<string>>(genres => genres.Count == 0),
                It.Is<IReadOnlyCollection<string>>(tracks => tracks.SequenceEqual(new[] { "track-1" })),
                It.Is<IReadOnlyDictionary<string, Dictionary<string, double>>>(features => features.Count == 0),
                20,
                null))
            .ReturnsAsync([]);
        var controller = CreateController(repository, spotify);

        await controller.GetRecommendations(new RecommendationRequest
        {
            TrackIds = [" https://open.spotify.com/track/track-1?si=value "]
        });

        repository.VerifyNoOtherCalls();
        spotify.VerifyAll();
    }

    [Fact(DisplayName = "Create playlist passes the authenticated user and request to the repository")]
    public async Task CreatePlaylist_AuthenticatedUser_PassesUserAndRequest()
    {
        var userId = Guid.NewGuid();
        var request = new UpsertPlaylist { Title = "Morning" };
        var response = ApiResponse<UserPlaylist>.Success(
            HttpStatusCode.Created,
            data: new UserPlaylist { Title = request.Title });
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.CreatePlaylist(userId, request)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.CreatePlaylist(request);

        AssertResponse(result, HttpStatusCode.Created, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "User playlists forwards paging filters and authenticated user ID")]
    public async Task GetPlaylists_MineSelected_ForwardsAuthenticatedOwner()
    {
        var userId = Guid.NewGuid();
        var moodId = Guid.NewGuid();
        var response = ApiResponse<List<UserPlaylist>>.Success(HttpStatusCode.OK, data: []);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetPlaylists(2, 25, "desc", userId, null, moodId, null)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.GetPlaylists(2, 25, "desc", moodId, null, "mine");

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "Public playlists passes a null user ID to the repository")]
    public async Task GetPlaylists_AllSelected_DoesNotApplyOwnerFilter()
    {
        var moodId = Guid.NewGuid();
        var response = ApiResponse<List<UserPlaylist>>.Success(HttpStatusCode.OK, data: []);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetPlaylists(3, 5, "desc", null, null, moodId, "creator-tag")).ReturnsAsync(response);
        var controller = CreateController(repository);

        var result = await controller.GetPlaylists(3, 5, "desc", moodId, "creator-tag", "all");

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "Other playlists excludes the authenticated user")]
    public async Task GetPlaylists_OthersSelected_ExcludesAuthenticatedOwner()
    {
        var userId = Guid.NewGuid();
        var response = ApiResponse<List<UserPlaylist>>.Success(HttpStatusCode.OK, data: []);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetPlaylists(1, 10, "asc", null, userId, null, null)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.GetPlaylists(view: " OTHERS ");

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "Mine playlist view requires an authenticated user")]
    public async Task GetPlaylists_MineSelectedWithoutUser_ThrowsUnauthorizedAccessException()
    {
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        var controller = CreateController(repository);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => controller.GetPlaylists(view: "mine"));

        Assert.Contains("Sign in", exception.Message);
        repository.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Playlist query rejects an unsupported view")]
    public async Task GetPlaylists_UnsupportedView_ThrowsRecommendationRequestException()
    {
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        var controller = CreateController(repository);

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.GetPlaylists(view: "friends"));

        Assert.Contains("mine, others, or all", exception.Message);
        repository.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Update playlist passes route request and authenticated user to the repository")]
    public async Task UpdatePlaylist_AuthenticatedUser_ForwardsAllArguments()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var request = new UpsertPlaylist { Title = "Updated" };
        var response = ApiResponse<UserPlaylist>.Success(
            HttpStatusCode.OK,
            data: new UserPlaylist { Title = request.Title });
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.UpdatePlaylist(userId, playlistId, request)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.UpdatePlaylist(playlistId, request);

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "Add track passes route body and authenticated user to the repository")]
    public async Task AddTracks_AuthenticatedUser_ForwardsAllArguments()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var track = new Track { Id = "track-1" };
        var tracks = new List<Track> { track };
        var response = ApiResponse<List<Track>>.Success(HttpStatusCode.Created, data: tracks);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.AddTracksAsync(userId, playlistId, tracks)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.AddTracks(playlistId, new SaveTracksRequest { Tracks = tracks });

        AssertResponse(result, HttpStatusCode.Created, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "Add tracks rejects an empty selection")]
    public async Task AddTracks_NoTracksSelected_ThrowsRecommendationRequestException()
    {
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        var controller = CreateAuthenticatedController(repository, Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.AddTracks(Guid.NewGuid(), new SaveTracksRequest { Tracks = [] }));

        Assert.Contains("at least one track", exception.Message);
        repository.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Refresh playlist gets recommendations for its mood and saves new tracks")]
    public async Task RefreshPlaylist_OwnedMoodPlaylist_SavesRecommendedTracks()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var moodId = Guid.NewGuid();
        var mood = new Mood
        {
            Id = moodId,
            Name = "Happy",
            SeedGenres = "{\"genres\":[\"pop\"]}",
            AudioFeatures = "{}"
        };
        var tracks = new List<Track> { new() { Id = "track-1" } };
        var response = ApiResponse<List<Track>>.Success(HttpStatusCode.OK, data: tracks);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetOwnedPlaylistMoodId(userId, playlistId)).ReturnsAsync(moodId);
        repository.Setup(x => x.GetByIdAsync(moodId)).ReturnsAsync(mood);
        repository.Setup(x => x.AddTracksAsync(userId, playlistId, tracks)).ReturnsAsync(response);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        spotify.Setup(x => x.GetTracksByMoodRecommendations(
                It.Is<List<string>>(genres => genres.SequenceEqual(new[] { "pop" })),
                It.IsAny<Dictionary<string, Dictionary<string, double>>>() ))
            .ReturnsAsync(tracks);
        var controller = CreateAuthenticatedController(repository, userId, spotify);

        var result = await controller.RefreshPlaylist(playlistId);

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
        spotify.VerifyAll();
    }

    [Fact(DisplayName = "Refresh playlist rejects a playlist without an owned mood")]
    public async Task RefreshPlaylist_PlaylistHasNoOwnedMood_ThrowsRecommendationRequestException()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetOwnedPlaylistMoodId(userId, playlistId)).ReturnsAsync((Guid?)null);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateAuthenticatedController(repository, userId, spotify);

        var exception = await Assert.ThrowsAsync<RecommendationRequestException>(
            () => controller.RefreshPlaylist(playlistId));

        Assert.Contains("not found", exception.Message);
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Refresh playlist throws when its associated mood no longer exists")]
    public async Task RefreshPlaylist_AssociatedMoodDoesNotExist_ThrowsMoodNotFoundException()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var moodId = Guid.NewGuid();
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetOwnedPlaylistMoodId(userId, playlistId)).ReturnsAsync(moodId);
        repository.Setup(x => x.GetByIdAsync(moodId)).ReturnsAsync((Mood?)null);
        var spotify = new Mock<ISpotifyService>(MockBehavior.Strict);
        var controller = CreateAuthenticatedController(repository, userId, spotify);

        var exception = await Assert.ThrowsAsync<MoodNotFoundException>(
            () => controller.RefreshPlaylist(playlistId));

        Assert.Contains(moodId.ToString(), exception.Message);
        spotify.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Remove track passes route values and authenticated user to the repository")]
    public async Task RemoveTrack_AuthenticatedUser_ForwardsAllArguments()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        const string trackId = "track-1";
        var response = ApiResponse<string>.Success(HttpStatusCode.OK, data: trackId);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.RemoveTrack(userId, playlistId, trackId)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.RemoveTrack(playlistId, trackId);

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
    }

    [Fact(DisplayName = "Track exists passes route values and authenticated user to the repository")]
    public async Task TrackExists_AuthenticatedUser_ForwardsAllArguments()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        const string trackId = "track-1";
        var response = ApiResponse<bool>.Success(HttpStatusCode.OK, data: true);
        var repository = new Mock<ILibraryRepository>(MockBehavior.Strict);
        repository.Setup(x => x.ExistsAsync(userId, playlistId, trackId)).ReturnsAsync(response);
        var controller = CreateAuthenticatedController(repository, userId);

        var result = await controller.TrackExists(playlistId, trackId);

        AssertResponse(result, HttpStatusCode.OK, response);
        repository.VerifyAll();
    }

    private static LibraryController CreateController(
        Mock<ILibraryRepository> repository,
        Mock<ISpotifyService>? spotify = null) => new(
            ControllerTestContext.CreateUnitOfWork(libraryRepository: repository).Object,
            (spotify ?? new Mock<ISpotifyService>(MockBehavior.Strict)).Object,
            new CacheService(new Microsoft.Extensions.Caching.Memory.MemoryCache(
                new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions())));

    private static LibraryController CreateAuthenticatedController(
        Mock<ILibraryRepository> repository,
        Guid userId,
        Mock<ISpotifyService>? spotify = null)
    {
        var controller = CreateController(repository, spotify);
        ControllerTestContext.Authenticate(controller, userId);
        return controller;
    }

    private static void AssertResponse<T>(
        IActionResult result,
        HttpStatusCode expectedStatus,
        ApiResponse<T> expectedResponse)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)expectedStatus, objectResult.StatusCode);
        Assert.Same(expectedResponse, objectResult.Value);
    }
}

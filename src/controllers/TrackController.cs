using System.Security.Claims;
using StackExchange.Redis;
using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

public static class TrackController
{
    public static async Task<IResult> Less(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity <= Constants.TracksLessPopularity)
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                Constants.PlaylistIdLess,
                trackWithPopularity
            );

            return added
                ? Results.Ok("Tracks added to playlist")
                : Results.BadRequest("Failed to add tracks to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> LessMedium(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessPopularity
                    && track.Track.Popularity <= Constants.TracksLessMediumPopularity
                )
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                Constants.PlaylistIdLessMedium,
                trackWithPopularity
            );

            return added
                ? Results.Ok("Tracks added to playlist")
                : Results.BadRequest("Failed to add tracks to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> Medium(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessMediumPopularity
                    && track.Track.Popularity <= Constants.TracksMediumPopularity
                )
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                Constants.PlaylistIdMedium,
                trackWithPopularity
            );

            return added
                ? Results.Ok("Tracks added to playlist")
                : Results.BadRequest("Failed to add tracks to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> MoreMedium(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessMediumPopularity
                    && track.Track.Popularity <= Constants.TracksMoreMediumPopularity
                )
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                Constants.PlaylistIdMoreMedium,
                trackWithPopularity
            );

            return added
                ? Results.Ok("Tracks added to playlist")
                : Results.BadRequest("Failed to add tracks to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> More(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity > Constants.TracksMoreMediumPopularity)
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                Constants.PlaylistIdMore,
                trackWithPopularity
            );

            return added
                ? Results.Ok("Tracks added to playlist")
                : Results.BadRequest("Failed to add tracks to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> Artist(
        string artistId,
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            var idsArtistPlaylists = await PlaylistHelper.GetOrCreateArtistPlaylists(
                spotifyClient,
                artistId
            );

            // clear artist playlists if they don't empty
            foreach (var (_, id) in idsArtistPlaylists)
            {
                var cleared = await PlaylistService.RemoveAllTracks(id);

                if (cleared != RemoveAllTracksResponse.Success)
                {
                    return Results.BadRequest(
                        "Failed to clear artist playlist before added new tracks"
                    );
                }
            }

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var allArtistTracks = allTracks
                .Where(track => track.Track.Artists.Any(artist => artist.Id == artistId))
                .ToList();

            var trackWithLessPopularity = allArtistTracks
                .Where(track => track.Track.Popularity <= Constants.ArtistTracksLessPopularity)
                .ToList();

            var trackWithMediumPopularity = allArtistTracks
                .Where(track =>
                    track.Track.Popularity > Constants.ArtistTracksLessPopularity
                    && track.Track.Popularity <= Constants.ArtistTracksMediumPopularity
                )
                .ToList();

            var trackWithMorePopularity = allArtistTracks
                .Where(track => track.Track.Popularity > Constants.ArtistTracksMediumPopularity)
                .ToList();

            var addedLess = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                idsArtistPlaylists["less"],
                trackWithLessPopularity
            );

            var addedMedium = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                idsArtistPlaylists["medium"],
                trackWithMediumPopularity
            );

            var addedMore = await TrackService.AddTracksToPlaylist(
                spotifyClient,
                idsArtistPlaylists["more"],
                trackWithMorePopularity
            );

            if (!addedLess || !addedMedium || !addedMore)
            {
                return Results.BadRequest("Failed to add tracks to playlist");
            }

            return Results.Ok("Tracks added to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }
}

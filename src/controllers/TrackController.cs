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
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

            var trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity <= Constants.TracksLessPopularity)
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdLess,
                trackWithPopularity,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> LessMedium(
        IConnectionMultiplexer cacheRedisConnection,
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessPopularity
                    && track.Track.Popularity <= Constants.TracksLessMediumPopularity
                )
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdLessMedium,
                trackWithPopularity,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> Medium(
        IConnectionMultiplexer cacheRedisConnection,
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessMediumPopularity
                    && track.Track.Popularity <= Constants.TracksMediumPopularity
                )
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdMedium,
                trackWithPopularity,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> MoreMedium(
        IConnectionMultiplexer cacheRedisConnection,
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessMediumPopularity
                    && track.Track.Popularity <= Constants.TracksMoreMediumPopularity
                )
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdMoreMedium,
                trackWithPopularity,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> More(
        IConnectionMultiplexer cacheRedisConnection,
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();

            // get all user tracks, if possible from cache
            var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

            var trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity > Constants.TracksMoreMediumPopularity)
                .ToList();

            var added = await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdMore,
                trackWithPopularity,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }

    public static async Task<IResult> Artist(
        string artistId,
        IConnectionMultiplexer cacheRedisConnection,
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();

            var idsArtistPlaylists = await PlaylistHelper.GetOrCreateArtistPlaylists(
                artistId,
                spotifyClient
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
            var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

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
                idsArtistPlaylists["less"],
                trackWithLessPopularity,
                spotifyClient
            );

            var addedMedium = await TrackService.AddTracksToPlaylist(
                idsArtistPlaylists["medium"],
                trackWithMediumPopularity,
                spotifyClient
            );

            var addedMore = await TrackService.AddTracksToPlaylist(
                idsArtistPlaylists["more"],
                trackWithMorePopularity,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }
}

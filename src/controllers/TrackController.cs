using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;
using tracksByPopularity.src.models;

namespace tracksByPopularity.controllers;

public static class TrackController
{
    public static async Task<IResult> Top50(
        HttpContext httpContext,
        IConnectionMultiplexer cacheRedisConnection
    )
    {
        var timerRange = QueryParamHelper.GetTimeRangeQueryParam(httpContext);

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var top50Tracks = await TrackService.GetTop50Tracks(timerRange, allTracks);

        // convert list of FullTrack to SavedTrack
        var tracks = top50Tracks.Select(track => new SavedTrack { Track = track }).ToList();

        // added tracks to playlist based on time range

        var addedToPlaylist = timerRange switch
        {
            TimeRangeEnum.ShortTerm => await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdTopShort,
                tracks
            ),
            TimeRangeEnum.MediumTerm => await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdTopMedium,
                tracks
            ),
            TimeRangeEnum.LongTerm => await TrackService.AddTracksToPlaylist(
                Constants.PlaylistIdTopLong,
                tracks
            ),
            _ => false,
        };

        return addedToPlaylist
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> Less(IConnectionMultiplexer cacheRedisConnection)
    {
        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track => track.Track.Popularity <= Constants.TracksLessPopularity)
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Constants.PlaylistIdLess,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> LessMedium(IConnectionMultiplexer cacheRedisConnection)
    {
        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track =>
                track.Track.Popularity > Constants.TracksLessPopularity
                && track.Track.Popularity <= Constants.TracksLessMediumPopularity
            )
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Constants.PlaylistIdLessMedium,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> MoreMedium(IConnectionMultiplexer cacheRedisConnection)
    {
        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track =>
                track.Track.Popularity > Constants.TracksLessMediumPopularity
                && track.Track.Popularity <= Constants.TracksMoreMediumPopularity
            )
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Constants.PlaylistIdMoreMedium,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> More(IConnectionMultiplexer cacheRedisConnection)
    {
        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track => track.Track.Popularity > Constants.TracksMoreMediumPopularity)
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Constants.PlaylistIdMore,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> Artist(
        string artistId,
        IdArtistPlaylistsBody idArtistPlaylistsBody,
        IConnectionMultiplexer cacheRedisConnection
    )
    {
        // check if playlists are valid
        if (
            !await PlaylistHelper.CheckValidityPlaylist(
                idArtistPlaylistsBody.Less,
                idArtistPlaylistsBody.LessMedium,
                idArtistPlaylistsBody.MoreMedium,
                idArtistPlaylistsBody.More
            )
        )
        {
            return Results.BadRequest("Playlist not found");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var allTracksArtist = allTracks
            .Where(track => track.Track.Artists.Any(artist => artist.Id == artistId))
            .ToList();

        var trackWithLessPopularity = allTracksArtist
            .Where(track => track.Track.Popularity <= Constants.TracksLessPopularity)
            .ToList();

        var trackWithLessMediumPopularity = allTracksArtist
            .Where(track =>
                track.Track.Popularity > Constants.TracksLessPopularity
                && track.Track.Popularity <= Constants.TracksLessMediumPopularity
            )
            .ToList();

        var trackWithMoreMediumPopularity = allTracksArtist
            .Where(track =>
                track.Track.Popularity > Constants.TracksLessMediumPopularity
                && track.Track.Popularity <= Constants.TracksMoreMediumPopularity
            )
            .ToList();

        var trackWithMorePopularity = allTracksArtist
            .Where(track => track.Track.Popularity > Constants.TracksMoreMediumPopularity)
            .ToList();

        var addedLess = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.Less,
            trackWithLessPopularity
        );

        var addedLessMedium = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.LessMedium,
            trackWithLessMediumPopularity
        );

        var addedMoreMedium = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.MoreMedium,
            trackWithMoreMediumPopularity
        );

        var addedMore = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.More,
            trackWithMorePopularity
        );

        if (!addedLess || !addedLessMedium || !addedMoreMedium || !addedMore)
        {
            return Results.BadRequest("Failed to add tracks to playlist");
        }

        return Results.Ok("Tracks added to playlist");
    }
}

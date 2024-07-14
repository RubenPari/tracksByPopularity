using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity;
using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;

public static class TrackController
{
    public static async Task<IResult> Top(
        HttpContext httpContext,
        IConnectionMultiplexer cacheRedisConnection
    )
    {
        var timeRangeString = httpContext.Request.Query["timeRange"].FirstOrDefault();

        if (string.IsNullOrEmpty(timeRangeString))
        {
            return Results.BadRequest("Time range is required");
        }

        // convert timeRange from string to enum
        var timeRangeEnum = timeRangeString.ToEnum<TimeRangeEnum>();

        if (timeRangeEnum == null)
        {
            return Results.BadRequest("Invalid time range");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var topTracks = await TrackService.GetTopTracks(timeRangeEnum.Value, allTracks);

        // convert list of FullTrack to SavedTrack
        var tracks = topTracks.Select(track => new SavedTrack { Track = track }).ToList();

        // added tracks to playlist based on time range
        bool addedToPlaylist = false;

        switch (timeRangeEnum)
        {
            case TimeRangeEnum.ShortTerm:
                addedToPlaylist = await TrackService.AddTracksToPlaylist(
                    Costants.PlaylistIdTopShort,
                    tracks!
                );
                break;
            case TimeRangeEnum.MediumTerm:
                addedToPlaylist = await TrackService.AddTracksToPlaylist(
                    Costants.PlaylistIdTopMedium,
                    tracks!
                );
                break;
            case TimeRangeEnum.LongTerm:
                addedToPlaylist = await TrackService.AddTracksToPlaylist(
                    Costants.PlaylistIdTopLong,
                    tracks!
                );
                break;
        }

        return addedToPlaylist
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> Less(IConnectionMultiplexer cacheRedisConnection)
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdLess))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdLess))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track => track.Track.Popularity <= Costants.TracksLessPopularity)
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdLess,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> LessMedium(IConnectionMultiplexer cacheRedisConnection)
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdLessMedium))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdLessMedium))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessPopularity
                && track.Track.Popularity <= Costants.TracksLessMediumPopularity
            )
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdLessMedium,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> MoreMedium(IConnectionMultiplexer cacheRedisConnection)
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdMoreMedium))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdMoreMedium))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessMediumPopularity
                && track.Track.Popularity <= Costants.TracksMoreMediumPopularity
            )
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdMoreMedium,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> More(IConnectionMultiplexer cacheRedisConnection)
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdMore))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdMore))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track => track.Track.Popularity > Costants.TracksMoreMediumPopularity)
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdMore,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }

    public static async Task<IResult> Artist(
        string artistId,
        IdArtistPlaylistsBody idArtistPlaylistsBody,
        HttpContext httpContext,
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

        // check if playlists are empty
        if (
            !await PlaylistHelper.CheckIsEmptyPlaylist(
                idArtistPlaylistsBody.Less,
                idArtistPlaylistsBody.LessMedium,
                idArtistPlaylistsBody.MoreMedium,
                idArtistPlaylistsBody.More
            )
        )
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var allTracksArtist = allTracks
            .Where(track => track.Track.Artists.Any(artist => artist.Id == artistId))
            .ToList();

        var trackWithLessPopularity = allTracksArtist
            .Where(track => track.Track.Popularity <= Costants.TracksLessPopularity)
            .ToList();

        var trackWithLessMediumPopularity = allTracksArtist
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessPopularity
                && track.Track.Popularity <= Costants.TracksLessMediumPopularity
            )
            .ToList();

        var trackWithMoreMediumPopularity = allTracksArtist
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessMediumPopularity
                && track.Track.Popularity <= Costants.TracksMoreMediumPopularity
            )
            .ToList();

        var trackWithMorePopularity = allTracksArtist
            .Where(track => track.Track.Popularity > Costants.TracksMoreMediumPopularity)
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

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), DebuggerStepThrough]
public class WcfSublightClient : ClientBase<IWcfSublight>, IWcfSublight
{
	public WcfSublightClient()
	{
	}

	public WcfSublightClient(string endpointConfigurationName) : base(endpointConfigurationName)
	{
	}

	public WcfSublightClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
	{
	}

	public WcfSublightClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
	{
	}

	public WcfSublightClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
	{
	}

	public bool DownloadByID4(out string data, out double points, out string error, Guid sessionID, Guid subtitleID, int codePage, bool removeFormatting, string ticket)
	{
		return base.Channel.DownloadByID4(out data, out points, out error, sessionID, subtitleID, codePage, removeFormatting, ticket);
	}

	public bool ReportSubtitle(Guid session, Guid subtitleId)
	{
		return base.Channel.ReportSubtitle(session, subtitleId);
	}

	public bool ReportSubtitle2(Guid session, Guid subtitleId, ReportReason reason, string comment)
	{
		return base.Channel.ReportSubtitle2(session, subtitleId, reason, comment);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetNewSubtitlesResponse IWcfSublight.GetNewSubtitles(GetNewSubtitlesRequest request)
	{
		return base.Channel.GetNewSubtitles(request);
	}

	public bool GetNewSubtitles(Guid session, out Subtitle[] subtitles, out Release[] releases, out SubtitleActions[] actions, out string error)
	{
		GetNewSubtitlesResponse newSubtitles = ((IWcfSublight)this).GetNewSubtitles(new GetNewSubtitlesRequest
		{
			session = session
		});
		subtitles = newSubtitles.subtitles;
		releases = newSubtitles.releases;
		actions = newSubtitles.actions;
		error = newSubtitles.error;
		return newSubtitles.GetNewSubtitlesResult;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetFavoriteSubtitlesResponse IWcfSublight.GetFavoriteSubtitles(GetFavoriteSubtitlesRequest request)
	{
		return base.Channel.GetFavoriteSubtitles(request);
	}

	public bool GetFavoriteSubtitles(Guid session, out Subtitle[] subtitles, out Release[] releases, out SubtitleActions[] actions, out string error)
	{
		GetFavoriteSubtitlesResponse favoriteSubtitles = ((IWcfSublight)this).GetFavoriteSubtitles(new GetFavoriteSubtitlesRequest
		{
			session = session
		});
		subtitles = favoriteSubtitles.subtitles;
		releases = favoriteSubtitles.releases;
		actions = favoriteSubtitles.actions;
		error = favoriteSubtitles.error;
		return favoriteSubtitles.GetFavoriteSubtitlesResult;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetMyUploadsResponse IWcfSublight.GetMyUploads(GetMyUploadsRequest request)
	{
		return base.Channel.GetMyUploads(request);
	}

	public bool GetMyUploads(Guid session, out Subtitle[] subtitles, out Release[] releases, out SubtitleActions[] actions, out string error)
	{
		GetMyUploadsResponse myUploads = ((IWcfSublight)this).GetMyUploads(new GetMyUploadsRequest
		{
			session = session
		});
		subtitles = myUploads.subtitles;
		releases = myUploads.releases;
		actions = myUploads.actions;
		error = myUploads.error;
		return myUploads.GetMyUploadsResult;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetMyDownloadsResponse IWcfSublight.GetMyDownloads(GetMyDownloadsRequest request)
	{
		return base.Channel.GetMyDownloads(request);
	}

	public bool GetMyDownloads(Guid session, Guid[] guids, out Subtitle[] subtitles, out Release[] releases, out SubtitleActions[] actions, out string error)
	{
		GetMyDownloadsResponse myDownloads = ((IWcfSublight)this).GetMyDownloads(new GetMyDownloadsRequest
		{
			session = session,
			guids = guids
		});
		subtitles = myDownloads.subtitles;
		releases = myDownloads.releases;
		actions = myDownloads.actions;
		error = myDownloads.error;
		return myDownloads.GetMyDownloadsResult;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetSubtitleThanksResponse IWcfSublight.GetSubtitleThanks(GetSubtitleThanksRequest request)
	{
		return base.Channel.GetSubtitleThanks(request);
	}

	public bool GetSubtitleThanks(Guid session, Guid subtitleId, out SubtitleThank[] users, out bool allowThanks, out string error)
	{
		GetSubtitleThanksResponse subtitleThanks = ((IWcfSublight)this).GetSubtitleThanks(new GetSubtitleThanksRequest
		{
			session = session,
			subtitleId = subtitleId
		});
		users = subtitleThanks.users;
		allowThanks = subtitleThanks.allowThanks;
		error = subtitleThanks.error;
		return subtitleThanks.GetSubtitleThanksResult;
	}

	public bool AddSubtitleThank(out string error, Guid session, Guid subtitleId)
	{
		return base.Channel.AddSubtitleThank(out error, session, subtitleId);
	}

	public bool SubtitlePreview(out string data, out string error, Guid session, Guid subtitleId)
	{
		return base.Channel.SubtitlePreview(out data, out error, session, subtitleId);
	}

	public bool AddAlternativeTitle(out string error, Guid session, Guid subtitleID, string title, SubtitleLanguage language)
	{
		return base.Channel.AddAlternativeTitle(out error, session, subtitleID, title, language);
	}

	public bool GetSubtitleComments(out int totalComments, out SubtitleComment[] comments, out string error, Guid session, Guid subtitleID, SubtitleLanguage language, int elementIndexStart, int elementIndexEnd)
	{
		return base.Channel.GetSubtitleComments(out totalComments, out comments, out error, session, subtitleID, language, elementIndexStart, elementIndexEnd);
	}

	public bool SubtitleCommentVote(out int newRate, out string error, Guid session, Guid subtitleCommentID, int rate)
	{
		return base.Channel.SubtitleCommentVote(out newRate, out error, session, subtitleCommentID, rate);
	}

	public bool SubtitleCommentDelete(out string error, Guid session, Guid subtitleCommentID)
	{
		return base.Channel.SubtitleCommentDelete(out error, session, subtitleCommentID);
	}

	public bool AddSubtitleComment(out string error, Guid session, Guid subtitleID, SubtitleLanguage language, string message)
	{
		return base.Channel.AddSubtitleComment(out error, session, subtitleID, language, message);
	}

	public bool SynchronizeSubtitle(out string data, out double points, out string error, Guid sessionID, Guid subtitleID, int codePage, float videoFrameRate, float delay, string ticket)
	{
		return base.Channel.SynchronizeSubtitle(out data, out points, out error, sessionID, subtitleID, codePage, videoFrameRate, delay, ticket);
	}

	public bool GetDetails(out IMDB imdbInfo, out AlternativeTitle[] alternativeTitles, out string error, Guid session, string imdb)
	{
		return base.Channel.GetDetails(out imdbInfo, out alternativeTitles, out error, session, imdb);
	}

	public bool GetDetailsByHash(out IMDB imdbInfo, out string error, Guid session, string hash)
	{
		return base.Channel.GetDetailsByHash(out imdbInfo, out error, session, hash);
	}

	public bool UpdatePosterUrl(out string error, Guid session, string imdb, string posterUrl)
	{
		return base.Channel.UpdatePosterUrl(out error, session, imdb, posterUrl);
	}

	public bool UpdatePoster2(out string error, Guid session, string imdb, string posterUrl, string data)
	{
		return base.Channel.UpdatePoster2(out error, session, imdb, posterUrl, data);
	}

	public bool GetPosterUrl(out string newPosterUrl, out string error, Guid session, string posterUrl)
	{
		return base.Channel.GetPosterUrl(out newPosterUrl, out error, session, posterUrl);
	}

	public bool GetPoster(out string data, out string error, Guid session, string imdb)
	{
		return base.Channel.GetPoster(out data, out error, session, imdb);
	}

	public bool GetPoster2(out string data, out int settings, out string error, Guid session, string imdb)
	{
		return base.Channel.GetPoster2(out data, out settings, out error, session, imdb);
	}

	public bool UpdateUserRating(out string error, Guid session, string imdb, float rate)
	{
		return base.Channel.UpdateUserRating(out error, session, imdb, rate);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	VoteMovieHashResponse IWcfSublight.VoteMovieHash(VoteMovieHashRequest request)
	{
		return base.Channel.VoteMovieHash(request);
	}

	public bool VoteMovieHash(Guid session, string imdb, byte? season, int? episode, string videoHash, MovieHashVote type, out string error)
	{
		VoteMovieHashResponse voteMovieHashResponse = ((IWcfSublight)this).VoteMovieHash(new VoteMovieHashRequest
		{
			session = session,
			imdb = imdb,
			season = season,
			episode = episode,
			videoHash = videoHash,
			type = type
		});
		error = voteMovieHashResponse.error;
		return voteMovieHashResponse.VoteMovieHashResult;
	}

	public bool RequestPasswordReset(out string error, Guid session, string username)
	{
		return base.Channel.RequestPasswordReset(out error, session, username);
	}

	public bool GetSummary(out string summary, out string error, Guid session, Guid subtitleId, string uiLanguage)
	{
		return base.Channel.GetSummary(out summary, out error, session, subtitleId, uiLanguage);
	}

	public Guid LogIn6(out Guid userId, out string[] roles, out SubtitleLanguage[] primaryLanguages, out string[] settings, out string error, string username, string passwordHash, ClientInfo clientInfo, string[] args)
	{
		return base.Channel.LogIn6(out userId, out roles, out primaryLanguages, out settings, out error, username, passwordHash, clientInfo, args);
	}

	public Guid LogInSecure(out Guid userId, out string[] roles, out SubtitleLanguage[] primaryLanguages, out string[] settings, out string error, string username, string passwordHash, ClientInfo clientInfo, string[] args)
	{
		return base.Channel.LogInSecure(out userId, out roles, out primaryLanguages, out settings, out error, username, passwordHash, clientInfo, args);
	}

	public Guid LogInAnonymous4(out string[] settings, out string error, ClientInfo clientInfo, string[] args)
	{
		return base.Channel.LogInAnonymous4(out settings, out error, clientInfo, args);
	}

	public bool LogOut(out string error, Guid session)
	{
		return base.Channel.LogOut(out error, session);
	}

	public bool GetUserBySession(out User user, out string error, Guid session)
	{
		return base.Channel.GetUserBySession(out user, out error, session);
	}

	public bool Register(out string error, string username, string password, string email)
	{
		return base.Channel.Register(out error, username, password, email);
	}

	public bool GetUserInfo(out UserInfo userInfo, out string error, Guid session)
	{
		return base.Channel.GetUserInfo(out userInfo, out error, session);
	}

	public bool GetUserLog2(out DataSet ds, out double points, out string error, Guid session)
	{
		return base.Channel.GetUserLog2(out ds, out points, out error, session);
	}

	public bool ChangePassword(out string error, Guid session, string oldPassword, string newPassword)
	{
		return base.Channel.ChangePassword(out error, session, oldPassword, newPassword);
	}

	public bool UpdateEmail(out string error, Guid session, string newEmail)
	{
		return base.Channel.UpdateEmail(out error, session, newEmail);
	}

	public bool SendComment(out string error, Guid session, string subject, string senderEmail, string message)
	{
		return base.Channel.SendComment(out error, session, subject, senderEmail, message);
	}

	public bool SuggestTitles(out IMDB[] titles, out string error, string keyword, int itemsCount)
	{
		return base.Channel.SuggestTitles(out titles, out error, keyword, itemsCount);
	}

	public bool SuggestTitles2(out IMDB[] titles, out string error, Guid sessionId, string keyword, int itemsCount)
	{
		return base.Channel.SuggestTitles2(out titles, out error, sessionId, keyword, itemsCount);
	}

	public bool AddRelease(Guid session, Guid subtitleId, string release, FPS fps)
	{
		return base.Channel.AddRelease(session, subtitleId, release, fps);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	SearchSubtitles3Response IWcfSublight.SearchSubtitles3(SearchSubtitles3Request request)
	{
		return base.Channel.SearchSubtitles3(request);
	}

	public bool SearchSubtitles3(Guid session, string videoHash, string title, int? year, byte? season, int? episode, SubtitleLanguage[] languages, Genre[] genres, string sender, float? rateGreaterThan, out Subtitle[] subtitles, out Release[] releases, out bool isLimited, out string error)
	{
		SearchSubtitles3Response searchSubtitles3Response = ((IWcfSublight)this).SearchSubtitles3(new SearchSubtitles3Request
		{
			session = session,
			videoHash = videoHash,
			title = title,
			year = year,
			season = season,
			episode = episode,
			languages = languages,
			genres = genres,
			sender = sender,
			rateGreaterThan = rateGreaterThan
		});
		subtitles = searchSubtitles3Response.subtitles;
		releases = searchSubtitles3Response.releases;
		isLimited = searchSubtitles3Response.isLimited;
		error = searchSubtitles3Response.error;
		return searchSubtitles3Response.SearchSubtitles3Result;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	SearchSubtitles4Response IWcfSublight.SearchSubtitles4(SearchSubtitles4Request request)
	{
		return base.Channel.SearchSubtitles4(request);
	}

	public bool SearchSubtitles4(Guid session, string[] videoHashes, string title, int? year, byte? season, int? episode, SubtitleLanguage[] languages, Genre[] genres, string sender, float? rateGreaterThan, out Subtitle[] subtitles, out Release[] releases, out bool isLimited, out string error)
	{
		SearchSubtitles4Response searchSubtitles4Response = ((IWcfSublight)this).SearchSubtitles4(new SearchSubtitles4Request
		{
			session = session,
			videoHashes = videoHashes,
			title = title,
			year = year,
			season = season,
			episode = episode,
			languages = languages,
			genres = genres,
			sender = sender,
			rateGreaterThan = rateGreaterThan
		});
		subtitles = searchSubtitles4Response.subtitles;
		releases = searchSubtitles4Response.releases;
		isLimited = searchSubtitles4Response.isLimited;
		error = searchSubtitles4Response.error;
		return searchSubtitles4Response.SearchSubtitles4Result;
	}

	public bool GetSubtitleById(out Subtitle subtitle, out string error, Guid session, Guid subtitleId)
	{
		return base.Channel.GetSubtitleById(out subtitle, out error, session, subtitleId);
	}

	public bool GetStatistics(out DataSet ds, out string error, Guid session, StatisticsType type, string language)
	{
		return base.Channel.GetStatistics(out ds, out error, session, type, language);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	FindIMDBResponse IWcfSublight.FindIMDB(FindIMDBRequest request)
	{
		return base.Channel.FindIMDB(request);
	}

	public bool FindIMDB(string keyword, int? year, out IMDB[] result, out string error)
	{
		FindIMDBResponse findIMDBResponse = ((IWcfSublight)this).FindIMDB(new FindIMDBRequest
		{
			keyword = keyword,
			year = year
		});
		result = findIMDBResponse.result;
		error = findIMDBResponse.error;
		return findIMDBResponse.FindIMDBResult;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	FindIMDB2Response IWcfSublight.FindIMDB2(FindIMDB2Request request)
	{
		return base.Channel.FindIMDB2(request);
	}

	public bool FindIMDB2(Guid sessionId, string keyword, int? year, out IMDB[] result, out string error)
	{
		FindIMDB2Response findIMDB2Response = ((IWcfSublight)this).FindIMDB2(new FindIMDB2Request
		{
			sessionId = sessionId,
			keyword = keyword,
			year = year
		});
		result = findIMDB2Response.result;
		error = findIMDB2Response.error;
		return findIMDB2Response.FindIMDB2Result;
	}

	public bool RateSubtitle(out long newVotes, out double newRate, Guid session, Guid subtitleId, int rate)
	{
		return base.Channel.RateSubtitle(out newVotes, out newRate, session, subtitleId, rate);
	}

	public bool PublishSubtitle(out Guid ID, out string error, Guid session, Subtitle subtitle, string data)
	{
		return base.Channel.PublishSubtitle(out ID, out error, session, subtitle, data);
	}

	public bool PublishSubtitle2(out Guid ID, out double points, out string error, Guid session, Subtitle subtitle, string data)
	{
		return base.Channel.PublishSubtitle2(out ID, out points, out error, session, subtitle, data);
	}

	public bool PublishEditedSubtitle2(out Guid ID, out double points, out string error, Guid session, Guid originalSubtitleId, string comment, string data)
	{
		return base.Channel.PublishEditedSubtitle2(out ID, out points, out error, session, originalSubtitleId, comment, data);
	}

	public bool PublishEditedSubtitle3(out Guid ID, out double points, out string error, Guid session, Guid originalSubtitleId, string[] attributes, string comment, string data)
	{
		return base.Channel.PublishEditedSubtitle3(out ID, out points, out error, session, originalSubtitleId, attributes, comment, data);
	}

	public bool MapHash(out string error, Guid session, string hash1, string hash2, string imdb, string title, long size)
	{
		return base.Channel.MapHash(out error, session, hash1, hash2, imdb, title, size);
	}

	public bool CheckSubtitle3(out SubtitleType subtitleType, out string error, Guid session, string ticket, string plugin, string subtitleId, string imdb, string title, string language, string subtitleXml, string data)
	{
		return base.Channel.CheckSubtitle3(out subtitleType, out error, session, ticket, plugin, subtitleId, imdb, title, language, subtitleXml, data);
	}

	public bool CheckSubtitle4(out SubtitleType subtitleType, out double points, out string error, Guid session, string ticket, string plugin, string subtitleId, string imdb, string title, string language, string subtitleXml, string data)
	{
		return base.Channel.CheckSubtitle4(out subtitleType, out points, out error, session, ticket, plugin, subtitleId, imdb, title, language, subtitleXml, data);
	}

	public bool GetDownloadTicket(out string ticket, out short que, out string error, Guid session, string plugin, string id)
	{
		return base.Channel.GetDownloadTicket(out ticket, out que, out error, session, plugin, id);
	}

	public bool GetDownloadTicket2(out string ticket, out short que, out double points, out string error, Guid session, string plugin, string id)
	{
		return base.Channel.GetDownloadTicket2(out ticket, out que, out points, out error, session, plugin, id);
	}

	public bool UpdateSubtitle(out string error, Guid session, Subtitle subtitle)
	{
		return base.Channel.UpdateSubtitle(out error, session, subtitle);
	}

	public bool DeleteSubtitle(out string error, Guid session, Guid subtitleId)
	{
		return base.Channel.DeleteSubtitle(out error, session, subtitleId);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	AddHashLink3Response IWcfSublight.AddHashLink3(AddHashLink3Request request)
	{
		return base.Channel.AddHashLink3(request);
	}

	public bool AddHashLink3(Guid session, Guid subtitleID, string videoHash, out double? points, out string error)
	{
		AddHashLink3Response addHashLink3Response = ((IWcfSublight)this).AddHashLink3(new AddHashLink3Request
		{
			session = session,
			subtitleID = subtitleID,
			videoHash = videoHash
		});
		points = addHashLink3Response.points;
		error = addHashLink3Response.error;
		return addHashLink3Response.AddHashLink3Result;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	AddHashLink4Response IWcfSublight.AddHashLink4(AddHashLink4Request request)
	{
		return base.Channel.AddHashLink4(request);
	}

	public bool AddHashLink4(Guid session, Guid subtitleID, string[] hashes, out double? points, out string error)
	{
		AddHashLink4Response addHashLink4Response = ((IWcfSublight)this).AddHashLink4(new AddHashLink4Request
		{
			session = session,
			subtitleID = subtitleID,
			hashes = hashes
		});
		points = addHashLink4Response.points;
		error = addHashLink4Response.error;
		return addHashLink4Response.AddHashLink4Result;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	AddHashLinkSemiAutomatic3Response IWcfSublight.AddHashLinkSemiAutomatic3(AddHashLinkSemiAutomatic3Request request)
	{
		return base.Channel.AddHashLinkSemiAutomatic3(request);
	}

	public bool AddHashLinkSemiAutomatic3(Guid session, Guid subtitleID, string videoHash, out double? points, out string error)
	{
		AddHashLinkSemiAutomatic3Response addHashLinkSemiAutomatic3Response = ((IWcfSublight)this).AddHashLinkSemiAutomatic3(new AddHashLinkSemiAutomatic3Request
		{
			session = session,
			subtitleID = subtitleID,
			videoHash = videoHash
		});
		points = addHashLinkSemiAutomatic3Response.points;
		error = addHashLinkSemiAutomatic3Response.error;
		return addHashLinkSemiAutomatic3Response.AddHashLinkSemiAutomatic3Result;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	AddHashLinkSemiAutomatic4Response IWcfSublight.AddHashLinkSemiAutomatic4(AddHashLinkSemiAutomatic4Request request)
	{
		return base.Channel.AddHashLinkSemiAutomatic4(request);
	}

	public bool AddHashLinkSemiAutomatic4(Guid session, Guid subtitleID, string[] hashes, out double? points, out string error)
	{
		AddHashLinkSemiAutomatic4Response addHashLinkSemiAutomatic4Response = ((IWcfSublight)this).AddHashLinkSemiAutomatic4(new AddHashLinkSemiAutomatic4Request
		{
			session = session,
			subtitleID = subtitleID,
			hashes = hashes
		});
		points = addHashLinkSemiAutomatic4Response.points;
		error = addHashLinkSemiAutomatic4Response.error;
		return addHashLinkSemiAutomatic4Response.AddHashLinkSemiAutomatic4Result;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	AddHashLinkAutomatic3Response IWcfSublight.AddHashLinkAutomatic3(AddHashLinkAutomatic3Request request)
	{
		return base.Channel.AddHashLinkAutomatic3(request);
	}

	public bool AddHashLinkAutomatic3(Guid session, Guid subtitleID, string videoHash, out double? points, out string error)
	{
		AddHashLinkAutomatic3Response addHashLinkAutomatic3Response = ((IWcfSublight)this).AddHashLinkAutomatic3(new AddHashLinkAutomatic3Request
		{
			session = session,
			subtitleID = subtitleID,
			videoHash = videoHash
		});
		points = addHashLinkAutomatic3Response.points;
		error = addHashLinkAutomatic3Response.error;
		return addHashLinkAutomatic3Response.AddHashLinkAutomatic3Result;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	AddHashLinkAutomatic4Response IWcfSublight.AddHashLinkAutomatic4(AddHashLinkAutomatic4Request request)
	{
		return base.Channel.AddHashLinkAutomatic4(request);
	}

	public bool AddHashLinkAutomatic4(Guid session, Guid subtitleID, string[] hashes, out double? points, out string error)
	{
		AddHashLinkAutomatic4Response addHashLinkAutomatic4Response = ((IWcfSublight)this).AddHashLinkAutomatic4(new AddHashLinkAutomatic4Request
		{
			session = session,
			subtitleID = subtitleID,
			hashes = hashes
		});
		points = addHashLinkAutomatic4Response.points;
		error = addHashLinkAutomatic4Response.error;
		return addHashLinkAutomatic4Response.AddHashLinkAutomatic4Result;
	}

	public bool ReportHashLink(out string error, Guid session, Guid subtitleID, string videoHash)
	{
		return base.Channel.ReportHashLink(out error, session, subtitleID, videoHash);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetReleasesResponse IWcfSublight.GetReleases(GetReleasesRequest request)
	{
		return base.Channel.GetReleases(request);
	}

	public bool GetReleases(Guid[] subtitleIds, out Release[] releases, out string error)
	{
		GetReleasesResponse releases2 = ((IWcfSublight)this).GetReleases(new GetReleasesRequest
		{
			subtitleIds = subtitleIds
		});
		releases = releases2.releases;
		error = releases2.error;
		return releases2.GetReleasesResult;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	GetReleases2Response IWcfSublight.GetReleases2(GetReleases2Request request)
	{
		return base.Channel.GetReleases2(request);
	}

	public bool GetReleases2(Guid session, Guid[] subtitleIds, out Release[] releases, out string error)
	{
		GetReleases2Response releases2 = ((IWcfSublight)this).GetReleases2(new GetReleases2Request
		{
			session = session,
			subtitleIds = subtitleIds
		});
		releases = releases2.releases;
		error = releases2.error;
		return releases2.GetReleases2Result;
	}

	public bool GetHistory(out HistoryItem[] items, out string error, Guid session, Guid subtitleID)
	{
		return base.Channel.GetHistory(out items, out error, session, subtitleID);
	}
}

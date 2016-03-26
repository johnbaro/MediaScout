using System;
using System.CodeDom.Compiler;
using System.Data;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), ServiceContract(Namespace = "http://www.sublight.si/", ConfigurationName = "IWcfSublight")]
public interface IWcfSublight
{
	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/DownloadByID4", ReplyAction = "http://www.sublight.si/IWcfSublight/DownloadByID4Response"), XmlSerializerFormat]
	bool DownloadByID4(out string data, out double points, out string error, Guid sessionID, Guid subtitleID, int codePage, bool removeFormatting, string ticket);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/ReportSubtitle", ReplyAction = "http://www.sublight.si/IWcfSublight/ReportSubtitleResponse"), XmlSerializerFormat]
	bool ReportSubtitle(Guid session, Guid subtitleId);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/ReportSubtitle2", ReplyAction = "http://www.sublight.si/IWcfSublight/ReportSubtitle2Response"), XmlSerializerFormat]
	bool ReportSubtitle2(Guid session, Guid subtitleId, ReportReason reason, string comment);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetNewSubtitles", ReplyAction = "http://www.sublight.si/IWcfSublight/GetNewSubtitlesResponse"), XmlSerializerFormat]
	GetNewSubtitlesResponse GetNewSubtitles(GetNewSubtitlesRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetFavoriteSubtitles", ReplyAction = "http://www.sublight.si/IWcfSublight/GetFavoriteSubtitlesResponse"), XmlSerializerFormat]
	GetFavoriteSubtitlesResponse GetFavoriteSubtitles(GetFavoriteSubtitlesRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetMyUploads", ReplyAction = "http://www.sublight.si/IWcfSublight/GetMyUploadsResponse"), XmlSerializerFormat]
	GetMyUploadsResponse GetMyUploads(GetMyUploadsRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetMyDownloads", ReplyAction = "http://www.sublight.si/IWcfSublight/GetMyDownloadsResponse"), XmlSerializerFormat]
	GetMyDownloadsResponse GetMyDownloads(GetMyDownloadsRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetSubtitleThanks", ReplyAction = "http://www.sublight.si/IWcfSublight/GetSubtitleThanksResponse"), XmlSerializerFormat]
	GetSubtitleThanksResponse GetSubtitleThanks(GetSubtitleThanksRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddSubtitleThank", ReplyAction = "http://www.sublight.si/IWcfSublight/AddSubtitleThankResponse"), XmlSerializerFormat]
	bool AddSubtitleThank(out string error, Guid session, Guid subtitleId);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SubtitlePreview", ReplyAction = "http://www.sublight.si/IWcfSublight/SubtitlePreviewResponse"), XmlSerializerFormat]
	bool SubtitlePreview(out string data, out string error, Guid session, Guid subtitleId);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddAlternativeTitle", ReplyAction = "http://www.sublight.si/IWcfSublight/AddAlternativeTitleResponse"), XmlSerializerFormat]
	bool AddAlternativeTitle(out string error, Guid session, Guid subtitleID, string title, SubtitleLanguage language);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetSubtitleComments", ReplyAction = "http://www.sublight.si/IWcfSublight/GetSubtitleCommentsResponse"), XmlSerializerFormat]
	bool GetSubtitleComments(out int totalComments, out SubtitleComment[] comments, out string error, Guid session, Guid subtitleID, SubtitleLanguage language, int elementIndexStart, int elementIndexEnd);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SubtitleCommentVote", ReplyAction = "http://www.sublight.si/IWcfSublight/SubtitleCommentVoteResponse"), XmlSerializerFormat]
	bool SubtitleCommentVote(out int newRate, out string error, Guid session, Guid subtitleCommentID, int rate);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SubtitleCommentDelete", ReplyAction = "http://www.sublight.si/IWcfSublight/SubtitleCommentDeleteResponse"), XmlSerializerFormat]
	bool SubtitleCommentDelete(out string error, Guid session, Guid subtitleCommentID);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddSubtitleComment", ReplyAction = "http://www.sublight.si/IWcfSublight/AddSubtitleCommentResponse"), XmlSerializerFormat]
	bool AddSubtitleComment(out string error, Guid session, Guid subtitleID, SubtitleLanguage language, string message);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SynchronizeSubtitle", ReplyAction = "http://www.sublight.si/IWcfSublight/SynchronizeSubtitleResponse"), XmlSerializerFormat]
	bool SynchronizeSubtitle(out string data, out double points, out string error, Guid sessionID, Guid subtitleID, int codePage, float videoFrameRate, float delay, string ticket);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetDetails", ReplyAction = "http://www.sublight.si/IWcfSublight/GetDetailsResponse"), XmlSerializerFormat]
	bool GetDetails(out IMDB imdbInfo, out AlternativeTitle[] alternativeTitles, out string error, Guid session, string imdb);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetDetailsByHash", ReplyAction = "http://www.sublight.si/IWcfSublight/GetDetailsByHashResponse"), XmlSerializerFormat]
	bool GetDetailsByHash(out IMDB imdbInfo, out string error, Guid session, string hash);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/UpdatePosterUrl", ReplyAction = "http://www.sublight.si/IWcfSublight/UpdatePosterUrlResponse"), XmlSerializerFormat]
	bool UpdatePosterUrl(out string error, Guid session, string imdb, string posterUrl);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/UpdatePoster2", ReplyAction = "http://www.sublight.si/IWcfSublight/UpdatePoster2Response"), XmlSerializerFormat]
	bool UpdatePoster2(out string error, Guid session, string imdb, string posterUrl, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetPosterUrl", ReplyAction = "http://www.sublight.si/IWcfSublight/GetPosterUrlResponse"), XmlSerializerFormat]
	bool GetPosterUrl(out string newPosterUrl, out string error, Guid session, string posterUrl);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetPoster", ReplyAction = "http://www.sublight.si/IWcfSublight/GetPosterResponse"), XmlSerializerFormat]
	bool GetPoster(out string data, out string error, Guid session, string imdb);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetPoster2", ReplyAction = "http://www.sublight.si/IWcfSublight/GetPoster2Response"), XmlSerializerFormat]
	bool GetPoster2(out string data, out int settings, out string error, Guid session, string imdb);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/UpdateUserRating", ReplyAction = "http://www.sublight.si/IWcfSublight/UpdateUserRatingResponse"), XmlSerializerFormat]
	bool UpdateUserRating(out string error, Guid session, string imdb, float rate);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/VoteMovieHash", ReplyAction = "http://www.sublight.si/IWcfSublight/VoteMovieHashResponse"), XmlSerializerFormat]
	VoteMovieHashResponse VoteMovieHash(VoteMovieHashRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/RequestPasswordReset", ReplyAction = "http://www.sublight.si/IWcfSublight/RequestPasswordResetResponse"), XmlSerializerFormat]
	bool RequestPasswordReset(out string error, Guid session, string username);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetSummary", ReplyAction = "http://www.sublight.si/IWcfSublight/GetSummaryResponse"), XmlSerializerFormat]
	bool GetSummary(out string summary, out string error, Guid session, Guid subtitleId, string uiLanguage);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/LogIn6", ReplyAction = "http://www.sublight.si/IWcfSublight/LogIn6Response"), XmlSerializerFormat]
	Guid LogIn6(out Guid userId, out string[] roles, out SubtitleLanguage[] primaryLanguages, out string[] settings, out string error, string username, string passwordHash, ClientInfo clientInfo, string[] args);

	[OperationContract(Action = "http://www.sublight.si/LogInSecure", ReplyAction = "http://www.sublight.si/IWcfSublight/LogInSecureResponse"), XmlSerializerFormat]
	Guid LogInSecure(out Guid userId, out string[] roles, out SubtitleLanguage[] primaryLanguages, out string[] settings, out string error, string username, string passwordHash, ClientInfo clientInfo, string[] args);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/LogInAnonymous4", ReplyAction = "http://www.sublight.si/IWcfSublight/LogInAnonymous4Response"), XmlSerializerFormat]
	Guid LogInAnonymous4(out string[] settings, out string error, ClientInfo clientInfo, string[] args);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/LogOut", ReplyAction = "http://www.sublight.si/IWcfSublight/LogOutResponse"), XmlSerializerFormat]
	bool LogOut(out string error, Guid session);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetUserBySession", ReplyAction = "http://www.sublight.si/IWcfSublight/GetUserBySessionResponse"), XmlSerializerFormat]
	bool GetUserBySession(out User user, out string error, Guid session);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/Register", ReplyAction = "http://www.sublight.si/IWcfSublight/RegisterResponse"), XmlSerializerFormat]
	bool Register(out string error, string username, string password, string email);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetUserInfo", ReplyAction = "http://www.sublight.si/IWcfSublight/GetUserInfoResponse"), XmlSerializerFormat]
	bool GetUserInfo(out UserInfo userInfo, out string error, Guid session);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetUserLog2", ReplyAction = "http://www.sublight.si/IWcfSublight/GetUserLog2Response"), XmlSerializerFormat]
	bool GetUserLog2(out DataSet ds, out double points, out string error, Guid session);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/ChangePassword", ReplyAction = "http://www.sublight.si/IWcfSublight/ChangePasswordResponse"), XmlSerializerFormat]
	bool ChangePassword(out string error, Guid session, string oldPassword, string newPassword);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/UpdateEmail", ReplyAction = "http://www.sublight.si/IWcfSublight/UpdateEmailResponse"), XmlSerializerFormat]
	bool UpdateEmail(out string error, Guid session, string newEmail);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SendComment", ReplyAction = "http://www.sublight.si/IWcfSublight/SendCommentResponse"), XmlSerializerFormat]
	bool SendComment(out string error, Guid session, string subject, string senderEmail, string message);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SuggestTitles", ReplyAction = "http://www.sublight.si/IWcfSublight/SuggestTitlesResponse"), XmlSerializerFormat]
	bool SuggestTitles(out IMDB[] titles, out string error, string keyword, int itemsCount);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SuggestTitles2", ReplyAction = "http://www.sublight.si/IWcfSublight/SuggestTitles2Response"), XmlSerializerFormat]
	bool SuggestTitles2(out IMDB[] titles, out string error, Guid sessionId, string keyword, int itemsCount);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddRelease", ReplyAction = "http://www.sublight.si/IWcfSublight/AddReleaseResponse"), XmlSerializerFormat]
	bool AddRelease(Guid session, Guid subtitleId, string release, FPS fps);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SearchSubtitles3", ReplyAction = "http://www.sublight.si/IWcfSublight/SearchSubtitles3Response"), XmlSerializerFormat]
	SearchSubtitles3Response SearchSubtitles3(SearchSubtitles3Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/SearchSubtitles4", ReplyAction = "http://www.sublight.si/IWcfSublight/SearchSubtitles4Response"), XmlSerializerFormat]
	SearchSubtitles4Response SearchSubtitles4(SearchSubtitles4Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetSubtitleById", ReplyAction = "http://www.sublight.si/IWcfSublight/GetSubtitleByIdResponse"), XmlSerializerFormat]
	bool GetSubtitleById(out Subtitle subtitle, out string error, Guid session, Guid subtitleId);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetStatistics", ReplyAction = "http://www.sublight.si/IWcfSublight/GetStatisticsResponse"), XmlSerializerFormat]
	bool GetStatistics(out DataSet ds, out string error, Guid session, StatisticsType type, string language);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/FindIMDB", ReplyAction = "http://www.sublight.si/IWcfSublight/FindIMDBResponse"), XmlSerializerFormat]
	FindIMDBResponse FindIMDB(FindIMDBRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/FindIMDB2", ReplyAction = "http://www.sublight.si/IWcfSublight/FindIMDB2Response"), XmlSerializerFormat]
	FindIMDB2Response FindIMDB2(FindIMDB2Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/RateSubtitle", ReplyAction = "http://www.sublight.si/IWcfSublight/RateSubtitleResponse"), XmlSerializerFormat]
	bool RateSubtitle(out long newVotes, out double newRate, Guid session, Guid subtitleId, int rate);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/PublishSubtitle", ReplyAction = "http://www.sublight.si/IWcfSublight/PublishSubtitleResponse"), XmlSerializerFormat]
	bool PublishSubtitle(out Guid ID, out string error, Guid session, Subtitle subtitle, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/PublishSubtitle2", ReplyAction = "http://www.sublight.si/IWcfSublight/PublishSubtitle2Response"), XmlSerializerFormat]
	bool PublishSubtitle2(out Guid ID, out double points, out string error, Guid session, Subtitle subtitle, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/PublishEditedSubtitle2", ReplyAction = "http://www.sublight.si/IWcfSublight/PublishEditedSubtitle2Response"), XmlSerializerFormat]
	bool PublishEditedSubtitle2(out Guid ID, out double points, out string error, Guid session, Guid originalSubtitleId, string comment, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/PublishEditedSubtitle3", ReplyAction = "http://www.sublight.si/IWcfSublight/PublishEditedSubtitle3Response"), XmlSerializerFormat]
	bool PublishEditedSubtitle3(out Guid ID, out double points, out string error, Guid session, Guid originalSubtitleId, string[] attributes, string comment, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/MapHash", ReplyAction = "http://www.sublight.si/IWcfSublight/MapHashResponse"), XmlSerializerFormat]
	bool MapHash(out string error, Guid session, string hash1, string hash2, string imdb, string title, long size);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/CheckSubtitle3", ReplyAction = "http://www.sublight.si/IWcfSublight/CheckSubtitle3Response"), XmlSerializerFormat]
	bool CheckSubtitle3(out SubtitleType subtitleType, out string error, Guid session, string ticket, string plugin, string subtitleId, string imdb, string title, string language, string subtitleXml, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/CheckSubtitle4", ReplyAction = "http://www.sublight.si/IWcfSublight/CheckSubtitle4Response"), XmlSerializerFormat]
	bool CheckSubtitle4(out SubtitleType subtitleType, out double points, out string error, Guid session, string ticket, string plugin, string subtitleId, string imdb, string title, string language, string subtitleXml, string data);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetDownloadTicket", ReplyAction = "http://www.sublight.si/IWcfSublight/GetDownloadTicketResponse"), XmlSerializerFormat]
	bool GetDownloadTicket(out string ticket, out short que, out string error, Guid session, string plugin, string id);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetDownloadTicket2", ReplyAction = "http://www.sublight.si/IWcfSublight/GetDownloadTicket2Response"), XmlSerializerFormat]
	bool GetDownloadTicket2(out string ticket, out short que, out double points, out string error, Guid session, string plugin, string id);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/UpdateSubtitle", ReplyAction = "http://www.sublight.si/IWcfSublight/UpdateSubtitleResponse"), XmlSerializerFormat]
	bool UpdateSubtitle(out string error, Guid session, Subtitle subtitle);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/DeleteSubtitle", ReplyAction = "http://www.sublight.si/IWcfSublight/DeleteSubtitleResponse"), XmlSerializerFormat]
	bool DeleteSubtitle(out string error, Guid session, Guid subtitleId);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddHashLink3", ReplyAction = "http://www.sublight.si/IWcfSublight/AddHashLink3Response"), XmlSerializerFormat]
	AddHashLink3Response AddHashLink3(AddHashLink3Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddHashLink4", ReplyAction = "http://www.sublight.si/IWcfSublight/AddHashLink4Response"), XmlSerializerFormat]
	AddHashLink4Response AddHashLink4(AddHashLink4Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddHashLinkSemiAutomatic3", ReplyAction = "http://www.sublight.si/IWcfSublight/AddHashLinkSemiAutomatic3Response"), XmlSerializerFormat]
	AddHashLinkSemiAutomatic3Response AddHashLinkSemiAutomatic3(AddHashLinkSemiAutomatic3Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddHashLinkSemiAutomatic4", ReplyAction = "http://www.sublight.si/IWcfSublight/AddHashLinkSemiAutomatic4Response"), XmlSerializerFormat]
	AddHashLinkSemiAutomatic4Response AddHashLinkSemiAutomatic4(AddHashLinkSemiAutomatic4Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddHashLinkAutomatic3", ReplyAction = "http://www.sublight.si/IWcfSublight/AddHashLinkAutomatic3Response"), XmlSerializerFormat]
	AddHashLinkAutomatic3Response AddHashLinkAutomatic3(AddHashLinkAutomatic3Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/AddHashLinkAutomatic4", ReplyAction = "http://www.sublight.si/IWcfSublight/AddHashLinkAutomatic4Response"), XmlSerializerFormat]
	AddHashLinkAutomatic4Response AddHashLinkAutomatic4(AddHashLinkAutomatic4Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/ReportHashLink", ReplyAction = "http://www.sublight.si/IWcfSublight/ReportHashLinkResponse"), XmlSerializerFormat]
	bool ReportHashLink(out string error, Guid session, Guid subtitleID, string videoHash);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetReleases", ReplyAction = "http://www.sublight.si/IWcfSublight/GetReleasesResponse"), XmlSerializerFormat]
	GetReleasesResponse GetReleases(GetReleasesRequest request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetReleases2", ReplyAction = "http://www.sublight.si/IWcfSublight/GetReleases2Response"), XmlSerializerFormat]
	GetReleases2Response GetReleases2(GetReleases2Request request);

	[OperationContract(Action = "http://www.sublight.si/IWcfSublight/GetHistory", ReplyAction = "http://www.sublight.si/IWcfSublight/GetHistoryResponse"), XmlSerializerFormat]
	bool GetHistory(out HistoryItem[] items, out string error, Guid session, Guid subtitleID);
}

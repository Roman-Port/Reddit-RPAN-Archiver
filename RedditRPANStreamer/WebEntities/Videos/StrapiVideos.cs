using System;
using System.Collections.Generic;
using System.Text;

//https://strapi.reddit.com/videos/t3_cspwzn

namespace RedditRPANStreamer.WebEntities.Videos
{
    public class OutboundLink
    {
        public DateTime expiresAt { get; set; }
        public string url { get; set; }
    }

    public class AuthorInfo
    {
        public string __typename { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Styles
    {
        public object legacyIcon { get; set; }
        public string primaryColor { get; set; }
        public string icon { get; set; }
    }

    public class PostFlairSettings
    {
        public object position { get; set; }
        public bool? isEnabled { get; set; }
    }

    public class Subreddit
    {
        public string __typename { get; set; }
        public string id { get; set; }
        public Styles styles { get; set; }
        public string name { get; set; }
        public double? subscribers { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string path { get; set; }
        public bool? isNSFW { get; set; }
        public bool? isQuarantined { get; set; }
        public string wls { get; set; }
        public string prefixedName { get; set; }
        public PostFlairSettings postFlairSettings { get; set; }
        public List<object> originalContentCategories { get; set; }
        public bool? isThumbnailsEnabled { get; set; }
        public bool? isFreeFormReportingAllowed { get; set; }
    }

    public class Post
    {
        public string __typename { get; set; }
        public DateTime createdAt { get; set; }
        public int? crosspostCount { get; set; }
        public string domain { get; set; }
        public object editedAt { get; set; }
        public object flair { get; set; }
        public List<object> gildingTotals { get; set; }
        public string id { get; set; }
        public bool? isArchived { get; set; }
        public bool? isContestMode { get; set; }
        public bool? isHidden { get; set; }
        public bool? isLocked { get; set; }
        public bool? isNsfw { get; set; }
        public bool? isOriginalContent { get; set; }
        public bool? isSaved { get; set; }
        public bool? isScoreHidden { get; set; }
        public bool? isSelfPost { get; set; }
        public bool? isSpoiler { get; set; }
        public bool? isStickied { get; set; }
        public bool? isVisited { get; set; }
        public string liveCommentsWebsocket { get; set; }
        public object moderationInfo { get; set; }
        public OutboundLink outboundLink { get; set; }
        public string permalink { get; set; }
        public object score { get; set; }
        public object suggestedCommentSort { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string whitelistStatus { get; set; }
        public string voteState { get; set; }
        public AuthorInfo authorInfo { get; set; }
        public object authorOnlyInfo { get; set; }
        public double? commentCount { get; set; }
        public object content { get; set; }
        public object distinguishedAs { get; set; }
        public bool? isCrosspostable { get; set; }
        public bool? isMediaOnly { get; set; }
        public bool? isPollIncluded { get; set; }
        public object media { get; set; }
        public object postEventInfo { get; set; }
        public object thumbnail { get; set; }
        public double? upvoteRatio { get; set; }
        public object viewCount { get; set; }
        public Subreddit subreddit { get; set; }
    }

    public class RedditVideoStream
    {
        public string stream_id { get; set; }
        public string hls_url { get; set; }
        public long? publish_at { get; set; }
        public string thumbnail { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public long? update_at { get; set; }
        public object ended_at { get; set; }
        public object ended_reason { get; set; }
        public object finished_by { get; set; }
        public string state { get; set; }
        public int? duration_limit { get; set; }
    }

    public class StrapiVideos
    {
        public int? total_streams { get; set; }
        public int? rank { get; set; }
        public int? upvotes { get; set; }
        public int? downvotes { get; set; }
        public int? unique_watchers { get; set; }
        public int? continuous_watchers { get; set; }
        public string updates_websocket { get; set; }
        public bool? chat_disabled { get; set; }
        public bool? is_first_broadcast { get; set; }
        public double? broadcast_time { get; set; }
        public Post post { get; set; }
        public string share_link { get; set; }
        public RedditVideoStream stream { get; set; }
    }

    public class StrapiVideosContainer
    {
        public string status { get; set; }
        public StrapiVideos data { get; set; }
    }

    public class StrapiVideosSeedContainer
    {
        public string status { get; set; }
        public StrapiVideos[] data { get; set; }
    }
}

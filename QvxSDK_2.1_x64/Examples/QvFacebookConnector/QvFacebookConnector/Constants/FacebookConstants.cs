namespace QvFacebookConnector.Constants
{
    /// <summary>
    /// The supported facebook tags. The corresponding permissions should be defined in the Permissions enum.
    /// </summary>
    public enum FacebookMetadataTag { friends, family, movies, television, likes, books, music, games };

    /// <summary>
    /// The supported fields for an entertainment table.
    /// The corresponding permissions should be defined in the Permissions enum.
    /// </summary>
    public enum EntertainmentFields { name, category, id };

    /// <summary>
    /// The supported fields for a person table (used for friends and family).
    /// The corresponding permissions should be defined in the Permissions enum.
    /// </summary>
    public enum PersonsFields { id, name, gender, relationship_status, hometown, languages, birthday };

    /// <summary>
    /// The permissions corresponding to the supported meta data tags and fields.
    /// The inclusion of offline_access makes the returned access token long-lived.
    /// </summary>
    public enum FacebookPermissions { user_likes, friends_birthday, friends_relationships, friends_hometown, friends_likes, offline_access };
}

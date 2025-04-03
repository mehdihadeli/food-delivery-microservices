using System.Diagnostics;

namespace Shared.Observability;

public static class TagsExtensions
{
    /// <summary>
    /// Adds metadata from a dictionary to an Activity as tags.
    /// </summary>
    /// <param name="metadata">The dictionary containing the metadata.</param>
    /// <param name="activity">The Activity to which the metadata should be added.</param>
    public static void AddMetadataAsTags(this IDictionary<string, object> metadata, Activity activity)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(activity);

        foreach (var kvp in metadata)
        {
            activity.SetTag(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Merges multiple collections of tags into one. If the same key exists in multiple collections,
    /// the value from the last collection will override the earlier ones.
    /// </summary>
    /// <param name="tagCollections">A params array of tag collections.</param>
    /// <returns>A merged collection of tags.</returns>
    public static IEnumerable<KeyValuePair<string, string?>> MergeTags(
        params IEnumerable<KeyValuePair<string, string?>>[] tagCollections
    )
    {
        ArgumentNullException.ThrowIfNull(tagCollections);

        var mergedTags = new Dictionary<string, string?>();

        foreach (var tags in tagCollections)
        {
            foreach (var tag in tags)
            {
                mergedTags[tag.Key] = tag.Value;
            }
        }

        return mergedTags;
    }
}

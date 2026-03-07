namespace Hypermedia;

public static class HypermediaExtensions
{
    public static T AddSelf<T>(this T dto, string url) where T : HypermediaDto
        => dto.AddGet(url, "self");

    public static T AddGet<T>(this T dto, string url, string rel, bool useLink = true) where T : HypermediaDto
        => dto.AddLink(url, rel, "GET", useLink);

    public static T AddPost<T>(this T dto, string url, string rel, bool useLink = true) where T : HypermediaDto
        => dto.AddLink(url, rel, "POST", useLink);

    public static T AddPut<T>(this T dto, string url, string rel, bool useLink = true) where T : HypermediaDto
        => dto.AddLink(url, rel, "PUT", useLink);

    public static T AddPatch<T>(this T dto, string url, string rel, bool useLink = true) where T : HypermediaDto
        => dto.AddLink(url, rel, "PATCH", useLink);

    public static T AddDelete<T>(this T dto, string url, string rel, bool useLink = true) where T : HypermediaDto
        => dto.AddLink(url, rel, "DELETE", useLink);

    /// <summary>
    /// Adds a hypermedia link.
    /// </summary>
    /// <param name="url">The value for href.
    /// <param name="rel">The relationship to the resource.
    /// <param name="method">HTTP method.
    /// <param name="useLink">
    /// Specifies whether the link should be used. If the value is false, the method does nothing.
    /// Useful in LINQ queries where the link is generated with a condition.
    /// </param>
    public static T AddLink<T>(this T dto, string url, string rel, string method, bool useLink = true) where T : HypermediaDto
    {
        if (string.IsNullOrEmpty(url) || !useLink) return dto;

        dto.AddLinkInternal(rel, new HypermediaLink(url, method));
        return dto;
    }
}

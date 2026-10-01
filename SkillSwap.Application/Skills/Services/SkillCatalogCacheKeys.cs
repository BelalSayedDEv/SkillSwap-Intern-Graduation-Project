using Microsoft.Extensions.Caching.Memory;

namespace SkillSwap.Application.Skills.Services;

public static class SkillCatalogCacheKeys
{
    public const string CategoriesKey = "skills:categories:all";

    public const string ApprovedVersionKey = "skills:approved:version";

    public static readonly TimeSpan CategoriesTtl = TimeSpan.FromMinutes(30);

    public static readonly TimeSpan ApprovedPageTtl = TimeSpan.FromMinutes(2);

    public static readonly TimeSpan VersionTtl = TimeSpan.FromHours(1);

    public static string BuildApprovedKey(int version, int? categoryId, int page, int pageSize)
    {
        return $"skills:approved:v{version}:{categoryId?.ToString() ?? "all"}:{page}:{pageSize}";
    }

    public static int ReadApprovedVersion(IMemoryCache cache)
    {
        if (cache.TryGetValue(ApprovedVersionKey, out int version) && version >= 1)
            return version;
        return 1;
    }

    public static void InvalidateCatalog(IMemoryCache cache)
    {
        cache.Remove(CategoriesKey);
        var next = ReadApprovedVersion(cache) + 1;
        cache.Set(ApprovedVersionKey, next, VersionTtl);
    }
}

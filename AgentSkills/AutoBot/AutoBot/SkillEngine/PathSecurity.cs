namespace AutoBot.SkillEngine;

/// <summary>
/// 提供路径安全工具，防止目录遍历攻击。
/// 适用于 Skill 目录访问和 Tool 运行时的路径校验。
/// </summary>
public static class PathSecurity
{
    /// <summary>
    /// 验证路径是否安全地包含在允许的基目录内（含基目录本身）。
    /// 防止使用 ".."、符号链接等进行目录遍历攻击。
    /// 适用于 Skill 目录访问和 Tool 运行时的路径校验。
    /// </summary>
    /// <param name="path">待验证的路径（可以是绝对路径或相对路径）。</param>
    /// <param name="allowedBaseDirectory">路径必须包含在其中的基目录。</param>
    /// <returns>如果路径安全且包含在基目录内（或等于基目录）则返回 true，否则返回 false。</returns>
    public static bool IsPathSafe(string path, string allowedBaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(allowedBaseDirectory))
        {
            return false;
        }

        try
        {
            // Get the full, normalized paths
            var normalizedPath = Path.GetFullPath(path);
            var normalizedBase = Path.GetFullPath(allowedBaseDirectory);

            // Allow the base directory itself (e.g. relativePath=".")
            if (string.Equals(normalizedPath, normalizedBase, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Ensure base path ends with directory separator for proper prefix checking
            if (!normalizedBase.EndsWith(Path.DirectorySeparatorChar))
            {
                normalizedBase += Path.DirectorySeparatorChar;
            }

            // Check if the path starts with the base directory
            return normalizedPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // Any exception during path resolution means the path is not safe
            return false;
        }
    }

    /// <summary>
    /// 验证路径是否安全地包含在任一允许的基目录内（含基目录本身）。
    /// 适用于 Skill 目录访问和 Tool 运行时的路径校验。
    /// </summary>
    /// <param name="path">待验证的路径。</param>
    /// <param name="allowedBaseDirectories">允许的基目录集合。</param>
    /// <returns>如果路径安全且包含在任一基目录内（或等于基目录）则返回 true，否则返回 false。</returns>
    public static bool IsPathSafe(string path, IEnumerable<string> allowedBaseDirectories)
    {
        return allowedBaseDirectories.Any(baseDir => IsPathSafe(path, baseDir));
    }

    /// <summary>
    /// 在指定基目录内安全地解析相对路径。
    /// 适用于 Skill 目录访问和 Tool 运行时的路径解析。
    /// </summary>
    /// <param name="baseDirectory">基目录（绝对路径）。</param>
    /// <param name="relativePath">基目录内的相对路径（"." 表示基目录本身）。</param>
    /// <returns>如果路径安全则返回解析后的绝对路径，否则返回 null。</returns>
    public static string? ResolveSafePath(string baseDirectory, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        try
        {
            var combinedPath = Path.Combine(baseDirectory, relativePath);
            var resolvedPath = Path.GetFullPath(combinedPath);

            if (IsPathSafe(resolvedPath, baseDirectory))
            {
                return resolvedPath;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 检查文件或目录是否为符号链接。
    /// </summary>
    /// <param name="path">待检查的路径。</param>
    /// <returns>如果是符号链接则返回 true，否则返回 false。</returns>
    public static bool IsSymbolicLink(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                return dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 解析符号链接，获取真实路径。
    /// </summary>
    /// <param name="path">待解析的路径。</param>
    /// <returns>解析后的真实路径，解析失败则返回 null。</returns>
    public static string? GetRealPath(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return fileInfo.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? fileInfo.FullName;
            }

            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                return dirInfo.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? dirInfo.FullName;
            }

            return Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 先解析符号链接，再验证路径是否安全地包含在允许的基目录内。
    /// </summary>
    /// <param name="path">待验证的路径。</param>
    /// <param name="allowedBaseDirectory">解析后的路径必须包含在其中的基目录。</param>
    /// <returns>如果解析后的路径安全则返回 true，否则返回 false。</returns>
    public static bool IsPathSafeWithSymlinkResolution(string path, string allowedBaseDirectory)
    {
        var realPath = GetRealPath(path);
        if (realPath is null)
        {
            return false;
        }

        return IsPathSafe(realPath, allowedBaseDirectory);
    }
}

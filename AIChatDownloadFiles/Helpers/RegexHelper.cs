using System.Text.RegularExpressions;

namespace AIChatDownloadFiles.Helpers {
    public static partial class RegexHelper {
        [GeneratedRegex(@"^cntr_[a-z0-9]+$", RegexOptions.IgnoreCase)]
        public static partial Regex ContainerRegex();

        [GeneratedRegex(@"^cfile_[a-z0-9]+$", RegexOptions.IgnoreCase)]
        public static partial Regex FileIdRegex();
    }
}

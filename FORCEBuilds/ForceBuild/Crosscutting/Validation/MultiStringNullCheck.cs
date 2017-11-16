using System.Linq;

namespace FORCEBuild.Crosscutting.Validation
{
    public class StringCheck
    {
        public static bool IsNullOrEmpty(params string[] strings)
        {
            return strings.Any(string.IsNullOrEmpty);
        }
    }
}
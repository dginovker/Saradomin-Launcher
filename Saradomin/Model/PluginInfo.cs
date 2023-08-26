using System.Linq;
using System.Text.RegularExpressions;

namespace Saradomin.Model
{
    public class PluginInfo
    {
        private static Regex DescSplitRegex = new Regex("\r\n|\r|\n");
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool Installed { get; set; }
        public bool UpdateAvailable { get; set; }
        public bool CanUpdate => UpdateAvailable && Installed;
        public string DescriptionShort => DescSplitRegex.Split(Description)[0];

        public PluginInfo(string name)
        {
            Name = name;
        }
    }
}
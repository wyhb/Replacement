using System.Collections.Generic;

namespace Replacement
{
    public class ReplaceSet
    {
        public string WorkSpace { get; set; }
        public List<ReplaceText> ReplaceTexts { get; set; }
        public List<FileRename> FileRenames { get; set; }
    }
}
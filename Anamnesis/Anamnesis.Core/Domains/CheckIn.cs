using System;

namespace Anamnesis.Core.Domains
{
    public class CheckIn
    {
        public DateTime CommitDate { get; set; }
        public string ChangesetID { get; set; }
        public string Author { get; set; }
        public string FileName { get; set; }
    }
}

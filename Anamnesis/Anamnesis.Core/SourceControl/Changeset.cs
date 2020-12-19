using System;
using System.Collections.Generic;

namespace Anamnesis.Core.SourceControl
{
    public class Changeset
    {
        public string ID { get; set; }
        public DateTime CommitDate { get; set; }
        public string Author { get; set; }
        public IEnumerable<Change> Changes { get; set; }
    }
}

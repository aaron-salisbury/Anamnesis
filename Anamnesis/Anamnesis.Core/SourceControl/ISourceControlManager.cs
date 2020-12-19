using Anamnesis.Core.Domains;
using System.Collections.Generic;

namespace Anamnesis.Core.SourceControl
{
    public interface ISourceControlManager
    {
        void Connect();
        IEnumerable<string> GetAuthorizedUsers();
        IEnumerable<Changeset> QueryChangesets(ChangesetQuery changesetQuery, string branchMapping);
    }
}

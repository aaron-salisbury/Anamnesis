using Anamnesis.Core.Data;
using Anamnesis.Core.Domains;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anamnesis.Core.SourceControl
{
    public class TFSManager : ISourceControlManager
    {
        private ILogger _logger;
        private TfsTeamProjectCollection _tfsTeamProjectCollection;
        private VersionControlServer _versionControlServer;

        public TFSManager(ILogger logger)
        {
            _logger = logger;
        }

        public void Connect()
        {
            try
            {
                _logger.Information("Connecting to TFS.");

                _tfsTeamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(Properties.Settings.Default.TFSUrl));
                _tfsTeamProjectCollection.EnsureAuthenticated();

                _versionControlServer = _tfsTeamProjectCollection.GetService<VersionControlServer>();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }

        public IEnumerable<string> GetAuthorizedUsers()
        {
            return CRUD.ReadTFSUsers(_tfsTeamProjectCollection, _logger)?.Select(tfi => tfi.DisplayName);
        }

        public IEnumerable<Changeset> QueryChangesets(ChangesetQuery changesetQuery, string branchMapping)
        {
            foreach (Microsoft.TeamFoundation.VersionControl.Client.Changeset tfsChangeset in CRUD.ReadTFSChangesets(_versionControlServer, changesetQuery, branchMapping, _logger))
            {
                yield return new Changeset()
                {
                    ID = tfsChangeset.ChangesetId.ToString(),
                    CommitDate = tfsChangeset.CreationDate,
                    Author = tfsChangeset.OwnerDisplayName,
                    Changes = tfsChangeset.Changes
                        .Where(c => c.Item != null)
                        .Select(c => new Change()
                        {
                            RepositoryPath = c.Item.ServerItem,
                            DownloadFile = c.Item.DownloadFile
                        })
                };
            }
        }
    }
}

using Anamnesis.Core.Base;
using Anamnesis.Core.Base.ValidationAttributes;
using Anamnesis.Core.Data;
using Anamnesis.Core.Domains;
using Anamnesis.Core.SourceControl;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Anamnesis.Core
{
    public class Manager : ValidatableModel
    {
        private List<CheckIn> _checkIns = null;
        public List<CheckIn> CheckIns
        {
            get => _checkIns;
            set
            {
                _checkIns = value;
                RaisePropertyChanged(nameof(CheckIns));
            }
        }

        private ChangesetQuery _changesetQuery;
        public ChangesetQuery ChangesetQuery
        {
            get => _changesetQuery;
            set
            {
                _changesetQuery = value;
                RaisePropertyChanged(nameof(ChangesetQuery));
            }
        }

        private string _userName;
        [UserIsAuthorized]
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                RaisePropertyChanged(nameof(UserName));
            }
        }

        private readonly ILogger _logger;
        private readonly ISourceControlManager _sourceControlManager;

        public Manager(AppLogger appLogger)
        {
            _logger = appLogger.Logger;
            _sourceControlManager = new TFSManager(_logger);

            _sourceControlManager.Connect();

            SetDefaultChangesetQuery();
        }

        private void SetDefaultChangesetQuery()
        {
            ChangesetQuery = new ChangesetQuery()
            {
                HighDate = GetMostRecentSunday(),
                AuthorizedUsers = _sourceControlManager.GetAuthorizedUsers()?.ToList()
            };

            ChangesetQuery.SelectedChangesetAuthors = new BindingList<string>(CRUD.ReadSelectedChangesetAuthors(ChangesetQuery.AuthorizedUsers, _logger).ToList());
            ChangesetQuery.LowDate = ChangesetQuery.HighDate.Value.AddDays((Properties.Settings.Default.NumDaysInCycle - 1) * -1);
        }

        private static DateTime GetMostRecentSunday()
        {
            DateTime mostRecentSunday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            while (mostRecentSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                mostRecentSunday = mostRecentSunday.AddDays(-1);
            }

            return mostRecentSunday;
        }

        public bool FindSourceControlCheckIns()
        {
            if (!Validate() || !ChangesetQuery.Validate())
            {
                _logger.Error("Failed to begin searching source control. Invalid settings.");
                return false;
            }

            try
            {
                // Save users used in query since they are usually the same every time for the person running this.
                CRUD.UpdateSelectedChangesetAuthors(ChangesetQuery.SelectedChangesetAuthors, _logger);

                HashSet<CheckIn> checkIns = new HashSet<CheckIn>();

                foreach (Changeset changeset in _sourceControlManager.QueryChangesets(ChangesetQuery, Properties.Settings.Default.TFSMasterBranch))
                {
                    BuildCheckInData(changeset);
                }

                CheckIns = checkIns.ToList();

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return false;
            }
        }

        private List<CheckIn> BuildCheckInData(Changeset changeset)
        {
            //TODO: Customize logic here to filter and create the check in data you care about. This default logic just looks for C# files.

            List<CheckIn> checkIns = new List<CheckIn>();

            foreach (Change change in changeset.Changes)
            {
                string fileName = Path.GetFileName(change.RepositoryPath);

                if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".cs"))
                {
                    using (StreamReader streamReader = new StreamReader(change.DownloadFile(), Encoding.Default))
                    {
                        //TODO: Can examine the code if you'd like.
                        string cSharpCode = streamReader.ReadToEnd().Replace("\r\n", " ");

                        CheckIn checkIn = new CheckIn
                        {
                            CommitDate = changeset.CommitDate,
                            ChangesetID = changeset.ID,
                            Author = changeset.Author,
                            FileName = fileName
                        };

                        checkIns.Add(checkIn);
                    }
                }
            }

            return checkIns;
        }
    }
}

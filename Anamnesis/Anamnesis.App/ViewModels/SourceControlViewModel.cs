using Anamnesis.Core;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Anamnesis.App.ViewModels
{
    public class SourceControlViewModel : BaseViewModel
    {
        public Manager Manager { get; set; }
        public RelayCommand AddUserCommand { get; }
        public RelayCommand<IList> RemoveAuthorCommand { get; }
        public RelayCommand FindCheckInsCommand { get; }

        private bool _canExport;
        public bool CanExport
        {
            get => _canExport;
            set
            {
                _canExport = value;
                RaisePropertyChanged(nameof(CanExport));
            }
        }

        public SourceControlViewModel()
        {
            Manager = new Manager(AppLogger);
            AddUserCommand = new RelayCommand(AddUser);
            RemoveAuthorCommand = new RelayCommand<IList>((l) => RemoveAuthor(l));
            FindCheckInsCommand = new RelayCommand(async () => await InitiateFindCheckInsAsync(), () => !IsBusy);
        }

        private void AddUser()
        {
            if (!string.IsNullOrEmpty(Manager.UserName) &&
                !Manager.ChangesetQuery.SelectedChangesetAuthors.Contains(Manager.UserName) &&
                Manager.ChangesetQuery.AuthorizedUsers.Contains(Manager.UserName))
            {
                Manager.ChangesetQuery.SelectedChangesetAuthors.Add(Manager.UserName);
                Manager.UserName = null;
            }
        }

        /// <param name="selectedItems">User selected names to remove from authors collection; passed as a command parameter.</param>
        private void RemoveAuthor(IList selectedItems)
        {
            foreach (string selectedAuthor in selectedItems.Cast<string>().ToList())
            {
                Manager.ChangesetQuery.SelectedChangesetAuthors.Remove(selectedAuthor);
            }
        }

        private async Task InitiateFindCheckInsAsync()
        {
            bool successfullyCompleted = false;

            try
            {
                if (Manager.Validate() && Manager.ChangesetQuery.Validate())
                {
                    IsBusy = true;
                    successfullyCompleted = await FindMigrationsAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    IsBusy = false;
                    FindCheckInsCommand.RaiseCanExecuteChanged();
                    CanExport = successfullyCompleted;
                });
            }
        }

        private Task<bool> FindMigrationsAsync()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            Task.Run(() =>
            {
                bool processIsSuccessful = Manager.FindSourceControlCheckIns();

                tcs.SetResult(processIsSuccessful);
            }).ConfigureAwait(false);

            return tcs.Task;
        }
    }
}

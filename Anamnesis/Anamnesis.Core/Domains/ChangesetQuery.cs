using Anamnesis.Core.Base;
using Anamnesis.Core.Base.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Anamnesis.Core.Domains
{
    public class ChangesetQuery : ValidatableModel
    {
        private DateTime? _lowDate;
        [Required]
        [HighDateGreaterThanLowDate]
        public DateTime? LowDate
        {
            get => _lowDate;
            set
            {
                _lowDate = value;
                RaisePropertyChanged(nameof(LowDate));
            }
        }

        private DateTime? _highDate;
        [Required]
        [HighDateGreaterThanLowDate]
        public DateTime? HighDate
        {
            get => _highDate;
            set
            {
                _highDate = value;
                RaisePropertyChanged(nameof(HighDate));
            }
        }

        private List<string> _authorizedUsers;
        public List<string> AuthorizedUsers
        {
            get => _authorizedUsers;
            set
            {
                _authorizedUsers = value;
                RaisePropertyChanged(nameof(AuthorizedUsers));
            }
        }

        private BindingList<string> _selectedChangesetAuthors;
        public BindingList<string> SelectedChangesetAuthors
        {
            get => _selectedChangesetAuthors;
            set
            {
                _selectedChangesetAuthors = value;
                RaisePropertyChanged(nameof(SelectedChangesetAuthors));
            }
        }
    }
}

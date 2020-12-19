using GalaSoft.MvvmLight.Ioc;

namespace Anamnesis.App.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<ShellWindowViewModel>();
            SimpleIoc.Default.Register<SettingsAppearanceViewModel>();
            SimpleIoc.Default.Register<LogViewModel>();
            SimpleIoc.Default.Register <SourceControlViewModel>();
        }

        public ShellWindowViewModel ShellWindowViewModel { get { return SimpleIoc.Default.GetInstance<ShellWindowViewModel>(); } }
        public SettingsAppearanceViewModel SettingsAppearanceViewModel { get { return SimpleIoc.Default.GetInstance<SettingsAppearanceViewModel>(); } }
        public LogViewModel LogViewModel { get { return SimpleIoc.Default.GetInstance<LogViewModel>(); } }
        public SourceControlViewModel SourceControlViewModel { get { return SimpleIoc.Default.GetInstance<SourceControlViewModel>(); } }
    }
}

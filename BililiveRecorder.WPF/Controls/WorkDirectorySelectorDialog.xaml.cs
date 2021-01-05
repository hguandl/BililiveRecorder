using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAPICodePack.Dialogs;
using WPFLocalizeExtension.Extensions;

namespace BililiveRecorder.WPF.Controls
{
    /// <summary>
    /// Interaction logic for WorkDirectorySelectorDialog.xaml
    /// </summary>
    public partial class WorkDirectorySelectorDialog : INotifyPropertyChanged
    {
        private string error = string.Empty;
        private string path = string.Empty;

        public string Error { get => this.error; set => this.SetField(ref this.error, value); }

        public string Path { get => this.path; set => this.SetField(ref this.path, value); }

        public WorkDirectorySelectorDialog()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) { return false; }
            field = value; this.OnPropertyChanged(propertyName); return true;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var title = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:WorkDirectorySelector_Title");
            var fileDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = title,
                AddToMostRecentlyUsedList = false,
                EnsurePathExists = true,
                NavigateToShortcut = true,
                InitialDirectory = Path,
            };
            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.Path = fileDialog.FileName;
            }
        }
    }
}

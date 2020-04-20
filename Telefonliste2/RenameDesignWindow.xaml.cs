using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für RenameDesignWindow.xaml
    /// </summary>
    public partial class RenameDesignWindow : Window, IDataErrorInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Regex reg = new Regex(@"(AUX|PRN|NUL|COM\d|LPT\d)+\s*$");

        readonly DesignsEditWindow dEditWindow;
        readonly GroupsList presets;
        string oldName, newName, errorMessage;

        //Konstruktor
        public RenameDesignWindow(DesignsEditWindow mWin, string oldname, GroupsList _presets)
        {
            this.presets = _presets;
            dEditWindow = mWin;
            oldName = oldname;
            InitializeComponent();
            this.DataContext = this;

            oldName = oldname;
            NewName = oldName;
            tblHelpText.Text = "Das Design \"" + oldName + "\" wird umbenannt in:";
            tbName.Text = oldName;
        }
       

        public void ShowDialog(Window owner)
        {
            this.Owner = owner;
            this.ShowDialog();
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public string NewName
        {
            get { return newName; }
            set
            {
                newName = value.Trim();
                OnPropertyChanged("NewName");
            }
        }

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                ErrorMessage = null;
                if (columnName == "NewName" && !string.IsNullOrEmpty(NewName))
                {
                    if (NewName == App.STANDARD_DESIGN_NAME)
                        ErrorMessage = string.Format("Der Name \"{0}\" ist reserviert.", App.STANDARD_DESIGN_NAME);
                    else if (NewName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1)
                    {
                        ErrorMessage = "Der Name enthält unerlaubte Zeichen!";
                    }
                    else
                    {
                        if (reg.IsMatch(NewName))
                            ErrorMessage = "Der Dateiname ist nicht erlaubt!";
                    }
                }
                return ErrorMessage;
            }
        }

        #region Event Handler

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (RenameDesign()) this.DialogResult = true;
        }

        #endregion

        #region PRIVATE

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool RenameDesign()
        {
            if (NewName == oldName) return true;

            if (string.IsNullOrEmpty(NewName))
            {
                MessageBox.Show(this, "Geben Sie einen Namen ein!",
                        "Telefonliste - Design umbenennen", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                NewName = oldName;
                return false;
            }

            if (dEditWindow.Designs.Contains(NewName))
            {
                MessageBox.Show(this, "Ein gleichnamiges Design existiert bereits.",
                        "Telefonliste - Design umbenennen", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                return false;
            }

            string newFileName = Utility.CreateDesignPath(NewName);
            string oldFileName = Utility.CreateDesignPath(oldName);

            try
            {
                System.IO.File.Move(oldFileName, newFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Das Design konnte nicht umbenannt werden." + Environment.NewLine +
                    "Fehler: " + ex.Message,
                        "Telefonliste - Design umbenennen", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return false;
            }

            foreach (var preset in this.presets)
            {
                if (preset.Design == oldName)
                    preset.Design = NewName;
            }

            int ind = dEditWindow.Designs.IndexOf(oldName);
            dEditWindow.Designs[ind] = newName;

            return true;
        }

        

        #endregion

    }//RenameDesignWindow-class
        

    [ValueConversion(typeof(string), typeof(bool))]
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)  return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;
using FK.ContactData2;
using FK.ContactIO;
using System.Linq;
using System.Threading.Tasks;


namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region private Felder und Konstanten

        private GroupsList presets;
        private string lastFile;
        private List<Contact> data;

        ///// <summary>
        ///// Wert der Property LastFile, wenn keine Datei geladen ist.
        ///// </summary>
        //private const string NO_FILENAME = "";

        #endregion


        #region Konstruktor

        public MainWindow()
        {
            this.Title = App.PROGRAM_NAME;
            InitializeComponent();
        }

        #endregion

        #region Properties

        public GroupsList Groups
        {
            get { return presets; }
            set
            {
                presets = value;
                OnPropertyChanged("Groups");
            }
        }

        public bool DataLoaded
        {
            get { return this.Data != null; }
        }


        public List<Contact> Data
        {
            get {return data;}
            private set
            {
                data = value;
                OnPropertyChanged("Data");
            }
        }
        

        public string LastFile
        {
            get { return this.lastFile; }
            set
            {
                this.lastFile = value;
                OnPropertyChanged("LastFile");
            }
        }


        #endregion


        #region Commands


        private void IsDataLoaded(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.DataLoaded;
        }


        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "", // Default file name
                DefaultExt = ".csv", // Default file extension
                Filter = "vcf-Dateien (.vcf)|*.vcf|csv-Dateien (.csv)|*.csv", // Filter files by extension
                Title = "Telefonliste - Telefondaten öffnen",
                CheckFileExists = true,
                DereferenceLinks = true,
                Multiselect = false
            };

            if (dlg.ShowDialog(this) == true)
            {
                OpenData(dlg.FileName);
            }
        }


        private void ImportVcfDirectory_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Ordner mit vcf-Dateien importieren.";
                dialog.ShowNewFolderButton = false;
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OpenData(dialog.SelectedPath, true);
                }
            }
        }


        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Data = null;
            OnPropertyChanged("DataLoaded");
            this.LastFile = null;
        }


        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(App.HELP_PATH);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Die Hilfedatei kann nicht geöffnet werden." + Environment.NewLine + ex.Message,
                        App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void GeneratePhoneListExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                List<TelEntry> telList = new List<TelEntry>();

                foreach (var item in this.Data)
                {
                    if (item.Visible)
                    {
                        var telEntry = new TelEntry(item);

                        if (telEntry.Data.Count != 0)
                            telList.Add(telEntry);
                    }
                }

                var lGen = new ListGenerator(telList, Groups[lbGroups.SelectedIndex].GroupName, "Telefon",  Groups[lbGroups.SelectedIndex].Design);
                lGen.GenerateOutput();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Die Telefonliste konnte nicht erstellt werden."
                    + Environment.NewLine + Environment.NewLine + "Fehler: " + ex.Message,
                        App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CreateMailExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                StringBuilder mailAddresses = new StringBuilder();
                mailAddresses.Append("mailto:?bcc=");

                foreach (var item in this.Data)
                {
                    if (item.Visible == true && item.ContactData.PreferredEmailAddress != null)
                    {
                        mailAddresses.Append(item.ContactData.PreferredEmailAddress.Address);
                        mailAddresses.Append(';');
                    }
                }

                mailAddresses.Remove(mailAddresses.Length - 1, 1);

                Task.Factory.StartNew(()=> Process.Start(mailAddresses.ToString()));

                


                //System.Windows.Resources.StreamResourceInfo sri =
                //    Application.GetResourceStream(new Uri(@"Resources\mail.eml", UriKind.Relative));
                //string s;
                //using (StreamReader sr = new StreamReader(sri.Stream))
                //{
                //    s = sr.ReadToEnd();
                //}
                //s = s.Replace("<FK.Telefonliste.BCC>", mailAddresses.ToString());

                //string fileName = Utility.CreateTmpFilePath(".eml");

                //File.WriteAllText(fileName, s);

                ////WindowsLiveMail starten
                //Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Es konnte keine E-Mail erstellt werden."
                    + Environment.NewLine + Environment.NewLine + "Fehler: " + ex.Message,
                        App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void GenerateAgeListExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            List<AgeEntry> ageList = new List<AgeEntry>();

            foreach (var item in this.Data)
            {
                if (item.Visible)
                {
                    ageList.Add(new AgeEntry(item));
                }
            }

            ageList.Sort((a1, a2) =>
            {
                var result = a1.Age.CompareTo(a2.Age);
                return result == 0 ? a1.Name.CompareTo(a2.Name) : result;
            });

            var lGen = new ListGenerator(ageList, Groups[lbGroups.SelectedIndex].GroupName, "Alter", Groups[lbGroups.SelectedIndex].Design);
            lGen.GenerateOutput();
        }


        private void EditDesigns_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new DesignsEditWindow(this).ShowDialog();
        }


        #region Group

        #region NewGroup

        private void NewGroupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var item in this.Data)
            {
                item.Visible = false;
            }

            var result = new GroupEditWindow(this, GroupEditWindow.GroupAction.New, "Telefonliste - Neue Gruppe").ShowDialog();
            if (result != true)
            {
                this.lbGroups_SelectionChanged(null, null);
            }
        }

        #endregion

        #region EditGroup

        private void EditGroupExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            if (new GroupEditWindow(this, GroupEditWindow.GroupAction.Edit, "Telefonliste - Gruppe bearbeiten").ShowDialog() == true)
                lbGroups_SelectionChanged(null, null);
        }


        private void EditGroupCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lbGroups.SelectedIndex > 0;
        }

        #endregion

        #region DeleteGroup

        private void DeleteGroupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Group pset = (Group)lbGroups.SelectedItem;

            MessageBoxResult result = MessageBox.Show(this, "Wollen Sie die Gruppe \"" +
                pset.GroupName + "\" wirklich unwiderruflich löschen?", App.PROGRAM_NAME,
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes)
            {
                lbGroups.SelectedIndex -= 1;
                lbGroups.ScrollIntoView(lbGroups.SelectedItem);
                Groups.Remove(pset);
            }
        }

        #endregion

        #region RenameGroup

        private void RenameGroupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            new GroupRenameWindow(this).ShowDialog(this);
        }

        #endregion

        #region ImportGroups

        private void ImportGroups_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "", // Default file name
                               //dlg.DefaultExt = ".csv"; // Default file extension
                Filter = "Exportierte Gruppen|" + GroupsIOWindow.BACKUP_FILENAME, // Filter files by extension
                Title = "Telefonliste - Gruppen importieren",
                CheckFileExists = true,
                DereferenceLinks = true,
                Multiselect = false
            };

            if (dlg.ShowDialog(this) == true)
            {
                var wnd = new GroupsIOWindow(GroupsIOWindow.Job.Import, this)
                {
                    Path = dlg.FileName
                };
                wnd.ShowDialog();
            }

        }

        #endregion

        #region ExportGroups

        private void ExportGroups_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var wnd = new GroupsIOWindow(GroupsIOWindow.Job.Export, this);
            wnd.ShowDialog();
        }

        private void ExportGroups_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.lbGroups.Items.Count > 1;
        }

        #endregion

        #endregion


        #endregion


        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region Event Handler

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGroups();
            LoadLastFile();
            lbGroups.SelectedIndex = 0;
            lbGroups.Focus();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            SaveLastFile();
            SaveGroups();
        }
 

        private void lbGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.DataLoaded) return;

            if (lbGroups.SelectedIndex < 1)
            {
                foreach (var item in this.Data)
                {
                    item.Visible = true;
                }
            }
            else
            {
                Group grp = (Group)lbGroups.SelectedItem;

                foreach (var item in this.Data)
                {
                    item.Visible = grp.Members.Contains(item.Name);
                }
            }
        }


        private void lbPreview_GotFocus(object sender, RoutedEventArgs e)
        {
            lbPreview.SelectedIndex = -1;
            lbGroups.Focus();
        }


        private void Info_Click(object sender, RoutedEventArgs e)
        {
            string s = string.Format("{0}{1}{1}Version: {2}{1}{1}Copyright \u00A9 2012 Folker Kinzel",
                App.PROGRAM_NAME, Environment.NewLine, App.PROGRAM_VERSION);

            MessageBox.Show(s, App.PROGRAM_NAME + " - Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        

        private void EndProgram_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion


        #region private

        private void LoadGroups()
        {
            FileInfo fi = new FileInfo(App.PRESETS_PATH);

            if (fi.Exists)
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(GroupsList));

                    using (var fs = fi.OpenRead())
                    {
                        using (var zipStream = new GZipStream(fs, CompressionMode.Decompress))
                        {
                            this.Groups = (GroupsList)serializer.Deserialize(zipStream);
                            this.Groups.ResetChanged(true);
                            return;
                        }
                    }
                }
                catch (Exception)  { }
            }// if (fi.Exists)

            this.Groups = new GroupsList
            {
                new Group(App.STANDARD_PRESET_NAME)
            };
            this.Groups.ResetChanged(); // verhindert, dass eine unnötige Datei erzeugt wird
        }



        private void SaveGroups()
        {
            if (Groups.Changed)
            {
                this.Groups.SortCollection();
                try
                {
                    var serializer = new XmlSerializer(typeof(GroupsList));

                    using (var fs = new FileStream(App.PRESETS_PATH, FileMode.Create))
                    {
                        using (var zipStream = new GZipStream(fs, CompressionMode.Compress))
                        {
                            serializer.Serialize(zipStream, this.Groups);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, "Fehler beim Speichern der Gruppen:" + Environment.NewLine + e.Message,
                            App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

#if DEBUG
            var serializer2 = new XmlSerializer(typeof(GroupsList));
            using (var ms = new MemoryStream())
            {
                serializer2.Serialize(ms, this.Groups);

                ms.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(ms);
                Debug.WriteLine("Groups serialisiert:");
                Debug.WriteLine(sr.ReadToEnd());
            }
#endif
        }



        private void OpenData(string filename, bool vcfDirectory = false)
        {
            try
            {
                if (vcfDirectory)
                    this.LoadVcfDirectory(filename);
                else
                    this.LoadFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Die Daten konnten nicht gelesen werden."
                    + Environment.NewLine + Environment.NewLine + "Fehler: " + ex.Message,
                    App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                this.Data = null;
                OnPropertyChanged("DataLoaded");
                this.LastFile = null;
                return;
            }

            lbGroups_SelectionChanged(null, null);
            OnPropertyChanged("Data");
            OnPropertyChanged("DataLoaded");
        }


        private void SaveLastFile()
        {
            App.Settings.LastFile = this.LastFile;
            App.SaveSettings();
        }


        private void LoadLastFile()
        {
            var filename = App.Settings.LastFile;

            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    if(File.Exists(filename)) // File.Exists() gibt false zurück, wenn es sich um einen Ordner handelt.
                    {
                        this.OpenData(filename);
                    }
                    else if (Directory.Exists(filename))
                    {
                        this.OpenData(filename, true);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }


        private void LoadFile(string filename)
        {
            WabContact[] wabContacts;

            if (Path.GetExtension(filename) == ".csv")
                wabContacts = WabIO.ReadCsv(filename);
            else
                wabContacts = WabIO.ReadVcard(filename);

            InitData(filename, wabContacts);
        }



        private void LoadVcfDirectory(string filename)
        {
            List<WabContact> wabContacts = new List<WabContact>(20);
            bool error = false;

            var files = Directory.GetFiles(filename, "*.vcf");

            foreach (var file in files)
            {
                try
                {
                    var wc = WabIO.ReadVcard(file);
                    wabContacts.AddRange(wc);
                }
                catch (Exception)
                {
                    error = true;
                }
            }

            if (error)
            {
                MessageBox.Show(this,
                    "Es konnten nicht alle Daten gelesen werden!",
                    App.PROGRAM_NAME,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            InitData(filename, wabContacts);
        }


        private void InitData(string filename, IList<WabContact> wabContacts)
        {
            Data = new List<Contact>(wabContacts.Count);

            foreach (WabContact wabContact in wabContacts)
            {
                Contact cont = new Contact(wabContact);
                Data.Add(cont);
            }

            if (Data.Count == 0)
            {
                MessageBox.Show(this,
                    "Unter dem Pfad \""
                    + Path.GetFileName(filename)
                    + "\" wurden keine Daten gefunden.",
                    App.PROGRAM_NAME,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.Data = null;
                this.LastFile = null;
                return;
            }

            Data.Sort((d1, d2) => d1.Name.CompareTo(d2.Name));
            this.LastFile = filename;
        }

        #endregion

        

    }
}

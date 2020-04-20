using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.ComponentModel;
using FK.Serialization;


namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für GroupsIOWindow.xaml
    /// </summary>
    public partial class GroupsIOWindow : Window
    {
        public enum Job { Import, Export }

        public const string BACKUP_FILENAME = "TlGrBak.dat";

        private readonly Job job;
        private readonly MainWindow mainWindow;
        private const string EXPORT_DIRECTORY_NAME = "Telefonliste-Gruppen";
        private const string README_FILENAME = "Liesmich.txt";


        public GroupsIOWindow(Job _job, MainWindow mnWnd)
        {
            this.job = _job;
            this.mainWindow = mnWnd;
            this.Owner = this.mainWindow;
            this.Title = (this.job == Job.Import) ?
                "Auswählen: Gruppen importieren" : "Auswählen: Gruppen exportieren";
            InitializeComponent();
        }


        private Group[] Groups {get; set;}
        

        public string Path { get; set; }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.job == Job.Import)
                this.InitImport();
            else
                this.InitExport();
        }


        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.job == Job.Import)
                this.DoImport();
            else
                this.DoExport();
            
        }

        #region Export

        private void InitExport()
        {
            this.tbHeader.Visibility = Visibility.Collapsed;
            this.Groups = mainWindow.Groups.GetRange(1).ToArray();

            InitlbGroups();
        }

        

        private void DoExport()
        {
            List<Group> groupsToExport = new List<Group>(lbGroups.Items.Count);

            for (int i = 0; i < lbGroups.Items.Count; i++)
            {
                if (((CheckBox)lbGroups.Items[i]).IsChecked == true)
                {
                    groupsToExport.Add(this.Groups[i]);
                }
            }


            if (groupsToExport.Count == 0)
            {
                MessageBox.Show(this, "Es wurden keine Gruppen zum Exportieren ausgewählt.",
                            App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Pfad auswählen, in den die Gruppen exportiert werden.";
            dialog.ShowNewFolderButton = true;
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Path = System.IO.Path.Combine(dialog.SelectedPath, EXPORT_DIRECTORY_NAME);
                if (this.ExportSelectedGroups(groupsToExport.ToArray()))
                {
                    this.DialogResult = true;
                }
            }
            else
            {
                this.DialogResult = false;
            }
        }


        private bool ExportSelectedGroups(Group[] groups)
        {
            var grBak = new GroupsBackup(true);
            grBak.Groups = groups;

            try
            {
                CreateDirectory();
                grBak.Serialize(this.Path);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Die Gruppen konnten nicht exportiert werden."
                    + Environment.NewLine + Environment.NewLine + "Fehler: " + ex.Message,
                    App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return false;
            }

            try { CreateReadme(grBak); }
            catch (Exception) { }

            MessageBox.Show(this, "Die ausgewählten Gruppen wurden exportiert nach:"
                    + Environment.NewLine + this.Path,
                    App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

            return true;
        }

        private void CreateReadme(GroupsBackup grBak)
        {
            using(StreamWriter sw = new System.IO.StreamWriter(System.IO.Path.Combine(this.Path, README_FILENAME)))
            {
                sw.Write("Die Datei \"");
                sw.Write(BACKUP_FILENAME);
                sw.Write("\" enthält exportierte Gruppen aus dem Programm \"");
                sw.Write(App.PROGRAM_NAME);
                sw.Write("\", Version ");
                sw.Write(App.PROGRAM_VERSION);
                sw.Write(". ");
                sw.WriteLine(grBak.GetStatusString());

                sw.Write("BENENNEN SIE DIE DATEI \"");
                sw.Write(BACKUP_FILENAME);
                sw.WriteLine("\" AUF KEINEN FALL UM: SIE WIRD SONST UNBRAUCHBAR!");

                sw.WriteLine();

                sw.Write('\"');
                sw.Write(BACKUP_FILENAME);
                sw.WriteLine("\" enthält folgende Gruppen:");
                sw.WriteLine();

                foreach (var group in grBak.Groups)
                {
                    sw.WriteLine(group.GroupName);
                }
            }
        }


        private void CreateDirectory(int counter = 1)
        {
            if (counter == 1)
            {
                if (Directory.Exists(this.Path))
                {
                    CreateDirectory(++counter);
                }
                else
                {
                    Directory.CreateDirectory(this.Path);
                }
            }
            else
            {
                string path = string.Format("{0} {1}", this.Path, counter);

                if (Directory.Exists(path))
                {
                    CreateDirectory(++counter);
                }
                else
                {
                    Directory.CreateDirectory(path);
                    this.Path = path;
                }
            }
        }

        #endregion


        #region Import

        private void InitImport()
        {
            GroupsBackup grBak;
            try
            {
                 grBak = GroupsBackup.FromFile(this.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Die Gruppen konnten nicht importiert werden."
                    + Environment.NewLine + Environment.NewLine + "Fehler: " + ex.Message,
                    App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                this.DialogResult = false;
                return;
            }

            if (VersionCheck(grBak))
            {
                this.tbHeader.Text =
                    string.Format("{0}{1}Durch das Importieren von Gruppen werden vorhandene Gruppen nicht automatisch gelöscht oder überschrieben.", grBak.GetStatusString(), Environment.NewLine);

                this.Groups = grBak.Groups;

                InitlbGroups();
            }
            else
                this.DialogResult = false;
        }


        private bool VersionCheck(GroupsBackup grBak)
        {
            //if (ApplicationDeployment.IsNetworkDeployed && grBak.ExportedVersion > App.PROGRAM_VERSION)
            //{
            //    MessageBox.Show(this,
            //        "Die Gruppen wurden aus einem Programm exportiert, dessen Version höher ist,"
            //        + Environment.NewLine + 
            //        "als die des derzeit ausgeführten Programms."
            //        + Environment.NewLine +
            //        "Aktualisieren Sie Ihr Programm auf die neuste Version!",
            //        App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            //    return false;
            //}
            return true;
        }


        private void DoImport()
        {
            List<Group> groupsToImport = new List<Group>(lbGroups.Items.Count);

            for (int i = 0; i < lbGroups.Items.Count; i++)
            {
                if (((CheckBox)lbGroups.Items[i]).IsChecked == true)
                {
                    groupsToImport.Add(this.Groups[i]);
                }
            }


            if (groupsToImport.Count == 0)
            {
                MessageBox.Show(this, "Es wurden keine Gruppen zum Importieren ausgewählt.",
                            App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

       
            foreach (var group in groupsToImport)
            {
                if (mainWindow.Groups.Any(g => g.GroupName == group.GroupName))
                {
                    if (MessageBoxResult.Yes == MessageBox.Show(this,
                        $"Es existiert bereits eine Gruppe mit dem Namen \"{group.GroupName}\". Soll die existierende Gruppe durch die importierte ersetzt werden?",
                        App.PROGRAM_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes))
                    {
                        mainWindow.Groups.Remove(mainWindow.Groups.First(g => g.GroupName == group.GroupName));
                    }
                    else
                    {

                        for (int i = 2; true; i++)
                        {
                            string newGroupName = group.GroupName;

                            newGroupName += $" ({i.ToString()})";

                            if (mainWindow.Groups.All(g => g.GroupName != newGroupName))
                            {
                                group.GroupName = newGroupName;
                                break;
                            }

                            if (i == 2000)
                            {
                                throw new Exception("Die Gruppe konnte nicht importiert werden.");
                            }
                        }
                    }
                }

                mainWindow.Groups.Add(group);
            }//foreach

            mainWindow.Groups.SortCollection();
            this.DialogResult = true;
        }

        #endregion


        #region Handler

        private void cbAlleKeine_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lbGroups.Items)
            {
                ((CheckBox)item).IsChecked = true;
            }
        }

        private void cbAlleKeine_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lbGroups.Items)
            {
                ((CheckBox)item).IsChecked = false;
            }
        }

        private void SetValue_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((CheckBox)lbGroups.SelectedItem).IsChecked = true;
        }

        private void UnsetValue_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((CheckBox)lbGroups.SelectedItem).IsChecked = false;
        }

        #endregion


        private void InitlbGroups()
        {
            foreach (var item in this.Groups)
            {
                CheckBox cb = new CheckBox();
                cb.IsChecked = true;
                cb.Content = item.GroupName;
                lbGroups.Items.Add(cb);
            }
        }


        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class GroupsBackup
        {
            public GroupsBackup() { }
            
            public GroupsBackup(bool export)
            {
                if (export)
                {
                    this.ExportedVersion = App.PROGRAM_VERSION;
                    this.User = Environment.UserName;
                    this.ComputerName = Environment.MachineName;
                    this.ExportTime = DateTime.Now;
                }
            }

            public SerializableVersion ExportedVersion { get; set; }

            [XmlAttribute]
            public string User { get; set; }

            [XmlAttribute]
            public string ComputerName { get; set; }

            [XmlAttribute]
            public DateTime ExportTime { get; set; }

            public Group[] Groups { get; set; }


            public string GetStatusString()
            {
                return string.Format("Die Gruppen wurden am {0} um {1} Uhr vom Benutzer {2} auf dem Computer {3} gespeichert.",
                    this.ExportTime.ToShortDateString(), this.ExportTime.ToShortTimeString(), this.User, this.ComputerName);
            }


            public void Serialize(string path)
            {
                var serializer = new XmlSerializer(typeof(GroupsBackup));

                using (var fs = new FileStream(System.IO.Path.Combine(path, GroupsIOWindow.BACKUP_FILENAME), FileMode.Create))
                {
                    using (var zipStream = new GZipStream(fs, CompressionMode.Compress))
                    {
                        serializer.Serialize(zipStream, this);
                    }
                }


#if DEBUG
                using (var ms = new MemoryStream())
                {
                    serializer.Serialize(ms, this);

                    ms.Seek(0, SeekOrigin.Begin);
                    var sr = new StreamReader(ms);
                    Debug.WriteLine("GroupsBackup serialisiert:");
                    Debug.WriteLine(sr.ReadToEnd());
                }
#endif
            }

            public static GroupsBackup FromFile(string path)
            {
                var serializer = new XmlSerializer(typeof(GroupsBackup));

                using (var fs = File.OpenRead(path))
                {
                    using (var zipStream = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        return (GroupsBackup)serializer.Deserialize(zipStream);
                    }
                }
            }


        }//GroupsBackup
    }//GroupsIOWindow
}//ns

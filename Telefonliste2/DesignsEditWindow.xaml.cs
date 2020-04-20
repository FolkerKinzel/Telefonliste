using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für DesignsEditWindow.xaml
    /// </summary>
    public partial class DesignsEditWindow : Window
    {
        private MainWindow mainWindow;

        public DesignsEditWindow(MainWindow mWin)
        {
            mainWindow = mWin;
            this.Owner = mainWindow;
            this.Designs = Utility.LoadDesigns(1);
            InitializeComponent();
            this.DataContext = this.Designs;
        }

        public DesignsList Designs {get; private set;}
        


        private void ImportDesign()
        {
            Keyboard.Focus(btnClose);
            string filename = GetFileName();

            if (filename == null) return;

            string name = System.IO.Path.GetFileNameWithoutExtension(filename);
            name = name.Trim();

            if (Designs.Contains(name))
            {
                if (name == App.STANDARD_DESIGN_NAME)
                {
                    MessageBox.Show(this,
                        string.Format("Der Name \"{0}\" ist für benutzerdefinierte Designs nicht erlaubt.", App.STANDARD_DESIGN_NAME), 
                        "Telefonliste - Design importieren",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);

                    return;
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(this,
                        "Durch das Importieren des Designs \"" + name + "\" wird das gleichnamige" + Environment.NewLine +
                        "Design in allen Gruppen, die es verwenden, ersetzt." + Environment.NewLine +
                        "Wollen Sie den Vorgang wirklich fortsetzen?", "Telefonliste - Design importieren",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                    if (result != MessageBoxResult.Yes) return;
                }
            }

            try
            {
                if(VerifyFile(filename))
                    ImportOdtFile(filename, name);
                else
                    MessageBox.Show(this, "Das Design entspricht nicht den Anforderungen des Programms.",
                        "Telefonliste - Design importieren", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Das Design konnte nicht importiert werden." + Environment.NewLine +
                    "Fehler: " + ex.Message,
                        "Telefonliste - Design importieren", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            lbDesigns.SelectedIndex = lbDesigns.Items.Count - 1;
            lbDesigns.ScrollIntoView(lbDesigns.SelectedItem);
        }


        private string GetFileName()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".odt"; // Default file extension
            dlg.Filter = "odt-Dateien (.odt)|*.odt"; // Filter files by extension
            dlg.CheckFileExists = true;
            dlg.DereferenceLinks = true;
            dlg.Multiselect = false;
            dlg.Title = "Telefonliste - Design importieren";

            //Show open file dialog box
            bool? result = dlg.ShowDialog(this);

            //Das Arbeitsverzeichnis ändert sich durch das Laden einer Datei und muss zurückgesetzt werden
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            //Process open file dialog box results
            if (result == true)
                return dlg.FileName;
            else
                return null;
        }


        private bool VerifyFile(string filename)
        {
            Utility.CreateTmpDirectory();

            //content.xml aus Design extrahieren
            (new FastZip()).ExtractZip(filename, App.TMP_PATH, App.OO_NAME_CONTENT);

            //Wurzelelement laden
            XElement content = XElement.Load(App.CONTENT_TMP_PATH);

            //1. Tabellenzeile finden und speichern sowie ihr Elternelement (die Tabelle)
            XElement row = content.Element(App.OO_NS_OFFICE + "body")
                .Element(App.OO_NS_OFFICE + "text")
                .Element(App.OO_NS_TABLE + "table")
                .Element(App.OO_NS_TABLE + "table-row");

            if (row == null) return false;

            XElement tabelle = row.Parent;

            //1. Textabsatz in der Namensspalte finden und speichern sowie das Elternelement, die Tabellenzelle
            XElement absName = row.Element(App.OO_NS_TABLE + "table-cell").Element(App.OO_NS_TEXT + "p");

            if (absName == null) return false;

            XElement cellName = absName.Parent;

            //alle Textabsätze aus cellName löschen
            cellName.Elements().Remove();

            //1. Textabsatz in der Nummernspalte finden und speichern sowie das Elternelement, die Tabellenzelle
            XElement cellNummer = (XElement)row.Element(App.OO_NS_TABLE + "table-cell").NextNode;

            if (cellNummer == null) return false;

            XElement absNummer = cellNummer.Element(App.OO_NS_TEXT + "p");

            if (absNummer == null) return false;

            //alle Textabsätze aus cellNummer löschen
            cellNummer.Elements().Remove();

            //alle Tabellenzellen aus row löschen
            row.Elements().Remove();

            //alle Tabellenzeilen aus Tabelle löschen
            var tabZeilen = content.Element(App.OO_NS_OFFICE + "body")
                .Element(App.OO_NS_OFFICE + "text")
                .Element(App.OO_NS_TABLE + "table")
                .Elements(App.OO_NS_TABLE + "table-row");

            tabZeilen.Remove();
            tabZeilen = null;

            XElement tempRow = new XElement(row);

            XElement tempNameCell = new XElement(cellName);

            XElement tempNumCell = new XElement(cellNummer);

            XElement tempAbsName = new XElement(absName);

            tempAbsName.Value = "Name";
            tempNameCell.Add(tempAbsName);
            tempRow.Add(tempNameCell);

            XElement tempAbsNum = new XElement(absNummer);
            tempAbsNum.Value = "Telefonnummer";
            tempNumCell.Add(tempAbsNum);
            tempRow.Add(tempNumCell);
            tabelle.Add(tempRow);

            //Änderungen in content.xml speichern
            content.Save(App.CONTENT_TMP_PATH);

            return true;
        }

        private void ImportOdtFile(string filename, string name)
        {
            string destName = Utility.CreateDesignPath(name);

            Directory.CreateDirectory(App.DESIGN_PATH);

            File.Copy(filename, destName, true);

            //die veränderte content.xml - Datei zur dsgn-Datei hinzufügen
            ZipFile z = new ZipFile(destName);
            z.BeginUpdate();
            z.Add(new StaticDiskDataSource(App.CONTENT_TMP_PATH), App.OO_NAME_CONTENT, CompressionMethod.Stored);
            z.CommitUpdate();
            z.Close();

            if (!Designs.Contains(name))
                Designs.Add(name);
        }

        

        //public static string BuildFileName(string name)
        //{
        //    return string.Format(@"{0}\{1}.dsgn", App.DESIGN_PATH, name);
        //}

        void DeleteDesign()
        {
            Keyboard.Focus(btnClose);
            string name = lbDesigns.SelectedItem.ToString();

            MessageBoxResult result = MessageBox.Show(this,
                "Durch das Löschen des Designs \"" + name + "\" werden alle Gruppen," + Environment.NewLine +
                    "die es verwenden, auf das Design \"Standard\" zurückgesetzt." + Environment.NewLine +
                    "Wollen Sie den Vorgang wirklich fortsetzen?", "Telefonliste - Design löschen",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

            if (result != MessageBoxResult.Yes)
            {
                //Die folgende Maßnahme ist nötig, um dem Button den Focus zu klauen:
                --lbDesigns.SelectedIndex;
                ++lbDesigns.SelectedIndex;
                return;
            }

            //Das Arbeitsverzeichnis ändert sich durch das Laden einer Datei und muss zurückgesetzt werden
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string filename = Utility.CreateDesignPath(name);

            try
            {
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Das Design konnte nicht gelöscht werden." + Environment.NewLine +
                    "Fehler: " + ex.Message,
                        "Telefonliste - Design löschen", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            
            lbDesigns.SelectedIndex = lbDesigns.SelectedIndex - 1;
            lbDesigns.ScrollIntoView(lbDesigns.SelectedItem);

            Designs.Remove(name);

            foreach (var group in mainWindow.Groups)
            {
                if (group.Design == name)
                    group.Design = "Standard";
            }
        }


        //void RenameDesign()
        //{
        //    string name = lbDesigns.SelectedItem.ToString();

        //    new RenameDesignWindow(mainWindow, name, this).ShowDialog();
        //}


        void ExportDesign()
        {
            string name = lbDesigns.SelectedItem.ToString();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.OverwritePrompt = true;
            dlg.ValidateNames = true;
            dlg.FileName = name;
            dlg.Title = "Telefonliste - Design exportieren";
            dlg.Filter = "odt-Dateien (.odt)|*.odt"; // Filter files by extension
            dlg.DefaultExt = ".odt";
            dlg.AddExtension = true;

            bool? result = dlg.ShowDialog(this);

            Keyboard.Focus(btnClose);

            ////Das Arbeitsverzeichnis ändert sich durch das Laden einer Datei und muss zurückgesetzt werden
            //Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (result == true)
            {
                try
                {
                    if (name == App.STANDARD_DESIGN_NAME)
                        Utility.ExportStandardDesign(dlg.FileName);
                    else
                        File.Copy(Utility.CreateDesignPath(name), dlg.FileName, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Das Design konnte nicht exportiert werden." + Environment.NewLine +
                    "Fehler: " + ex.Message,
                        "Telefonliste - Design exportieren", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
        }


        


        private void ImportDesign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ImportDesign();
        }


        private void ExportDesign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExportDesign();
        }

        private void ExportDesign_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lbDesigns.SelectedIndex >= 0) e.CanExecute = true;
        }

        private void RenameDesign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string name = lbDesigns.SelectedItem.ToString();
            new RenameDesignWindow(this, name, mainWindow.Groups).ShowDialog(this);
            Keyboard.Focus(btnClose);
        }

        private void RenameDesign_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lbDesigns.SelectedIndex >= 1) e.CanExecute = true;
        }

        private void DeleteDesign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DeleteDesign();
        }

        private void DeleteDesign_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lbDesigns.SelectedIndex >= 1) e.CanExecute = true;
        }
    }
}

using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FK.Telefonliste
{
    static class PhoneListCommands
    {
        private static readonly RoutedUICommand generatePhoneList;
        private static readonly RoutedUICommand editGroup;
        private static readonly RoutedUICommand newGroup;
        private static readonly RoutedUICommand deleteGroup;
        private static readonly RoutedUICommand renameGroup;
        private static readonly RoutedUICommand editDesigns;
        private static readonly RoutedUICommand importDesign;
        private static readonly RoutedUICommand deleteDesign;
        private static readonly RoutedUICommand renameDesign;
        private static readonly RoutedUICommand exportDesign;
        private static readonly RoutedUICommand setValue;
        private static readonly RoutedUICommand unsetValue;
        private static readonly RoutedUICommand createMail;
        private static readonly RoutedUICommand generateAgeList;
        private static readonly RoutedUICommand importVcfDirectory;
        private static readonly RoutedUICommand importGroups;
        private static readonly RoutedUICommand exportGroups;
        


        static PhoneListCommands()
        {
            generatePhoneList = new RoutedUICommand("Telefonliste erstellen", "GeneratePhoneList", typeof(PhoneListCommands));
            generatePhoneList.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));

            editGroup = new RoutedUICommand("Gruppe bearbeiten", "EditGroup", typeof(PhoneListCommands));

            newGroup = new RoutedUICommand("Neue Gruppe", "NewGroup", typeof(PhoneListCommands));
            newGroup.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Alt));

            deleteGroup = new RoutedUICommand("Gruppe löschen", "DeleteGroup", typeof(PhoneListCommands));

            renameGroup = new RoutedUICommand("Gruppe umbenennen", "RenameGroup", typeof(PhoneListCommands));

            editDesigns = new RoutedUICommand("Designs verwalten", "EditDesigns", typeof(PhoneListCommands));

            importDesign = new RoutedUICommand("Design importieren", "ImportDesign", typeof(PhoneListCommands));

            importVcfDirectory = new RoutedUICommand("Ordner mit vcf-Daten importieren", "ImportVcfDirectory", typeof(PhoneListCommands));

            deleteDesign = new RoutedUICommand("Design löschen", "DeleteDesign", typeof(PhoneListCommands));

            renameDesign = new RoutedUICommand("Design umbenennen", "RenameDesign", typeof(PhoneListCommands));

            exportDesign = new RoutedUICommand("Design exportieren", "ExportDesign", typeof(PhoneListCommands));

            setValue = new RoutedUICommand("Häkchen setzen", "SetValue", typeof(PhoneListCommands));
            setValue.InputGestures.Add(new KeyGesture(Key.Right));
            setValue.InputGestures.Add(new KeyGesture(Key.OemPlus));

            unsetValue = new RoutedUICommand("Häkchen entfernen", "UnsetValue", typeof(PhoneListCommands));
            unsetValue.InputGestures.Add(new KeyGesture(Key.Left));
            unsetValue.InputGestures.Add(new KeyGesture(Key.OemMinus));

            createMail = new RoutedUICommand("E-Mail erstellen", "CreateMail", typeof(PhoneListCommands));
            createMail.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));

            generateAgeList = new RoutedUICommand("Altersliste erstellen", "GenerateAgeList", typeof(PhoneListCommands));
            generateAgeList.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));

            importGroups = new RoutedUICommand("Gruppen importieren", "ImportGroups", typeof(PhoneListCommands));

            exportGroups = new RoutedUICommand("Gruppen exportieren", "ExportGroups", typeof(PhoneListCommands));
        }


        public static RoutedUICommand GeneratePhoneList
        { 
            get { return generatePhoneList; }
        }

        public static RoutedUICommand EditGroup
        {
            get { return editGroup; }
        }

        public static RoutedUICommand NewGroup
        {
            get { return newGroup; }
        }

        public static RoutedUICommand DeleteGroup
        {
            get { return deleteGroup; }
        }

        public static RoutedUICommand RenameGroup
        {
            get { return renameGroup; }
        }

        public static RoutedUICommand EditDesigns
        {
            get { return editDesigns; }
        }

        public static RoutedUICommand ImportDesign
        {
            get { return importDesign; }
        }

        public static RoutedUICommand DeleteDesign
        {
            get { return deleteDesign; }
        }

        public static RoutedUICommand RenameDesign
        {
            get { return renameDesign; }
        }

        public static RoutedUICommand ExportDesign
        {
            get { return exportDesign; }
        }

        public static RoutedUICommand SetValue
        {
            get { return setValue; }
        }

        public static RoutedUICommand UnsetValue
        {
            get { return unsetValue; }
        }

        public static RoutedUICommand CreateMail
        {
            get { return createMail; }
        }

        public static RoutedUICommand GenerateAgeList
        {
            get { return generateAgeList; }
        }

        public static RoutedUICommand ImportVcfDirectory
        {
            get { return importVcfDirectory; }
        }

        public static RoutedUICommand ImportGroups
        {
            get { return importGroups; }
        }

        public static RoutedUICommand ExportGroups
        {
            get { return exportGroups; }
        }
    }
}

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using FK.Serialization;
using System.Xml.Serialization;

namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        #region private Konstanten

        /// <summary>
        /// Name des Entwicklers
        /// </summary>
        private const string DEVELOPER_NAME = "Folker_Kinzel";


        /// <summary>
        /// Name des tmp-Ordners.
        /// </summary>
        private const string TMP_NAME = "Telefonliste";

        /// <summary>
        /// Name der Gruppendatei.
        /// </summary>
        private const string PRESET_NAME = "TlGr.dat";

        /// <summary>
        /// Name der Datei für die Programmeinstellungen
        /// </summary>
        private const string APP_SETTINGS_FILE_NAME = "AppSettings.xml";

        /// <summary>
        /// Name des Ordners für benutzerdefinierte Designs.
        /// </summary>
        private const string DESIGNS_DIRECTORY_NAME = "Designs";

        /// <summary>
        /// Relativer Pfad der Hilfe-Datei.
        /// </summary>
        private const string HELP_NAME = @"Resources\help.chm";


        /// <summary>
        /// Absoluter Pfad für programmbezogene Daten des aktuellen Benutzers.
        /// </summary>
        private static readonly string APPDATA_PATH;


        /// <summary>
        /// Absoluter Pfad für die Datei mit den Programmeinstellungen
        /// </summary>
        private static readonly string SETTINGS_PATH;

        #endregion

        /// <summary>
        /// Name des Standarddesigns.
        /// </summary>
        public const string STANDARD_DESIGN_NAME = "Standard";


        /// <summary>
        /// Name der Standardvorlage.
        /// </summary>
        public const string STANDARD_PRESET_NAME = "Alle";
        

        /// <summary>
        /// Absoluter Pfad des tmp-Orders für dieses Programm.
        /// </summary>
        public static readonly string TMP_PATH;
    

        /// <summary>
        /// Absoluter Pfad der Gruppendatei.
        /// </summary>
        public static readonly string PRESETS_PATH;


        /// <summary>
        /// Absoluter Pfad des Ordners für benutzerdefinierte Designs.
        /// </summary>
        public static readonly string DESIGN_PATH;


        /// <summary>
        /// Absoluter Pfad der Hilfedatei.
        /// </summary>
        public static readonly string HELP_PATH;

        public static readonly string PROGRAM_NAME;

        public static readonly SerializableVersion PROGRAM_VERSION;


        // Xml-Namespaces:
        public static readonly XNamespace OO_NS_OFFICE = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        public static readonly XNamespace OO_NS_TEXT = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
        public static readonly XNamespace OO_NS_TABLE = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";

        public const string OO_NAME_CONTENT = "content.xml";
        public const string OO_NAME_STYLES = "styles.xml";
        public static readonly string CONTENT_TMP_PATH;
        public static readonly string STYLES_TMP_PATH;

        public static readonly MyAppSettings Settings;
        private static readonly XmlSerializer SettingsSerializer = new XmlSerializer(typeof(MyAppSettings));

        static App()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            APPDATA_PATH = Path.GetDirectoryName(assembly.Location);
            SETTINGS_PATH = Path.Combine(APPDATA_PATH, APP_SETTINGS_FILE_NAME);
            HELP_PATH = Path.Combine(APPDATA_PATH, HELP_NAME);
            TMP_PATH = Path.Combine(Path.GetTempPath(), DEVELOPER_NAME, TMP_NAME);
            CONTENT_TMP_PATH = Path.Combine(TMP_PATH, OO_NAME_CONTENT);
            STYLES_TMP_PATH = Path.Combine(TMP_PATH, OO_NAME_STYLES);

            AssemblyName assemblyName = assembly.GetName();
            PROGRAM_NAME = assemblyName.Name;
            PROGRAM_VERSION = new SerializableVersion(assemblyName.Version);


            //if (ApplicationDeployment.IsNetworkDeployed)
            //{
            //    ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
            //    APPDATA_PATH = deployment.DataDirectory;
            //    PROGRAM_VERSION = new SerializableVersion(deployment.CurrentVersion);
            //}
            //else
            //{
            //    APPDATA_PATH = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            //    APPDATA_PATH = Path.GetDirectoryName(APPDATA_PATH);
            //    APPDATA_PATH = Path.GetDirectoryName(APPDATA_PATH);


            //}

            
            DESIGN_PATH = Path.Combine(APPDATA_PATH, DESIGNS_DIRECTORY_NAME);
            PRESETS_PATH = Path.Combine(APPDATA_PATH, PRESET_NAME);

            Settings = LoadSettings();
        }


        private static MyAppSettings LoadSettings()
        {
            FileInfo fi = new FileInfo(SETTINGS_PATH);

            if (fi.Exists)
            {
                try
                {
                    using (var fs = fi.OpenRead())
                    {
                        return (MyAppSettings)SettingsSerializer.Deserialize(fs);
                    }
                }
                catch
                {

                }
            }

            return new MyAppSettings();
        }


        public static void SaveSettings()
        {
            try
            {
                using (var fs = new FileStream(SETTINGS_PATH, FileMode.Create))
                {
                    SettingsSerializer.Serialize(fs, Settings);
                }
            }
            catch
            {

            }
        }

        //public static string CompanyName
        //{
        //    get
        //    {
        //        AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)AssemblyCompanyAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute));

        //        return company.Company;
        //    }
        //}


        //public static string ProductName
        //{
        //    get
        //    {
        //        AssemblyProductAttribute product = (AssemblyProductAttribute)AssemblyProductAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));

        //        return product.Product;
        //    }
        //}



        protected override void OnStartup(StartupEventArgs e)
        {
            CleanTmpFiles();

            //Programm durch Rechtsklick auf csv-Datei und "Öffnen mit" starten
            if (e.Args.Length > 0)
            {
                Settings.LastFile = e.Args[0];
            } 
            // GUI starten
            base.OnStartup(e);
        }


        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Ein unbekannter Fehler ist aufgetreten. " + Environment.NewLine
                + "Das Programm muss beendet werden." + Environment.NewLine + Environment.NewLine +
            "Fehler: " + e.Exception.Message, App.PROGRAM_NAME,
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

            e.Handled = true;

            this.Shutdown();
        }



        private void CleanTmpFiles()
        {
            DirectoryInfo di = new DirectoryInfo(TMP_PATH);
            try
            {
                di.Delete(true);
            }
            catch (Exception) { }
        }
    }
}

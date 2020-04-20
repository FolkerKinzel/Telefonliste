using System;
using System.IO;
using System.Windows;
using System.Linq;

namespace FK.Telefonliste
{
    static class Utility
    {
        /// <summary>
        /// Erzeugt einen temporären Dateinamen mit der angegebenen Dateiendung und stellt sicher, dass das tmp-verzeichnis existiert.
        /// </summary>
        /// <param name="fileTypeExtension">Die Dateiendung.</param>
        /// <returns>Der tmp-File-Name (absoluter Pfad).</returns>
        public static string CreateTmpFilePath(string fileTypeExtension)
        {
            CreateTmpDirectory();
            DateTime dt = DateTime.Now;
            return Path.Combine(App.TMP_PATH, string.Format(@"{0}{1}{2}{3}", dt.Hour.ToString("00"), dt.Minute.ToString("00"), dt.Second.ToString("00"), fileTypeExtension));
        }


        public static string CreateDesignPath(string designName)
        {
            return Path.Combine(App.DESIGN_PATH, string.Format("{0}.dsgn", designName));
        }


        /// <summary>
        /// Stellt sicher, dass das tmp-Verzeichnis existiert.
        /// </summary>
        public static void CreateTmpDirectory()
        {
            try
            {
                Directory.CreateDirectory(App.TMP_PATH);
            }
            catch (Exception ex)
            {
                throw new IOException("Es konnte kein Ordner für temporäre Dateien erstellt werden.", ex);
            }
        }


        public static void ExportStandardDesign(string exportFileName)
        {
            System.Windows.Resources.StreamResourceInfo sri =
                Application.GetResourceStream(new Uri(@"Resources\standard.dsgn", UriKind.Relative));
            byte[] b;
            using (BinaryReader br = new BinaryReader(sri.Stream))
            {
                b = br.ReadBytes((int)sri.Stream.Length);
            }

            File.WriteAllBytes(exportFileName, b);
        }


        public static DesignsList LoadDesigns(int addCapacity = 0)
        {
            if (Directory.Exists(App.DESIGN_PATH))
            {
                var files = Directory.GetFiles(App.DESIGN_PATH, "*.dsgn");
                Array.Sort(files);
                var designs = new DesignsList(files.Length + addCapacity);
                foreach (var file in files)
                {
                    designs.Add(Path.GetFileNameWithoutExtension(file));
                }
                return designs;
            }
            else
                return new DesignsList(addCapacity);
        }
    }
}

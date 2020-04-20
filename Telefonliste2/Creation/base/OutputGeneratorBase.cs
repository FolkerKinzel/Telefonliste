using System;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using FK.ContactData2;
using FK.ContactIO;
using ICSharpCode.SharpZipLib.Zip;

namespace FK.Telefonliste
{
    class OutputGeneratorBase
    {
        protected const string HEADER_TM = "$HEADER$";
        protected const string DATE_TM = "$DATE$";
        protected readonly string PRESET;
        protected readonly string JOB;
        protected readonly IEnumerable<DataRowBase> Data;


        public OutputGeneratorBase(IEnumerable<DataRowBase> data, string presetName, string jobName)
        {
            this.PRESET = presetName;
            this.JOB = jobName;
            this.Data = data;
        }


        public void GenerateOutput()
        {
            var tmpFileName = this.CreateTempFile();
            ManageXml();
            SaveAndFinish(tmpFileName);
        }


        protected virtual string CreateTempFile()
        {
            throw new NotImplementedException();
        }


        protected virtual void ManageXml()
        {
            throw new NotImplementedException();
        }


        private void SaveAndFinish(string tmpFileName)
        {
            //die veränderte content.xml - Datei zur odt-Datei hinzufügen
            using (ZipFile zipFile = new ZipFile(tmpFileName))
            {
                zipFile.BeginUpdate();
                zipFile.Add(new StaticDiskDataSource(App.CONTENT_TMP_PATH), App.OO_NAME_CONTENT, CompressionMethod.Stored);

                string styles = File.ReadAllText(App.STYLES_TMP_PATH);

                if (styles.Contains(HEADER_TM) || styles.Contains(DATE_TM))
                {
                    styles = styles.Replace(HEADER_TM, string.Format("{0} - {1}", PRESET, JOB));
                    styles = styles.Replace(DATE_TM, DateTime.Now.ToShortDateString());

                    File.WriteAllText(App.STYLES_TMP_PATH, styles);

                    //die veränderte styles.xml - Datei zur odt-Datei hinzufügen
                    zipFile.Add(new StaticDiskDataSource(App.STYLES_TMP_PATH), App.OO_NAME_STYLES, CompressionMethod.Stored);
                }

                zipFile.CommitUpdate();
            }

            //OpenOffice starten
            Process.Start(tmpFileName);
        }

    }
}

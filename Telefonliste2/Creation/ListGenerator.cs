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
    class ListGenerator : OutputGeneratorBase
    {  
        private readonly string DESIGN;

        public ListGenerator(IEnumerable<DataRowBase> data, string presetName, string jobName, string designName)
            : base(data, presetName, jobName)
        {
            DESIGN = designName;
        }
                    

        protected override string CreateTempFile()
        {
            string tmpFileName = Utility.CreateTmpFilePath(".odt");

            if (DESIGN == App.STANDARD_DESIGN_NAME)
            {
                Utility.ExportStandardDesign(tmpFileName);
            }
            else
            {
                var fi = new FileInfo(Utility.CreateDesignPath(DESIGN));

                if (fi.Exists)
                    fi.CopyTo(tmpFileName, true);
                else
                    Utility.ExportStandardDesign(tmpFileName);
            }

            //content.xml und styles.xml aus Design extrahieren
            var fastZip = new FastZip();
            fastZip.ExtractZip(tmpFileName, App.TMP_PATH, App.OO_NAME_CONTENT);
            fastZip.ExtractZip(tmpFileName, App.TMP_PATH, App.OO_NAME_STYLES);

            return tmpFileName;
        }
            

        protected override void ManageXml()
        {
            //Wurzelelement laden
            XElement content = XElement.Load(App.CONTENT_TMP_PATH);

            //// Namespaces definieren
            //XNamespace office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
            //XNamespace text =   "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
            //XNamespace table =  "urn:oasis:names:tc:opendocument:xmlns:table:1.0";

            //1. Tabellenzeile finden und speichern sowie ihr Elternelement (die Tabelle)
            XElement row = content.Element(App.OO_NS_OFFICE + "body")
                .Element(App.OO_NS_OFFICE + "text")
                .Element(App.OO_NS_TABLE + "table")
                .Element(App.OO_NS_TABLE + "table-row");

            XElement tabelle = row.Parent;

            //1. Textabsatz in der Namensspalte finden und speichern sowie das Elternelement, die Tabellenzelle
            XElement absName = row.Element(App.OO_NS_TABLE + "table-cell").Element(App.OO_NS_TEXT + "p");
            XElement cellName = absName.Parent;

            //alle Textabsätze aus cellName löschen
            cellName.Elements().Remove();


            //1. Textabsatz in der Nummernspalte finden und speichern sowie das Elternelement, die Tabellenzelle
            XElement cellNummer = (XElement)row.Element(App.OO_NS_TABLE + "table-cell").NextNode;
            XElement absNummer = cellNummer.Element(App.OO_NS_TEXT + "p");

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


            foreach (var dataRow in Data)
            {
                if (!dataRow.Visible) continue;
                if (dataRow.Data.Count == 0) continue;

                XElement tempRow = new XElement(row);

                XElement tempNameCell = new XElement(cellName);

                XElement tempNumCell = new XElement(cellNummer);

                XElement tempAbsName = new XElement(absName);

                tempAbsName.Value = dataRow.Name;
                tempNameCell.Add(tempAbsName);
                tempRow.Add(tempNameCell);

                foreach (var telNum in dataRow.Data)
                {
                    XElement tempAbsNum = new XElement(absNummer);
                    tempAbsNum.Value = telNum;
                    tempNumCell.Add(tempAbsNum);
                }

                tempRow.Add(tempNumCell);
                tabelle.Add(tempRow);
            }

            //Änderungen in content.xml speichern
            content.Save(App.CONTENT_TMP_PATH);
        }


        
    }
    
}

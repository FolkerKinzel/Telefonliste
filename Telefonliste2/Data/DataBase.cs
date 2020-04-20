using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FK.ContactData2;
using FK.ContactIO;

namespace FK.Telefonliste
{
    public static class DataBase
    {
        public static List<Contact> Data { get; set; }

        //public static WabContact[] WabContacts { get; private set; }


        public static void InitData(string filename)
        {
            try
            {
                WabContact[] WabContacts = WabIO.ReadCsv(filename);
            
                Data = new List<Contact>(WabContacts.Length);

                foreach (WabContact wabContact in WabContacts)
                {
                    Contact cont = new Contact(wabContact);
                    Data.Add(cont);
                }

                if (Data.Count == 0)
                {
                    MessageBox.Show(
                        "Die Datei \""
                        + Path.GetFileName(filename)
                        + "\" enthält keine Telefonnummern.",
                        "Telefonliste",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Data = null;
                    return;
                }

                Data.Sort();
            }
            catch (Exception e)
            {
                Data = null;
                throw new Exception(e.Message, e);
            }

        }


    }
}

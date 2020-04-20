using System;
using System.Collections.Generic;
using System.Text;
using FK.ContactData2;
using FK.ContactData2.Enums;

namespace FK.Telefonliste
{
    class TelEntry : DataRowBase, IComparable
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="cont">Kontaktdaten</param>
        public TelEntry(Contact cont) : base(cont)
        {
            InitPhone(cont.ContactData);
        }


        private void InitPhone(WabContact wab)
        {
            if (wab.PhoneNumbers != null)
            {
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.Phone)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.Phone]);
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.MobilePhone)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.MobilePhone]);
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.OtherPhone)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.OtherPhone]);
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.Pager)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.Pager]);
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.PhoneWork)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.PhoneWork] + " (dienstl.)");
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.FaxWork)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.FaxWork] + " (Fax dienstl.)");
                if (wab.PhoneNumbers.ContainsKey(PhoneNumberMapping.Fax)) Data.Add(wab.PhoneNumbers[PhoneNumberMapping.Fax] + " (Fax privat)");
            }
        }

    }
}

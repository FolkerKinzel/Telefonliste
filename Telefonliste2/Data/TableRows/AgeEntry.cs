using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FK.ContactData2;

namespace FK.Telefonliste
{
    class AgeEntry : DataRowBase
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="cont">Kontaktdaten</param>
        public AgeEntry(Contact cont) : base(cont)
        {
            var person = cont.ContactData.Person;
            if (person != null && person.BirthDay.HasValue)
            {
                InitAge(person.BirthDay.Value);
            }
            else
            {
                Age = int.MaxValue;
                Data.Add(string.Empty);
            }
        }

        public int Age { get; private set; }


        private void InitAge(DateTime birthday)
        {
            this.Age = GetYears(birthday);
            Data.Add(string.Format("{0} J. ({1})", Age, birthday.ToShortDateString()));
        }


        private int GetYears(DateTime birthday)
        {
            var now = DateTime.Now;

            int age = now.Year - birthday.Year;
            if (now.Month < birthday.Month || (now.Month == birthday.Month && now.Day < birthday.Day)) age--;
            return age;
        }

    }
}

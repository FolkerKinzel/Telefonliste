using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FK.ContactData2;

namespace FK.Telefonliste
{
    class DataRowBase : IComparable
    {
        private readonly List<string> data = new List<string>(4);
        private bool visible = true;

        public DataRowBase(Contact cont)
        {
            this.Name = cont.Name;
        }


        /// <summary>
        /// Name des Kontakts (Die Property ist nie null.)
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Die dem Kontaktnamen zugeordneten Datenzeilen (Die Property ist nie null.)
        /// </summary>
        public List<string> Data { get { return data; } }

       

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public virtual int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() != GetType())
                throw new ArgumentException("Ungültiger Vergleich.");

            return this.Name.CompareTo(((DataRowBase)obj).Name);
        }
    }
}

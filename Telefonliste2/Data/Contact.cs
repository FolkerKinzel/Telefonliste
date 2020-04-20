using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FK.ContactData2;
using System.ComponentModel;

namespace FK.Telefonliste
{
    public class Contact : IComparable, INotifyPropertyChanged
    {
        private bool visible = true;

        public Contact(WabContact wab)
        {
            this.ContactData = wab;
            InitName(this.ContactData);
        }


        /// <summary>
        /// Name des Kontakts (Die Property ist nie null.)
        /// </summary>
        public string Name { get; protected set; }

        public WabContact ContactData { get; private set; }

        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                OnPropertyChanged("Visible");
            }
        }


        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() != GetType())
                throw new ArgumentException("Ungültiger Vergleich.");

            return this.Name.CompareTo(((Contact)obj).Name);
        }


        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void InitName(WabContact wab)
        {
            StringBuilder name = new StringBuilder();

            if (wab.Work != null)
                name.Append(wab.Work.CompanyName);
            if (name.Length > 0)
            {
                if (wab.Person != null && wab.Person.FamilyName != null)
                {
                    name.AppendFormat(" ({0}", wab.Person.FamilyName);
                    if (wab.Person.FirstName != null)
                        name.AppendFormat(", {0})", wab.Person.FirstName);
                    else
                        name.Append(")");
                }
                Name = name.ToString();
                return;
            }

            if (wab.Person != null)
            {
                name.Append(wab.Person.FamilyName);

                if (name.Length > 0)
                {
                    if (wab.Person.FirstName != null)
                    {
                        name.Append(", ");
                        name.Append(wab.Person.FirstName);
                        if (wab.Person.MiddleName != null)
                        {
                            name.Append(" ");
                            name.Append(wab.Person.MiddleName);
                        }
                    }
                    Name = name.ToString();
                    return;
                }
                else
                {
                    name.Append(wab.Person.NickName);

                    if (name.Length > 0)
                    {
                        Name = name.ToString();
                        return;
                    }
                }


                if (wab.Person.FirstName != null)
                {
                    name.Append(wab.Person.FirstName);
                    if (wab.Person.MiddleName != null)
                    {
                        name.Append(" ");
                        name.Append(wab.Person.MiddleName);
                    }

                    if (name.Length > 0)
                    {
                        Name = name.ToString();
                        return;
                    }
                }
            }//if (wab.Person != null)

            name.Append(wab.PreferredEmailAddress.DisplayName);
            if (name.Length > 0)
            {
                Name = name.ToString();
                return;
            }

            name.Append(wab.ContactName);
            Name = name.ToString();
        }

    }
}

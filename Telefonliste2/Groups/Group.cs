using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FK.DataChangedInterface;
using System.Xml.Serialization;

namespace FK.Telefonliste
{
    [Serializable()]
    public class Group : INotifyPropertyChanged, IDataChanged
    {
        string groupName = GROUPNAME_PLACEHOLDER;
        string design = App.STANDARD_DESIGN_NAME;
        string[] members;

        #region Konstruktoren

        public Group() { }

        public Group(string presetName)
        {
            groupName = presetName;
        }

        //public Group(Group preset)
        //{
        //    groupName = preset.GroupName;
        //    if (preset.Members != null) members = (string[])preset.Members.Clone();
        //    design = preset.Design;
        //}

        #endregion

        #region Properties

        public const string GROUPNAME_PLACEHOLDER = "Benutzerdefiniert";

        [XmlAttribute]
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    groupName = GROUPNAME_PLACEHOLDER;
                else
                    groupName = value.Trim();
                OnPropertyChanged("Name");
                OnDataChanged();
            }
        }

        [XmlAttribute]
        public string Design
        {
            get
            {
                return design;
            }
            set
            {
                design = value;
                //OnPropertyChanged("Design");
                OnDataChanged();
            }
        }


        [XmlArray(IsNullable=true)]
        [XmlArrayItem("Member")]
        public string[] Members
        {
            get
            {
                return members;
            }
            set
            {
                members = value;
                //OnPropertyChanged("NameList");
                OnDataChanged();
            }
        }

        #endregion

        # region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        #region DataChanged

        [XmlIgnore]
        public bool Changed
        {
            get;
            private set;
        }

        public event EventHandler DataChanged;

        public void OnDataChanged()
        {
            if (Changed) return;

            Changed = true;

            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);
        }

        public void ResetChanged(bool resetSubObjects = false)
        {
            Changed = false;
        }

        #endregion
    }
}

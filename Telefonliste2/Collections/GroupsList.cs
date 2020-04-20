using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FK.Collections;

namespace FK.Telefonliste
{
    [Serializable()]
    public class GroupsList : FK.Collections.DCRefTypeObservable<Group>
    {
        public GroupsList()
            : this(2)
        {

        }

        public GroupsList(int capacity) : base(capacity) { this.NotifyElementChanges = false; }

        //public GroupsList(GroupsList coll)
        //    : base(coll.Count)
        //{
        //    this.NotifyElementChanges = false;
        //    foreach (var preset in coll)
        //    {
        //        this.Add(new Group(preset));
        //    }
        //}

        public void SortCollection()
        {
            Group pset = this[0];

            this.Sort((p1, p2) => p1.GroupName.CompareTo(p2.GroupName));

            this.Move(IndexOf(pset), 0);
        }
    }
}

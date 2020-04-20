using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;

namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für GroupEditWindow.xaml
    /// </summary>
    public partial class GroupEditWindow : Window
    {

        public enum GroupAction { Edit,  New }


        public GroupEditWindow(MainWindow mWindow, GroupAction pJob, string title)
        {
            this.Job = pJob;
            this.MnWindow = mWindow;
            this.Owner = MnWindow;
            this.Designs = Utility.LoadDesigns();
            if (Job == GroupAction.Edit)
                CurrentGroup = (Group)MnWindow.lbGroups.SelectedItem;
            else
                CurrentGroup = new Group();
            if(!this.Designs.Contains(this.CurrentGroup.Design))
                this.CurrentGroup.Design = App.STANDARD_DESIGN_NAME;
            this.Title = title;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializelbNames();
            lbDesigns.Focus();
        }

        #region Properties

        public DesignsList Designs { get; private set; }

        public Group CurrentGroup { get; set; }

        MainWindow MnWindow { get; set; }

        GroupAction Job { get; set; }

        #endregion

        //private void InitializetbName()
        //{
        //    tbName.Text = CurrentGroup.Name;
        //}

        //void Initialize_lbDesigns()
        //{
        //    lbDesigns.SelectedItem = CurrentGroup.Design;
        //}
   

        private void InitializelbNames() 
        {
            if (CurrentGroup.Members != null)
            {
                foreach (var item in CurrentGroup.Members)
                {
                    AddName(name:item, isChecked: true);
                }
            }

            if (this.MnWindow.Data != null)
            {
                foreach (var item in this.MnWindow.Data)
                {
                    if (!item.Visible)
                    {
                        AddName(name: item.Name, isChecked: false);
                    }
                }
                lbNames.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
            }
            lbNames.SelectedIndex = -1;
        }


        private void AddName(string name, bool isChecked)
        {
            if (this.Job == GroupAction.New) isChecked = true;
      
            var cb = new CheckBox();
            cb.IsChecked = isChecked;
            cb.Content = name;
            cb.IsTabStop = false;
            //cb.Click += new RoutedEventHandler(cb_Click);
            lbNames.Items.Add(cb);
        }



    
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var namelist =
                new List<string>(lbNames.Items.Count);

            foreach (var item in lbNames.Items)
            {
                var cb = (CheckBox)item;
                if (cb.IsChecked == true)
                    namelist.Add(cb.Content.ToString());
            }
            this.CurrentGroup.Members = namelist.ToArray();
            this.CurrentGroup.GroupName = tbName.Text;

            this.CurrentGroup.Design = lbDesigns.SelectedItem.ToString();
      

            if (Job == GroupAction.New)
            {
                MnWindow.Groups.Add(this.CurrentGroup);
                MnWindow.lbGroups.SelectedIndex = MnWindow.lbGroups.Items.Count - 1;
                MnWindow.lbGroups.ScrollIntoView(MnWindow.lbGroups.SelectedItem);
            }

            DialogResult = true;
        }

        private void cbAlleKeine_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lbNames.Items)
            {
                ((CheckBox)item).IsChecked = true;
            }
        }

        private void cbAlleKeine_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lbNames.Items)
            {
                ((CheckBox)item).IsChecked = false;
            }
        }

        private void SetValue_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((CheckBox)lbNames.SelectedItem).IsChecked = true;
        }

        private void UnsetValue_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((CheckBox)lbNames.SelectedItem).IsChecked = false;
        }

        

        
    }
}

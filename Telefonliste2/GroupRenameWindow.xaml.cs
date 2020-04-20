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

namespace FK.Telefonliste
{
    /// <summary>
    /// Interaktionslogik für GroupRenameWindow.xaml
    /// </summary>
    public partial class GroupRenameWindow : Window
    {
        private MainWindow mainWindow { get; set; }

        public GroupRenameWindow(MainWindow mainWin)
        {
            mainWindow = mainWin;
            InitializeComponent();
            Group pset = (Group)mainWindow.lbGroups.SelectedItem;
            tbName.Text = pset.GroupName;
        }

        public void ShowDialog(Window owner)
        {
            this.Owner = owner;
            this.ShowDialog();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Group pset = (Group)mainWindow.lbGroups.SelectedItem;
            pset.GroupName = tbName.Text;
            DialogResult = true;
        }
    }
}

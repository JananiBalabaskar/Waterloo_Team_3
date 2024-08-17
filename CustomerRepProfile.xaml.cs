﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Digident_Group3
{
    /// <summary>
    /// Interaction logic for CustomerRepProfile.xaml
    /// </summary>
    public partial class CustomerRepProfile : Page
    {
        public CustomerRepProfile(int userID)
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ChangePage(new CustomerRepDash());
            }
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

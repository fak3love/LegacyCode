using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UGL.Model;

namespace UGL.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : Border
    {
        public SettingsPanel()
        {
            InitializeComponent();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var userContainer = FindResource("userListContainer") as SettingsContainer;
            userContainer.Source = listBoxUser;
            userContainer.OldSource = listBoxGames;

            var gamesContainer = FindResource("gamesListContainer") as SettingsContainer;
            gamesContainer.Source = listBoxGames;
            gamesContainer.OldSource = listBoxUser;
        }
    }
}

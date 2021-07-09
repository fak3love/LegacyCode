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
using UGL.View.Controls;
using UGL.ViewModel.FriendList_;

namespace UGL.View.FriendList
{
    /// <summary>
    /// Логика взаимодействия для UserItem.xaml
    /// </summary>
    public partial class UserItem : Border
    {
        public UserItem()
        {
            InitializeComponent();
        }

        public UserItem(UserItemViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

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
using UGL.ViewModel.FriendList_;

namespace UGL.View.Controls.FriendList
{
    /// <summary>
    /// Логика взаимодействия для FriendItem.xaml
    /// </summary>
    public partial class FriendItem : Border
    {
        public FriendItem()
        {
            InitializeComponent();
        }
        
        public FriendItem(FriendItemViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

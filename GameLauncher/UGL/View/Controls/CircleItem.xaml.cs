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
using UGL.ViewModel;
using UGL.ViewModel.CircleItem_;

namespace UGL.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для CircleItem.xaml
    /// </summary>
    public partial class CircleItem : Border
    {
        public CircleItem()
        {
            InitializeComponent();
        }

        public CircleItem(CircleServerItemViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

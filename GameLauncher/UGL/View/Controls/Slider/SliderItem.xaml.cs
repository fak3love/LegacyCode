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
using UGL.ViewModel.Slider_;

namespace UGL.View.Controls.Slider
{
    /// <summary>
    /// Логика взаимодействия для SliderItem.xaml
    /// </summary>
    public partial class SliderItem : Border
    {
        public string ServerNameText
        {
            get => serverName.Text;
            set => serverName.Text = value;
        }
        public double ServerNameTextSize
        {
            get => serverName.FontSize;
            set => serverName.FontSize = value;
        }
        public double CircleItemSize
        {
            get => circleItem.Width;
            set => (circleItem.Width, circleItem.Height) = (value, value);
        }
        public bool IsVisibleServerState
        {
            get => serverState.Visibility == Visibility.Visible;
            set => serverState.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public SliderItem()
        {
            InitializeComponent();
        }

        public SliderItem(SliderItemViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

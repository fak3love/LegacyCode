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
    /// Логика взаимодействия для Slider.xaml
    /// </summary>
    public partial class SliderBox : Border
    {
        public SliderBox()
        {
            InitializeComponent();
        }

        public SliderBox(SliderBoxViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

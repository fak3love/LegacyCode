using System;
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
using UGL.View.Controls;
using UGL.View.Controls.FriendList;
using UGL.View.Controls.History;
using UGL.View.FriendList;
using UGL.ViewModel;
using UGL.ViewModel.Slider_;

namespace UGL.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Initializer.Initialize();
            //
            sliderControlBox.SliderBox = sliderBox;
            (sliderBox.DataContext as SliderBoxViewModel).SliderItemSelectedServer = sliderItemSelectedServer;

            if ((DataContext as MainWindowViewModel).SliderBoxViewModel.SliderItemViewModels.Count > 0)
                sliderItemSelectedServer.DataContext = (DataContext as MainWindowViewModel).SliderBoxViewModel.SliderItemViewModels[0];
            else
                sliderItemSelectedServer.DataContext = new SliderItemViewModel();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}

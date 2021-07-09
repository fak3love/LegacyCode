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
using UGL.ViewModel.History_;

namespace UGL.View.Controls.History
{
    /// <summary>
    /// Логика взаимодействия для HistoryElement.xaml
    /// </summary>
    public partial class HistoryElement : Border
    {
        public HistoryElement()
        {
            InitializeComponent();
        }

        public HistoryElement(HistoryElementViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

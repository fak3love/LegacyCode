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
    /// Логика взаимодействия для HistoryPreviewBox.xaml
    /// </summary>
    public partial class HistoryPreviewBox : Border
    {
        public HistoryPreviewBox()
        {
            InitializeComponent();
        }

        public HistoryPreviewBox(HistoryPreviewBoxViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

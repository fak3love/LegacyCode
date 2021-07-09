using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using UGL.ViewModel.CircleItem_;

namespace UGL.ViewModel.Slider_
{
    public class SliderItemViewModel : BaseNotify
    {
        private string _serverName;
        private CircleServerItemViewModel _circleViewModel;

        public string ServerName
        {
            get => _serverName;
            set
            {
                _serverName = value;
                OnPropertyChanged();
            }
        }
        public CircleServerItemViewModel CircleViewModel
        {
            get => _circleViewModel;
            set
            {
                _circleViewModel = value;
                OnPropertyChanged();
            }
        }

        public SliderItemViewModel()
        {
            ServerName = "ServerNameIsEmpty";
            CircleViewModel = new CircleServerItemViewModel();
        }
        public SliderItemViewModel(string serverName, CircleServerItemViewModel viewModel)
        {
            ServerName = serverName;
            CircleViewModel = viewModel;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UGL.Helpers;
using UGL.Model;

namespace UGL.ViewModel.CircleItem_
{
    public class CircleServerItemViewModel : BaseNotify
    {
        private const double progressConst = 136 / 100;

        private double _progressValue = 136;
        private Color _borderColor;
        private ImageSource _serverLogo;
        private ServerStates _serverState;

        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value * progressConst;
                OnPropertyChanged();
            }
        }
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                OnPropertyChanged();
            }
        }
        public ImageSource ServerLogo
        {
            get => _serverLogo;
            set
            {
                _serverLogo = value;
                OnPropertyChanged();
            }
        }
        public ServerStates ServerState
        {
            get => _serverState;
            set
            {
                _serverState = value;
                BorderColor = ResourceHelper.FindColorByName(value.ToString());
            }
        }

        public CircleServerItemViewModel()
        {
            BorderColor = ResourceHelper.FindColorByName(ServerStates.Offline.ToString());
        }
        public CircleServerItemViewModel(ServerStates state, ImageSource source)
        {
            ServerState = state;
            ServerLogo = source;
        }
    }
}

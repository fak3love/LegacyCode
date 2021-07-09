using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Windows;
using System.Windows.Input;
using UGL.View.Controls;
using UGL.View.Controls.Slider;

namespace UGL.ViewModel.Slider_
{
    public class SliderControlBoxViewModel : BaseNotify
    {
        private SliderBoxViewModel _sliderBoxViewModel;
        private SliderBox _sliderBox;

        public SliderBoxViewModel SliderBoxViewModel
        {
            get => _sliderBoxViewModel;
            set
            {
                _sliderBoxViewModel = value;
                OnPropertyChanged();
            }
        }
        public SliderBox SliderBox
        {
            get => _sliderBox;
            set
            {
                _sliderBox = value;
                OnPropertyChanged();
            }
        }

        public SliderControlBoxViewModel(SliderBoxViewModel viewModel)
        {
            _sliderBoxViewModel = viewModel;
        }

        public SliderControlBoxViewModel()
        {
            _sliderBoxViewModel = new SliderBoxViewModel();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UGL.Model;

namespace UGL.ViewModel.History_
{
    public class HistoryPreviewBoxViewModel : BaseNotify
    {
        private HistoryItem _historyItem;

        public HistoryItem HistoryItem
        {
            get => _historyItem;
            set
            {
                _historyItem = value;
                OnPropertyChanged();
            }
        }

        public HistoryPreviewBoxViewModel()
        {
            _historyItem = HistoryItem.Default;
        }
        public HistoryPreviewBoxViewModel(HistoryItem historyItem)
        {
            _historyItem = historyItem;
        }
    }
}

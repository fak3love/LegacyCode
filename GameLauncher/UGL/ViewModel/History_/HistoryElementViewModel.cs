using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using UGL.Model;

namespace UGL.ViewModel.History_
{
    public class HistoryElementViewModel : BaseNotify
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

        public HistoryElementViewModel()
        {
            _historyItem = HistoryItem.Default;
        }
        public HistoryElementViewModel(HistoryItem historyItem)
        {
            _historyItem = historyItem;
        }
    }
}

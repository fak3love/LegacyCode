using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using UGL.Model;

namespace UGL.ViewModel.FriendList_
{
    public class FriendItemViewModel : BaseNotify
    {
        private FriendItemNotify _friendItem;

        public FriendItemNotify FriendItem
        {
            get => _friendItem;
            set
            {
                _friendItem = value;
                OnPropertyChanged();
            }
        }

        public FriendItemViewModel()
        {
            _friendItem = FriendItemNotify.Default;
        }
        public FriendItemViewModel(FriendItemNotify friendItem)
        {
            _friendItem = friendItem;
        }
    }
}

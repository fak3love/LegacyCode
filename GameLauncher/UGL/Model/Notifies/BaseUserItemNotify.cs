using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using UGL.Helpers;
using UGL.ViewModel;

namespace UGL.Model.Notifies
{
    public abstract class BaseUserItemNotify : BaseNotify
    {
        private string _nickName;
        private ImageSource _logoSource;
        private UserStatuses _status;
        private Color _statusColor;

        public string NickName
        {
            get => _nickName;
            set
            {
                _nickName = value;
                OnPropertyChanged();
            }
        }
        public ImageSource LogoSource
        {
            get => _logoSource;
            set
            {
                _logoSource = value;
                OnPropertyChanged();
            }
        }
        public UserStatuses Status
        {
            get => _status;
            set
            {
                _status = value;
                StatusColor = ResourceHelper.FindColorByName(value.ToString());
                OnPropertyChanged();
            }
        }
        public Color StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged();
            }
        }
    }
}

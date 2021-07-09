using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UGL.Model;
using UGL.View.Controls;

namespace UGL.ViewModel
{
    public class SettingsPanelViewModel : BaseNotify
    {
        private string _panelTitle = "Settings2";

        public string PanelTitle
        {
            get => _panelTitle;
            set
            {
                _panelTitle = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenSettingsPanel
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    SettingsPanel settingsPanel = parameter as SettingsPanel;

                    settingsPanel.Visibility = Visibility.Visible;
                });
            }
        }
        public ICommand CloseSettingsPanel
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    SettingsPanel settingsPanel = parameter as SettingsPanel;

                    settingsPanel.Visibility = Visibility.Collapsed;
                });
            }
        }
        public ICommand SettingsSelectionChanged
        {
            get
            {
                return new RelayCommand((parameter) =>
                {
                    SettingsContainer container = parameter as SettingsContainer;

                    if (container.Source.SelectedIndex == -1)
                        return;

                    container.OldSource.SelectedIndex = -1;

                    SettingsPanel settingsPanel = GetSettingsMain(container.Source);

                    foreach (var item in (settingsPanel.FindName("gridBorders") as Grid).Children)
                    {
                        if (item is Border border)
                            border.Visibility = Visibility.Collapsed;
                    }

                    PanelTitle = (container.Source.SelectedItem as ListBoxItem).Tag.ToString();
                    SelectedItem = container.Source.SelectedItem as ListBoxItem;

                    GetSettingsPanelByName((container.Source.SelectedItem as ListBoxItem).Tag.ToString(), settingsPanel).Visibility = Visibility.Visible;
                });
            }
        }
        public ICommand SaveSettings
        {
            get
            {
                return new RelayCommand(new Action<object>((parameter) =>
                {
                    //if (SelectedItem == ...)
                    MessageBox.Show("Saved!");
                }));
            }
        }

        public ListBoxItem SelectedItem { get; private set; }

        private Border GetSettingsPanelByName(string name, SettingsPanel settingsPanel)
        {
            return settingsPanel.FindName("borderSettingsPanel" + name) as Border;
        }
        private SettingsPanel GetSettingsMain(FrameworkElement element)
        {
            while (element.Parent != null)
            {
                if (element.Parent is SettingsPanel container)
                    return container;

                element = element.Parent as FrameworkElement;
            }

            return null;
        }
    }
}

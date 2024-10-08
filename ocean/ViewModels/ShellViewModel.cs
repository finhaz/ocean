﻿using System;
using System.Collections.ObjectModel;
using MahApps.Metro.IconPacks;
using ocean.Mvvm;
using ocean.Views;
using ocean.UI;

namespace ocean.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        public ObservableCollection<MenuItem> Menu { get; } = new();

        public ObservableCollection<MenuItem> OptionsMenu { get; } = new();

        public ShellViewModel()
        {
            // Build the menus
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.BugSolid },
                Label = "Bugs",
                NavigationType = typeof(BugsPage),
                NavigationDestination = new Uri("UI/debug_serial.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserSolid },
                Label = "User",
                NavigationType = typeof(UserPage),
                NavigationDestination = new Uri("UI/DataAnal.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserSolid },
                Label = "Break",
                NavigationType = typeof(BreakPage),
                NavigationDestination = new Uri("Views/BreakPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FontAwesomeBrands },
                Label = "Awesome",
                NavigationType = typeof(AwesomePage),
                NavigationDestination = new Uri("Views/AwesomePage.xaml", UriKind.RelativeOrAbsolute)
            });

            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserSolid },
                Label = "Settings",
                NavigationType = typeof(SettingsPage),
                //NavigationDestination = new Uri("Views/SettingsPage.xaml", UriKind.RelativeOrAbsolute)
                NavigationDestination = new Uri("UI/Settings.xaml", UriKind.RelativeOrAbsolute)
            });
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserSolid },
                Label = "About",
                NavigationType = typeof(AboutPage),
                NavigationDestination = new Uri("Views/AboutPage.xaml", UriKind.RelativeOrAbsolute)
            });
        }
    }
}
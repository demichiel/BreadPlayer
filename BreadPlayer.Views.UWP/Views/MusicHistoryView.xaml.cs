﻿using BreadPlayer.ViewModels;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace BreadPlayer
{
    public partial class MusicHistoryView : Page
    {
        private MusicHistoryViewModel MusicHistoryVM;

        public MusicHistoryView()
        {
            InitializeComponent();
            MusicHistoryVM = new MusicHistoryViewModel();
            this.DataContext = MusicHistoryVM;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Any())
                (e.RemovedItems[0] as PivotItem).Content = null;
            (mainPivot.SelectedItem as PivotItem).Content = recentlyPlayedList;
            MusicHistoryVM.CurrentCollection = null;
            switch (mainPivot.SelectedIndex)
            {
                case 0:
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    MusicHistoryVM.GetRecentlyPlayedSongs();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    break;

                case 1:
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    MusicHistoryVM.GetRecentlyAddedSongs();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    break;

                case 2:
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    MusicHistoryVM.GetMostPlayedSongs();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    break;
            }
        }
    }
}
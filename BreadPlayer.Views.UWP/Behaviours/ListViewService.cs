using BreadPlayer.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BreadPlayer.Behaviours
{
    public class ListViewService
    {
       // private static SelectionChangedEventHandler selectionChangedEventHandler;
       
        static ListViewService()
        {
            //selectionChangedEventHandler = new SelectionChangedEventHandler(OnSelectionChanged);
            FocusBeforeSelectProperty =
              DependencyProperty.RegisterAttached(
                  "FocusBeforeSelect", typeof(bool), typeof(ListViewService),
                   new PropertyMetadata(false, OnFocusBeforeSelectChanged));
            SelectedItemsProperty =
              DependencyProperty.RegisterAttached(
                  "SelectedItems", typeof(IList<object>), typeof(ListViewService),
                   new PropertyMetadata(null, OnSelectedItemsChanged));
        }

        #region SelectedItemsProperty
        public static readonly DependencyProperty SelectedItemsProperty;
        public static List<Mediafile> GetSelectedItems(ListView listView)
        {
            return (List<Mediafile>)listView.GetValue(SelectedItemsProperty);
        }
        public static void SetSelectedItems(ListView listView, List<Mediafile> value)
        {
            listView.SetValue(SelectedItemsProperty, value);
        }
        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listView = d as ListView;
            if (listView != null)
            {
                listView.SelectionChanged += (sender, args) =>
                {
                    if (args.RemovedItems.Count > 0)
                    {
                        foreach (var toRemove in args.RemovedItems.Cast<Mediafile>())
                        {
                            GetSelectedItems(listView).Remove(toRemove);
                        }
                    }
                    if (args.AddedItems.Count > 0)
                    {
                        GetSelectedItems(listView).AddRange(args.AddedItems.Cast<Mediafile>());
                    }
                };
            }
        }
        #endregion

        #region FocusBeforeSelectProperty
        public static readonly DependencyProperty FocusBeforeSelectProperty;

        public static bool GetFocusBeforeSelect(ListView listView)
        {
            return (bool)listView.GetValue(FocusBeforeSelectProperty);
        }

        public static void SetFocusBeforeSelect(ListView listView, bool value)
        {
            listView.SetValue(FocusBeforeSelectProperty, value);
        }

        private static void OnFocusBeforeSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listView = d as ListView;
            if (listView != null)
            {
                listView.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;

            if (listView != null)
            {
                //listView.ScrollIntoView(listView.SelectedItem);
                listView.Focus(FocusState.Programmatic);
            }
        }
        #endregion

    }
}

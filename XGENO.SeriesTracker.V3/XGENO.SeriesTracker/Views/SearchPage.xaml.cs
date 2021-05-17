using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Services;
using XGENO.SeriesTracker.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace XGENO.SeriesTracker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        private void SearchShowQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string searchTerm = (args.QueryText != null ? args.QueryText.Trim() : "");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = System.Net.WebUtility.HtmlEncode(searchTerm);

                SearchShow(searchTerm);
            }
            else
            {
                var dialogService = new DialogService();
                dialogService.ShowAsync("Please provide name of the show to search.", "SEARCH");
            }
        }

        private async Task SearchShow(string searchTerm)
        {
            var viewModel = this.DataContext as SearchPageViewModel;
            await viewModel.SearchShows(searchTerm);
        }
    }
}

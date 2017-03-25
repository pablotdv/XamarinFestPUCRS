using Core.Helpers;
using Core.Models;
using Core.ViewModels;
using System;
using Xamarin.Forms;

namespace Core.Views
{
	public partial class AttendeesView : ContentPage
	{
        private AttendeesViewModel _viewModel;
        public AttendeesView()
		{
			InitializeComponent();

            // Create the view model and set as binding context
            _viewModel = new AttendeesViewModel();
            BindingContext = _viewModel;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            (sender as ListView).SelectedItem = null;
            var attendeeModel = e.Item as AttendeeModel;
            await NavigationHelper.Instance.GotoDetails(attendeeModel);
        }

        public void OnMore(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            DisplayAlert("More Context Action", mi.CommandParameter + " more context action", "OK");
        }

        public void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            DisplayAlert("Delete Context Action", mi.CommandParameter + " delete context action", "OK");
        }
    }
}

using Core.Helpers;
using Core.Models;
using Core.ViewModels;
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
    }
}

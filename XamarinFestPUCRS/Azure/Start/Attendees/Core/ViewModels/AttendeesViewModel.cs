using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Helpers;
using Core.Models;
using Core.Services;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.ViewModels
{
    public class AttendeesViewModel : BaseViewModel
    {
        public ObservableCollection<AttendeeModel> Attendees { get; set; }
        public ICommand AddAttendeeCommand => new Command(async () => await AddAttendee());
        public Command GetAttendeesCommand { get; set; }

        public AttendeesViewModel()
        {
            Attendees = new ObservableCollection<AttendeeModel>();

            GetAttendeesCommand = new Command(
                async () => await GetAttendees());
        }

        private async Task AddAttendee()
        {
            await NavigationHelper.Instance.GotoDetails(new AttendeeModel());
        }

        public async Task GetAttendees()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var items = await AzureMobileService.Instance.GetAttendees();

                Attendees.Clear();
                foreach (var item in items)
                    Attendees.Add(item);
            }
            catch (Exception e)
            {
                LogHelper.Instance.AddLog(e);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

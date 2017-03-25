using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Helpers;
using Core.Models;
using Core.Plugins;
using Core.Services;
using Xamarin.Forms;

namespace Core.ViewModels
{
	public class DetailsViewModel : BaseViewModel
	{
        public ICommand SaveCommand => new Command(async () => await Save());
        public ICommand DeleteCommand => new Command(async () => await Delete());
        public ICommand ChangePhotoCommand => new Command(async () => await ChangePhoto());

        public ICommand GetPhotoCommand => new Command(async () => await GetPhoto());

        public DetailsViewModel(AttendeeModel attendeeModel)
		{
            Attendee = attendeeModel;
            Photo = "profile.png";
            GetPhotoCommand.Execute(null);
        }

        private async Task GetPhoto()
        {
            if (IsBusy)
                return;

            if (string.IsNullOrEmpty(Attendee.PhotoName))
                return;

            try
            {
                IsBusy = true;
                var bytes = await AzureStorageService.Instance.DownloadFile(Attendee.PhotoName);

                if (bytes == null)
                    throw new Exception("Not image to load");

                Photo = ImageSource.FromStream(() =>
                {
                    return new MemoryStream(bytes);
                });
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

        private AttendeeModel _attendee;
        public AttendeeModel Attendee
        {
            get { return _attendee; }
            set
            {
                _attendee = value;
                OnPropertyChanged();
            }
        }

        private object _photo;
        public object Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                OnPropertyChanged();
            }
        }

        private async Task Save()
        {
            if (IsBusy)
                return;

            Exception exception = null;

            try
            {
                IsBusy = true;

                if (string.IsNullOrEmpty(Attendee.Name))
                    throw new Exception("The name is required");

                if (string.IsNullOrEmpty(Attendee.Email))
                    throw new Exception("The e-mail is required");

                if (_photoStream != null)
                {
                    if (string.IsNullOrEmpty(Attendee.PhotoName))
                        Attendee.PhotoName = Guid.NewGuid().ToString();

                    await AzureStorageService.Instance.UploadFile(_photoStream, Attendee.PhotoName);
                }

                await AzureMobileService.Instance.SaveAttendee(Attendee);
            }
            catch (Exception e)
            {
                exception = e;
                LogHelper.Instance.AddLog(e);
            }
            finally
            {
                IsBusy = false;
            }

            if (exception != null)
            {
                await MessageHelper.Instance.ShowMessage(
                    "An error has occurred",
                    exception.Message,
                    "Ok"
                );
                return;
            }

            await NavigationHelper.Instance.GoBack();
        }

        private Stream _photoStream;

        private async Task ChangePhoto()
        {
            var textTakePhoto = "Take photo";
            var textOpenGallery = "Open gallery";
            var textCancel = "Cancel";
            var textDelete = "Delete";

            var actions = new string[] { textTakePhoto, textOpenGallery };

            var response = await MessageHelper.Instance.ShowOptions(
                "What to want to do?",
                textCancel,
                textDelete,
                actions
            );

            if (response == textCancel)
                return;

            Exception exception = null;

            try
            {
                if (response == textOpenGallery)
                {
                    if (await MediaService.Instance.IsPickPhotoSupported() == false)
                        throw new Exception("Is not possible open image gallery");

                    var file = await MediaService.Instance.PickPhotoAsync();
                    if (file != null)
                    {
                        Photo = ImageSource.FromFile(file.Path);
                        _photoStream = file.GetStream();
                    }
                }

                if (response == textTakePhoto)
                {
                    if (await MediaService.Instance.IsCameraAvailable() == false)
                        throw new Exception("Camera is not available on your device");

                    var file = await MediaService.Instance.TakePhotoAsync();
                    if (file != null)
                    {
                        Photo = ImageSource.FromFile(file.Path);
                        _photoStream = file.GetStream();
                    }
                }

                if (response == textDelete)
                {
                    await AzureStorageService.Instance.DeleteFile(Attendee.PhotoName);
                    Attendee.PhotoName = null;
                    Photo = "profile.png";
                }
            }
            catch (Exception e)
            {
                exception = e;
                LogHelper.Instance.AddLog(e);
            }

            if (exception != null)
            {
                await MessageHelper.Instance.ShowMessage(
                    "Something is wrong",
                    exception.Message,
                    "Ok"
                );
                return;
            }
        }

        private async Task Delete()
        {
            if (IsBusy)
                return;

            if (string.IsNullOrEmpty(Attendee.Id))
                return;

            var delete = await MessageHelper.Instance.ShowAsk(
                "Delete attendee",
                "You sure delete this attendee?",
                "Yes",
                "No"
            );

            if (delete == false)
                return;

            Exception exception = null;

            try
            {
                IsBusy = true;
                await AzureMobileService.Instance.DeleteAttendee(Attendee);
            }
            catch (Exception e)
            {
                exception = e;
                LogHelper.Instance.AddLog(e);
            }
            finally
            {
                IsBusy = false;
            }

            if (exception != null)
            {
                await MessageHelper.Instance.ShowMessage(
                    "An error has ocurred",
                    exception.Message,
                    "Ok"
                );
                return;
            }

            await NavigationHelper.Instance.GoBack();
        }
    }
}

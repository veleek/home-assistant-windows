using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace Ben.HomeAssistant
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string PushNotificationChannelSetting = "PushNotificationChannel";

        private IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;

        private HttpClient hassClient;
        private Uri homeAssistantUrl = new Uri("http://localhost:8123");
        private string password;
        private string deviceName;

        public event PropertyChangedEventHandler PropertyChanged;

        public Uri HomeAssistantUrl
        {
            get
            {
                return this.homeAssistantUrl;
            }

            set
            {
                if (this.Set(ref this.homeAssistantUrl, value))
                {
                    this.hassClient = null;
                }
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.Set(ref this.password, value);
            }
        }

        public HttpClient HassClient
        {
            get
            {
                if(hassClient == null && HomeAssistantUrl != null)
                {
                    hassClient = new HttpClient
                    {
                        BaseAddress = HomeAssistantUrl
                    };
                }

                return hassClient;
            }
        }

        public string DeviceName
        {
            get
            {
                return this.deviceName;
            }

            set
            {
                this.Set(ref this.deviceName, value);
            }
        }

        public bool PushNotificationsEnabled => this.PushNotificationsChannel != null;

        public string PushNotificationsChannel
        {
            get => (string)localSettings[PushNotificationChannelSetting];
            set
            {
                if (value != this.PushNotificationsChannel)
                {
                    localSettings[PushNotificationChannelSetting] = value;
                    NotifyPropertyChanged();
                }

                NotifyPropertyChanged(nameof(PushNotificationsEnabled));
            }
        }

        public async Task SetPushNotificationsEnabledAsync(bool enabled)
        {
            if (enabled)
            {
                await this.EnablePushNotificationsAsync();
            }
            else
            {
                await this.DisablePushNotificationsAsync();
            }
        }

        public async Task EnablePushNotificationsAsync()
        {
            try
            {
                PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                if (this.PushNotificationsChannel != channel.Uri)
                {
                    // Register it with HASS
                    var body = new
                    {
                        id = GetIdentifier(),
                        name = this.DeviceName,
                        channel = channel.Uri.ToString(),
                        expiry = channel.ExpirationTime.ToUnixTimeSeconds()
                    };

                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(body, settings), Encoding.UTF8, "application/json");

                    using (HttpResponseMessage resp = await this.HassClient.PostAsync("/api/notify.windows", content))
                    {
                        if (!resp.IsSuccessStatusCode)
                        {
                            return;
                        }
                    }

                    // Then save it.
                    this.PushNotificationsChannel = channel.Uri;
                }
            }
            catch (Exception e)
            {
                this.PushNotificationsChannel = null;
            }
        }

        public async Task DisablePushNotificationsAsync()
        {
            try
            {
                var body = new
                {
                    id = GetIdentifier()
                };

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, "/api/notify.windows")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };

                using (HttpResponseMessage response = await this.HassClient.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // If we couldn't successfully unregister, just pretend we're still registered
                        // to give us the opportunity to unregister again later.
                        return;
                    }
                }

                this.PushNotificationsChannel = null;
            }
            catch
            {
                NotifyPropertyChanged(nameof(PushNotificationsChannel));
                NotifyPropertyChanged(nameof(PushNotificationsEnabled));
                throw;
            }
        }

        private string GetIdentifier()
        {
            var systemId = SystemIdentification.GetSystemIdForPublisher();
            IBuffer id;
            if (systemId.Source != SystemIdentificationSource.None)
            {
                id = systemId.Id;
            }
            else
            {
                HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
                id = token.Id;
            }

            return CryptographicBuffer.EncodeToHexString(id);
        }

        public bool Set<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if(object.Equals(field, value))
            {
                return false;
            }

            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

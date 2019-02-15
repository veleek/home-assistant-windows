using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ben.HomeAssistant
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.ViewModel = new MainViewModel();
            this.InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get; set;
        }

        private async void PushNotificationsToggled(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable until we're done
                this.PushNotificationsToggle.IsEnabled = false;
                await this.ViewModel.SetPushNotificationsEnabledAsync(PushNotificationsToggle.IsOn);
            }
            finally
            {
                this.PushNotificationsToggle.IsEnabled = true;
            }
        }

        public async Task SendToastAsync()
        {
            await Task.Yield();

            // In a real app, these would be initialized with actual data
            string title = "Andrew sent you a picture";
            string content = "Check this out, Happy Canyon in Utah!";
            string image = "https://picsum.photos/360/202?image=883";
            //string logo = "ms-appdata:///local/Andrew.jpg";
            string logo = "ms-appx:///Assets/release_1024.png";

            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText() { Text = title },
                        new AdaptiveText() { Text = content },
                    },

                    HeroImage = new ToastGenericHeroImage { Source = image },

                    AppLogoOverride = new ToastGenericAppLogo()
                    {
                        Source = logo,
                        HintCrop = ToastGenericAppLogoCrop.None
                    }
                }
            };

            // In a real app, these would be initialized with actual data
            int conversationId = 384928;

            // Construct the actions for the toast (inputs and buttons)
            ToastActionsCustom actions = new ToastActionsCustom()
            {
                Inputs =
                {
                    new ToastTextBox("tbReply")
                    {
                        PlaceholderContent = "Type a response",
                    },
                },

                Buttons =
                {
                    new ToastButton("Throw it on the ground", new QueryString()
                    {
                        { "action", "partOfTheSystem" },
                        { "conversationId", conversationId.ToString() }

                    }.ToString())
                    {
                        ActivationType = ToastActivationType.Foreground,
                        ImageUri = "Assets/Reply.png",
 
                        // Reference the text box's ID in order to
                        // place this button next to the text box
                        TextBoxId = "tbReply2"
                    },

                    new ToastButton("Reply", new QueryString()
                    {
                        { "action", "reply" },
                        { "conversationId", conversationId.ToString() }

                    }.ToString())
                    {
                        ActivationType = ToastActivationType.Background,
                        ImageUri = "Assets/Reply.png",
 
                        // Reference the text box's ID in order to
                        // place this button next to the text box
                        TextBoxId = "tbReply"
                    },

                    new ToastButton("Like", new QueryString()
                    {
                        { "action", "like" },
                        { "conversationId", conversationId.ToString() }

                    }.ToString())
                    {
                        ActivationType = ToastActivationType.Background
                    },

                    new ToastButton("View", new QueryString()
                    {
                        { "action", "viewImage" },
                        { "imageUrl", image }

                    }.ToString())
                }
            };

            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,

                // Arguments when the user taps body of toast
                Launch = new QueryString()
                {
                    { "action", "viewConversation" },
                    { "conversationId", conversationId.ToString() }

                }.ToString(),
                //Header = new ToastHeader("cat-header", "Cats", "bestAnimal=Cat"),
                //DisplayTimestamp = DateTimeOffset.Now.AddDays(-9000),
            };

            // And create the toast notification
            var toastXml = toastContent.GetXml();
            var toastRaw = toastContent.GetContent();

            XmlDocument rawToast = new XmlDocument();
            rawToast.LoadXml(@"<toast launch=""action=viewStory&amp;storyId=92187"">
                                   <visual>
                                       <binding template=""ToastGeneric"">
                                           <text>Tortoise beats rabbit in epic race</text>
                                           <text>In a surprising turn of events, Rockstar Rabbit took a nasty crash, allowing Thomas the Tortoise to win the race.</text>
                                           <text placement=""attribution"">The Animal Times</text>
                                       </binding>
                                   </visual>
                               </toast>");
            var toast = new ToastNotification(toastXml);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void SendToast(object sender, RoutedEventArgs e)
        {
            await SendToastAsync();
        }
    }
}

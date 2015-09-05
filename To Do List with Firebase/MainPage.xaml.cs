using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp;
using FireSharp.Response;
using FireSharp.EventStreaming;
using FireSharp.Extensions;
using Windows.UI.Popups;
using System.Diagnostics;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace To_Do_List_with_Firebase
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ListenToStream();
        }
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "your-auth-secret",
            BasePath = "<your-firebase-reference-link>.firebaseio.com/"

        };
        IFirebaseClient client;

        private static Dictionary<string, string> keyHolder = new Dictionary<string, string>();
        private async void ListenToStream()
        {

            client = new FirebaseClient(config);
            await client.OnAsync("FireSharp/Name/", (sender, args) =>
            {
                //Gets the Unique ID and deletes the any other string attached to it
                string dataFromFB = args.Data;
                string paths = args.Path;
                string key = RemoveNameSubstring(paths);
                string uniqueKey = key.Split('/').Last();
                if (keyHolder.ContainsKey(uniqueKey))
                {
                    keyHolder[uniqueKey] = dataFromFB;
                    AddToListView(dataFromFB);
                }
                else
                {
                    keyHolder.Add(uniqueKey, dataFromFB);
                    AddToListView(dataFromFB);
                }
            });

        }

        private async void AddToListView(string data)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    if (send.Content.Equals("Update"))
                    {
                        listView.Items.Insert(listView.SelectedIndex, data);
                        listView.Items.RemoveAt(listView.SelectedIndex);
                        send.Content = "Send";

                    }
                    else
                    {
                        listView.Items.Add(data);

                    }

                });

        }


        private async void SetDataToFirebase(string text)
        {
            client = new FirebaseClient(config);


            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("Name", text);

            var response = await client.PushAsync("FireSharp/Name/", values);
            textBox.Text = "";
        }
        private async void send_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> val = new Dictionary<string, string>();
            string selectedIndex;
            string pathToUpdate;


            if (send.Content.Equals("Update"))
            {
                selectedIndex = keyHolder.Keys.ElementAt(listView.SelectedIndex);

                pathToUpdate = selectedIndex;
                if (textBox.Text == "")
                {
                    MessageDialog message = new MessageDialog("An updated entry cannot be empty", "Sorry");
                    await message.ShowAsync();

                }
                else
                {
                    val["Name"] = textBox.Text;
                    ListUpdation(pathToUpdate, val);
                }




            }
            else
            {
                if (textBox.Text == "")
                {
                    MessageDialog message = new MessageDialog("Enter Some Text", "Sorry");
                    await message.ShowAsync();
                }
                else
                {

                    SetDataToFirebase(textBox.Text);

                }
            }

        }
        private async void DeleteFromFirebase(string val)
        {
            client = new FirebaseClient(config);
            FirebaseResponse delete = await client.DeleteAsync("FireSharp/Name/" + val);
            var status = delete.StatusCode;
            //If Firebase returns OK status, value is removed from ListView
            if (status.ToString() == "OK")
            {
                listView.Items.Remove(listView.SelectedItem);
                keyHolder.Remove(val);
            }

        }

        private async void ListUpdation(string key, Dictionary<string, string> updatedValue)
        {
            FirebaseResponse response;


            response = await client.UpdateAsync("FireSharp/Name/" + key, updatedValue);

            if (listView.SelectedIndex == 0 || listView.SelectedIndex == keyHolder.Count - 1)
            {
                keyHolder[keyHolder.Keys.ElementAt(listView.SelectedIndex)] = textBox.Text;
                listView.Items.Insert(listView.SelectedIndex, textBox.Text);
                listView.Items.RemoveAt(listView.SelectedIndex);
                send.Content = "Send";


            }

            textBox.Text = "";

        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

            if (listView.SelectedItem == null)
            {
                MessageDialog message = new MessageDialog("Please select an Item to Remove", "Sorry");
                await message.ShowAsync();

            }
            else
            {
                int currentIndex = listView.SelectedIndex;

                string currentKey = keyHolder.Keys.ElementAt(currentIndex);
                if (listView.SelectedItem.ToString() == keyHolder.Values.ElementAt(currentIndex))
                {
                    DeleteFromFirebase(currentKey);

                }
            }
        }
        public string RemoveNameSubstring(string name)
        {
            int index = name.IndexOf("/Name");
            string uniqueKey = (index < 0) ? name : name.Remove(index, "/Name".Length);
            return uniqueKey;


        }

        private async void EditBarButton_Click(object sender, RoutedEventArgs e)
        {

            if (listView.SelectedItem == null)
            {
                MessageDialog message = new MessageDialog("Please select an Item to Update", "Sorry");
                await message.ShowAsync();

            }
            else
            {
                textBox.Text = listView.SelectedItem.ToString();
                send.Content = "Update";
            }
        }




    }


}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UniversalManagerLight.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UniversalManagerLight.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Lamp : Page
    {
        public Lamp()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var vm = this.DataContext as LampViewModel;
            if (e.Parameter is LampVoiceCommand)
            {
                var voiceCommand = e.Parameter as LampVoiceCommand;
                switch (voiceCommand.VoiceCommand)
                {
                    case "offLight":
                        vm.OffLight.Execute(null);
                        break;
                    case "onLight":
                        vm.OnLight.Execute(null);
                        break;
                    case "changeColor":
                        vm.ChangeColor.Execute(voiceCommand.Color);
                        break;
                    default:
                        break;
                }
            }

            base.OnNavigatedTo(e);

        }
    }
}

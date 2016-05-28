using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalManagerLight.ViewModel
{
    public class LampViewModel : ViewModelBase
    {
        private bool _lightState = true;
        private bool _isEnabledMyo = false;
        private string _message;
        private DataAccess.Light _dataAccess;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    this.RaisePropertyChanged(nameof(Message));
                }
            }
        }
        public ICommand OnLight { get; set; }
        public ICommand OffLight { get; set; }
        public ICommand ChangeColor { get; set; }
        public ICommand StartListeningMyo { get; set; }

        public bool IsEnabledMyo
        {
            get
            {
                return !_isEnabledMyo;
            }

            set
            {
                if (_isEnabledMyo != value)
                {
                    _isEnabledMyo = value;
                    RaisePropertyChanged(nameof(IsEnabledMyo));
                }
            }
        }

        private void initMyo()
        {
            IsEnabledMyo = true;
            try
            {
                var channel = Channel.Create(
                       ChannelDriver.Create(ChannelBridge.Create(),
                       MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
                var hub = MyoSharp.Device.Hub.Create(channel);

                // listen for when the Myo connects
                hub.MyoConnected += (sender, e1) =>
                {
                    Debug.WriteLine("Myo {0} has connected!", e1.Myo.Handle);
                    e1.Myo.Vibrate(VibrationType.Short);
                    e1.Myo.Unlock(UnlockType.Hold);

                    e1.Myo.PoseChanged += Myo_PoseChanged;
                };
               
                // listen for when the Myo disconnects
                hub.MyoDisconnected += (sender, e1) =>
                {
                    Debug.WriteLine("It looks like {0} arm Myo has disconnected!", e1.Myo.Arm);
                    e1.Myo.PoseChanged -= Myo_PoseChanged;
                };

                channel.StartListening();
            }
            catch (Exception ex)
            {
                Debug.Write($"Myo Error : {ex}");
            }
        }

        private async void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            switch (e.Myo.Pose)
            {
                case Pose.DoubleTap:
                    if (_lightState)
                    {
                        await _dataAccess.On(new Models.Light() { State = true, LightId = 1, Color = new Models.Color() { R = 1, G = 1, B = 1 } });
                    }
                    else
                    {
                        await _dataAccess.Off(1);
                    }
                    _lightState = !_lightState;
                    break;
                case Pose.Unknown:
                    break;
                default:
                    break;
            }
        }

        public LampViewModel()
        {
            _dataAccess = new DataAccess.Light();
            Message = "Exemple : Génie allume la lampe";

            OnLight = new RelayCommand<int?>(async (idLight) =>
            {
                bool res = await _dataAccess.On(new Models.Light() { State = true, LightId = idLight ?? 1, Color = new Models.Color() });
                if (res)
                {
                    Message = "Lampe Allumée";
                }
                else
                {
                    Message = "Erreur durant l'execution";
                }
            });

            OffLight = new RelayCommand<int?>(async (param) =>
            {
                bool res = await _dataAccess.Off(1);
                if (res)
                {
                    Message = "Lampe éteinte";
                }
                else
                {
                    Message = "Erreur durant l'execution";
                }

            });

            ChangeColor = new RelayCommand<string>(async (color) =>
            {
                var light = new Models.Light() { State = true, LightId = 1, Color = new Models.Color() };
                switch (color)
                {
                    case "rouge":
                        light.Color.SetRedColor();
                        break;
                    case "vert":
                        light.Color.SetGreenColor();
                        break;
                    case "bleu":
                        light.Color.SetBlueColor();
                        break;
                    default:
                        break;
                }
                bool res = await _dataAccess.On(light);

                if (res)
                {
                    Message = "La couleur est : " + color;
                }
                else
                {
                    Message = "Erreur durant l'execution";
                }

            });

            StartListeningMyo = new RelayCommand(() =>
            {
                initMyo();
            }, () =>
            {
                return !_isEnabledMyo;
            });
        }
    }
}

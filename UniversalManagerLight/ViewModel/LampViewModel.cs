using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniversalManagerLight.ViewModel
{
    public class LampViewModel : ViewModelBase
    {
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
        }
    }
}

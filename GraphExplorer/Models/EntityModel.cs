using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorer.Models
{
    public class EntityModel : ViewModelBase
    {
        public EntityModel()
        {
            Attributes = new ObservableCollection<string>();
        }
        private string _Field;
        public string Field
        {
            get { return _Field; }
            set
            {
                _Field = value;
                RaisePropertyChanged<string>(() => this.Field);
            }
        }

        private string _TimeField;
        public string TimeField
        {
            get { return _TimeField; }
            set
            {
                _TimeField = value;
                RaisePropertyChanged<string>(() => this.TimeField);
            }
        }


        private ObservableCollection<string> _Attributes;
        public ObservableCollection<string> Attributes
        {
            get { return _Attributes; }
            set
            {
                _Attributes = value;
                RaisePropertyChanged<ObservableCollection<string>>(() => this.Attributes);
            }
        }
        
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                RaisePropertyChanged<string>(() => this.Name);
            }
        }
    }
}

using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorer.Models
{
    public class EntityRelationModel : ViewModelBase
    {

        private string _From;
        public string From
        {
            get { return _From; }
            set
            {
                _From = value;
                RaisePropertyChanged<string>(() => this.From);
            }
        }

        private string _To;
        public string To
        {
            get { return _To; }
            set
            {
                _To = value;
                RaisePropertyChanged<string>(() => this.To);
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

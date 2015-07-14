using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorer.Models
{
    public class EntityModel : ViewModelBase
    {
        
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

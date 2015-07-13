using Northwoods.GoXam.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorer.Models
{

    // Define custom link data; the node key is of type String,
    // the port key should be of type String but is unused in this app.
    [Serializable]  // serializable in WPF to support the clipboard
    public class LinkData : GraphLinksModelLinkData<String, String>
    {
        public String Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    String old = _Type;
                    _Type = value;
                    RaisePropertyChanged("Type", old, value);
                }
            }
        }
        private String _Type = "";
    }
}

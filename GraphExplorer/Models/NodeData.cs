using Northwoods.GoXam;
using Northwoods.GoXam.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorer.Models
{

    // Define custom node data; the node key is of type String.
    // Add a property named Color that might change.
    [Serializable]  // serializable in WPF to support the clipboard
    public class NodeData : GraphLinksModelNodeData<String>
    {
        public NodeFigure Shape
        {
            get
            {
                switch (_Type)
                {
                    case "User":
                        return NodeFigure.Actor;
                    case "Host":
                        return NodeFigure.Ethernet;
                    case "Application":
                        return NodeFigure.Process;
                    default:
                        return NodeFigure.RoundedRectangle;
                }
            }
        }

        public String Color
        {
            get
            {
                switch (_Type)
                {
                    case "User":
                        return "LightGreen";
                    case "Host":
                        return "Blue";
                    case "Application":
                        return "Orange";
                    default:
                        return "White";
                }
            }
        }

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
                    RaisePropertyChanged("Shape", old, value);
                }
            }
        }
        private String _Type = "Unknown";
    }

}

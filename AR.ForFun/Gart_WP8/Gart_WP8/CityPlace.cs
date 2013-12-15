using GART.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gart_WP8
{
    public class CityPlace : ARItem
    {
        private string description;
 
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyPropertyChanged(() => Description);
                }
            }
        }
    }
}

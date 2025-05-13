using System;
using System.ComponentModel;
using System.Data;

namespace MyApp.Data
{
    
    public class TdmMain : Component
    {
        private DataTable _dsPeople;

        public TdmMain()
        {
            
            _dsPeople = new DataTable("People");
            
            
        }

        
        public DataTable DsPeople
        {
            get { return _dsPeople; }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GRIDLibraries.Libraries
{
    public partial class GridLib
    {
        //WDEC5009GRDSQ01

        string conStringMain = "Data Source=DESKTOP-A0R75AD;" + "Initial Catalog=TestDB" + ";" + "Persist Security Info=True;" + "Integrated Security=SSPI;" + "Connect Timeout=3000;";
        //string conStringMain = "Data Source=WPEC5009GRDRP01;" + "Initial Catalog=GRID_MAIN" + ";" + "Persist Security Info=True;" + "Integrated Security=SSPI;" + "Connect Timeout=3000;"; 

        string conStringAHS_QA = "Data Source=DESKTOP-A0R75AD;" + "Initial Catalog=RPA_GRID_MAIN" + ";" + "Persist Security Info=True;" + "Integrated Security=SSPI;" + "Connect Timeout=1500;";
        //string conStringAHS_QA = "Data Source=WPPHL039SQL01;" + "Initial Catalog=RPA_GRID_MAIN" + ";" + "Persist Security Info=True;" + "Integrated Security=SSPI;" + "Connect Timeout=1500;";

        public string conString = "";

       

        public GridLib()
        {
           gridData = new gridDataStore();
         
        }

    }
}

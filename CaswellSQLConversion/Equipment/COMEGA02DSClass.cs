using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.Equipment
{
    public class COMEGA02_DSClass
    {
        public string WaferNumber { get; set; }
        public List<string> FileDetailList { get; set; }
        public List<string> FilePreConditionList { get; set; }
        public List<COMEGA02_CSVClass> CSVObjectList { get; set; }
    }

    public class COMEGA02_CSVClass
    {
        public string RecipeName { get; set; }
        public DateTime RecipeDate { get; set; }
        public string CSV { get; set; }
        public string TestStep { get; set; }
    }
}

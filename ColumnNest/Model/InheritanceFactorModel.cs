using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnNest.Model
{
    public class InheritanceFactorModel
    {
        /// <summary>
        /// 利用率
        /// </summary>
        public double UtilizationRatio { get; set; }
        /// <summary>
        /// 标准差
        /// </summary>
        public double StandardDeviation { get; set; }
        /// <summary>
        /// 最大余料
        /// </summary>
        public int MaxRestMasterial { get; set; }
    }
}

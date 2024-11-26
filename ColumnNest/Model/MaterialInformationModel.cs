using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnNest.Model
{
	public class MaterialInformationModel
	{
        /// <summary>
        /// 余料
        /// </summary>
        public Dictionary<int, int> RestMaterials { get; set; } = new Dictionary<int, int>();
        /// <summary>
        /// 需下料构件
        /// </summary>
        public List<int> RequireMaterials { get; set; }=new List<int>();
        /// <summary>
        /// 原料长度
        /// </summary>
        public int MaterialLength { get; set; }
        /// <summary>
        /// 是否可拼接
        /// </summary>
        public bool Connectable{get; set; }
        /// <summary>
        /// 是否包含原料
        /// </summary>
        public bool isIncludeRawBars{get; set; }
        /// <summary>
        /// 单构件拼接次数限值
        /// </summary>
        public int ConnectNumberLimited { get; set; }
        /// <summary>
        /// 总拼接次数限值
        /// </summary>
        public int ConnectTotalNumberLimited { get; set; }
        /// <summary>
        /// 余料可利用长度限值
        /// </summary>
        public int RestMaterialLengthLimited { get; set; }

        internal void variation(int exchangeCount)
        {
            Random random = new Random();
            List<int> randList = new List<int>();
            if (exchangeCount > RequireMaterials.Count/2)
            {
                exchangeCount = RequireMaterials.Count/2;
            }
            for (int i = 0; i < exchangeCount * 2; i++)
            {
                int tempRandNum=random.Next(RequireMaterials.Count);
                while (randList.Contains(tempRandNum))
                { 
                    tempRandNum=random.Next(RequireMaterials.Count);
                }
                randList.Add(tempRandNum);
            }
            for (int i = 0; i < exchangeCount; i++)
            {
                (RequireMaterials[randList[i*2]], RequireMaterials[randList[i *2+1]]) = (RequireMaterials[randList[i * 2 + 1]], RequireMaterials[randList[i * 2]]);
            }
        }
    }
}

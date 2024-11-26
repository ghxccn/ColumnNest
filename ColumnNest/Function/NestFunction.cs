using ColumnNest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColumnNest.Function
{
	public class NestFunction
	{
        public (List<List<int>>, List<(int, List<int>)>,Dictionary<int,int>) NestAchieve(List<MaterialInformationModel> modelList)
		{
			List<MaterialInformationModel> notcombinableModelList=new List<MaterialInformationModel>();
			List<MaterialInformationModel> combinableModelList=new List<MaterialInformationModel>();
			foreach (MaterialInformationModel model in modelList)
			{
				if (model.RequireMaterials.Count==0)
				{
					continue;
				}
                if (model.Connectable==false)
                {
					notcombinableModelList.Add(model);
                }
				else
				{
					if (model.Connectable==true)
					{
						combinableModelList.Add(model);
					}
				}
            }
			var materialLength=notcombinableModelList.Count==0?combinableModelList.FirstOrDefault().MaterialLength:notcombinableModelList.FirstOrDefault().MaterialLength;
			var (barsComb, restCombList, restMaterialDic) = GetAllCombination(notcombinableModelList, combinableModelList);
            double utilizationRatio = GetUtilizationRatio(barsComb, restCombList, materialLength);//初始组合
            for (int i = combinableModelList.Count - 1; i >= 0; i--)
            {
				combinableModelList [i].Connectable = false;
				notcombinableModelList.Add(combinableModelList[i]);
				combinableModelList.RemoveAt(i);
                var (barsComb0, restCombList0, restMaterialDic0) = GetAllCombination(notcombinableModelList, combinableModelList);
				double tempUtilizationRatio = GetUtilizationRatio(barsComb0, restCombList0, notcombinableModelList[0].MaterialLength);
				if (tempUtilizationRatio>utilizationRatio)
				{
					barsComb = barsComb0;
					restCombList = restCombList0;
					restMaterialDic=restMaterialDic0;
				}
            }
            return (barsComb, restCombList,restMaterialDic);
        }
		/// <summary>
		/// 所有柱类别全求解
		/// </summary>
		/// <param name="notcombinableModelList"></param>
		/// <param name="combinableModelList"></param>
		/// <returns></returns>
		private (List<List<int>>, List<(int, List<int>)>, Dictionary<int, int>) GetAllCombination(List<MaterialInformationModel> notcombinableModelList, List<MaterialInformationModel> combinableModelList)
		{
			//1，初始下料
			//非拼接件下料
			var barsComb=new List<List<int>> ();
			var restCombList=new List<(int, List<int>)> ();
			var restMaterialDic=new Dictionary<int, int> ();
			List<int> tempFirstMaterialList = new List<int>();//暂存第一个不拼接数据组的下料清单
			MaterialInformationModel notCombinableModel = notcombinableModelList.FirstOrDefault();
            if (notCombinableModel!=default)
			{
				notCombinableModel.RequireMaterials.ForEach(x => tempFirstMaterialList.Add(x));
				if (notcombinableModelList.Count > 1)
				{
					for (int i = 1; i < notcombinableModelList.Count; i++)
					{
						notCombinableModel.RequireMaterials.AddRange(notcombinableModelList[i].RequireMaterials);
					}
				}
				(barsComb/*整料切割*/, restCombList/*拼接下料*/, restMaterialDic/*余料*/) = GenerateCombination(notCombinableModel);
			}
			//拼接件下料
			foreach (var combinableModel in combinableModelList)
			{
				combinableModel.RestMaterials.MergeDic(restMaterialDic );
				var (barsComb0,restCombList0 ,restMaterialDic0 )=GenerateCombination(combinableModel);
				barsComb.AddRange(barsComb0);
				restCombList.AddRange(restCombList0 );
				combinableModel.RestMaterials.Clear();
				restMaterialDic.Clear();
				restMaterialDic.MergeDic(restMaterialDic0 );
			}

			//2，迭代下料
			if (notCombinableModel != default)
			{
                InheritanceFactorModel inheritanceFactor = new InheritanceFactorModel();
                inheritanceFactor.UtilizationRatio = GetUtilizationRatio(barsComb, restCombList, notCombinableModel.MaterialLength);
                inheritanceFactor.StandardDeviation = MathTool.CalcuateVariance(EnumTool.DicExpandToList(restMaterialDic)); //方差法
                if (restMaterialDic.Count == 0)
                {
                    inheritanceFactor.MaxRestMasterial = 0;
                }
                else
                {
                    inheritanceFactor.MaxRestMasterial = EnumTool.DicExpandToList(restMaterialDic).Max();
                }
                int inheritanceCount = 0;//遗传计数器
                int variationCount = 0;//变异计数器
                while (variationCount < notCombinableModel.RequireMaterials.Count * 20)
                {
                    List<int> tempMaterialList = new List<int>();
                    notCombinableModel.RequireMaterials.ForEach(x => tempMaterialList.Add(x));
                    variationCount++;//变异
                    notCombinableModel.variation((int)Math.Ceiling((double)variationCount / 100));
                    var (tempBarsComb, tempRestCombList, tempRestMaterialDic) = GenerateCombination(notCombinableModel);//计算结果
                    foreach (var combinableModel in combinableModelList)
                    {
                        combinableModel.RestMaterials.MergeDic(tempRestMaterialDic);
                        var (barsComb0, restCombList0, restMaterialDic0) = GenerateCombination(combinableModel);
                        tempBarsComb.AddRange(barsComb0);
                        tempRestCombList.AddRange(restCombList0);
                        combinableModel.RestMaterials.Clear();
                        tempRestMaterialDic.Clear();
                        tempRestMaterialDic.MergeDic(restMaterialDic0);
                    }
                    InheritanceFactorModel tempInheritanceFactor = new InheritanceFactorModel();
                    tempInheritanceFactor.UtilizationRatio = GetUtilizationRatio(tempBarsComb, tempRestCombList, notCombinableModel.MaterialLength);//计算结果对应指标
                    tempInheritanceFactor.StandardDeviation = MathTool.CalcuateVariance(EnumTool.DicExpandToList(tempRestMaterialDic));
                    tempInheritanceFactor.MaxRestMasterial = tempRestMaterialDic.Count == 0 ? 0 : EnumTool.DicExpandToList(tempRestMaterialDic).Max();
                    if (isInherit(inheritanceFactor, tempInheritanceFactor))
                    {
                        //遗传
                        barsComb = tempBarsComb;
                        restCombList = tempRestCombList;
                        restMaterialDic = tempRestMaterialDic;
                        inheritanceFactor.UtilizationRatio = tempInheritanceFactor.UtilizationRatio;
                        inheritanceFactor.MaxRestMasterial = tempInheritanceFactor.MaxRestMasterial;
                        inheritanceFactor.StandardDeviation = tempInheritanceFactor.StandardDeviation;
                        inheritanceCount++;
                        variationCount = 0;
                    }
                    else
                    {
                        //不遗传
                        notCombinableModel.RequireMaterials = tempMaterialList;
                    }
                }
                //3,model数据恢复
                notCombinableModel.RequireMaterials = tempFirstMaterialList;//恢复第一个不拼接数据组的下料清单
				}
                return (barsComb, restCombList, restMaterialDic);
        }

        private bool isInherit(InheritanceFactorModel inheritanceFactor, InheritanceFactorModel tempInheritanceFactor)
        {
			//利用率不小于父组合
			if (tempInheritanceFactor.UtilizationRatio<inheritanceFactor.UtilizationRatio)
			{
				return false;
			}
			if (inheritanceFactor.UtilizationRatio<tempInheritanceFactor.UtilizationRatio)
			{
				//利用率提高可以遗传
				return true;
			}
			else
			{
				//利用率不变，最大余料提升也可以遗传
				if (inheritanceFactor.MaxRestMasterial<tempInheritanceFactor.MaxRestMasterial)
				{
					return true;
				}
				else
				{
					//利用率和最大余料不变，但是余料标准差变大也可以遗传
					if (inheritanceFactor.StandardDeviation < tempInheritanceFactor.StandardDeviation)
					{
						return true;
					}
				}
			}
			return false;
        }


        /// <summary>
        /// 单组套料方法
        /// </summary>
        /// <param name="materialList">余料清单</param>
        /// <param name="model">参数</param>
        /// <returns>(整料切割清单、拼接清单、余料字典)</returns>
        private (List<List<int>>, List<(int, List<int>)>,Dictionary<int,int>) GenerateCombination(MaterialInformationModel model)
		{
			 List<int> restMaterialList = new List<int>();//余料清单
			List<List<int>> barsComb= new List<List<int>>();//整根材料构成列表
			var materialList = model.RequireMaterials;//采用model自带下料清单
            List<int> materialListA=materialList.Where(x=> x <=model.MaterialLength).ToList();//不超过原料长度的需求材料清单
			List<int> materialListB=materialList.Where(x=>x>model.MaterialLength).ToList();//超过原料长度的需求材料清单
            //添加前部分既有余料
            foreach (var restMaterial in model.RestMaterials)
            {
                if (restMaterial.Key >= model.RestMaterialLengthLimited)
                {
                    restMaterialList.AddRange(Enumerable.Repeat(restMaterial.Key, restMaterial.Value));
                }
            }
            int rawBarsCount=Convert.ToInt16(Math.Ceiling((decimal)(materialListA.Sum() - restMaterialList.Sum())/model.MaterialLength));//生料数量
			int n = 0;
			//原料切割
			for (int i = 0; i < rawBarsCount; i++)
			{
				List<int> barComb = new List<int>();
				int z = 0;
				while ((z+ materialListA[n]) <= model.MaterialLength)
				{
					barComb.Add(materialListA[n]);
					z += materialListA[n];
					n++;
					if (n>materialListA.Count()-1)
					{
						break;
					}
				}
				barsComb.Add(barComb);
			}
			//余料拼装
			int combTotalNum = 0;//拼接总数计数
			int connectNumberLimited=model.ConnectNumberLimited;
			restMaterialList.AddRange((barsComb.Select(x => model.MaterialLength - x.Sum())).Where(x=>x>model.RestMaterialLengthLimited));//添加本部分原料切割余料
            restMaterialList.Sort();//余料清单（从小到大）
			List<int> restRequireList=materialListA.Skip(n).ToList();
			restRequireList.AddRange(materialListB);//需求清单加入比原材料长的构件
			restRequireList.Sort();//下料清单（从小到大）
			List<(int, List<int>)> restCombList = new List<(int, List<int>)>();//拼装料清单
			while (restRequireList.Count > 0)
			{
				for (int i = restRequireList.Count - 1; i >= 0; i--)
				{
					var restRequire = restRequireList[i];
					if (combTotalNum>=model.ConnectTotalNumberLimited)
					{
						model.ConnectNumberLimited = 0;
					}
					var (bestComb, lestRestLength) = getBestComb(restRequire, restMaterialList, model);//最少余料组合序号
					if (bestComb.Count() == 0)
					{
						continue;
					}
					restCombList.Add((restRequire, bestComb.Select(x => restMaterialList[x]).ToList()));
					for (global::System.Int32 j = bestComb.Count() - (1); j >= 0; j--)
					{
						restMaterialList.RemoveAt(bestComb[j]);//余料列表中去除已使用余料
					}
                    restRequireList.RemoveAt(i);//下料清单中去处已完成下料
                    if (lestRestLength>=model.RestMaterialLengthLimited)
					{
                        restMaterialList.Add(lestRestLength);
                    }
					combTotalNum++;
				}
				//当还有需求没有处理时，再加一根原料
				if (restRequireList.Count()>0)
				{
					restMaterialList.Add(model.MaterialLength);
				}
			};
			model.ConnectNumberLimited=connectNumberLimited;
            //剩余材料转字典；
			Dictionary<int,int> restMaterialDic = new Dictionary<int,int>();
            for (int i = restMaterialList.Count - 1; i >= 0; i--)
            {
				if (restMaterialDic.Where(x => x.Key == restMaterialList[i]).Count()==0)
				{
					restMaterialDic.Add(restMaterialList[i], 1);
				}
				else
				{
					restMaterialDic[restMaterialList[i]]++;
				}
            }
            return (barsComb, restCombList,restMaterialDic);

		}
		/// <summary>
		/// 在余料中寻找需要单根下料的最优拼接方式
		/// </summary>
		/// <param name="restRequire"></param>
		/// <param name="restMaterialList"></param>
		/// <param name="connectNumberLimited"></param>
		/// <returns></returns>
		private (int[],int) getBestComb(int restRequire, List<int> restMaterialList, MaterialInformationModel model)
		{
			int[] bestComb=new int[] { };//最少余料组合序号
			int lestRestLength = model.MaterialLength;
            if (restMaterialList.Count!=0)
			{
                lestRestLength = restMaterialList.Max();//最少余料长度
            }
            if (model.isIncludeRawBars)
			{
				//限值含原料拼接
                for (int i = 0; i <= Math.Min(model.ConnectNumberLimited, restMaterialList.Count() - 1); i++)
                {
                    var combs = PermutationAndCombination<List<int>>.GetCombinationnum(restMaterialList.ToArray(), i + 1);
                    var restCombs = combs.Select(x => x.Sum(y => restMaterialList[y]) - restRequire);
                    if (restCombs.Count(x => x >= 0) == 0)
                    {
                        continue;
                    }
                    var lestRestLengthTemp = restCombs.Where(x => x >= 0).Min();
                    var bestCombTemp = combs[restCombs.ToList().IndexOf(lestRestLengthTemp)];
                    if (i == 0)
                    {
                        bestComb = bestCombTemp;
                        lestRestLength = lestRestLengthTemp;
                    }
                    else
                    {
                        if (lestRestLengthTemp < lestRestLength)
                        {
                            bestComb = bestCombTemp;
                            lestRestLength = lestRestLengthTemp;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                if (bestComb.Count()==0&&restMaterialList.Where(x => x == model.MaterialLength).Sum() >= restRequire)
                {
                    throw new ArgumentException("拼接限值太少，无法满足要求！");
                }
            }
			else
			{
				//限值不含原料拼接
				//var noRawBarsList = restMaterialList.Where(x=>x!=model.MaterialLength).ToList();
				var RawBarsList = restMaterialList.Where(x => x == model.MaterialLength).ToList();
				for (int i = 0; i <= Math.Min(model.ConnectNumberLimited+RawBarsList.Count, restMaterialList.Count() - 1); i++)
                {
                    var combs = PermutationAndCombination<List<int>>.GetCombinationnum(restMaterialList.ToArray(), i + 1);
                    for (global::System.Int32 j = combs.Count - (1); j >= 0; j--)
                    {
						//删除不含原材料拼接次数超过限值的组合
						if(combs[j].ToList().Where(x => restMaterialList[x] != model.MaterialLength).Count() > model.ConnectNumberLimited + 1)
						combs.RemoveAt(j);
					}
                    var restCombs = combs.Select(x => x.Sum(y => restMaterialList[y]) - restRequire).ToList();
                    if (restCombs.Count(x => x >= 0) == 0)
                    {
                        continue;
                    }
                    //int usedRawBarsCount = 0;
                    //while (restCombs.Count(x => x >= 0) ==0)
                    //{
                    //	for (int j = 0; j < restCombs.Count; j++)
                    //	{
                    //		restCombs[j] += model.MaterialLength;
                    //	}
                    //	usedRawBarsCount++;
                    //}
                    //if (usedRawBarsCount>=RawBarsList.Count)
                    //{
                    //	continue;
                    //}
                    var lestRestLengthTemp = restCombs.Where(x => x >= 0).Min();
                    var bestCombTemp = combs[restCombs.ToList().IndexOf(lestRestLengthTemp)];
                    if (i == 0)
                    {
                        bestComb = bestCombTemp;
                        lestRestLength = lestRestLengthTemp;
                    }
                    else
                    {
                        if (lestRestLengthTemp < lestRestLength)
                        {
                            bestComb = bestCombTemp;
                            lestRestLength = lestRestLengthTemp;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
				
            }
			return (bestComb,lestRestLength);
		}
		/// <summary>
		/// 遗传变异
		/// </summary>
		/// <param name="materialList"></param>
		/// <exception cref="NotImplementedException"></exception>
        private List<int> CombInheritance(List<int> materialList)
        {
			return new List<int>();
        }
		/// <summary>
		/// 根据套料结果计算利用率
		/// </summary>
		/// <param name="barsComb"></param>
		/// <param name="restCombList"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
        private double GetUtilizationRatio(List<List<int>> barsComb, List<(int, List<int>)> restCombList, int materialLength)
        {
			int usedMaterialLength = 0;
			int finishedRequireLength = 0;
			usedMaterialLength+=barsComb.Count* materialLength;
			usedMaterialLength += restCombList.Select(x => x.Item2.Where(y => y == materialLength).Sum()).Sum();
			finishedRequireLength = barsComb.Sum(x => x.Sum()) + restCombList.Sum(x => x.Item1);
			if (finishedRequireLength==0)
			{
				return 0;
			}
			else
			{
                return (double)finishedRequireLength/ (double)usedMaterialLength;
            }
        }
    }
}

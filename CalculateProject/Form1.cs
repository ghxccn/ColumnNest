using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ColumnNest.Function;
using ColumnNest.Model;

namespace CalculateProject
{
	public partial class Form1 : Form
	{
		MaterialInformationModel materialParam=new MaterialInformationModel();
		public Form1()
		{
			InitializeComponent();
		}
		List<RequireMaterial> requireList1;
		List<RequireMaterial> requireList2;
		List<RequireMaterial> requireList3;
        private void Form1_Load(object sender, EventArgs e)
		{
            requireList1 = new List<RequireMaterial>()
            { 
				new RequireMaterial{RequireMaterialLength=6000,Count=8 },
                new RequireMaterial{RequireMaterialLength=182,Count=7 },
                new RequireMaterial{RequireMaterialLength=731,Count=8 },
                new RequireMaterial{RequireMaterialLength=235,Count=22 },
                new RequireMaterial{RequireMaterialLength=4731,Count=13 },
                new RequireMaterial{RequireMaterialLength=3731,Count=8 },
                new RequireMaterial{RequireMaterialLength=2731,Count=15 },
                new RequireMaterial{RequireMaterialLength=1731,Count=8 },
                new RequireMaterial{RequireMaterialLength=3871,Count=5 },
                new RequireMaterial{RequireMaterialLength=1281,Count=8 }
            };
            dataGridView1.DataSource = requireList1;
            requireList2 = new List<RequireMaterial>()
			{
                new RequireMaterial{RequireMaterialLength=1830,Count=0 },
                new RequireMaterial{RequireMaterialLength=2379,Count=0 },
                new RequireMaterial{RequireMaterialLength=2928,Count=0 },
                new RequireMaterial{RequireMaterialLength=5170,Count=0 }
            };
			dataGridView2.DataSource=requireList2;
            requireList3 = new List<RequireMaterial>()
            {
                new RequireMaterial{RequireMaterialLength=6000,Count=0 },
                new RequireMaterial{RequireMaterialLength=2173,Count=0 },
                new RequireMaterial{RequireMaterialLength=2074,Count=0 },
                new RequireMaterial{RequireMaterialLength=1800,Count=0 }
            };
            dataGridView3.DataSource = requireList3;
        }

		private void button1_Click(object sender, EventArgs e)
		{
			listBox1.Items.Clear();
			var (model1,model2,model3)=GetModelParam();
			NestFunction function=new NestFunction();
            
            List<MaterialInformationModel> modelList= new List<MaterialInformationModel>() {model1,model2,model3 };
			var(barsComb, restCombList,restMaterialDic) =function.NestAchieve(modelList);
            var barsDic=DataMerge(barsComb, restCombList,modelList);
            //显示结果
            listBox1.Items.Add("套料结果：");
            publishResult(barsDic, restCombList);
            listBox1.Items.Add("剩余材料：");
            foreach (var restMaterial in restMaterialDic)
            {
                listBox1.Items.Add($"{restMaterial.Key}x{restMaterial.Value}");
            }
        }



        private (MaterialInformationModel,MaterialInformationModel,MaterialInformationModel) GetModelParam()
		{
			MaterialInformationModel model1 = new MaterialInformationModel();
            MaterialInformationModel model2 = new MaterialInformationModel();
            MaterialInformationModel model3 = new MaterialInformationModel();
            model1.MaterialLength = Convert.ToInt32(textBox1.Text);
            model1.Connectable = checkBox1.Checked;
            model1.ConnectNumberLimited = Convert.ToInt32(textBox2.Text);
            model1.ConnectTotalNumberLimited = Convert.ToInt32(textBox3.Text);
            model1.RestMaterialLengthLimited = Convert.ToInt32(textBox4.Text);
            if (radioButton1.Checked)
            {
                model1.isIncludeRawBars = false;
            }
            else
            {
                model1.isIncludeRawBars = true;
            }
            requireList1.ForEach(x => model1.RequireMaterials.AddRange(Enumerable.Repeat(x.RequireMaterialLength,x.Count).ToList()));
            model2.MaterialLength = Convert.ToInt32(textBox1.Text);
            model2.Connectable = checkBox2.Checked;
            model2.ConnectNumberLimited = Convert.ToInt32(textBox5.Text);
            model2.ConnectTotalNumberLimited = Convert.ToInt32(textBox6.Text);
            model2.RestMaterialLengthLimited = Convert.ToInt32(textBox7.Text);
            if (radioButton3.Checked)
            {
                model2.isIncludeRawBars = false;
            }
            else
            {
                model2.isIncludeRawBars = true;
            }
            requireList2.ForEach(x => model2.RequireMaterials.AddRange(Enumerable.Repeat(x.RequireMaterialLength, x.Count).ToList()));
            model3.MaterialLength = Convert.ToInt32(textBox1.Text);
            model3.Connectable = checkBox3.Checked;
            model3.ConnectNumberLimited = Convert.ToInt32(textBox8.Text);
            model3.ConnectTotalNumberLimited = Convert.ToInt32(textBox9.Text);
            model3.RestMaterialLengthLimited = Convert.ToInt32(textBox10.Text);
            if (radioButton5.Checked)
            {
                model3.isIncludeRawBars = false;
            }
            else
            {
                model3.isIncludeRawBars = true;
            }
            requireList3.ForEach(x => model3.RequireMaterials.AddRange(Enumerable.Repeat(x.RequireMaterialLength, x.Count).ToList()));
            return (model1,model2,model3);
		}
        private Dictionary<List<int>, int> DataMerge(List<List<int>> barsComb, List<(int, List<int>)> restCombList,List<MaterialInformationModel> modelList)
        {
            Dictionary<List<int>,int> barsDic= new Dictionary<List<int>,int>();
            //restCombList单拼结果归并
            var tempList1=restCombList.Where(x => x.Item2.Count == 1 && x.Item2[0] == 6000);//拼接中用单根原料切的移入barsComb
            barsComb.AddRange(tempList1.Select(x =>new List<int>() { x.Item1 }));
            tempList1.ToList().ForEach(x=>restCombList.Remove(x));
            while (CutFromRest(restCombList,barsComb,modelList)!=default)//余料整切并入barsComb
            {
                var tempItem = CutFromRest(restCombList, barsComb, modelList);
                barsComb.Where(x => (modelList[0].MaterialLength - x.Sum()) == tempItem.Item2[0]).FirstOrDefault().Add(tempItem.Item1);
                restCombList.Remove(tempItem);
            }
            //barsComb结果归并
            foreach (var comb in barsComb)
            {
                comb.Sort();
                if (barsDic.Keys.ToList().Any(x=>x.SequenceEqual(comb)))
                {
                    barsDic[barsDic.Keys.ToList().Where(x => x.SequenceEqual(comb)).FirstOrDefault()]++;
                }
                else
                {
                    barsDic.Add(comb,1);
                }
            }
            return barsDic;
        }
        /// <summary>
        /// 在restCombList里找用了barsComb余料的非拼接零件，并返回其中的第一件
        /// </summary>
        /// <param name="restCombList"></param>
        /// <param name="barsComb"></param>
        /// <param name="modelList"></param>
        /// <returns></returns>
        private (int,List<int>) CutFromRest(List<(int, List<int>)> restCombList, List<List<int>> barsComb,List<MaterialInformationModel> modelList)
        {
            var qulifyList =restCombList.Where(x => x.Item2.Count == 1 && barsComb.Select(y => modelList[0].MaterialLength - y.Sum()).ToList().Contains(x.Item2[0]));
            return qulifyList.FirstOrDefault();
        }

        private void publishResult(Dictionary<List<int>, int> barsDic,List<(int,List<int>)> restCombList)
		{
            listBox1.Items.Add("整件：");
            int materialSum = 0;//原料消耗长度
            int requireSum = 0;//完成下料需求长度
            foreach (var barCombs in barsDic)
            {
                string barCombStr = "";
                barCombStr += $"{barCombs.Value}组：";
                var barComb = barCombs.Key;
                foreach (var comb in barComb)
                {
                    barCombStr += comb;
                    barCombStr += ",";
                    requireSum += comb*barCombs.Value;
                }
                barCombStr.Remove(barCombStr.Length - 1);
                barCombStr += "<<==6000，余料";
                barCombStr += 6000 - barComb.Sum();
                materialSum += 6000*barCombs.Value;
                listBox1.Items.Add(barCombStr);
            }
            foreach (var restComb in restCombList)
            {
                if (restComb.Item2.Count==1)
                {
                    string restCombStr = "";
                    restCombStr += restComb.Item1;
                    restCombStr += "<=";
                    foreach (var comb in restComb.Item2)
                    {
                        restCombStr += comb;
                        restCombStr += ",";
                    }
                    requireSum += restComb.Item1;
                    materialSum += restComb.Item2.Where(x => x == 6000).Sum();
                    restCombStr += ",余料";
                    restCombStr += restComb.Item2.Sum() - restComb.Item1;
                    restCombStr.Remove(restCombStr.Length - 1);
                    listBox1.Items.Add(restCombStr);
                }
            }
            listBox1.Items.Add("拼接件：");
            foreach (var restComb in restCombList)
            {
                if (restComb.Item2.Count > 1)
                {
                    string restCombStr = "";
                    restCombStr += restComb.Item1;
                    restCombStr += "<=";
                    foreach (var comb in restComb.Item2)
                    {
                        restCombStr += comb;
                        restCombStr += ",";
                    }
                    requireSum += restComb.Item1;
                    materialSum += restComb.Item2.Where(x => x == 6000).Sum();
                    restCombStr += ",余料";
                    restCombStr += restComb.Item2.Sum() - restComb.Item1;
                    restCombStr.Remove(restCombStr.Length - 1);
                    listBox1.Items.Add(restCombStr);
                }
            }
            listBox1.Items.Add("~~~~~~~~~~~~~~~~~~~~~~~~");
            listBox1.Items.Add("总计：");
            listBox1.Items.Add($"本次消耗原材料{materialSum}mm");
            listBox1.Items.Add($"本次完成下料{requireSum}mm");
            listBox1.Items.Add($"材料利用率为{Math.Round((double)requireSum / materialSum * 100, 2)}%");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                panel1.Enabled = true;
            }
            else
            {
                panel1.Enabled = false;
                textBox2.Text = "0";
                radioButton1.Checked = true;
            }
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                panel2.Enabled = true;
            }
            else
            {
                panel2.Enabled = false;
                textBox5.Text = "0";
                radioButton3.Checked = true;
            }
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                panel3.Enabled = true;
            }
            else
            {
                panel3.Enabled = false;
                textBox8.Text = "0";
                radioButton5.Checked = true;
            }
        }
    }
    public class RemainMaterial
	{
        public int RemainMaterialLength { get; set; }
		public int Count { get; set; }
	}
	public class RequireMaterial
	{
		public int RequireMaterialLength { get; set; }
		public int Count { get; set; }
	}
}

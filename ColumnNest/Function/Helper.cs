using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColumnNest.Function
{
	/// <summary>
	/// 排列组合
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class PermutationAndCombination<T>
	{
		/// <summary>
		/// 交换两个变量
		/// </summary>
		/// <param name="a">变量1</param>
		/// <param name="b">变量2</param>
		public static void Swap(ref T a, ref T b)
		{
			T temp = a;
			a = b;
			b = temp;
		}
		/// <summary>
		/// 递归算法求数组的组合(私有成员)
		/// </summary>
		/// <param name="list">返回的范型</param>
		/// <param name="t">所求数组</param>
		/// <param name="n">辅助变量</param>
		/// <param name="m">辅助变量</param>
		/// <param name="b">辅助数组</param>
		/// <param name="M">辅助变量M</param>
		private static void GetCombination(ref List<T[]> list, T[] t, int n, int m, int[] b, int M)
		{
			for (int i = n; i >= m; i--)
			{
				b[m - 1] = i - 1;
				if (m > 1)
				{
					GetCombination(ref list, t, i - 1, m - 1, b, M);
				}
				else
				{
					if (list == null)
					{
						list = new List<T[]>();
					}
					T[] temp = new T[M];
					for (int j = 0; j < b.Length; j++)
					{
						temp[j] = t[b[j]];
					}
					list.Add(temp);
				}
			}
		}
		/// <summary>
		/// 递归算法求排列(私有成员)
		/// </summary>
		/// <param name="list">返回的列表</param>
		/// <param name="t">所求数组</param>
		/// <param name="startIndex">起始标号</param>
		/// <param name="endIndex">结束标号</param>
		private static void GetPermutation(ref List<T[]> list, T[] t, int startIndex, int endIndex)
		{
			if (startIndex == endIndex)
			{
				if (list == null)
				{
					list = new List<T[]>();
				}
				T[] temp = new T[t.Length];
				t.CopyTo(temp, 0);
				list.Add(temp);
			}
			else
			{
				for (int i = startIndex; i <= endIndex; i++)
				{
					Swap(ref t[startIndex], ref t[i]);
					GetPermutation(ref list, t, startIndex + 1, endIndex);
					Swap(ref t[startIndex], ref t[i]);
				}
			}
		}
		/// <summary>
		/// 求从起始标号到结束标号的排列，其余元素不变
		/// </summary>
		/// <param name="t">所求数组</param>
		/// <param name="startIndex">起始标号</param>
		/// <param name="endIndex">结束标号</param>
		/// <returns>从起始标号到结束标号排列的范型</returns>
		public static List<T[]> GetPermutation(T[] t, int startIndex, int endIndex)
		{
			if (startIndex < 0 || endIndex > t.Length - 1)
			{
				return null;
			}
			List<T[]> list = new List<T[]>();
			GetPermutation(ref list, t, startIndex, endIndex);
			return list;
		}
		/// <summary>
		/// 返回数组所有元素的全排列
		/// </summary>
		/// <param name="t">所求数组</param>
		/// <returns>全排列的范型</returns>
		public static List<T[]> GetPermutation(T[] t)
		{
			return GetPermutation(t, 0, t.Length - 1);
		}
		/// <summary>
		/// 求数组中n个元素的排列
		/// </summary>
		/// <param name="t">所求数组</param>
		/// <param name="n">元素个数</param>
		/// <returns>数组中n个元素的排列</returns>
		public static List<T[]> GetPermutation(T[] t, int n)
		{
			if (n > t.Length)
			{
				return null;
			}
			List<T[]> list = new List<T[]>();
			List<T[]> c = GetCombination(t, n);
			for (int i = 0; i < c.Count; i++)
			{
				List<T[]> l = new List<T[]>();
				GetPermutation(ref l, c[i], 0, n - 1);
				list.AddRange(l);
			}
			return list;
		}
		/// <summary>
		/// 求数组中n个元素的组合
		/// </summary>
		/// <param name="t">所求数组</param>
		/// <param name="n">元素个数</param>
		/// <returns>数组中n个元素的组合的范型</returns>
		public static List<T[]> GetCombination(T[] t, int n)
		{
			if (t.Length < n)
			{
				return null;
			}
			int[] temp = new int[n];
			List<T[]> list = new List<T[]>();
			GetCombination(ref list, t, t.Length, n, temp, n);
			return list;
		}

		private static void GetCombinationnum(ref List<int[]> list, int[] t, int n, int m, int[] b, int M)
		{
			for (int i = n; i >= m; i--)
			{
				b[m - 1] = i - 1;
				if (m > 1)
				{
					GetCombinationnum(ref list, t, i - 1, m - 1, b, M);
				}
				else
				{
					if (list == null)
					{
						list = new List<int[]>();
					}
					int[] temp = new int[M];
					for (int j = 0; j < b.Length; j++)
					{
						temp[j] = b[j];
					}
					list.Add(temp);
				}
			}
		}
		public static List<int[]> GetCombinationnum(int[] t, int n)
		{
			if (t.Length < n)
			{
				return null;
			}
			int[] temp = new int[n];
			List<int[]> list = new List<int[]>();
			GetCombinationnum(ref list, t, t.Length, n, temp, n);
			list.Reverse();
			return list;
		}


	}
	/// <summary>
	/// 数学工具
	/// </summary>
	public static class MathTool
	{
        /// <summary>
        /// 求方差
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double CalcuateVariance(List<int> list)
        {
            if (list == null||list.Count==0)
            {
                return 0;
            }
            double meanValue = list.Average();
            return list.Select(x =>
            {
                return Math.Pow(x - meanValue, 2);
            }).Average();
        }
        public static double CalcuateVariance(List<double> list)
		{
			if (list == null)
			{ 
				return 0;
			}
			double meanValue = list.Average();
			return list.Select(x =>
			{
				return Math.Pow(x - meanValue, 2);
			}).Average();
		}
		/// <summary>
		/// 求标准差
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static double CalculateStandardDeviation(List<double> list)
		{ 
			return Math.Sqrt(CalcuateVariance(list));
		}
	}
	public static class EnumTool
	{
		/// <summary>
		/// 计数字典展开成数列
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dic"></param>
		/// <returns></returns>
		public static List<T> DicExpandToList<T>(this Dictionary<T,int> dic)
		{ 
			List<T> list = new List<T>();
			foreach (var pairs in dic)
			{
				list.AddRange(Enumerable.Repeat(pairs.Key, pairs.Value));
			}
			return list;
		}
		public static Dictionary<T, int> MergeDic<T>(this Dictionary<T, int> dic1, Dictionary<T, int> dic2)
		{ 
			Dictionary<T,int> mergeResult= new Dictionary<T,int>();
            foreach (var keypair1 in dic1)
            {
				mergeResult.Add(keypair1.Key, keypair1.Value);
            }
			foreach (var keypair2 in dic2)
			{
				if (dic1.ContainsKey(keypair2.Key))
				{
                    dic1[keypair2.Key] += keypair2.Value;
				}
				else
				{
                    dic1.Add(keypair2.Key, keypair2.Value);
				}
			}
			return dic1;
        }
    }
}

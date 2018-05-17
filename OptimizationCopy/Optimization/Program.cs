using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using muWrapper;
using NPlot;

namespace Optimization
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return;
            NMOptimizer opt = new NMOptimizer();
            NMInitialParams ip = new NMInitialParams(1, 0.5, 2, 0.001,
                new string[] { "x", "y" },
                new string[] { "x", "y" },
                "x ^ 2 + y ^ 2");
            opt.Initialize(ip);
            List<NMIteration> listIter = new List<NMIteration>();
            NMIteration it = new NMIteration(new double[][] { new double[]{ 1, 2 }, new double[]{ 5, 6 }, new double[]{ 7, 10 } });
            it.CalcFuncAndResult(ip);
            do
            {
                listIter.Add(it);
                it = (NMIteration)opt.DoIteration(it);
            }
            while (it != null);

            HJOptimizer opt2 = new HJOptimizer();
            HJInitialParams ip2 = new HJInitialParams(0.01, 0.1,
                new string[] { "x", "y" }, new string[] { "x", "y" }, 
                "x * x + 2 * x + 1 + y * y + 2 * y + 1");
            opt2.Initialize(ip2);
            List<HJIteration> listIter2 = new List<HJIteration>();
            HJIteration it2 = new HJIteration(new double[] { 20, 20 }, new double[] { 0.1, 0.1 }, null, 1);
            it2.CalcResult(ip2);
            do
            {
                listIter2.Add(it2);
                it2 = (HJIteration)opt2.DoIteration(it2);
            }
            while (it2 != null);            
        }
    }
    public interface IOptimizer
    {
        void Initialize(IInitialParams initParams);
        IIteration DoIteration(IIteration prevIter);
    }
    public interface IInitialParams
    {
        double GetFuncValue(double[] arrX);
        double GetDerivative(double[] arrX, int index);
        string ToHtml(int d);
    }
    public interface IIteration
    {
        string ToHtml(IInitialParams initParams, int d);
    }
    public class NMInitialParams : IInitialParams
    {
        public double alpha, beta, gamma, epsilon;
        Parser prs;
        ParserVariable[] arrVar;
        string[] arrName, arrHtmlName;
        public NMInitialParams(double alpha, double beta,
            double gamma, double epsilon,
            string[] arrName, string[] arrHtmlName,
            string expression)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.gamma = gamma;
            this.epsilon = epsilon;
            this.arrName = arrName;
            this.arrHtmlName = arrHtmlName;
            prs = new Parser();
            arrVar = new ParserVariable[arrName.Length];
            for (int i = 0; i < arrName.Length; i++)
            {
                arrVar[i] = new ParserVariable(0);
                prs.DefineVar(arrName[i], arrVar[i]);
            }
            prs.SetExpr(expression);
        }
        public double GetFuncValue(double[] arrX)
        {
            if (arrX.Length != arrVar.Length)
                throw new Exception();
            for (int i = 0; i < arrX.Length; i++)
                arrVar[i].Value = arrX[i];
            return prs.Eval();
        }
        public double GetDerivative(double[] arrX, int index)
        {
            double xOld = arrX[index];
            double f1 = GetFuncValue(arrX);
            double eps = 0.001;
            arrX[index] += eps;
            double f2 = GetFuncValue(arrX);
            arrX[index] = xOld;
            return (f2 - f1) / eps;
        }
        public string ToHtml(int d)
        {
            return string.Format("<P>ПАРАМЕТРЫ<BR>" +
                "Количество переменных: n = {0}<BR>" +
                "Коэффициент отражения: α = {1}<BR>" +
                "Коэффициент сжатия: β = {2}<BR>" +
                "Коэффициент растяжения: γ = {3}<BR>" +
                "Точность: ε = {4}<BR></P>",
                arrVar.Length, Math.Round(alpha, d), Math.Round(beta, d),
                Math.Round(gamma, d), Math.Round(epsilon, d));
        }        
    }
    public class NMIteration : IIteration
    {
        public double[][] matrX;    // в строках - коорд. точек симпл.
        public double[] arrF;
        public int h, g, l;         // индексы
        public double fh, fg, fl, f0, fr, fe, fc, fRes;
        public double[] arrX0, arrXr, arrXe, arrXc, arrXRes;
        public double fAverage, sigmaPow2, sigma;
        public NMIteration(double[][] matrXToClone)
        {
            matrX = new double[matrXToClone.Length][];
            for (int i = 0; i < matrXToClone.Length; i++)
                matrX[i] = (double[])matrXToClone[i].Clone();
        }
        public void CalcFuncAndResult(NMInitialParams ip)
        {
            arrF = new double[matrX.Length];
            for (int i = 0; i < arrF.Length; i++)
                arrF[i] = ip.GetFuncValue(matrX[i]);
            CalcResult(ip);
        }
        public void CalcResult(NMInitialParams ip)
        {
            double fMax = float.MinValue;
            int index = -1;

            for (int i = 0; i < arrF.Length; i++)
                if (arrF[i] > fMax)
                {
                    fMax = arrF[i];
                    index = i;
                }

            int n = arrF.Length - 1;
            arrXRes = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < matrX.Length; j++)
                {
                    if (j == index)
                        continue;
                    arrXRes[i] += matrX[j][i];
                }
                arrXRes[i] /= (matrX.Length - 1);
            }
            fRes = ip.GetFuncValue(arrXRes);
        }
        public string ToHtml(IInitialParams initParams, int d)
        {
            return string.Format("<P>Значение целевой функции:<BR>{0}<BR>" +
                "Текущее решение:<BR>{1}<BR>" +
                "Симплекс:<BR>{2}<BR>" +
                "Среднее квадратическое отклонение:<BR>{3}</P>",
                Math.Round(fRes, d), Html.ArrayToHtml(arrXRes, d),
                Html.MatrixTranToHtml(matrX, d), Math.Round(sigma, d));
        }
        public override string ToString()
        {
            return "F = " + fRes.ToString();
        }       
    }
    public class NMOptimizer : IOptimizer
    {
        NMInitialParams ip;
        NMIteration it;
        public void Initialize(IInitialParams initParams)
        {
            ip = (NMInitialParams)initParams;
        }
        public IIteration DoIteration(IIteration prevIter)
        {
            it = (NMIteration)prevIter;
            for (int i = 0; i < it.matrX.Length; i++)
                if (it.matrX[i].Length + 1 != it.matrX.Length)
                    throw new Exception();
            int n = it.matrX[0].Length;
            it.arrF = new double[n + 1];
            for (int i = 0; i < n + 1; i++)
                it.arrF[i] = ip.GetFuncValue(it.matrX[i]);
            it.fg = it.fh = float.MinValue;
            it.fl = float.MaxValue;
            for (int i = 0; i < n + 1; i++)
            {
                if (it.arrF[i] > it.fh)
                {
                    it.fh = it.arrF[i];
                    it.h = i;
                }
                if (it.arrF[i] > it.fg && it.arrF[i] < it.fh)
                {
                    it.fg = it.arrF[i];
                    it.g = i;
                }
                if (it.arrF[i] < it.fl)
                {
                    it.fl = it.arrF[i];
                    it.l = i;
                }
            }

            // центр
            it.arrX0 = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    if (j == it.h)
                        continue;
                    it.arrX0[i] += it.matrX[j][i];
                }
                it.arrX0[i] /= n;
            }
            it.f0 = ip.GetFuncValue(it.arrX0);

            // отражение
            it.arrXr = new double[n];
            for (int i = 0; i < n; i++)
                it.arrXr[i] = (1 + ip.alpha) * it.arrX0[i] - ip.alpha * it.matrX[it.h][i];
            it.fr = ip.GetFuncValue(it.arrXr);

            if (it.fr < it.fl)
            {
                // растяжение
                it.arrXe = new double[n];
                for (int i = 0; i < n; i++)
                    it.arrXe[i] = ip.gamma * it.arrXr[i] + (1 - ip.gamma) * it.arrX0[i];
                it.fe = ip.GetFuncValue(it.arrXe);

                if (it.fe < it.fl)
                {
                    it.matrX[it.h] = (double[])it.arrXe.Clone();
                    it.CalcResult(ip);
                    if (!CheckIsDone())
                        return new NMIteration(it.matrX);
                    else
                        return null;
                }

                it.matrX[it.h] = (double[])it.arrXr.Clone();
                it.CalcResult(ip);
                if (!CheckIsDone())
                    return new NMIteration(it.matrX);
                else
                    return null;
            }

            if (it.fr <= it.fg)
            {
                it.matrX[it.h] = (double[])it.arrXr.Clone();
                it.CalcResult(ip);
                if (!CheckIsDone())
                    return new NMIteration(it.matrX);
                else
                    return null;
            }

            if (it.fr <= it.fh)
                it.matrX[it.h] = (double[])it.arrXr.Clone();

            // сжатие
            it.arrXc = new double[n];
            for (int i = 0; i < it.arrXc.Length; i++)
                it.arrXc[i] = ip.beta * it.matrX[it.h][i] + (1 - ip.beta) * it.arrX0[i];
            it.fc = ip.GetFuncValue(it.arrXc);

            if (it.fc > it.fh)
            {
                for (int i = 0; i < n + 1; i++)
                    for (int j = 0; j < n; j++)
                        it.matrX[i][j] = 0.5 * (it.matrX[i][j] + it.matrX[it.l][j]);
                it.CalcResult(ip);
                if (!CheckIsDone())
                    return new NMIteration(it.matrX);
                else
                    return null;
            }

            it.matrX[it.h] = (double[])it.arrXc.Clone();
            it.CalcResult(ip);
            if (!CheckIsDone())
                return new NMIteration(it.matrX);
            else
                return null;
        }
        bool CheckIsDone()
        {            
            it.fAverage = 0;
            for (int i = 0; i < it.arrF.Length; i++)
                it.fAverage += it.arrF[i];
            it.fAverage = it.fAverage / it.arrF.Length;

            it.sigmaPow2 = 0;
            for (int i = 0; i < it.arrF.Length; i++)
                it.sigmaPow2 += (it.arrF[i] - it.fAverage) *
                    (it.arrF[i] - it.fAverage);
            it.sigmaPow2 /= it.arrF.Length;
            it.sigma = Math.Sqrt(it.sigmaPow2);
            if (it.sigma < ip.epsilon)
                return true;
            return false;
        }        
    }

    public class HJInitialParams : IInitialParams
    {
        public double xEpsilon, fEpsilon;
        Parser prs;
        ParserVariable[] arrVar;
        string[] arrName, arrHtmlName;        
        public HJInitialParams(double xEpsilon, double fEpsilon,
            string[] arrName, string[] arrHtmlName,
            string expression)
        {
            this.xEpsilon = xEpsilon;
            this.fEpsilon = fEpsilon;
            this.arrName = arrName;
            this.arrHtmlName = arrHtmlName;
            prs = new Parser();
            arrVar = new ParserVariable[arrName.Length];
            for (int i = 0; i < arrName.Length; i++)
            {
                arrVar[i] = new ParserVariable(0);
                prs.DefineVar(arrName[i], arrVar[i]);
            }
            prs.SetExpr(expression);
        }
        public double GetFuncValue(double[] arrX)
        {
            if (arrX.Length != arrVar.Length)
                throw new Exception();
            for (int i = 0; i < arrX.Length; i++)
                arrVar[i].Value = arrX[i];
            return prs.Eval();
        }
        public double GetDerivative(double[] arrX, int index)
        {
            double xOld = arrX[index];
            double f1 = GetFuncValue(arrX);
            double eps = 0.001;
            arrX[index] += eps;
            double f2 = GetFuncValue(arrX);
            arrX[index] = xOld;
            return (f2 - f1) / eps;
        }
        public string ToHtml(int d)
        {
            return string.Format("<P>ПАРАМЕТРЫ<BR>" +
                "Количество переменных: {0}<BR>" +
                "Точность X: {1}<BR>" +
                "Точность F: {2}<BR>",
                arrVar.Length, Math.Round(xEpsilon, d), Math.Round(fEpsilon, d));
        }
    }
    public class HJIteration : IIteration
    {
        public double[] arrX, arrE; // arrE - направление
        public double[] arrXDelta;
        public double mult;         // mult - множитель в поиске по обр.
        public double fRes;
        public HJIteration(double[] arrXToClone, double[] arrXDeltaToClone)
        {
            arrX = (double[])arrXToClone.Clone();
            arrXDelta = (double[])arrXDeltaToClone.Clone();
            arrE = null;
            mult = 1;
        }
        public HJIteration(double[] arrXToClone, double[] arrXDeltaToClone,
            double[] arrEToClone, double mult)
        {
            arrX = (double[])arrXToClone.Clone();            
            arrXDelta = (double[])arrXDeltaToClone.Clone();
            if (arrEToClone != null)
                arrE = (double[])arrEToClone.Clone();
            this.mult = mult;
        }
        public void CalcResult(HJInitialParams ip)
        {
            fRes = ip.GetFuncValue(arrX);
        }
        public string ToHtml(IInitialParams initParams, int d)
        {
            return string.Format("<P>Значение целевой функции:<BR>{0}<BR>" +
                "Текущее решение:<BR>{1}<BR>" +
                "Величины шагов:<BR>{2}<BR>" +
                "Направление поиска по образцу:<BR>{3}<BR>" +
                "Значение множителя в поиске по образцу:<BR>{4}</P>",
                Math.Round(fRes, d), Html.ArrayToHtml(arrX, d),
                Html.ArrayToHtml(arrXDelta, d), Html.ArrayToHtml(arrE, d),
                Math.Round(mult, d));
        }
        public override string ToString()
        {
            return "F = " + fRes.ToString();
        }        
    }
    public class HJOptimizer : IOptimizer
    {
        HJInitialParams ip;
        HJIteration it;
        public void Initialize(IInitialParams initParams)
        {
            ip = (HJInitialParams)initParams;
        }
        public IIteration DoIteration(IIteration prevIter)
        {
            it = (HJIteration)prevIter;
            double f = ip.GetFuncValue(it.arrX);
            while (it.arrE == null)
            {
                f = ip.GetFuncValue(it.arrX);
                double[] arrENext, arrXNext;
                double fNext = Research(it.arrX, out arrENext, out arrXNext);
                if (Math.Abs(fNext - f) < ip.fEpsilon)
                {
                    for (int i = 0; i < it.arrXDelta.Length; i++)
                        it.arrXDelta[i] /= 2;
                    for (int i = 0; i < it.arrXDelta.Length; i++)
                        if (it.arrXDelta[i] < ip.xEpsilon)
                        {
                            it.CalcResult(ip);
                            return null;
                        }
                }
                else
                    it.arrE = arrENext;
            }
            double[] arrXSmp = new double[it.arrX.Length];
            for (int i = 0; i < arrXSmp.Length; i++)
                arrXSmp[i] = it.arrX[i] + it.arrE[i] * it.mult;
            double fSmp = ip.GetFuncValue(arrXSmp);
            if (Math.Abs(fSmp - f) < ip.fEpsilon || fSmp > f)
            {
                it.fRes = f;
                return new HJIteration(it.arrX, it.arrXDelta, null, 1);
            }
            double[] arrXRes, arrERes;
            double fRes = Research(arrXSmp, out arrERes, out arrXRes);
            HJIteration itNext;
            if (Math.Abs(fRes - f) < ip.fEpsilon)
            {
                it.fRes = f;
                itNext = new HJIteration(it.arrX, it.arrXDelta, null, 1);
            }
            else
            {
                it.fRes = fSmp;
                itNext = new HJIteration(arrXSmp, it.arrXDelta, it.arrE, it.mult + 1);
            }
            return itNext;
        }
        double Research(double[] arrX, out double[] arrE, out double[] arrXNext)
        {
            arrE = new double[it.arrX.Length];
            arrXNext = (double[])arrX.Clone();
            for (int i = 0; i < arrE.Length; i++)
            {
                double f = ip.GetFuncValue(arrXNext);
                double xOld = arrXNext[i];
                arrXNext[i] -= it.arrXDelta[i];
                double f1 = ip.GetFuncValue(arrXNext);
                arrXNext[i] = xOld + it.arrXDelta[i];
                double f2 = ip.GetFuncValue(arrXNext);
                arrXNext[i] = xOld;
                if (f1 < f && f1 <= f2)
                    arrE[i] = -it.arrXDelta[i];
                else if (f2 < f && f2 < f1)
                    arrE[i] = it.arrXDelta[i];
                else
                    arrE[i] = 0;
                arrXNext[i] += arrE[i];
            }
            return ip.GetFuncValue(arrXNext);
        }
        /*double PowellOptimization(double lambda1, double lambdaDelta,
            double fEpsilon, double lambdaEpsilon, int maxIter)
        {
            int iterCount = 0;        
            step2: double lambda2 = lambda1 + lambdaDelta;
            double f1 = GetFuncValue(lambda1), f2 = GetFuncValue(lambda2), lambda3;
            if (f1 > f2)
                lambda3 = lambda1 + 2 * lambdaDelta;
            else
                lambda3 = lambda1 - lambdaDelta;

            List<XY> listLF = new List<XY>();
            listLF.Add(new XY(lambda1, f1));
            listLF.Add(new XY(lambda2, f2));
            listLF.Add(new XY(lambda3, GetFuncValue(lambda3)));

        step6: listLF.Sort(new XYComparerByY());
            XY xyMin = listLF[0];
            listLF.Sort();

            double denom = 2 * ((listLF[1].x - listLF[2].x) * listLF[0].y +
                (listLF[2].x - listLF[0].x) * listLF[1].y +
                (listLF[0].x - listLF[1].x) * listLF[2].y);
            if (denom == 0)
            {
                lambda1 = xyMin.x;
                if (iterCount++ > maxIter)
                    throw new Exception();
                goto step2;
            }
            double numer = (listLF[1].x * listLF[1].x - listLF[2].x * listLF[2].x) * listLF[0].y +
                (listLF[2].x * listLF[2].x - listLF[0].x * listLF[0].x) * listLF[1].y +
                (listLF[0].x * listLF[0].x - listLF[1].x * listLF[1].x) * listLF[2].y;
            XY xyOpt = new XY(numer / denom, GetFuncValue(numer / denom));
            listLF.Add(xyOpt);
            listLF.Sort();
            if (Math.Abs(xyMin.y - xyOpt.y) < fEpsilon &&
                Math.Abs(xyMin.x - xyOpt.x) < lambdaEpsilon)
                return xyOpt.x;

            XY xyNext = xyOpt;
            if (xyMin.y < xyOpt.y)
                xyNext = xyMin;

            int index = listLF.BinarySearch(xyNext);
            if (index > 0 && index < 2)
            {
                if (index == 2)
                    listLF.RemoveAt(0);
                else
                    listLF.RemoveAt(3);
                if (iterCount++ > maxIter)
                    throw new Exception();
                goto step6;
            }

            lambda1 = xyNext.x;
            if (iterCount++ > maxIter)
                throw new Exception();
            goto step2;
        }
        double GetFuncValue(double lambda)
        {
            return ip.GetFuncValue(GetXNext(lambda));
        }
        double[] GetXNext(double lambda)
        {
            double[] arrXNext = new double[it.arrX.Length];
            for (int i = 0; i < arrXNext.Length; i++)
                arrXNext[i] = it.arrX[i] +
                    it.arrE[i] * it.mult * it.arrXDelta[i] * lambda;
            return arrXNext;
        }*/
    }
    public struct XY : IComparable<XY>
    {
        public double x, y;
        public XY(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public int CompareTo(XY other)
        {
            return x.CompareTo(other.x);
        }
    }
    public class XYComparerByY : IComparer<XY>
    {
        public int Compare(XY x, XY y)
        {
            return x.y.CompareTo(y.y);
        }
    }
    public static class Html
    {
        public static string ArrayToHtml(double[] arr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            if (arr == null)
                s += "<TR><TD>Не определено</TR></TD>";
            else
                for (int i = 0; i < arr.Length; i++)
                    s += string.Format("<TR><TD>{0}</TD></TR>",
                        Math.Round(arr[i], d));
            return s + "</TABLE>";
        }
        public static string MatrixToHtml(double[,] matr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr.GetLength(1); j++)
                    s += string.Format("<TD>{0}</TD>",
                        Math.Round(matr[i, j], d));
                s += "</TR>";
            }
            return s + "</TABLE>";
        }
        public static string MatrixToHtml(double[][] matr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            for (int i = 0; i < matr.Length; i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr[i].Length; j++)
                    s += string.Format("<TD>{0}</TD>",
                        Math.Round(matr[i][j], d));
                s += "</TR>";
            }
            return s + "</TABLE>";
        }
        public static string MatrixTranToHtml(double[][] matr, int d)
        {
            string s = "<TABLE BORDER = 3>";
            for (int i = 0; i < matr[0].Length; i++)
            {
                s += "<TR>";
                for (int j = 0; j < matr.Length; j++)
                    s += string.Format("<TD>{0}</TD>",
                        Math.Round(matr[j][i], d));
                s += "</TR>";
            }
            return s + "</TABLE>";
        }
    }
}

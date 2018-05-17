using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using muWrapper;
using NPlot;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Optimization
{
    public partial class MainForm : Form
    {
        double xMin = -10, xMax = 10, yMin = -10, yMax = 10;
        double mult = 0.04, mult2 = 20, denom = 1;
        int d = 3, nGridX = 10, nGridY = 10, maxIt = 1000;
        NMInitialParams ipNM;
        HJInitialParams ipHJ;
        List<NMIteration> listNM;
        List<HJIteration> listHJ;
        LinePlot lpItLast, lpTrLast;
        PointPlot ppItLast, ppTrLast;

        public MainForm()
        {
            InitializeComponent();
            nudN.Value = 2;
            dgvXHJ.Columns["Точка"].ValueType = typeof(double);
            dgvXHJ.Columns["Шаг"].ValueType = typeof(double);
            tbAlpha.Text = "1";
            tbBeta.Text = "0,5";
            tbGamma.Text = "2";
            tbEpsilon.Text = "0,001";
            tbXEps.Text = "0,001";
            tbFEps.Text = "0,000001";
            tbExpression.Text = "2*(x1^2-x2)^2+(x1-1)^2";
            toolStripTextBoxXMin.Text = xMin.ToString();
            toolStripTextBoxXMax.Text = xMax.ToString();
            toolStripTextBoxYMin.Text = yMin.ToString();
            toolStripTextBoxYMax.Text = yMax.ToString();
            toolStripTextBoxD.Text = d.ToString();
            toolStripTextBoxNGridX.Text = nGridX.ToString();
            toolStripTextBoxNGridY.Text = nGridY.ToString();
            toolStripTextBoxMult.Text = mult.ToString();
            toolStripTextBoxMult2.Text = mult2.ToString();
            toolStripTextBoxDenom.Text = denom.ToString();
            toolStripTextBoxMaxIt.Text = maxIt.ToString();
        }
        void DrawNMIteration(NMIteration it)
        {
            ReadGraphParams();
            if (it.arrXRes.Length != 2)
                throw new Exception();
            psItGraph.Remove(lpItLast, false);
            psItGraph.Remove(ppItLast, false);
            double[] arrX = new double[] {
                it.matrX[0][0], it.matrX[1][0], it.matrX[2][0], it.matrX[0][0]
            };
            double[] arrY = new double[] {
                it.matrX[0][1], it.matrX[1][1], it.matrX[2][1], it.matrX[0][1]
            };
            lpItLast = new LinePlot();
            lpItLast.OrdinateData = arrY;
            lpItLast.AbscissaData = arrX;
            lpItLast.Pen = new Pen(Color.Green, 2f);
            psItGraph.Add(lpItLast);

            ppItLast = new PointPlot(new Marker(Marker.MarkerType.Circle, 4));
            ppItLast.OrdinateData = new double[] { it.arrXRes[1] };
            ppItLast.AbscissaData = new double[] { it.arrXRes[0] };
            psItGraph.Add(ppItLast);

            psItGraph.XAxis1.WorldMin = xMin;
            psItGraph.XAxis1.WorldMax = xMax;
            psItGraph.YAxis1.WorldMin = yMin;
            psItGraph.YAxis1.WorldMax = yMax;
            psItGraph.XAxis1.Label = (string)dgvX.Rows[0].Cells["Имя"].Value;
            psItGraph.YAxis1.Label = (string)dgvX.Rows[1].Cells["Имя"].Value;
            psItGraph.Refresh();
        }
        void DrawNMTrajectory()
        {
            ReadGraphParams();
            if (listNM[0].arrXRes.Length != 2)
                throw new Exception();
            psGraph.Remove(lpTrLast, false);
            psGraph.Remove(ppTrLast, false);
            double[] arrX = new double[listNM.Count];
            double[] arrY = new double[listNM.Count];
            for (int i = 0; i < listNM.Count; i++)
            {
                arrX[i] = listNM[i].arrXRes[0];
                arrY[i] = listNM[i].arrXRes[1];
            }
            lpTrLast = new LinePlot();
            lpTrLast.OrdinateData = arrY;
            lpTrLast.AbscissaData = arrX;
            lpTrLast.Pen = new Pen(Color.Blue);
            psGraph.Add(lpTrLast);
            ppTrLast = new PointPlot(new Marker(Marker.MarkerType.Circle, 4));
            ppTrLast.OrdinateData = arrY;
            ppTrLast.AbscissaData = arrX;
            psGraph.Add(ppTrLast);

            psGraph.XAxis1.WorldMin = xMin;
            psGraph.XAxis1.WorldMax = xMax;
            psGraph.YAxis1.WorldMin = yMin;
            psGraph.YAxis1.WorldMax = yMax;
            psGraph.XAxis1.Label = (string)dgvX.Rows[0].Cells["Имя"].Value;
            psGraph.YAxis1.Label = (string)dgvX.Rows[1].Cells["Имя"].Value;
            psGraph.Refresh();
        }
        void DrawHJIteration(HJIteration it)
        {
            ReadGraphParams();
            if (it.arrX.Length != 2)
                throw new Exception();
            psItGraph.Remove(lpItLast, false);
            psItGraph.Remove(ppItLast, false);
            ppItLast = new PointPlot(new Marker(Marker.MarkerType.Circle, 4));
            ppItLast.OrdinateData = new double[] { it.arrX[1] };
            ppItLast.AbscissaData = new double[] { it.arrX[0] };
            psItGraph.Add(ppItLast);

            psItGraph.XAxis1.WorldMin = xMin;
            psItGraph.XAxis1.WorldMax = xMax;
            psItGraph.YAxis1.WorldMin = yMin;
            psItGraph.YAxis1.WorldMax = yMax;
            psItGraph.XAxis1.Label = (string)dgvX.Rows[0].Cells["Имя"].Value;
            psItGraph.YAxis1.Label = (string)dgvX.Rows[1].Cells["Имя"].Value;
            psItGraph.Refresh();
        }
        void DrawHJTrajectory()
        {
            ReadGraphParams();
            if (listHJ[0].arrX.Length != 2)
                throw new Exception();
            psGraph.Remove(lpTrLast, false);
            psGraph.Remove(ppTrLast, false);

            double[] arrX = new double[listHJ.Count];
            double[] arrY = new double[listHJ.Count];
            for (int i = 0; i < listHJ.Count; i++)
            {
                arrX[i] = listHJ[i].arrX[0];
                arrY[i] = listHJ[i].arrX[1];
            }
            lpTrLast = new LinePlot();
            lpTrLast.OrdinateData = arrY;
            lpTrLast.AbscissaData = arrX;
            lpTrLast.Pen = new Pen(Color.Blue);
            psGraph.Add(lpTrLast);
            ppTrLast = new PointPlot(new Marker(Marker.MarkerType.Circle, 4));
            ppTrLast.OrdinateData = arrY;
            ppTrLast.AbscissaData = arrX;
            psGraph.Add(ppTrLast);

            psGraph.XAxis1.WorldMin = xMin;
            psGraph.XAxis1.WorldMax = xMax;
            psGraph.YAxis1.WorldMin = yMin;
            psGraph.YAxis1.WorldMax = yMax;
            psGraph.XAxis1.Label = (string)dgvX.Rows[0].Cells["Имя"].Value;
            psGraph.YAxis1.Label = (string)dgvX.Rows[1].Cells["Имя"].Value;
            psGraph.Refresh();
        }
        LinePlot[] GetPotentials(IInitialParams ip)
        {
            ReadGraphParams();
            List<LinePlot> listPotent = new List<LinePlot>();
            double stepX = (xMax - xMin) / (nGridX - 1);
            double stepY = (yMax - yMin) / (nGridY - 1);

            for (int i = 0; i < nGridX; i++)
                for (int j = 0; j < nGridY; j++)
                {
                    double x1 = xMin + stepX * i;
                    double y1 = yMin + stepY * j;
                    try
                    {
                        double k = -ip.GetDerivative(new double[] { x1, y1 }, 0) /
                            ip.GetDerivative(new double[] { x1, y1 }, 1);
                        double x2 = x1 + mult * stepX;
                        double y2 = y1 + k * (x2 - x1);
                        if (Math.Abs(y2 - y1) > stepY * mult * mult2)
                            goto nextTry;
                        double f = ip.GetFuncValue(new double[] { (x1 + x2) / 2, (y1 + y2) / 2 });
                        int red = (int)(f / denom);
                        if (red > 255)
                            red = 255;
                        LinePlot lp = new LinePlot();
                        lp.AbscissaData = new double[] { x1, x2 };
                        lp.OrdinateData = new double[] { y1, y2 };
                        lp.Pen.Color = Color.FromArgb(red, 0, 0);
                        listPotent.Add(lp);
                        continue;
                    }
                    catch { }
                nextTry:
                    try
                    {
                        double k = -ip.GetDerivative(new double[] { x1, y1 }, 1) /
                            ip.GetDerivative(new double[] { x1, y1 }, 0);
                        double y2 = y1 + mult * stepY;
                        double x2 = x1 + k * (y2 - y1);
                        if (Math.Abs(x2 - x1) > stepX * mult * mult2)
                            continue;
                        double f = ip.GetFuncValue(new double[] { (x1 + x2) / 2, (y1 + y2) / 2 });
                        int red = (int)(f / denom);
                        if (red > 255)
                            red = 255;
                        LinePlot lp = new LinePlot();
                        lp.AbscissaData = new double[] { x1, x2 };
                        lp.OrdinateData = new double[] { y1, y2 };
                        lp.Pen.Color = Color.FromArgb(red, 0, 0);
                        listPotent.Add(lp);
                    }
                    catch { }
                }
            return listPotent.ToArray();
        }
        void nudN_ValueChanged(object sender, EventArgs e)
        {
            int n = (int)nudN.Value;
            dgvX.Rows.Clear();
            dgvX.RowCount = n;
            for (int i = 0; i < dgvX.Rows.Count; i++)
                dgvX.Rows[i].Cells["Имя"].Value = string.Format("x{0}", i + 1);

            dgvArrXNM.Columns.Clear();
            dgvArrXNM.Rows.Clear();
            for (int i = 0; i < n + 1; i++)
            {
                DataGridViewColumn c = new DataGridViewTextBoxColumn();
                c.HeaderText = string.Format("Точка{0}", i + 1);
                c.ValueType = typeof(double);
                dgvArrXNM.Columns.Add(c);
            }
            dgvArrXNM.RowCount = n;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n + 1; j++)
                    dgvArrXNM.Rows[i].Cells[j].Value = 0.0;

            dgvXHJ.Rows.Clear();
            dgvXHJ.RowCount = n;
            for (int i = 0; i < dgvXHJ.Rows.Count; i++)
            {
                dgvXHJ.Rows[i].Cells["Точка"].Value = 0.0;
                dgvXHJ.Rows[i].Cells["Шаг"].Value = 0.5;
            }
        }
        void ReadNMParams(out NMInitialParams ip, out NMIteration it)
        {
            string[] arrName = new string[dgvX.RowCount];
            for (int i = 0; i < dgvX.RowCount; i++)
                arrName[i] = (string)dgvX.Rows[i].Cells["Имя"].Value;
            ip = new NMInitialParams(double.Parse(tbAlpha.Text),
                double.Parse(tbBeta.Text), double.Parse(tbGamma.Text),
                double.Parse(tbEpsilon.Text), arrName, arrName,
                tbExpression.Text);
            
            int n = (int)nudN.Value;
            double[][] matr = new double[n + 1][];
            for (int i = 0; i < n + 1; i++)
                matr[i] = new double[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n + 1; j++)
                matr[j][i] = (double)dgvArrXNM.Rows[i].Cells[j].Value;            
            it = new NMIteration(matr);
        }
        void ReadHJParams(out HJInitialParams ip, out HJIteration it)
        {
            string[] arrName = new string[dgvX.RowCount];
            for (int i = 0; i < dgvX.RowCount; i++)
                arrName[i] = (string)dgvX.Rows[i].Cells["Имя"].Value;
            ip = new HJInitialParams(double.Parse(tbXEps.Text), double.Parse(tbFEps.Text),
                arrName, arrName, tbExpression.Text);

            double[] arrX = new double[dgvXHJ.RowCount];
            double[] arrXDelta = new double[dgvXHJ.RowCount];
            for (int i = 0; i < dgvXHJ.RowCount; i++)
            {
                arrX[i] = (double)dgvXHJ.Rows[i].Cells["Точка"].Value;
                arrXDelta[i] = (double)dgvXHJ.Rows[i].Cells["Шаг"].Value;
            }
            it = new HJIteration(arrX, arrXDelta);
        }
        void ReadGraphParams()
        {
            try
            {
                xMin = double.Parse(toolStripTextBoxXMin.Text);
                xMax = double.Parse(toolStripTextBoxXMax.Text);
                yMin = double.Parse(toolStripTextBoxYMin.Text);
                yMax = double.Parse(toolStripTextBoxYMax.Text);
                mult = double.Parse(toolStripTextBoxMult.Text);
                mult2 = double.Parse(toolStripTextBoxMult2.Text);
                denom = double.Parse(toolStripTextBoxDenom.Text);
                d = int.Parse(toolStripTextBoxD.Text);
                nGridX = int.Parse(toolStripTextBoxNGridX.Text);
                nGridY = int.Parse(toolStripTextBoxNGridY.Text);
                maxIt = int.Parse(toolStripTextBoxMaxIt.Text);
                if (xMin >= xMax || yMin >= yMax || d < 0 || d > 100 ||
                            nGridX <= 0 || nGridY <= 0 || nGridX > 30 ||
                            nGridY > 30 || mult > 0.5 || mult < 0.005 ||
                            mult2 > 40 || mult2 < 1 || denom == 0 || maxIt <= 0)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Неверные параметры графика", "Ошибка построения графика",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void SetTask(Task t)
        {
            nudN.Value = t.arrName.Length;
            for (int i = 0; i < dgvX.RowCount; i++)
                dgvX.Rows[i].Cells["Имя"].Value = t.arrName[i];
            tbExpression.Text = t.expression;
            tbAlpha.Text = t.alpha;
            tbBeta.Text = t.beta;
            tbGamma.Text = t.gamma;
            tbEpsilon.Text = t.eps;
            tbXEps.Text = t.xEps;
            tbFEps.Text = t.fEps;
        }
        Task ReadTask()
        {
            Task t = new Task();
            t.arrName = new string[dgvX.RowCount];
            for (int i = 0; i < dgvX.RowCount; i++)
                t.arrName[i] = (string)dgvX.Rows[i].Cells["Имя"].Value;
            t.expression = tbExpression.Text;
            t.alpha = tbAlpha.Text;
            t.beta = tbBeta.Text;
            t.gamma = tbGamma.Text;
            t.eps = tbEpsilon.Text;
            t.xEps = tbXEps.Text;
            t.fEps = tbFEps.Text;
            return t;
        }
        void lbIterNM_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                NMIteration it = (NMIteration)lbIterNM.SelectedItem;
                wb.DocumentText = it.ToHtml(ipNM, d);
                DrawNMIteration(it);                
            }
            catch { }
        }
        void lbIterHJ_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                HJIteration it = (HJIteration)lbIterHJ.SelectedItem;
                wb.DocumentText = it.ToHtml(ipHJ, d);
                DrawHJIteration(it);                
            }
            catch { }
        }
        void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedIndex == 1)
                    DrawNMTrajectory();
                if (tabControl1.SelectedIndex == 2)
                    DrawHJTrajectory();
            }
            catch { }
        }        
        void dgvArrXNM_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                double x = (double)dgvArrXNM.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                for (int i = 1; i < dgvArrXNM.ColumnCount; i++)
                    if (i == e.RowIndex + 1)
                        dgvArrXNM.Rows[e.RowIndex].Cells[i].Value = x + 1;
                    else
                        dgvArrXNM.Rows[e.RowIndex].Cells[i].Value = x;
            }
        }
        void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "Файл задачи|*.tsk";
                saveFileDialog1.FileName = "Задача1";
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, ReadTask());
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка сохранения файла",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                SetTask((Task)formatter.Deserialize(fs));
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка открытия файла",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void saveNMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "Файл HTML|*.html";
                saveFileDialog1.FileName = "Отчет1";
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
                sw.Write(string.Format("<P>Метод Нелдера-Мида</P>{0}",
                    ipNM.ToHtml(d)));
                for (int i = 0; i < listNM.Count; i++)
                    sw.Write(string.Format("<P>ИТЕРАЦИЯ {0}</P>{1}",
                        i, listNM[i].ToHtml(ipNM, d)));
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка сохранения отчета",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void saveHJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "Файл HTML|*.html";
                saveFileDialog1.FileName = "Отчет1";
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
                sw.Write(string.Format("<P>Метод Хука-Дживса</P>{0}",
                    ipHJ.ToHtml(d)));
                for (int i = 0; i < listHJ.Count; i++)
                    sw.Write(string.Format("<P>ИТЕРАЦИЯ {0}</P>{1}",
                        i, listHJ[i].ToHtml(ipHJ, d)));
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка сохранения отчета",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }  
        void nMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                NMIteration it;
                ReadNMParams(out ipNM, out it);

                NMOptimizer opt = new NMOptimizer();
                opt.Initialize(ipNM);
                listNM = new List<NMIteration>();
                it.CalcFuncAndResult(ipNM);
                listNM.Add(it);
                it = new NMIteration(it.matrX);                
                it.CalcFuncAndResult(ipNM);
                int iterNum = 0;
                do
                {
                    listNM.Add(it);
                    it = (NMIteration)opt.DoIteration(it);
                    iterNum++;
                }
                while (it != null && iterNum < maxIt);
                lbIterNM.Items.Clear();
                lbIterNM.Items.AddRange(listNM.ToArray());
                psGraph.Clear();
                psItGraph.Clear();
                if (listNM[0].arrXRes.Length != 2)
                    return;
                LinePlot[] arrLp = GetPotentials(ipNM);
                foreach (LinePlot lp in arrLp)
                {
                    psGraph.Add(lp);
                    psItGraph.Add(lp);
                }
                try
                {
                    DrawNMTrajectory();
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Расчеты прерваны",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        void hJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                HJIteration it;
                ReadHJParams(out ipHJ, out it);

                HJOptimizer opt = new HJOptimizer();
                opt.Initialize(ipHJ);
                listHJ = new List<HJIteration>();
                it.CalcResult(ipHJ);
                listHJ.Add(it);
                it = new HJIteration(it.arrX, it.arrXDelta);
                it.CalcResult(ipHJ);
                int iterNum = 0;
                do
                {
                    listHJ.Add(it);
                    it = (HJIteration)opt.DoIteration(it);
                    iterNum++;
                }
                while (it != null && iterNum < maxIt);
                lbIterHJ.Items.Clear();
                lbIterHJ.Items.AddRange(listHJ.ToArray());
                psGraph.Clear();
                psItGraph.Clear();
                if (listHJ[0].arrX.Length != 2)
                    return;
                LinePlot[] arrLp = GetPotentials(ipHJ);
                foreach (LinePlot lp in arrLp)
                {
                    psGraph.Add(lp);
                    psItGraph.Add(lp);
                }
                try
                {
                    DrawHJTrajectory();
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Расчеты прерваны",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }        
        void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Выполнил Кондауров А.С., группа АС-05-2");
        }
        void btnExample_Click(object sender, EventArgs e)
        {
            string s = "Тригонометрические: sin, cos, tan\r\n" +
                "Логирифмы (десятичный и натуральный): log, ln\r\n" +
                "Экспонента: exp\r\n" +
                "Стандартные: +, -, *, /, ^\r\n" +
                "Скобки: поддерживаются, обычные круглые ()";
            MessageBox.Show(s, "Доступные функции", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }        
    }
    [Serializable]
    public class Task
    {
        public string[] arrName;
        public string expression, alpha, beta, gamma, eps, xEps, fEps;
    }
}
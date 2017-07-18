﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ConvertLibrary;
using log4net;
using TestLinkConverter;
using TransferLibrary;
using TransferModel;
using Timer = System.Windows.Forms.Timer;

namespace TestLinkTransfer
{
    //TODO 添加打赏功能
    //TODO 处理完成后保存文件功能
    public partial class Form : System.Windows.Forms.Form
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Form));

        private DateTime _starTime; 

        private List<TestCase> tcList = new List<TestCase>();

        public Form()
        {
            InitializeComponent();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string filePath = (string) e.Argument;
            if (filePath.EndsWith("xml"))
            {
                this.XmlToExcel(filePath);
            }
            else
            {
                this.ExcelToXml(filePath);
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.timer.Stop();
            this.progressBar.Value = this.progressBar.Maximum;
            TimeSpan consume = DateTime.Now - this._starTime;
            MessageBox.Show($"转换用例数: {tcList.Count}. 耗时: {consume.Minutes.ToString("D2")}:{consume.Seconds.ToString("D2")}.\n用例生成目录: {System.Environment.CurrentDirectory.ToString()}");
            this.progressBar.Value = this.progressBar.Minimum;
            this.filePathTb.Text = string.Empty;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.progressBar.Value < this.progressBar.Maximum)
            {
                this.progressBar.PerformStep();
            }
        }


        private void getFilePathBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePathTb.Text = openFileDialog.FileName;
            }
        }


        private void startBtn_Click(object sender, EventArgs e)
        {
            this._starTime = DateTime.Now;
            CommonHelper.KillExcelProcess();
            if (this.FileChecked(filePathTb.Text)) return;
          
            this.backgroundWorker.RunWorkerAsync(filePathTb.Text);
            this.timer.Start();
        }

        /// <summary>
        /// Excel转换为XML
        /// </summary>
        /// <param name="fileDir">文件路径</param>
        private void ExcelToXml(string fileDir)
        {
            try
            {
                ExcelAnalysis excelAnalysis = new ExcelAnalysis(fileDir);
                tcList = excelAnalysis.ReadExcel();
                XmlHandler xh = new XmlHandler(tcList);
                xh.WriteXml();
            }
            catch (Exception ex)
            {
                this._logger.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// XML转换为Excel
        /// </summary>
        /// <param name="fileDir">文件路径</param>
        private void XmlToExcel(string fileDir)
        {
            try
            {
                XmlAnalysis xmlAnalysis = new XmlAnalysis(fileDir);
                XmlToModel xtm = new XmlToModel(xmlAnalysis.GetAllTestCaseNodes());
                tcList = xtm.OutputTestCases();
                ExcelHandler eh = new ExcelHandler(tcList);
                eh.WriteExcel();
            }
            catch (Exception ex)
            {
                this._logger.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 检查输入文件地址是否符合要求
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns>isChecked</returns>
        private bool FileChecked(string filePath)
        {
            if (filePathTb.Text == string.Empty)
            {
                this._logger.Info(new Exception("请输入文件地址."));
                MessageBox.Show("请输入文件地址.", "Warning");
                return true;
            }

            if (!(filePathTb.Text.EndsWith(".xml") || filePathTb.Text.EndsWith(".xls") || filePathTb.Text.EndsWith(".xlsx")))
            {
                this._logger.Info(new Exception("输入文件要求为xml，xls或xlsx格式."));
                MessageBox.Show("输入文件要求为xml，xls或xlsx格式.", "Warning");
                return true;
            }

            if (!File.Exists(filePathTb.Text))
            {
                this._logger.Info(new Exception($"{filePathTb.Text} 已不存在，请重新输入文件地址."));
                MessageBox.Show($"{filePathTb.Text} 已不存在，请重新输入文件地址.", "Warning");
                return true;
            }

            return false;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://yaitza.github.io/");
        }

        private void DonateLab_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Form donateForm = new DonateForm();
            donateForm.Show();
        }

        private void xeRb_CheckedChanged(object sender, EventArgs e)
        {
            this.openFileDialog.Filter = "xml|*.xml|Excel 2007Plus|*.xlsx";
        }

        private void exRb_CheckedChanged(object sender, EventArgs e)
        {
            this.openFileDialog.Filter = "Excel 2007Plus|*.xlsx|xml|*.xml";
        }
    }
}

#region copyright

// <copyright file="Form1.cs" project="DirList">
// Copyright (c) 2018 All Rights Reserved
// </copyright>
// <author>Andreas Vogelbacher</author>
// <date>2018</date>
// <summary></summary>

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirList
{
    public partial class Form1 : Form
    {
        string folderName = String.Empty;
        private Object thisLock = new Object();
        private CancellationTokenSource ts;// = new CancellationTokenSource();
        private bool cancelTask = false;

        public Form1()
        {
            InitializeComponent();
            
            //Initialize certain components, can be done in designer too
            textBox1.Text = String.Empty;
            button1.Text = "Start";
            button2.Text = "Save";
            button3.Text = "Cancel";
            button3.Enabled = false;
            button4.Text = "Remove Duplicates";
            button4.Enabled = false;
            button5.Text = "Sort";
            button5.Enabled = false;
            label2.Text = "Status:";
            label1.Text = String.Empty;
            circularProgressBar1.Style = ProgressBarStyle.Blocks;
            circularProgressBar1.Value = 0;
            circularProgressBar1.Font = new Font(FontFamily.GenericSansSerif, 10, GraphicsUnit.Point);// Microsoft Sans Serif; 72pt; style = Bold
            circularProgressBar1.Text = "Idle";
            circularProgressBar1.SubscriptText = String.Empty;
            circularProgressBar1.SuperscriptText = String.Empty;

        }

        /// <summary>
        /// Recursive go trough directories and get directory names
        /// </summary>
        /// <param name="startDirName"></param>
        void DirectorySearcher(string startDirName)
        {
            try
            {
                foreach (string dirName in Directory.GetDirectories(startDirName))
                {
                    lock (thisLock)//Prevent task clashes
                    {
                        if (!cancelTask) //Cancel Button pressed
                        {
                            //Update Label1
                            label1.Invoke((MethodInvoker)delegate
                            {
                                label1.Text = dirName;
                            });
                            //Update TextBox1
                            textBox1.Invoke((MethodInvoker)delegate
                            {
                                textBox1.AppendText(getBasename(dirName) + Environment.NewLine);
                            });
                            DirectorySearcher(dirName);
                        }
                        else
                        {
                            label1.Invoke((MethodInvoker)delegate
                            {
                                label1.Text = "Canceled by User...";
                            });
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Remove all charactes before the last '\\'
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public string getBasename(string dirName)
        {
            return (dirName.Remove(0, dirName.LastIndexOf(Path.DirectorySeparatorChar) + 1));
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //Select folder
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                folderName = folderBrowserDialog1.SelectedPath;
            }
            else
            {
                folderName = String.Empty;
            }
            textBox1.Text = String.Empty;
            //Start work
            if (folderName != String.Empty)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button3.Enabled = true;
                cancelTask = false;
                circularProgressBar1.Style = ProgressBarStyle.Marquee;
                circularProgressBar1.Value = 10;
                circularProgressBar1.Text = "Running";
                //CancellationToken not working, using cancelTask instead
                //ts = new CancellationTokenSource();
                //CancellationToken ct = ts.Token;
                Task.Factory.StartNew(() =>
                {
                            label1.Invoke((MethodInvoker)delegate {
                                label1.Text = "Canceled by User...";
                          });
                        DirectorySearcher(folderName);
                }/*,ct*/).ContinueWith(task => {
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = false;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    cancelTask = false;
                    circularProgressBar1.Style = ProgressBarStyle.Blocks;
                    circularProgressBar1.Value = 0;
                    circularProgressBar1.Text = "Idle";
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text List|*.txt";
            saveFileDialog1.Title = "Save the List to a File";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "" && textBox1.Text != String.Empty)
            {
                File.WriteAllText(saveFileDialog1.FileName, textBox1.Text);
            }
        }
        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            //ts.Cancel();
            cancelTask = true;
        }
        /// <summary>
        /// remove duplicates from TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Join(Environment.NewLine, textBox1.Lines.Distinct());
        }

        /// <summary>
        /// Sort TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Lines = textBox1.Lines.OrderBy(l => l).ToArray();
        }
    }
}

﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SearchDiskFilesGI {
    public partial class Form1 : Form {
        private string folder = "";
        private string query = "";
        private long numberOfFiles = 0;
        private IProgress<CustomProgress> progress;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private bool maximumSet = false;

        public Form1()
        {
            InitializeComponent();
            progress = new Progress<CustomProgress>(state => {
                // will run with the UI thread context

                if (state.file != null)
                    listBox1.Items.Add(state.file);

                // only set the maximum one time, 
                // does it affect performance to set it every time?
                if (!maximumSet)
                {
                    progressBar1.Maximum = state.total;
                    maximumSet = true;
                }

                progressBar1.PerformStep();

            });
        }
        private void Form1_Load(object sender, EventArgs e) {
        }

        private async void search_Click(object sender, EventArgs e) {
            if (thereIsValidInformation() == false)
                return;

            listBox1.Items.Clear();
            cancelButton.Enabled = true;
            results.Visible = false;

            try {
                var res = await SearchDiskFiles.Find(folder, numberOfFiles, cts.Token, progress);
                // This code will run within the UI context
                var resText = String.Format("Total files: {0},files: {1}. Files matched the search: {2}",
                    res.TotalFiles, numberOfFiles, res.Files.Count);
                results.Text = resText;
                results.Visible = true;
            } catch (OperationCanceledException ex) {
                // benign exception
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message); // Ooopss..
            }
            
            progressBar1.Value = 0;
            cancelButton.Enabled = false;
            maximumSet = false;
            cts = new CancellationTokenSource(); // reset the token
            
            
        }

        private void folder_Click(object sender, EventArgs e) {
            var result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK) {
                folder = folderBrowserDialog1.SelectedPath;

                textBox1.Text = folder;
            }
        }


        /*
        |--------------------------------------------------------------------------
        | Validation
        |--------------------------------------------------------------------------
        */

        private bool thereIsValidInformation() {
            folder = textBox1.Text;
            query = textBox3.Text;

            if (folder.Length == 0) {
                MessageBox.Show("A folder is necessary.");
                return false;
            }

            if (!Directory.Exists(folder)) { // ignore sync io, should be very very very fast
                MessageBox.Show("That folder does not exist!");
                return false;
            }

            if (query.Length == 0) {
                MessageBox.Show("A Maximum Number of files is necessary.");
                return false;
            }
            try
            {
               numberOfFiles= Convert.ToInt64(query);
            }
            catch
            {
                MessageBox.Show("A Number shall be inserted here");
                return false;
            }
           

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }

        private void results_Click(object sender, EventArgs e)
        {

        }
    }
}

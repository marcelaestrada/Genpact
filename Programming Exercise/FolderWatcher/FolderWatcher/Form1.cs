using Spire.Xls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderWatcher
{
    public partial class FolderWatcherForm : Form
    {
        // global variables
        string masterPath = "";
        string targetPath = "";
        string targetExcel = "";
        string txtChanges = "";
        string pathS = "";
        int processedQ = 0;
        int notProcessedQ = 0;

        public FolderWatcherForm()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // select folder to watch and begin watching
            folderBrowser.ShowDialog();
            string path = folderBrowser.SelectedPath;

            if (path != "") {
                // textbox disabled until canceled
                tbPath.Text = path;
                tbPath.Enabled = false;
                folderWatcher(path);
            }
        }

        private void folderWatcher (string path)
        {
            // declare watch and specifications
            FileSystemWatcher watch = new FileSystemWatcher();
            watch.Path = path;
            watch.IncludeSubdirectories = false;
            pathS = path;

            watch.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Security |
                NotifyFilters.Size;

            watch.Filter = "*.*";

            // watch events
            watch.Created += new FileSystemEventHandler(createdFile);
            watch.Deleted += new FileSystemEventHandler(closedFile);
            watch.EnableRaisingEvents = true;

            // check if directories and files exist, otherwise, create them
            DirectoryInfo dInfo = new DirectoryInfo(path);

            if (!Directory.Exists(dInfo.FullName + "\\Processed"))
            {
                dInfo.CreateSubdirectory("Processed");
            }

            if (!Directory.Exists(dInfo.FullName + "\\Not applicable"))
            {
                dInfo.CreateSubdirectory("Not applicable");
            }

            if (!File.Exists(dInfo.FullName + "\\master.xlsx"))
            {
                File.Create(dInfo.FullName + "\\master.xlsx");
            }

            if (!File.Exists(dInfo.FullName + "\\changes.txt"))
            {
                File.Create(dInfo.FullName + "\\changes.txt");
            }

            masterPath = dInfo.FullName + "\\master.xlsx";
            targetPath = dInfo.FullName + "\\Not applicable\\";
            targetExcel = dInfo.FullName + "\\Processed\\";
            txtChanges = dInfo.FullName + "\\changes.txt";

            FileInfo[] files = dInfo.GetFiles();

            // quantity of files in processed
            DirectoryInfo processed = new DirectoryInfo(targetExcel);
            FileInfo[] pQuantity = processed.GetFiles();
            processedQ = pQuantity.Length;

            // quantity of files in not applicable
            DirectoryInfo notApplicable = new DirectoryInfo(targetPath);
            FileInfo[] pNotApplicable = notApplicable.GetFiles();
            notProcessedQ = pNotApplicable.Length;

            if (files.Length != 0)
            {
                foreach (FileInfo f in files)
                {
                    if (f.Extension == ".xlsx" && f.Name != "master.xlsx")
                    {
                        // check if it is excel or not, save the worksheets into a general worksheet
                        isExcel(f.Name, f.FullName, masterPath);

                        try
                        {
                            // move file to another directory inside the folder
                            string newName = (processedQ + 1) + " - " + f.Name;
                            File.Move(f.FullName, targetExcel + newName);
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " was copied to " + masterPath + " and then moved to " + targetExcel + Environment.NewLine);
                        }
                        catch
                        {
                            // not moved since it is open
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " was copied to " + masterPath + " but it can not be moved since it is open" + Environment.NewLine);
                        }
                    }
                    else if (f.Name != "master.xlsx" && f.Name != "changes.txt" && f.Name != "Processed" && f.Name != "Not applicable")
                    {
                        try
                        {
                            // move file to another directory inside the folder
                            string newName = (notProcessedQ + 1) + " - " + f.Name;
                            File.Move(f.FullName, targetPath + newName);
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " was moved to " + targetPath + Environment.NewLine);
                        }
                        catch
                        {
                            // not moved since it is open
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " can not be moved to " + targetPath + " since it is open. " + Environment.NewLine);
                        }
                    }
                }
            }
        }

        private void createdFile(object source, FileSystemEventArgs e)
        {
            // check if it is excel or not, save the worksheets into a general worksheet
            if (e.Name.Contains(".xlsx") && e.Name != "master.xlsx")
            {
                isExcel(e.Name, e.FullPath, masterPath);

                // move file to another directory inside the folder
                try
                {
                    string newName = (processedQ + 1) + " - " + e.Name;
                    File.Move(e.FullPath, targetExcel + newName);
                    checkTxt(txtChanges);
                    File.AppendAllText(txtChanges, e.Name + " was copied to " + masterPath + " and then moved to " + targetPath + Environment.NewLine);
                }
                catch
                {
                    checkTxt(txtChanges);
                    File.AppendAllText(txtChanges, e.Name + " was copied to " + masterPath + " but it can not be moved since it is open" + Environment.NewLine);
                }
                
            }
            else if (e.Name != "master.xlsx" && e.Name != "changes.txt" && e.Name != "Processed" && e.Name != "Not applicable")
            {
                int milliseconds = 2000;
                Thread.Sleep(milliseconds);

                // move file to another directory inside the folder 
                try
                {
                    string newName = (notProcessedQ + 1) + " - " + e.Name;
                    File.Move(e.FullPath, targetPath + newName);
                    checkTxt(txtChanges);
                    File.AppendAllText(txtChanges, e.Name + " was moved to " + targetPath + Environment.NewLine);
                }
                catch
                {
                    checkTxt(txtChanges);
                    File.AppendAllText(txtChanges, e.Name + " can not be moved to " + targetPath + " since it is open. " + Environment.NewLine);
                }
            }

        }

        private void closedFile(object source, FileSystemEventArgs e)
        {
            DirectoryInfo dInfo = new DirectoryInfo(pathS);
            FileInfo[] files = dInfo.GetFiles();

            if (files.Length != 0)
            {
                foreach (FileInfo f in files)
                {
                    if (f.Extension == ".xlsx" && f.Name != "master.xlsx")
                    {
                        try
                        {
                            // move file to another directory inside the folder
                            string newName = (processedQ + 1) + " - " + f.Name;
                            File.Move(f.FullName, targetExcel + newName);
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " was moved to " + targetExcel + Environment.NewLine);
                        }
                        catch
                        {
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " can not be moved since it is open" + Environment.NewLine);
                        }
                    }
                    else if (f.Name != "master.xlsx" && f.Name != "changes.txt" && e.Name != "Processed" && e.Name != "Not applicable")
                    {
                        try
                        {
                            // move file to another directory
                            string newName = (notProcessedQ + 1) + " - " + f.Name;
                            File.Move(f.FullName, targetPath + newName);
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " was moved to " + targetPath + Environment.NewLine);
                        }
                        catch
                        {
                            checkTxt(txtChanges);
                            File.AppendAllText(txtChanges, f.Name + " can not be moved to " + targetPath + " since it is open. " + Environment.NewLine);
                        }
                    }
                }
            }
        }

        private void isExcel(string name, string filepath, string masterPath)
        {
            int milliseconds = 2000;
            Thread.Sleep(milliseconds);

            if (!filepath.Contains("$"))
            {
                // Source excel
                Workbook source = new Workbook();
                source.LoadFromFile(filepath);

                // Target excel - master
                Workbook target = new Workbook();
                target.LoadFromFile(masterPath);

                // Copy and save worksheets as "filename - sheet name"
                foreach (Worksheet w in source.Worksheets)
                {
                    Worksheet copyS = target.Worksheets.AddCopy(w);
                    copyS.Name = name + " - " + w.Name + " " + target.Worksheets.Count;
                    target.SaveToFile(masterPath);
                }
            }
            else
            {
                // files generated when some file is opened are not copied
                File.AppendAllText(txtChanges, name + " file is open." + Environment.NewLine);
            }
            
        }

        private void checkTxt(string path)
        {
            // verifies if file exists, otherwise create it
            if (!File.Exists(path))
            {
                File.Create(path);
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // cancel button
            tbPath.Text = "";
            tbPath.Enabled = true;
        }
    }
}

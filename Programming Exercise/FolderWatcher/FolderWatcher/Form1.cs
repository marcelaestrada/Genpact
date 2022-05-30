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
        string masterPath = "";
        string targetPath = "";
        string targetExcel = "";

        public FolderWatcherForm()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            folderBrowser.ShowDialog();
            string path = folderBrowser.SelectedPath;

            if (path != "") {
                tbPath.Text = path;
                tbPath.Enabled = false;
                folderWatcher(path);
            }
        }

        private void folderWatcher (string path)
        {
            FileSystemWatcher watch = new FileSystemWatcher();
            watch.Path = path;
            watch.IncludeSubdirectories = false;

            watch.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Security |
                NotifyFilters.Size;

            watch.Filter = "*.*";

            watch.Created += new FileSystemEventHandler(createdFile);
            watch.Changed += new FileSystemEventHandler(moveFile);
            watch.EnableRaisingEvents = true;

            // First in 
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
            masterPath = dInfo.FullName + "\\master.xlsx";
            targetPath = dInfo.FullName + "\\Not applicable\\";
            targetExcel = dInfo.FullName + "\\Processed\\";

            FileInfo[] files = dInfo.GetFiles();

            if(files.Length != 0)
            {
                foreach (FileInfo f in files)
                {
                    if (f.Extension == ".xlsx" && f.Name != "master.xlsx")
                    {
                        // Check if it is excel or not, save the worksheets into a general worksheet
                        isExcel(f.Name, f.FullName, masterPath);


                        try
                        {
                            // Move file to another directory inside the folder
                            File.Move(f.FullName, targetExcel + f.Name);
                            lblChanges.Text = f.Name + " was copied to " + masterPath + " and then moved to " + targetExcel;
                            //addGroupBox(f.Name + " was copied to " + masterPath + " and then moved to " + targetExcel);
                        }
                        catch
                        {
                            //addGroupBox(f.Name + " was copied to " + masterPath + " but it can not be moved since it is open");
                        }
                    }
                    else if (f.Name != "master.xlsx")
                    {
                        try
                        {
                            File.Move(f.FullName, targetPath + f.Name);
                            //addGroupBox(f.Name + " was moved to " + targetPath);
                        }
                        catch
                        {
                            //addGroupBox(f.Name + " can not be moved to " + targetPath + " since it is open. ");
                        }
                    }
                }
            }
        }

        private void createdFile(object source, FileSystemEventArgs e)
        {
            // Check if it is excel or not, save the worksheets into a general worksheet
            if (e.Name.Contains(".xlsx") && e.Name != "master.xlsx")
            {
                isExcel(e.Name, e.FullPath, masterPath);

                // Move file to another directory inside the folder
                try
                {
                    File.Move(e.FullPath, targetExcel + e.Name);
                    //addGroupBox(e.Name + " was copied to " + masterPath + " and then moved to " + targetPath);
                }
                catch
                {
                    //addGroupBox(e.Name + " was copied to " + masterPath + " but it can not be moved since it is open");
                }
                
            }
            else if (e.Name != "master.xlsx")
            {
                int milliseconds = 2000;
                Thread.Sleep(milliseconds);

                try
                {
                    File.Move(e.FullPath, targetPath + e.Name);
                    //addGroupBox(e.Name + " was moved to " + targetPath);
                }
                catch
                {
                    //addGroupBox(e.Name + " can not be moved to " + targetPath +  " since it is open. ");
                }
            }

        }

        private void moveFile(object source, FileSystemEventArgs e)
        {
            try
            {
                File.Move(e.FullPath, targetPath + e.Name);
                //addGroupBox(e.Name + " was moved to " + targetPath);
            }
            catch
            {
                //addGroupBox(e.Name + " can not be moved to " + targetPath + " since it is open. ");
            }
        }

        private void isExcel(string name, string filepath, string masterPath)
        {
            int milliseconds = 2000;
            Thread.Sleep(milliseconds);

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

        //private void addGroupBox(string text)
        //{
        //    gbCambios.Text += "\n" + text;
        //}

    }
}

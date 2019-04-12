using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Renci.SshNet.Common;
using Renci.SshNet;
using System.IO;
using System.Diagnostics;
using BAGIT_FILE_TRANSFER;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.AccountManagement;
using System.Collections.Specialized;
using System.Configuration;

namespace BagItProcess
{
    public partial class frmBagitprocess : Form
    {
        public static string HostName = string.Empty;
        public static string PythonDirectory = string.Empty;
        public static string PythonExePath = string.Empty;
        public static string create_bag_path = string.Empty;
        public static string BagitDirectory = string.Empty;
        public static string validate_bag_path = string.Empty;
        public static string ProcessedArchive = string.Empty;
        public static string SFTPUserName = string.Empty;
        public static string SFTPPassword = string.Empty;
        public static int SFTPPort = 0;
        public static string SFTP_Directory_Path = string.Empty;
        public static string Dest_Directory_Path = string.Empty;
        public static string ProcessPath = string.Empty;
        public static long filesize = 0;
        public static SftpClient sftp = null;
        public static ConnectionInfo conInfo = null;
        public string SuccessResult = string.Empty;
        public string SuccessBagMessage = string.Empty;
        public string ErrorsResult = string.Empty;
        public static string Base_processPath = string.Empty;
        public static string bagTemplatepath = string.Empty;
        public static bool bLoadFlag = false;
        public static bool bValidateFlag = false;
        public static string sToolargeFileDirectory= string.Empty;
        public static string BagitProcessDirName = string.Empty;
        public static string DirTimeStamp = string.Empty;
        public static int ProcessFolderLength = 0;
        public static string Profile_Json_Url = string.Empty;
        UserPrincipal userPrincipal = UserPrincipal.Current;
        public static string MsgWarning = "RAC Aurora";
        Boolean bReturn = false;
        Boolean bReturnlongFile = false;
        Bagit_Props objprop = new Bagit_Props();
        public frmBagitprocess()
        {
            InitializeComponent();
        }

        private Boolean  DirectoryLengthValidate(string path)
        {
            string[] files = Directory.GetDirectories(path,
           "*.*",
           SearchOption.AllDirectories);
            // Display all the files.
          Boolean  bretval = true;
            sToolargeFileDirectory = string.Empty;
            int DirectLength= new DirectoryInfo(path).Name.Length+1;
            int lengthDir = path.Length - DirectLength;
            int InstalledDirLength = ProcessFolderLength;
            if (files.Length > 0)
            {
                foreach (string file in files)
                {
                    string Filepath = file.Substring(lengthDir, file.Length - lengthDir);
                    if (Filepath.Length + InstalledDirLength > 248)
                    {
                        sToolargeFileDirectory = path;
                        bretval = false;
                        break;
                    }
                }

            }
            else
            {
                if (DirectLength + InstalledDirLength > 248)
                {
                    sToolargeFileDirectory = path;
                    bretval = false;                   
                }
            }
            return bretval;
            //foreach (string s in Directory.GetDirectories(path))
            //{
            //    Console.WriteLine(s.Remove(0, path.Length));
            //}
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string strEmptyFolderFind = string.Empty;
            try
            {
                lstBagItProcess.BackColor = SystemColors.Window;

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();               
                dialog.InitialDirectory = "C:\\";
                dialog.IsFolderPicker = rdFolders.Checked;
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                 
                    var dirlist1 = dialog.FileNames.ToArray();
                    var  dirDiclist = FilePathLengthValidate(new List<string>(dirlist1), rdFolders.Checked);                   
                 
                    foreach (KeyValuePair<string, int> entry in dirDiclist)
                    {

                        if (rdFolders.Checked && Directory.GetFiles(entry.Key).Length == 0 && rdFolders.Checked && Directory.GetDirectories(entry.Key).Length == 0)
                        {
                            strEmptyFolderFind = strEmptyFolderFind + entry.Key + Environment.NewLine;
                        }
                        if ((rdFolders.Checked && Directory.GetFiles(entry.Key).Length > 0 || rdFolders.Checked && Directory.GetDirectories(entry.Key).Length > 0) || (rdFiles.Checked))
                        {
                            ListViewItem item = lstview.FindItemWithText(entry.Key);
                            if (item==null)// &&(item.Text != entry.Key))//  if (!lstview.Items.ContainsKey(entry.Key))
                            {
                                var listViewItem = new ListViewItem(entry.Key);
                                lstview.Items.Add(listViewItem);

                                if (entry.Value == 1)
                                    lstview.Items[lstview.Items.Count - 1].BackColor = Color.Yellow;

                            }
                            else if (item.Text != entry.Key)
                            {
                                var listViewItem = new ListViewItem(entry.Key);
                                lstview.Items.Add(listViewItem);

                                if (entry.Value == 1)
                                    lstview.Items[lstview.Items.Count - 1].BackColor = Color.Yellow;
                            }
                        }
                      
                    }

                    if (strEmptyFolderFind.Length > 0)
                        MessageBox.Show("Process will ignore the below empty folder(s): " + strEmptyFolderFind, MsgWarning,
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
               
                lstBagItProcess.Items.Clear();

            }
            catch (Exception ex)
            {
                Logger.LogException("Exception while choosing the files/folders:" + ex.Message);               
                throw;
            }

        }
      
        private string[] FilePathLength(List<string> dirlist, Boolean FolderChecked)
        {
            try
            {
                string strIssueFile = string.Empty;
                sToolargeFileDirectory = string.Empty;
                if (FolderChecked)
                {

                    for (var i = 0; i < dirlist.Count; i++)
                    {                        
                        Toolongfilecheck(dirlist[i]);
                        strIssueFile = strIssueFile + sToolargeFileDirectory;
                        sToolargeFileDirectory = string.Empty;                       
                    }
                }
                else
                {
                    for (var i = 0; i < dirlist.Count; i++)
                    {
                        if (!File.Exists(dirlist[i]))
                        {
                            strIssueFile = strIssueFile + dirlist[i] + Environment.NewLine;
                            dirlist.Remove(dirlist[i]);
                            i--;
                        }
                    }

                }

                if (strIssueFile.Length > 0 )
                    MessageBox.Show("Process will ingore these below folder(s) too long files: " + strIssueFile, MsgWarning,
                         MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

                //  MessageBox.Show("Source/Destination file/folder path, file name or both if it is too long,the process will ingore those files/folders. " + strIssueFile, "File Path is too long",
                //       MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);               

                return dirlist.ToArray();
            }
            catch (Exception)
            {
                return dirlist.ToArray();
            }
        }
        private Dictionary<string, int> FilePathLengthValidate(List<string> dirlist, Boolean FolderChecked)
        {
            Dictionary<string, int> Diritem = new Dictionary<string, int>();
            try
            {
                // var DirItem = new List<KeyValuePair<string, int>>();
               
                 string strIssueFile = string.Empty;
                sToolargeFileDirectory = string.Empty;
                if (FolderChecked)
                {

                    for (var i = 0; i < dirlist.Count; i++)
                    {
                        bReturnlongFile = true;
                        sToolargeFileDirectory = string.Empty;
                        bReturnlongFile = DirectoryLengthValidate(dirlist[i]);                      
                    
                        
                        if (bReturnlongFile)
                        {
                            Diritem.Add(dirlist[i], 0);
                        }
                        else
                        {
                            strIssueFile = strIssueFile + sToolargeFileDirectory;
                            Diritem.Add(dirlist[i], 1);
                        }
                       
                    }
                }
                else
                {
                    for (var i = 0; i < dirlist.Count; i++)
                    {
                        string DirectName = new FileInfo(dirlist[0]).DirectoryName;
                        int InstalledDirLength = ProcessFolderLength;
                        string FileName = new DirectoryInfo(dirlist[i]).Name;
                        string FilePath = @"\\?\" + DirectName + "\\" + FileName;
                        try
                        {                            
                            if (!File.Exists(FilePath))
                            {
                                strIssueFile = strIssueFile + FilePath + Environment.NewLine;
                                dirlist.Remove(FilePath);
                                i--;
                            }

                            if (File.Exists(FilePath))
                            {                              
                                Diritem.Add(FilePath.TrimStart ("\\?\"".ToCharArray()), 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException("Error on FilePathLengthValidate method: " + ex.Message);
                        } 
                    }

                }

                if (strIssueFile.Length > 0)
                    MessageBox.Show("Process will ignore these below folder(s), too long sub folder: " + strIssueFile, MsgWarning,
                           MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                   
                return Diritem;
            }
            catch (Exception)
            {
                return Diritem;
            }
        }
        private Boolean Toolongfilecheck(string Direc)
        {
            
            if (Directory.Exists(Direc))
            {
                    foreach (string files in Directory.GetFiles(Direc))
                    {
                        try
                        {
                            bool FileExist = !File.Exists(new FileInfo(files).FullName);
                        }
                        catch (Exception)
                        {                        
                            sToolargeFileDirectory = sToolargeFileDirectory + Direc + Environment.NewLine;
                             bReturnlongFile = false;
                            break;
                    }
                    }
                }
            foreach (string drs in Directory.GetDirectories(Direc))
            {
                Toolongfilecheck(drs);                
            }
            return true;
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
                Logger.LogInformation("------------------------------------------------------");
                this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
                this.Close();          

        }

        /// <summary>
        /// Log the Input which is given in User Interface
        /// </summary>
        private void LoggingInput()
        {            
            Logger.LogInformation("Source Organization:" + txtSourceOrg.Text);
            Logger.LogInformation("Internal Sender Description:" + txtInternalSenderDesc.Text);
            Logger.LogInformation("Title:" + txtTitle.Text);
            Logger.LogInformation("Start Date:" + dtStart.Text);
            Logger.LogInformation("End Date:" + dtEnd.Text);
            Logger.LogInformation("Record Type:" + rdRecodType.Text);

        }
             
        
      /// <summary>
      /// To delete the processed directory and files
      /// </summary>
      /// <param name="target_dir"></param>
        private void DeleteDirectory(string target_dir)
        {
            try
            {
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }

                Directory.Delete(target_dir, false);
            }
            catch (Exception ex) { Logger.LogInformation("Error on while deleting the directory:" + ex.Message); }

        }

        /// <summary>
        /// Process the information and files/folders and create a bag in Aurorar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                var langs = ConfigurationManager.GetSection("ISOLanguages") as NameValueCollection;
               
                lstBagItProcess.BackColor = SystemColors.Window;
                tabConfiguration.SelectedIndex = 0;
                string PyExepath = string.Empty;
                string path = string.Empty;
                string[] subDirs;
                lstBagItProcess.Items.Clear();
                this.progressBar1.Visible = true;
                PythonExeCheck();
               
                bValidateFlag = true;

               
                if (lstview.Items.Count == 0)
                {
                    MessageBox.Show("Please choose files/folders for process", MsgWarning,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1);
                    return;
                }
                if (Convert.ToDateTime(dtStart.Text) > Convert.ToDateTime(dtEnd.Text))
                {
                    MessageBox.Show("Start date is greater than End date", MsgWarning,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1);
                    return;
                }
               if(txtbagcntmax.Text !="?"  && txtBagcnt.Text.Trim()!=string.Empty)
                {
                    
                        if (txtbagcntmax.Text.Trim()==string.Empty)
                        { 
                            MessageBox.Show("Please eneter the total number of bag in the 'Bag Count' field,If not known specifiy '?'", MsgWarning,
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                    MessageBoxDefaultButton.Button1);
                            return;
                        }
                        if (Convert.ToInt32(txtBagcnt.Text) > Convert.ToInt32(txtbagcntmax.Text))
                        {
                            MessageBox.Show(txtBagcnt.Text + " of " + txtbagcntmax.Text + ", Please enter a valid bag count", MsgWarning,
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                    MessageBoxDefaultButton.Button1);
                            return;
                        }
                        if (txtBagGrpIdentifier.Text.Trim() == string.Empty)
                        {
                            MessageBox.Show("Please enter the Bag group Identifier, if Bag Count text has value", MsgWarning,
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                   MessageBoxDefaultButton.Button1);
                            return;

                        }
                }
                if (txtbagcntmax.Text == "?")
                {
                    if (txtBagGrpIdentifier.Text.Trim() == string.Empty)
                    {
                        MessageBox.Show("Please enter the Bag group Identifier, if Bag Count text has value", MsgWarning,
                               MessageBoxButtons.OK, MessageBoxIcon.Warning,
                               MessageBoxDefaultButton.Button1);
                        return;

                    }
                }
                if (txtBagGrpIdentifier.Text.Trim()!=string.Empty )
                {
                    if(txtBagcnt.Text.Trim()==string.Empty )
                    {
                        MessageBox.Show("Please enter the bag count, if Bag group Identifier text has value", MsgWarning,
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                   MessageBoxDefaultButton.Button1);
                        return;
                    }
                }
                if (rdRecodType.Items.Count == 0)
                {
                    MessageBox.Show("Record Types are not able to load from Aurora API", MsgWarning,
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    return;
                }

                if (lstview.Items.Count == 0)
                {
                    BagitProcessErrorProvider.SetError(lstview, lblselecteditems.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(lstview, null);
                }               

                    if (this.ValidateChildren() == true)
                {

                    if (PythonExePath == string.Empty)
                        MessageBox.Show("Python exe is not exist in C:\\ drive", MsgWarning,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button1);

                    else
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        LoggingInput();
                        string strBagcnt = string.Empty;
                        txtTitle.Text= txtTitle.Text.Replace("'", "").Trim();
                        txtInternalSenderDesc.Text = txtInternalSenderDesc.Text.Replace(Environment.NewLine, " ").Replace("'", "").Trim();
                        txtExternalId.Text = txtExternalId.Text.Replace("'", "").Trim();
                        txtRecordCreator.Text = txtRecordCreator.Text.Replace("'", "").Trim();
                        txtBagcnt.Text = txtBagcnt.Text.Replace("'", "").Trim();
                        txtBagGrpIdentifier.Text = txtBagGrpIdentifier.Text.Replace("'", "").Trim();
                       if (txtBagcnt.Text.Trim() != string.Empty)
                        {
                            strBagcnt = txtBagcnt.Text + " of " + txtbagcntmax.Text;
                            if (txtbagcntmax.Text.Trim() == string.Empty)
                            {
                                strBagcnt = txtBagcnt.Text + " of ?";
                            }
                        }
                        string readFileText = File.ReadAllText(bagTemplatepath);
                        readFileText = readFileText.Replace("##" + txtTitle.Name + "##", "'" + txtTitle.Text + "'");
                        readFileText = readFileText.Replace("##" + txtSourceOrg.Name + "##", "'" + txtSourceOrg.Text.Trim() + "'");
                        readFileText = readFileText.Replace("##" + txtInternalSenderDesc.Name + "##", "'" + txtInternalSenderDesc.Text.Trim() + "'");
                        readFileText = readFileText.Replace("##" + dtStart.Name + "##", "'" + dtStart.Text + "'");
                        readFileText = readFileText.Replace("##" + dtEnd.Name + "##", "'" + dtEnd.Text + "'");
                        readFileText = readFileText.Replace("##" + rdRecodType.Name + "##", "'" + rdRecodType.Text + "'");
                        readFileText = readFileText.Replace("##" + txtExternalId.Name + "##", "'" + txtExternalId.Text.Trim() + "'");
                        readFileText = readFileText.Replace("##" + txtRecordCreator.Name + "##", "'" + txtRecordCreator.Text + "'");                        
                        readFileText = readFileText.Replace("##" + cmbLanguage.Name + "##", "'" + langs[cmbLanguage.Text].ToString() + "'");
                        readFileText = readFileText.Replace("##" + txtBagcnt.Name + "##", "'" + strBagcnt + "'");
                        readFileText = readFileText.Replace("##" + txtBagGrpIdentifier.Name + "##", "'" + txtBagGrpIdentifier.Text + "'");
                        
                        if (File.Exists(create_bag_path))
                        {
                            // Create a file to write to.
                            File.Delete(create_bag_path);
                        }
                        File.WriteAllText(create_bag_path, readFileText);

                        if (lstview.Items.Count > 0)
                        {
                            subDirs = System.IO.Directory.GetDirectories(ProcessPath);
                            if (subDirs.Length > 0)
                            {
                                try
                                {
                                    for (int loop = 0; loop < subDirs.Length; loop++)
                                    {                                        
                                        DeleteDirectory(subDirs[loop]);                                       
                                    }
                                }
                                catch (Exception ex)
                                { Logger.LogInformation("Deleting the exist directory from process path: " + ProcessPath + " :" + ex.Message); }
                            }

                            string ProcessTempPath =  DateTime.Now.ToString(BagitProcessDirName); 
                            System.IO.Directory.CreateDirectory(ProcessPath + "\\" + ProcessTempPath);
                            foreach (ListViewItem item in this.lstview.Items)
                            {
                                String temp = @"\\?\"+Convert.ToString(item.Text);  
                                // copy the directory from Browse selected folder to ProcessPath folder.                              
                                if (File.Exists(temp))
                                {
                                    string fname = string.Empty;
                                    try
                                    {
                                        FileInfo fileInfo = new FileInfo(temp);
                                        fname = fileInfo.Name;
                                        fileInfo.CopyTo(string.Format(@"{0}\{1}", ProcessPath + "\\" + ProcessTempPath, fileInfo.Name), true);
                                    }
                                    catch (Exception ex) { Logger.LogInformation("File Copy to process:Destination filename path is too long: " + ProcessPath + "\\" + ProcessTempPath + "\\" + fname + " " + ex.Message); }
                                }
                                else
                                {
                                    string dname = string.Empty;
                                    try
                                    {
                                        if (Directory.Exists(temp))
                                        {
                                            DirectoryInfo directoryInfo = new DirectoryInfo(temp);
                                            dname = directoryInfo.Name;
                                            Directory.CreateDirectory(ProcessPath + "\\" + ProcessTempPath + "\\" + directoryInfo.Name);
                                             CopyFolderContents(temp, ProcessPath + "\\" + ProcessTempPath + "\\" + directoryInfo.Name);
                                        }
                                    }
                                    catch (Exception ex)
                                    { Logger.LogInformation("Directory file Copy to process:Destination Directory path is too long: " + ProcessPath + "\\" + ProcessTempPath + "\\" + dname + " " + ex.Message); }
                                }
                            } 
                            subDirs = System.IO.Directory.GetDirectories(ProcessPath);  
                            progressBar1.Minimum = 1;
                            progressBar1.Value = 1;
                            progressBar1.Step = 1;
                           
                            if ( Directory.Exists(ProcessPath + "\\" + ProcessTempPath))
                            {
                                
                                //Copy the Mainpath folder one by one into process directory.
                                ProcessPath = ProcessPath + "\\" + ProcessTempPath;
                                this.progressBar1.Value = 20;
                                List<string> fileEntries = Directory.EnumerateFiles(ProcessPath, "*.*", SearchOption.AllDirectories).ToList();

                                if (fileEntries.Count == 0)
                                {
                                    MessageBox.Show("There is no files in the processing diectory.This might be due to long file/directory name", MsgWarning,
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                            MessageBoxDefaultButton.Button1);
                                    return;
                                }
                                bReturn = CreateValidateBag(ProcessPath);
                                this.progressBar1.Value = 50;
                                if (bReturn)
                                {
                                     lstBagItProcess.Items.Add("This " + Path.GetFileName(ProcessPath) + " Bag creation/validation completed.");
                                    lstBagItProcess.BackColor = Color.LightGreen;
                                    lstBagItProcess.HorizontalScrollbar = true;
                                }
                                else
                                {
                                    lstBagItProcess.Items.Add("This " + Path.GetFileName(ProcessPath) + " Bag creation/validation failed.");
                                    lstBagItProcess.BackColor = Color.LightCoral;
                                    lstBagItProcess.HorizontalScrollbar = true;
                                    Logger.LogException(Path.GetFileName(ProcessPath) + ": Bag is creation/validation failed.");
                                }

                                if (bReturn)
                                {
                                    bReturn = SFTPBagTransfer(ProcessPath);
                                    this.progressBar1.Value = 75;
                                    if (bReturn)
                                    {
                                        Logger.LogInformation(Path.GetFileName(ProcessPath) + ": Bag transferred to SFTP server.");
                                        lstBagItProcess.Items.Add(Path.GetFileName(ProcessPath) + " Bag transferred to SFTP server.");
                                        lstBagItProcess.BackColor = Color.LightGreen;
                                        lstBagItProcess.HorizontalScrollbar = true;
                                        lstview.Items.Clear();
                                        txtInternalSenderDesc.Text = string.Empty;
                                        txtTitle.Text = string.Empty;
                                        txtExternalId.Text = string.Empty;
                                        txtBagcnt.Text = string.Empty ;
                                        txtbagcntmax.Text = string.Empty;
                                        txtBagGrpIdentifier.Text = string.Empty;
                                         txtRecordCreator.Text = userPrincipal.GivenName + " " + userPrincipal.Surname;
                                        dtStart.Value = DateTime.Now;
                                        dtEnd.Value = DateTime.Now;
                                        if (rdRecodType.Items.Count > 0)
                                            rdRecodType.SelectedIndex = 0;
                                       
                                        if (cmbLanguage.Items.Contains("English"))                                        
                                            cmbLanguage.Text = "English";
                                        else
                                             if (cmbLanguage.Items.Count > 0)
                                            cmbLanguage.SelectedIndex = 0;
                                    }
                                    else
                                    {
                                        lstBagItProcess.Items.Add(Path.GetFileName(ProcessPath) + ":Failed while transferring to SFTP server. ");
                                        lstBagItProcess.BackColor = Color.LightCoral;
                                        Logger.LogException(Path.GetFileName(ProcessPath) + ":  Failed while transferring to SFTP server.");
                                        lstBagItProcess.HorizontalScrollbar = true;
                                    }
                                }
                                //Move the processed directory to Processed_Archive directory
                                try
                                {
                                    ProcessedArchive = ProcessedArchive + @"\" + Path.GetFileName(ProcessPath);                                    
                                 
                                    Directory.Move(ProcessPath, ProcessedArchive); 
                                }catch (Exception ex)
                                { 
                                    Logger.LogInformation("Error while moving the Processed folder to Archive:"+ex.Message);
                                }
                                                           
                                 this.progressBar1.Value = 100;
                               
                                    Logger.LogInformation(Path.GetFileName(ProcessPath) + " moved to Processed Archive folder.");
                                    lstBagItProcess.Items.Add(Path.GetFileName(ProcessPath) + " moved to Processed Archive folder.");
                                    lstBagItProcess.HorizontalScrollbar = true;
                                progressBar1.PerformStep();
                            }

                            if (subDirs.Length > 0)
                            {
                                tabConfiguration.SelectedIndex = 1;
                            }
                            else
                            {
                                tabConfiguration.SelectedIndex = 0;
                                Logger.LogInformation(" There is no folder to create bag in the Main Directory ");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Main method Exception :" + ex.Message);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                LoadProperties();
                bValidateFlag = false;
                Cursor.Current = Cursors.Default;
                Logger.LogInformation("Bagit process execution completed:" + DateTime.Now);
                Logger.LogInformation("--------------------------------------------------------------");
            }
        }
        
        /// <summary>
        /// Valid the SFTP user Authentication and process the file path
        /// </summary>
        /// <param name="ProcessPath"></param>
        /// <returns></returns>
        private static Boolean SFTPBagTransfer(string ProcessPath)
        {
            Boolean bretrunval = false;
            try
            {
                Logger.LogInformation("SFTP Process started");
                KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(SFTPUserName);
                keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
                conInfo = new ConnectionInfo(HostName, SFTPPort, SFTPUserName, keybAuth);
                using (sftp = new SftpClient(conInfo))
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        Logger.LogInformation("SFTP connected");
                        if (Directory.Exists(ProcessPath))
                        {
                            sftp.ChangeDirectory(SFTP_Directory_Path);
                            RecurseDirectorySearch(ProcessPath);
                            Console.WriteLine("Listing directory:");
                        }
                    }
                    bretrunval = true;
                    sftp.Disconnect();

                }
            }
            catch (Exception ex)
            {
                bretrunval = false;
                Logger.LogException(ProcessPath + ": Failed Bag file transfer to SFTP server. Exception: " + ex.Message);
            }
            return bretrunval;
        }
      
        /// <summary>
        /// Create and validate bag using python script
        /// </summary>
        /// <param name="ProcessPath"></param>
        /// <returns></returns>
        private Boolean CreateValidateBag(string ProcessPath)
        {
            Boolean bReturnval = false;
            Boolean successflag = true;
            string[] successmsg = SuccessBagMessage.Split('|');
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(@"cmd.exe")
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true
                };
                Process proc = new Process() { StartInfo = psi };
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                proc.Start();
                proc.StandardInput.WriteLine(CommandScript(ProcessPath));
                proc.StandardInput.Flush();
                proc.StandardInput.Close();
                proc.WaitForExit();
                SuccessResult = proc.StandardOutput.ReadToEnd();
                ErrorsResult = proc.StandardError.ReadToEnd();
                proc.Close();

                Logger.LogInformation("Success Result:" + SuccessResult);
                if (ErrorsResult != string.Empty)
                {
                    Logger.LogInformation("Error:" + ErrorsResult);
                     }
                else
                {
                    for (int loop = 0; loop < successmsg.Length; loop++)
                    {
                        if (!SuccessResult.Contains(successmsg[loop]))
                            successflag = false;
                    }
                    if (!successflag)
                    {
                        ErrorsResult = "Error Occurs";
                    }
                }

                if (ErrorsResult == string.Empty)
                    bReturnval = true;
                return bReturnval;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ProcessPath + ": Bag creation/validation failed");
                Logger.LogException(ProcessPath + ": Bag creation/validation failed. Exception: " + ex.Message);
                Logger.LogException(ProcessPath + ": Bag creation/validation failed. Result: " + SuccessResult);
                Logger.LogException(ProcessPath + ": Bag creation/validation failed. ErrorsResult: " + ErrorsResult);
                return bReturnval;
            }
        }

        /// <summary>
        /// Build the command script
        /// </summary>
        /// <param name="bagpath"></param>
        /// <returns></returns>
        private static string CommandScript(string bagpath)
        {
            StringBuilder sb = new StringBuilder();
                sb.Append(PythonExePath + Environment.NewLine);
                sb.Append(PythonDirectory + Environment.NewLine);
                sb.Append("python.exe " + "\"" + create_bag_path + "\"" + " " + "\"" + bagpath + "\"" + Environment.NewLine);
                sb.Append("python.exe " + "\"" + validate_bag_path + "\"" + " " + "\"" + bagpath + "\"" + Environment.NewLine);            
                return sb.ToString();
        }

        /// <summary>
        /// Upload the files and directories in the SFTP location
        /// </summary>
        /// <param name="dir"></param>
        public static void RecurseDirectorySearch(string dir)
        {
            string Dirpath = string.Empty;
            string SFTPPATH = string.Empty;
            string sftpDirec = string.Empty;
            string parentDirect = string.Empty;
            int intindex = 0;
            try
            {
                Dirpath = Path.GetFullPath(dir).Replace(Path.GetFullPath(dir).Substring(0, Base_processPath.Length), "");
                if (Dirpath.Length > 0 && Dirpath.StartsWith("\\"))
                {
                    intindex = Dirpath.LastIndexOf("\\");
                    sftpDirec = Dirpath.Substring(intindex + 1);
                    if (intindex > 0)
                    {
                        parentDirect = Dirpath.Substring(0, Dirpath.LastIndexOf("\\"));
                        string[] parentDircs = parentDirect.Split(new char[] { '\\' });
                        sftp.ChangeDirectory("/");
                        sftp.ChangeDirectory(SFTP_Directory_Path);
                        foreach (var dirc in parentDircs)
                        {
                            if (dirc != string.Empty)
                                sftp.ChangeDirectory(dirc);
                        }
                    }
                    if (!IsDirectoryExists(sftpDirec))
                    {
                        sftp.CreateDirectory(sftpDirec);
                        sftp.ChangeDirectory(sftpDirec);
                    }
                }

                foreach (string f in Directory.GetFiles(dir)) 
                {
                    using (FileStream fs = new FileStream(f, FileMode.Open))
                    {
                        sftp.BufferSize = 4 * 1024;
                        sftp.UploadFile(fs, Path.GetFileName(f), true); //overrite if file exist
                    }

                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    RecurseDirectorySearch(d);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RecurseDirectorySearch: " +Base_processPath + " :"+ ex.Message);
            }
        }
        /// <summary>
        /// For searching the directory
        /// </summary>
        /// <param name="dir"></param>
        public static void DirectorySearch(string dir, string Dest_Direc_Path, int flag)
        {
            string Dirpath = string.Empty;
            string SFTPPATH = string.Empty;
            string sftpDirec = string.Empty;
            string parentDirect = string.Empty;
            
            try
            {
                 string[] files = Directory.GetFiles(dir, "*.*");
                string[] subDirs = Directory.GetDirectories(dir);
                if (flag == 0)
                {                   
                    Dest_Direc_Path = Dest_Direc_Path + "/" + Path.GetFileName(dir);
                    flag = 1;
                }
                sftp.ChangeDirectory(Dest_Direc_Path);               

                foreach (string f in files)
                {
                    using (FileStream fs = new FileStream(f, FileMode.OpenOrCreate))
                    {
                        sftp.BufferSize = 4 * 1024;
                        sftp.UploadFile(fs, Path.GetFileName(f), true); //overrite if file exist
                    }

                }
                foreach (string d in subDirs)
                {
                    if (!IsDirectoryExists(Path.GetFileName(d)))
                        sftp.CreateDirectory(Path.GetFileName(d));
                    DirectorySearch(d, Dest_Direc_Path + "/" + Path.GetFileName(d), 1);
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogException("DirectorySearch: " + ex.Message);
            }
        }

        /// <summary>
        /// Copy the specified files/folders to process
        /// </summary>
        /// <param name="SourcePath"></param>
        /// <param name="DestinationPath"></param>
        /// <returns></returns>
        private static bool CopyFolderContents(string SourcePath, string DestinationPath)
        {
            int destflaglarge = 0;
            SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
            DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";
            try
            {
                if (Directory.Exists(SourcePath))
                {
                   if(DestinationPath.Length>235)
                    {
                        destflaglarge = 1;
                        Logger.LogInformation("Destination path is too long:" + DestinationPath );
                    }
                    if (destflaglarge == 0)
                    {
                        if (Directory.Exists(DestinationPath) == false)
                        {
                            Directory.CreateDirectory(DestinationPath);
                        }
                        foreach (string files in Directory.GetFiles(SourcePath))
                        {
                            try
                            {
                                if (File.Exists(files))
                                {
                                    FileInfo fileInfo = new FileInfo(files);
                                       string destFile = System.IO.Path.Combine(DestinationPath, fileInfo.Name);
                                        fileInfo.CopyTo(destFile, true);
                                }
                            }
                            catch (Exception ex) { Logger.LogException("CopyFolderContents:File path is too long: " + files + " :Error Message" + ex.Message); }
                        }
                    }

                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(drs);
                        if (CopyFolderContents(drs, DestinationPath + directoryInfo.Name) == false)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException("CopyFolderContents: " + ex.Message);  
                return false;
            }
        }
        /// <summary>
        /// For checking the directory exist or not
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsDirectoryExists(string path)
        {
            bool isDirectoryExist = false;
            try
            {
                sftp.ChangeDirectory(path);
                isDirectoryExist = true;
            }
            catch (SftpPathNotFoundException)
            {
                return false;
            }
            return isDirectoryExist;
        }

        private static void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        {
            try
            {
                foreach (AuthenticationPrompt prompt in e.Prompts)
                {
                    if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        prompt.Response = SFTPPassword;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Form Load event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmBagitprocess_Load(object sender, EventArgs e)
        {
            try
            {
                Logger.LogInformation("------------------------------------------------------");
                Logger.LogInformation("Bagit process execution Started:" + DateTime.Now);
                dtStart.Value = DateTime.Now;
                dtEnd.Value = DateTime.Now;
                LoadProperties();
                bLoadFlag = true;
                PythonExeCheck();
            }

            catch (Exception ex)
            {
                Logger.LogException("Main Exception :" + ex.Message);
            }
        }
        private void PythonExeCheck()
        {
            try
            {
                string Pythonpath = PythonExePath.Replace("cd", "").Trim();
                PythonExePath = string.Empty;
                if (!Directory.Exists(Pythonpath))
                {
                    string[] getDir = Pythonpath.Split('/');
                    string prgPath = @PythonDirectory + "/Program Files/" + getDir[1];
                    string prgPath86 = @PythonDirectory + "/Program Files (x86)/" + getDir[1];
                    if (Directory.Exists(prgPath))
                    {
                        PythonExePath = "cd " + prgPath;
                    }
                    else if (Directory.Exists(prgPath86))
                    {
                        PythonExePath = "cd " + prgPath86;
                    }
                }
                else
                {
                    PythonExePath = "cd " + Pythonpath;
                }
                if (PythonExePath == string.Empty)
                    Logger.LogInformation("There is no Python EXE location:" + PythonExePath);
                else
                    Logger.LogInformation("Python EXE location:" + PythonExePath);

            }
            catch (Exception ex)
            { 
                Logger.LogException("Python exe verfication :" + ex.Message);
            }
        }
        /// <summary>
        /// Load the field infomration from configuration
        /// </summary>
        private void LoadProperties()
        {
            try
            {
                this.progressBar1.Value = 1;
                this.progressBar1.Step = 1;
                this.progressBar1.Visible = false;
               
                SFTP_Directory_Path = objprop.SFTPDirectory;
                SFTPUserName = objprop.SFTPUserName;
                SFTPPassword = objprop.SFTPPassword;
                SFTPPort = Convert.ToInt32(objprop.SFTPPort);
                PythonDirectory = objprop.PythonDirectory;
                PythonExePath = objprop.PythonExePath;               
                BagitProcessDirName = objprop.BagitDirectoryName;
                ProcessFolderLength = Application.StartupPath.Length + BagitProcessDirName.Length + 15;
                
                ProcessPath = @"\\?\"+ Application.StartupPath + "\\" + objprop.ProcessDirectory;
                ProcessedArchive = @"\\?\" + Application.StartupPath + "\\" + objprop.ProcessArchiveDirectory;
                bagTemplatepath = @"\\?\" + Application.StartupPath + "\\" + objprop.BagitDirectory + "\\" + objprop.Template_bag; 
                create_bag_path = @"\\?\" + Application.StartupPath + "\\" + objprop.BagitDirectory + "\\" + objprop.Create_bag;
                validate_bag_path = @"\\?\" + Application.StartupPath + "\\" + objprop.BagitDirectory + "\\" + objprop.Validate_bag;
                SuccessBagMessage = objprop.SuccessBagMessage;
                Base_processPath = ProcessPath;
                Profile_Json_Url = objprop.Profile_Json;
                if (!bLoadFlag)
                {
                    txtSourceOrg.Text = objprop.SourceOrganization;

                    Logger.LogInformation("ProcessPath:" + ProcessPath);
                    Logger.LogInformation("ProcessedArchive:" + ProcessedArchive);
                    Logger.LogInformation("bagTemplatepath:" + bagTemplatepath);
                    Logger.LogInformation("create_bag_path:" + create_bag_path);
                    Logger.LogInformation("validate_bag_path:" + validate_bag_path);
                    Logger.LogInformation("SFTP_Directory_Path:" + SFTP_Directory_Path);

                    if (!Directory.Exists(ProcessPath))
                    { Directory.CreateDirectory(ProcessPath); }

                    if (!Directory.Exists(ProcessedArchive))
                    { Directory.CreateDirectory(ProcessedArchive); }

                    ReadRecordType();
                                    
                   var LangCollection = ConfigurationManager.GetSection("ISOLanguages") as NameValueCollection;

                    foreach (var kv in LangCollection.AllKeys.OrderBy(k => k))//ConfigurationManager.GetSection("ISOLanguages") as Enumerable())
                    {
                        if (!cmbLanguage.Items.Contains(kv))
                        {
                            cmbLanguage.Items.Add(kv);
                        }
                    }
                    if(cmbLanguage.Items.Contains("English"))                    
                        cmbLanguage.Text = "English";                   
                    else
                        if(cmbLanguage.Items.Count >0)
                        cmbLanguage.SelectedIndex = 0;
                }              
                txtRecordCreator.Text = userPrincipal.GivenName + " " + userPrincipal.Surname;
            }
            catch (Exception ex)
            {
                Logger.LogException("Load Properties Exception :" + ex.Message);
            }
        }
        
        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                string strTip = "";
                int nIdx = lstBagItProcess.IndexFromPoint(e.Location);
                if ((nIdx >= 0) && (nIdx < lstBagItProcess.Items.Count))
                    strTip = lstBagItProcess.Items[nIdx].ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void txtSourceOrga_Validating(object sender, CancelEventArgs e)
        {
                TextBox objTextBox = (TextBox)sender;

                if ((objTextBox.Text.Trim() == string.Empty) && (bValidateFlag))
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblSourceOrganization.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }            
            
        }

        private void txtInternalSenderDescription_Validating(object sender, CancelEventArgs e)
        {
                TextBox objTextBox = (TextBox)sender;
                if ((objTextBox.Text.Trim() == string.Empty) && (bValidateFlag))
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblInternalSenderDescription.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }
        }

        private void txtTitle_Validating(object sender, CancelEventArgs e)
        {
                TextBox objTextBox = (TextBox)sender;
                if ((objTextBox.Text.Trim() == string.Empty) && (bValidateFlag))
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblTitle.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }
        }

        private void dtStart_Validating(object sender, CancelEventArgs e)
        {
                DateTimePicker objTextBox = (DateTimePicker)sender;
                if (objTextBox.Text.Trim() == string.Empty)
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblDateStart.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }
        }

        private void dtEnd_Validating(object sender, CancelEventArgs e)
        {
                DateTimePicker objTextBox = (DateTimePicker)sender;
                if (objTextBox.Text.Trim() == string.Empty)
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblDateEnd.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }
        }

        private void txtRecordType_Validating(object sender, CancelEventArgs e)
        {
                TextBox objTextBox = (TextBox)sender;
                if (objTextBox.Text.Trim() == string.Empty)
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblRecordType.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }
        }

        private void txtBagGroupIdentifier_Validating(object sender, CancelEventArgs e)
        {
                TextBox objTextBox = (TextBox)sender;
                if (objTextBox.Text.Trim() == string.Empty)
                {
                    e.Cancel = true;
                    BagitProcessErrorProvider.SetError(objTextBox, lblBagGroupIdentifier.Text + " " + "Required");
                }
                else
                {
                    BagitProcessErrorProvider.SetError(objTextBox, null);
                }
        }

        private void frmBagitprocess_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;

        }

        private void txtInternalSenderDesc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;
        }

        private void txtInternalSenderDesc_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void txtSourceOrg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;
        }

        private void txtSourceOrg_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void txtTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;
        }

        private void txtTitle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void txtExternalId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;

        }

        private void txtExternalId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void cmbRecordCreator_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void cmbLanguage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

      

        private void txtRecordCreator_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void txtRecordCreator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;
        }

        private void btnLstremove_Click(object sender, EventArgs e)
        {
            try
            {                
                foreach (ListViewItem itemSelected in lstview.SelectedItems)
                {
                    lstview.Items.Remove(itemSelected);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Error while deleting the list item:"  + ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Reset the controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtTitle.Text = string.Empty;
            lstview.Items.Clear();
            //txtSourceOrg.Text = string.Empty;
            txtInternalSenderDesc.Text = string.Empty;
            txtExternalId.Text = string.Empty;
            dtStart.Value = DateTime.Now;
            dtEnd.Value = DateTime.Now;
            rdRecodType.SelectedIndex = 0;
            txtBagcnt.Text = string.Empty;
            txtbagcntmax.Text = string.Empty;
            txtBagGrpIdentifier.Text = string.Empty;
            txtRecordCreator.Text = userPrincipal.GivenName + " " + userPrincipal.Surname;
            rdRecodType.Text = string.Empty;            
            if (cmbLanguage.Items.Contains("English"))            
                cmbLanguage.Text = "English";
            else
                 if (cmbLanguage.Items.Count > 0)
                cmbLanguage.SelectedIndex = 0;

        }
        /// <summary>
        /// Load the record type from profile json URL
        /// </summary>
        private void ReadRecordType()
        {
            try
            {
             
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Profile_Json_Url);
                request.ContentType = "application/json";
                request.Accept = "application/json";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                string profileJson = reader.ReadToEnd();
                dynamic array = JObject.Parse(profileJson);
                JObject jObject = JObject.Parse(profileJson);               

                string version = (string)jObject["BagIt-Profile-Info"]["Version"];
                string type = (string)jObject["Bag-Info"]["Record-Type"]["Values"];

                JArray jsonvl = jObject["Bag-Info"]["Record-Type"]["values"] as JArray;
                foreach (dynamic array1 in jsonvl)
                {
                    rdRecodType.Items.Add(array1.Value);

                }
                rdRecodType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException("Error while reading ReadRecordType from URL:"+ Profile_Json_Url +",Error " + ex.Message);
               
            }
        }

        private void txtBagcnt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void txtbagcntmax_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {                
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != '?'))
                {
                    e.Handled = true;
                }
                if (e.KeyChar== '?')
                { txtbagcntmax.Text = string.Empty ;
                }
                else
                { txtbagcntmax.Text = txtbagcntmax.Text.Replace('?', ' ').Trim();   }
            }
            catch (Exception) { }
        }
        
    }
}


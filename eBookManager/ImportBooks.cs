using eBookManager.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static eBookManager.Helper.ExtensionMethods;
using static System.Math;

namespace eBookManager
{
    public partial class ImportBooks : Form
    {
        //Declarations
        //The _jsonPath variable will contain the path to the file used to store ebook information
        private string _jsonPath;
        private List<StorageSpace> _space;
        private enum _storageSpaceSelection
        {
            New = -9999, NoSelection = -1
        }
        public ImportBooks()
        {
            InitializeComponent();

            //this code line below is to modify the constructor
            //_jsonPath will be initialized in the excuting folder for the application
            //and the file will be hardcoded to bookData.txt
            //(Note: planning to provide a setting screen to configure these settings)
            _jsonPath = Path.Combine(Application.StartupPath, "bookData.txt");
        }

        //Load data when the form loads
        //the Form_load event is attached
        //the event should load the following code from the data store asynchronously
        private async void ImportBooks_Load(object sender, EventArgs e)
        {
            _space = await _space.ReadFromDataStore(_jsonPath);
        }

        //Anothe two enumerators
        //that define the file extensions that will be able to save in the application.
        private HashSet<string> AllowedExtensions => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ".doc",
            ".docx",
            ".pdf",
            ".epub",
            ".lit"
        };
        private enum Extension
        {
            doc = 0,
            docx = 1,
            pfd = 2,
            epub = 3,
            lit = 4
        }

        //Populating the TreeView control with files and folders found at the selected source locathion
        //using the PolpulateBookList() method
        //1st call the method
        public void PopulateBookList(string paramDir, TreeNode paramNote)
        {
            DirectoryInfo dir = new DirectoryInfo(paramDir);
            foreach (DirectoryInfo dirInfo in dir.GetDirectories())
            {
                TreeNode node = new TreeNode(dirInfo.Name);
                node.ImageIndex = 4;
                node.SelectedImageIndex = 5;

                if (paramNote != null)
                    paramNote.Nodes.Add(node);
                else
                    tvFoundBooks.Nodes.Add(node);
                PopulateBookList(dirInfo.FullName, node);
            }
            foreach (FileInfo fileInfo in dir.GetFiles().Where(x => AllowedExtensions.Contains(x.Extension)).ToList())
            {
                TreeNode node = new TreeNode(fileInfo.Name);
                node.Tag = fileInfo.FullName;
                int iconIndex = Enum.Parse(typeof(Extension), fileInfo.Extension.TrimStart('.'), true).GetHashCode();
                node.ImageIndex = iconIndex;
                node.SelectedImageIndex = iconIndex;

                if (paramNote != null)
                    paramNote.Nodes.Add(node);
                else
                    tvFoundBooks.Nodes.Add(node);
            }
        }

        //Select Folder button on WinForm Design
        //see the 1st called the method, its obviosly from within itself
        //and its a recursive method
        //2nd, it will be called from the btnSelectSourceFolder button click
        private void btnSelectSourceFolder_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Select the location of your eBooks and Documents";
                DialogResult dialogResult = folderBrowserDialog.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    tvFoundBooks.Nodes.Clear();
                    string path = folderBrowserDialog.SelectedPath;

                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    TreeNode root = new TreeNode(directoryInfo.Name);
                    root.ImageIndex = 4;
                    root.SelectedImageIndex = 5;
                    tvFoundBooks.Nodes.Add(root);
                    PopulateBookList(directoryInfo.FullName, root);
                    tvFoundBooks.Sort();
                    root.Expand();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        // --->This is straightforward
        //    >Select the folder to recurse and populate the TreeView control
        //    with all the files found that match the file extension contained in the AllowedExtensions property
        //
        //--->Now looking at the code when someone selects a book in the tvFoundBooks TreeView control
        //   >When a book is selected -> read the properties of the selected file and return those properties to the file detais section:
        //   >therefor see the code below
        private void tvFoundBooks_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DocumentEngine documentEngine = new DocumentEngine();
            string path = e.Node.Tag?.ToString() ?? "";

            if (File.Exists(path))
            {
                var (dateCreated, dateLastAccessed, fileName, fileExtension, fileLength, hasError) = documentEngine.GetFileProperties(e.Node.Tag.ToString());

                if (!hasError)
                {
                    txtFileName.Text = fileName;
                    txtExtension.Text = fileExtension;
                    dtCreated.Value = dateCreated;
                    dtLastAccessed.Value = dateLastAccessed;
                    txtFilePath.Text = e.Node.Tag.ToString();
                    txtFileSize.Text = $"{Round(fileLength.ToMegabytes(), 2).ToString()} MB";
                }
            }
        }
    }
}

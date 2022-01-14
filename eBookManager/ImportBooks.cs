using eBookManager.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private List<StorageSpace> _spaces;
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

        //Loading data when the form loads
        //the Form_load event is attached
        //the event should load the following code from the data store asynchronously
        private async void ImportBooks_Load(object sender, EventArgs e)
        {
            _spaces = await _spaces.ReadFromDataStore(_jsonPath);

            //Call to the new method PopulateStorageSpaceList()
            //dlVirtualStorageSpaces
            PopulateStorageSpaceList();

            if (dlVirtualStorageSpaces.Items.Count == 0)
            {
                dlVirtualStorageSpaces.Items.Add("<create new storage space> ");
            }
            lblEbookCount.Text = "";
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

        //Populating the storage space list
        //The PopulatingStorageSpaceList() method is using a local function
        //essentially the function will allow to declare a piece of functionality that is accessble only from within its parent.
        private void PopulateStorageSpaceList()
        {
            List<KeyValuePair<int, string>> listSpaces = new List<KeyValuePair<int, string>>();
            BindStorageSpaceList((int)_storageSpaceSelection.NoSelection, "Select Storage Space");
            void BindStorageSpaceList(int key, string value) => listSpaces.Add(new KeyValuePair<int, string>(key, value));

            if (_spaces is null || _spaces.Count() == 0) //Pattern matching
            {
                BindStorageSpaceList((int)_storageSpaceSelection.New, "<create new>");
            }
            else
            {
                foreach (var space in _spaces)
                {
                    BindStorageSpaceList(space.ID, space.Name);
                }
            }
            dlVirtualStorageSpaces.DataSource = new BindingSource(listSpaces, null);
            dlVirtualStorageSpaces.DisplayMember = "Value";
            dlVirtualStorageSpaces.ValueMember = "Key";
        }

        //Logic for changing the selected storage space
        //The SelectedIndexChanged() event of the dlVirtualStorageSpaces control is modified
        private void dlVirtualStorageSpaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedValue = dlVirtualStorageSpaces.SelectedValue.ToString().ToInt();

            if (selectedValue == (int)_storageSpaceSelection.New) //-9999
            {
                txtNewStorageSpaceName.Visible = true;
                lblStorageSpaceDescription.Visible = true;
                txtStorageSpaceDescription.ReadOnly = false;
                btnSaveNewStorageSpace.Visible = true;
                btnCancelNewStorageSpaceSave.Visible = true;
                dlVirtualStorageSpaces.Enabled = false;
                btnAddNewStorageSpace.Enabled = false;
                lblEbookCount.Text = "";
            }
            else if (selectedValue != (int)_storageSpaceSelection.NoSelection)
            {
                //Find the contents of the selected storage space
                int contentCount = (from c in _spaces where c.ID == selectedValue select c).Count();

                if (contentCount > 0)
                {
                    StorageSpace selectedSpace = (from c in _spaces where c.ID == selectedValue select c).First();
                    txtStorageSpaceDescription.Text = selectedSpace.Description;
                    List<Document> eBooks = (selectedSpace.BookList == null)
                        ? new List<Document> { }
                        : selectedSpace.BookList;
                    lblEbookCount.Text = $"Storage Space Contains { eBooks.Count() } { (eBooks.Count() == 1 ? "eBooks" : "eBooks") }";
                }
            }
            else
            {
                lblEbookCount.Text = "";
            }
        }

        //Save a new storage space
        private void btnSaveNewStorageSpace_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNewStorageSpaceName.Text.Length != 0)
                {
                    string newName = txtNewStorageSpaceName.Text;
                    bool spaceExists = (!_spaces.StorageSpaceExists(newName, out int nextID))
                        ? false
                        : throw new Exception("The storage space you are trying to add already exists.");
                    if (!spaceExists)
                    {
                        StorageSpace newSpace = new StorageSpace();
                        newSpace.Name = newName;
                        newSpace.ID = nextID;
                        newSpace.Description = txtStorageSpaceDescription.Text;
                        _spaces.Add(newSpace);

                        PopulateStorageSpaceList();
                        //Save new storage space name
                        txtNewStorageSpaceName.Clear();
                        txtNewStorageSpaceName.Visible = false;
                        lblStorageSpaceDescription.Visible = false;
                        txtStorageSpaceDescription.ReadOnly = true;
                        txtStorageSpaceDescription.Clear();
                        btnSaveNewStorageSpace.Visible = false;
                        btnCancelNewStorageSpaceSave.Visible = false;
                        dlVirtualStorageSpaces.Enabled = true;
                        btnAddNewStorageSpace.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {

                txtNewStorageSpaceName.SelectAll();
                MessageBox.Show(ex.Message);
            }
        }
        //Save eBooks in the selected virtual storage space by clicking Add Book button (btnAddeBookToStorageSpace)
        private async void btnAddeBookToStorageSpace_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedStorageSpaceID = dlVirtualStorageSpaces.SelectedValue.ToString().ToInt();

                if ((selectedStorageSpaceID != (int)_storageSpaceSelection.NoSelection) && (selectedStorageSpaceID != (int)_storageSpaceSelection.New))
                {
                    await UpdateStorageSpaceBooks(selectedStorageSpaceID);
                }
                else throw new Exception("Please select a Storage Space to add your eBook to"); // throw expressions
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        //Saving a selected eBook to a storage space
        private async Task UpdateStorageSpaceBooks(int selectedStorageSpaceID)
        {
            try
            {

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    }
}

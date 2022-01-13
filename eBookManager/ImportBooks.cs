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

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace eBookManager.Engine
{
    public class DocumentEngine
    {
        //GetFileProperties() method that will return the properties of a selected file
        public (DateTime dateCreated, DateTime dateLastAccessed, string fileName, string fileExtension, long fileLength, bool error) GetFileProperties(string filePath)
        {
            var returnTuble = (created: DateTime.MinValue,
                lastDateAccessed: DateTime.MinValue,
                name: "",
                ext: "",
                fileSize: 0L,
                error: false);
            try
            {
                FileInfo fi = new FileInfo(filePath);
                fi.Refresh();
                returnTuble = (fi.CreationTime, fi.LastAccessTime, fi.Name, fi.Extension, fi.Length, false);
            }
            catch
            {

                returnTuble.error = true;
            }

            return returnTuble;
        }
    }
}

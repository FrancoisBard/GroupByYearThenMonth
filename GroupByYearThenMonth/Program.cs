using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace GroupByYearThenMonth
{
    /// <summary>
    /// Sort the files in the five folder by the year and month when the phot was taken.
    /// For each file, it checks if it is an image and has a PropertyTagExifDTOrig PropertyItem tag.
    /// If it does, it moves the file in the correct folder.
    /// (If the directory path was DIR/, the file is moved to DIR/YEAR/MONTH/).
    /// Other files are ignored.
    /// The program doesn't look for files in subfolders.
    /// </summary>
    class Program
    {
        /// <summary>
        /// PropertyItem.Id : PropertyTagExifDTOrig
        /// 
        /// Date and time when the original image data was generated. 
        /// For a DSC, the date and time when the picture was taken. 
        /// The format is YYYY:MM:DD HH:MM:SS with time shown in 24-hour format and the date and time separated by one blank character (0x2000). 
        /// The character string length is 20 bytes including the NULL terminator. 
        /// When the field is empty, it is treated as unknown.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms534416%28v=vs.85%29.aspx
        /// </summary>
        private const int PropertyTagExifDTOrig = 0x9003;
        private const int YearStartIndex = 0;
        private const int YearStringLength = 4;
        private const int MonthStartIndex = 5;
        private const int MonthStringLength = 2;

        static void Main(string[] args)
        {
            Console.Write("This program will sort your images based on the date they were taken.\n");

            MoveFiles();

            Console.Write("Press any key to exit.\n");
            Console.ReadKey();
        }

        private static void MoveFiles()
        {
            var path = ConsoleReadPathOrDefault();

            if (!Directory.Exists(path))
            {
                Console.Write("The directory doesn't exist or is not valid.\n");
                return;
            }

            MoveFiles(path);
        }

        private static void MoveFiles(string path)
        {
            var files = Directory.GetFiles(path);
            var filesCount = files.Length;
            var fileNumber = 0;

            foreach (var file in files)
            {
                if (fileNumber % 100 == 0)
                {
                    Console.Write(String.Format("Treated {0} of {1} files.\n", fileNumber, filesCount));
                }

                fileNumber++;

                try
                {
                    MoveFile(file);
                }
                catch (Exception ex)
                {
                    Console.Write(String.Format("Could not treat file {0}\n", file));
                    Console.Write(String.Format("Exception : {0}\n", ex.Message));
                    Console.Write(String.Format("Stack : {0}\n", ex.StackTrace));
                }
            }
        }

        private static void MoveFile(string file)
        {
            var propertyItem = GetPropertyTagExifDTOrig(file);

            if (propertyItem == null)
            {
                return;
            }

            MoveFile(file, propertyItem);
        }

        private static void MoveFile(string file, PropertyItem propertyItem)
        {
            string newDirectoryPathCandidate;
            string newFilePathCandidate;
            GetNewDirectoryAndFilePathCandidates(file, propertyItem, out newDirectoryPathCandidate, out newFilePathCandidate);

            MoveFile(file, newDirectoryPathCandidate, newFilePathCandidate);
        }

        private static void GetNewDirectoryAndFilePathCandidates(string file, PropertyItem propertyItem, out string newDirectoryPathCandidate, out string newFilePathCandidate)
        {
            string year;
            string month;
            GetYearAndMonth(propertyItem, out year, out month);

            newDirectoryPathCandidate = String.Format(
                "{1}{0}{2}{0}{3}",
                Path.DirectorySeparatorChar,
                Path.GetDirectoryName(file),
                year,
                month);

            newFilePathCandidate = String.Format(
                "{0}{1}{2}",
                newDirectoryPathCandidate,
                Path.DirectorySeparatorChar,
                Path.GetFileName(file));
        }

        private static void GetYearAndMonth(PropertyItem propertyItem, out string year, out string month)
        {
            var dateTime = Encoding.ASCII.GetString(propertyItem.Value);
            
            year = dateTime.Substring(YearStartIndex, YearStringLength);
            month = dateTime.Substring(MonthStartIndex, MonthStringLength);
        }

        private static void MoveFile(string file, string newDirectoryPathCandidate, string newFilePathCandidate)
        {
            try
            {
                File.Move(file, newFilePathCandidate);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(newDirectoryPathCandidate);
                File.Move(file, newFilePathCandidate);
            }
        }

        private static string ConsoleReadPathOrDefault()
        {
            Console.Write("Enter a path to directory, or leave it blank to use the current directory:\n");

            var path = Console.ReadLine(); //todo i wish i could get tab autocompletion... isn't it possible in an easy way ?

            if (string.IsNullOrWhiteSpace(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            return path;
        }

        private static PropertyItem GetPropertyTagExifDTOrig(string file)
        {
            try
            {
                using (var image = Image.FromFile(file))
                {
                    return image.PropertyItems.FirstOrDefault(p => p.Id == PropertyTagExifDTOrig);
                }
            }
            catch (OutOfMemoryException)
            {
                //not a valid Image, we don't want to throw an exception, just to continue silently
                return null;
            }
        }
    }
}

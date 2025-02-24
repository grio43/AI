using System;
using System.Text;
using SharedComponents.EVE;
using SharedComponents.ISBELExtensions;

namespace SharedComponents.Web
{
    public class CookieStorage
    {
        public static string GetCookieStoragePath()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "ISBoxer EVE Launcher","Cookies");
            Cache.Instance.Log("GetCookieStoragePath: path [" + path + "]");
            System.IO.Directory.CreateDirectory(path);

            return path;
        }

        public static string GetCookiesFilename(string eveAccountName)
        {
            string filename = eveAccountName.ToLowerInvariant().SHA256();
            return System.IO.Path.Combine(GetCookieStoragePath(), filename);
        }

        public static string GetCookies(string eveAccountName)
        {
            try
            {
                string filePath = GetCookiesFilename(eveAccountName);
                string tempCookieAsString = System.IO.File.ReadAllText(filePath, Encoding.ASCII);
                Cache.Instance.Log("GetCookies: filePath [" + filePath + "] Cookie (from file): [" + tempCookieAsString + "]");
                return tempCookieAsString;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void WriteAllTextSafe(string filename, string text, Encoding encoding)
        {
            string tempPath = System.IO.Path.GetTempPath();
            string tempFile = System.IO.Path.Combine(tempPath, System.IO.Path.GetRandomFileName());
            string backupFile = System.IO.Path.Combine(tempPath, System.IO.Path.GetRandomFileName());

            System.IO.File.WriteAllText(tempFile, text, encoding);

            bool fileExists = System.IO.File.Exists(filename);
            if (fileExists)
            {
                try
                {
                    System.IO.File.Move(filename, backupFile);
                }
                catch
                {

                }
            }

            try
            {
                System.IO.File.Copy(tempFile, filename, true);

                System.IO.File.Delete(tempFile);
            }
            catch (Exception)
            {
                if (fileExists)
                {
                    try
                    {
                        System.IO.File.Move(backupFile, filename);
                    }
                    catch
                    {

                    }
                }
                throw;
            }
        }

        public static void SetCookies(string eveAccountName, string cookies)
        {
            string filePath = GetCookiesFilename(eveAccountName);
            Cache.Instance.Log("SetCookies: filePath [" + filePath + "] cookies (to file) [" + cookies + "]");
            WriteAllTextSafe(filePath, cookies, Encoding.ASCII);
        }

        public static void DeleteCookies(string eveAccountName)
        {
            string filePath = GetCookiesFilename(eveAccountName);
            Cache.Instance.Log("DeleteCookies: filePath [" + filePath + "] Deleting Cookies");
            System.IO.File.Delete(filePath);
        }
    }
}

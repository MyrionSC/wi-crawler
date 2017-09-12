namespace we_crawler
{
    public class Utils
    {
        public static string getBaseDir(string basedir)
        {
            if (basedir.Substring(basedir.Length - 10, 10) == "bin/Debug/")
            {
                basedir = basedir.Substring(0, basedir.Length - 10);
            }
            return basedir;
        }
    }
}
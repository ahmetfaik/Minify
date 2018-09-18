using NUglify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace AhmetFaik.Minify
{
    class Program
    {
        static void Main(string[] args)
        {
            List<FileInfo> cssFileList = new List<FileInfo>();
            List<FileInfo> jsFileList = new List<FileInfo>();
            long orginalDataLength = 0;
            long newDataLength = 0;

            Console.WriteLine("Düzenlenmesini istediğiniz ana dizin'i giriniz.");
            var mainDirectory = Console.ReadLine();

            Console.WriteLine("Dizine bağlantı sağlanıyor...");
            DirectoryInfo directoryInfo;
            try
            {
                directoryInfo = new DirectoryInfo(mainDirectory);

                List<string> directories = Directory.GetDirectories(mainDirectory, "*", SearchOption.AllDirectories).ToList();
                foreach (var item in directories)
                {
                    directoryInfo = new DirectoryInfo(item);
                    GetDirectoryAllFilesByExtension(directoryInfo, ref cssFileList, ref jsFileList); //Dosylar burada set ediliyor.
                }

                Console.WriteLine("Dizine bağlantı sağlandı.");
                Console.WriteLine($"Düzenlenecek {cssFileList.Count} adet CSS mevcut.");
                Console.WriteLine($"Düzenlenecek {jsFileList.Count} adet Js mevcut.");
                Console.WriteLine("CSS ve JS dosyaları düzenleniyor...");

                foreach (var file in cssFileList)
                {
                    string Content = "";
                    if (File.Exists(file.FullName) && HasWritePermissionOnDir(file.FullName))
                    {
                        try
                        {
                            Content = File.ReadAllText(file.FullName);
                            var result = Uglify.Css(Content); //Css dönüşümü sağlıyor

                            string outFile = !file.Name.Contains(".min.") ? file.FullName.Replace(".css", ".min.css") : file.FullName;
                            using (var streamWriter = new StreamWriter(outFile))
                                streamWriter.Write(result);

                            if (file.Name.Contains(".min."))
                                newDataLength += new FileInfo(outFile).Length;
                            if (!file.Name.Contains(".min."))
                                orginalDataLength += file.Length;

                            Console.WriteLine($"{outFile} Dosyası oluşturuldu.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{file.FullName} dosyası için bir hata oluştu. HATA: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Yazma İzni Hatası {file.FullName}");
                    }
                }

                foreach (var file in jsFileList)
                {
                    string Content = "";
                    if (File.Exists(file.FullName) && HasWritePermissionOnDir(file.FullName))
                    {
                        try
                        {
                            Content = File.ReadAllText(file.FullName);
                            var result = Uglify.Js(Content);  //Javascript dönüşümü sağlıyor

                            string outFile = !file.Name.Contains(".min.") ? file.FullName.Replace(".js", ".min.js") : file.FullName;
                            using (var streamWriter = new StreamWriter(outFile))
                                streamWriter.Write(result);

                            if (file.Name.Contains(".min."))
                                newDataLength += new FileInfo(outFile).Length;
                            if (!file.Name.Contains(".min."))
                                orginalDataLength += file.Length;

                            Console.WriteLine($"{outFile} Dosyası oluşturuldu.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{file.FullName} dosyası için bir hata oluştu. HATA: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Yazma İzni Hatası {file.FullName}");
                    }
                }
                Console.WriteLine($"Düzenleme İşlemi Bitti");
                Console.WriteLine($"Düzenlemeler sonrasında orjinal boyutu {orginalDataLength / 1000000}mb olan dosyalarınız {newDataLength / 1000000}mb'a kadar küçültülmüştür. Bölelikle {(orginalDataLength - newDataLength) / 1000000}mb tasarruf sağlanmıştır.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        private static void GetDirectoryAllFilesByExtension(DirectoryInfo directoryInfo, ref List<FileInfo> cssFileList, ref List<FileInfo> jsFileList)
        {
            foreach (var directory in directoryInfo.GetDirectories())
            {
                cssFileList.AddRange(directory.GetFiles("*.css"));
                jsFileList.AddRange(directory.GetFiles("*.js"));
            }
        }

        public static bool HasWritePermissionOnDir(string path)
        {
            var writeAllow = false;
            var writeDeny = false;
            var accessControlList = Directory.GetAccessControl(path);
            if (accessControlList == null)
                return false;
            var accessRules = accessControlList.GetAccessRules(true, true,
                                        typeof(System.Security.Principal.SecurityIdentifier));
            if (accessRules == null)
                return false;

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny;
        }
    }
}

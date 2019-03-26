using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;//参照追加

using DisclosureSqlite;

namespace Disclosures {
    public class ArchiveContainer {
        private byte[] buffer;
        private List<string> files;
        public string[] Files { get { return files.ToArray(); } }
        public ArchiveContainer(byte[] buf) {
            buffer = buf;
            files = new List<string>();
            using (Stream stream = new MemoryStream(buffer)) {
                using (var archive = new ZipArchive(stream)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        string filename = entry.FullName;
                        files.Add(filename);
                        if (entry.Name.ToLower().EndsWith(".xsd")) {
                            string source = Read(entry);
                            Taxonomy.ReadXsd(source);
                        }
                    }
                }
            }
        }
        public string GetSource(string filename) {
            string source = null;
            using (Stream stream = new MemoryStream(buffer)) {
                using (var archive = new ZipArchive(stream)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == filename) {
                            source = Read(entry);
                            break;
                        }
                    }
                }
            }
            return source;
        }

        public static string SaveImage(byte[] buffer, string filename) {
            using (Stream st = new MemoryStream(buffer)) {
                using (var archive = new ZipArchive(st)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == filename) {
                            using (Stream stream = entry.Open()) {
                                using (MemoryStream ms = new MemoryStream()) {
                                    stream.CopyTo(ms);
                                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms)) {
                                        string extension = Path.GetExtension(filename);
                                        string imagefile = Environment.CurrentDirectory + @"\pic" + extension;
                                        image.Save(imagefile);
                                        return imagefile;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static string Read(ZipArchiveEntry entry) {
            Encoding enc = Encoding.UTF8;
            if (entry.Name.EndsWith(".txt", false, System.Globalization.CultureInfo.CurrentCulture)
                | entry.Name.EndsWith(".csv", false, System.Globalization.CultureInfo.CurrentCulture))
                enc = Encoding.GetEncoding("shift_jis");
            using (Stream stream = entry.Open()) {
                using (StreamReader reader = new StreamReader(stream, enc)) {
                    return reader.ReadToEnd();
                }

            }
        }


    }

}
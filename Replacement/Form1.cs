using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Replacement
{
    public partial class Form1 : Form
    {
        private ReplaceSet rs;
        private List<string> fileList = new List<string>();
        private CancellationTokenSource cancelTokensource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var reader = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ReplaceSet.json")))
            {
                rs = (ReplaceSet)DynamicJson.Parse(reader.ReadToEnd());
            }
            fileList = Directory.GetFiles(rs.WorkSpace, "*.*", SearchOption.AllDirectories).ToList();
            Area.Text = Area.Text + rs.WorkSpace + " Total Files: " + fileList.Count;
        }

        async private void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;
            Area.Text = Area.Text + "\r\n" + "Processing...";

            cancelTokensource = new CancellationTokenSource();
            var cancelToken = cancelTokensource.Token;
            var p = new Progress<string>(ShowProgress);
            var result = await Task.Run(() => Process(p, cancelToken));
            if (result)
            {
                Area.Text = Area.Text + "\r\n" + "Processing Completion";
                CancleBtn.Enabled = false;
            }
            else
            {
                Area.Text = Area.Text + "\r\n" + "It was cancelled";
            }
            StartBtn.Enabled = true;
        }

        private void CancleBtn_Click(object sender, EventArgs e)
        {
            if (cancelTokensource != null)
            {
                cancelTokensource.Cancel();
            }
        }

        private bool Process(IProgress<string> progress, CancellationToken cancelToken)
        {
            var rtn = false;

            var fl = new List<string>();
            progress.Report("Replace Text Total:" + rs.ReplaceTexts.Count);

            foreach (var x in rs.ReplaceTexts)
            {
                if (string.IsNullOrEmpty(x.FileExtension))
                {
                    fl = fileList;
                }
                else
                {
                    fl = fileList.Where(y => y.EndsWith(x.FileExtension)).ToList();
                }
                fl.ForEach(y =>
                {
                    var s = string.Empty;
                    using (var sr = new StreamReader(y))
                    {
                        s = sr.ReadToEnd();
                    }
                    if (s.Contains(x.Before))
                    {
                        using (var sw = new StreamWriter(y, false))
                        {
                            sw.Write(s.Replace(x.Before, x.After));
                            progress.Report("Replace Text File:" + y);
                            progress.Report("Replace Text: Before[" + x.Before + "]" + " to After[" + x.After + "]");
                        }
                    }
                });

                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            #region func

            //rs.ReplaceTexts.ForEach(x =>
            //{
            //    if (string.IsNullOrEmpty(x.FileExtension))
            //    {
            //        fl = fileList;
            //    }
            //    else
            //    {
            //        fl = fileList.Where(y => y.EndsWith(x.FileExtension)).ToList();
            //    }
            //    fl.ForEach(y =>
            //    {
            //        var s = string.Empty;
            //        using (var sr = new StreamReader(y))
            //        {
            //            s = sr.ReadToEnd();
            //        }
            //        if (s.Contains(x.Before))
            //        {
            //            using (var sw = new StreamWriter(y, false))
            //            {
            //                sw.Write(s.Replace(x.Before, x.After));
            //                progress.Report("Replace Text File:" + y);
            //                progress.Report("Replace Text: Before[" + x.Before + "]" + " to After[" + x.After + "]");
            //            }
            //        }
            //    });

            //    if (cancelToken.IsCancellationRequested)
            //    {
            //        return false;
            //    }
            //});

            #endregion func

            progress.Report(string.Empty);
            progress.Report("File Rename Total Files:" + rs.FileRenames.Count);

            foreach (var x in rs.FileRenames)
            {
                fileList.Where(y => y.EndsWith(x.Before)).ToList().ForEach(z =>
                {
                    var fi = new FileInfo(z);
                    fi.CopyTo(fi.Directory + "\\" + x.After);
                    fi.Delete();
                    progress.Report("File Rename: Before[" + x.Before + "]" + " to After[" + x.After + "]");
                });
                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            #region func2

            rs.FileRenames.ForEach(x =>
            {
                fileList.Where(y => y.EndsWith(x.Before)).ToList().ForEach(z =>
                {
                    var fi = new FileInfo(z);
                    fi.CopyTo(fi.Directory + "\\" + x.After);
                    fi.Delete();
                    progress.Report("File Rename: Before[" + x.Before + "]" + " to After[" + x.After + "]");
                });
            });

            #endregion func2

            rtn = true;

            return rtn;
        }

        private void ShowProgress(string msg)
        {
            Area.Text = Area.Text + "\r\n" + msg;
        }
    }
}
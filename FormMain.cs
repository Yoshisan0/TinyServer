using MiniJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TinyServer
{
    public partial class FormMain : Form
    {
        private ushort m_Port = 0;
        private string m_Lang = "en-US";
        private string m_DocumentRoot = "./wwwroot";
        private HttpListener m_Listener = null;
        private Color m_DarkYellow = Color.FromArgb(128, 128, 0);

        public FormMain()
        {
            InitializeComponent();

            //初期化
            listView.Items.Clear();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //設定ファイルを読み込む
            ReadSetupFile();

            //サーバーを起動する
            StartHttpServer();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                m_Listener?.Stop();
            }
            catch (Exception err)
            {
                listView.Items.Add("Server close error. " + err.Message);
            }
        }

        /// <summary>
        /// 設定ファイルを読み込む
        /// </summary>
        private void ReadSetupFile()
        {
            try
            {
                string pathJsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TinyServer.json");
                if (File.Exists(pathJsonFile))
                {
                    //JSONファイルの内容を読み込む
                    string json = File.ReadAllText(pathJsonFile);
                    if (!string.IsNullOrEmpty(json))
                    {
                        json = json.Trim();

                        Dictionary<string, object> dic = Json.Deserialize(json) as Dictionary<string, object>;
                        if (dic != null)
                        {
                            bool isExist = dic.ContainsKey("port");
                            if (isExist)
                            {
                                m_Port = Convert.ToUInt16(dic["port"]);
                            }

                            isExist = dic.ContainsKey("lang");
                            if (isExist)
                            {
                                m_Lang = Convert.ToString(dic["lang"]);
                            }

                            isExist = dic.ContainsKey("documentRoot");
                            if (isExist)
                            {
                                m_DocumentRoot = Convert.ToString(dic["documentRoot"]);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                listView.Items.Add("Can't find TinyServer.json file. " + err.Message);
            }
        }

        /// <summary>
        /// Httpサーバーを起動する
        /// </summary>
        private async void StartHttpServer()
        {
            string ipAddress = "x.x.x.x";
            string timeStamp = "xxxx-xx-xx xx:xx:xx";
            string method = "x";
            string path = "x";
            ListViewItem item;

            try
            {
                string url = string.Format("http://localhost:{0}/", m_Port);

                m_Listener = new HttpListener();
                m_Listener.Prefixes.Add(url);
                m_Listener.Start();

                timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                item = listView.Items.Add($"[INFO] {timeStamp} - Server start. {url}");
                item.ForeColor = Color.DarkGreen;

                string rootPath = Path.GetFullPath(m_DocumentRoot);
                while (true)
                {
                    ipAddress = "x.x.x.x";
                    method = "x";
                    path = "x";

                    var context = await m_Listener.GetContextAsync();
                    string urlPath = context.Request.Url.LocalPath.TrimStart('/');
                    string requestedPath = Path.Combine(m_DocumentRoot, urlPath);
                    ipAddress = context.Request.RemoteEndPoint?.Address.ToString() ?? "unknown";
                    method = context.Request.HttpMethod;
                    path = context.Request.Url.LocalPath;

                    // デフォルトで index.html を返す
                    if (string.IsNullOrEmpty(urlPath))
                    {
                        requestedPath = Path.Combine(m_DocumentRoot, "index.html");
                    }

                    // パストラバーサル対策
                    string fullPath = Path.GetFullPath(requestedPath);
                    if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 403;
                        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                        {
                            writer.Write("403 Forbidden");
                        }

                        timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        item = listView.Items.Add($"[WARN] {timeStamp} - 403 Forbidden - Method: {method}, Path: {path}, IP: {ipAddress}");
                        item.ForeColor = m_DarkYellow;

                        context.Response.OutputStream.Close();
                        continue;
                    }

                    // ファイルの存在チェックとレスポンス送信
                    if (!File.Exists(fullPath))
                    {
                        context.Response.StatusCode = 404;
                        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                        {
                            writer.Write("404 Not Found");
                        }

                        timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        item = listView.Items.Add($"[WARN] {timeStamp} - 404 Not Found - Method: {method}, Path: {path}, IP: {ipAddress}");
                        item.ForeColor = m_DarkYellow;

                        context.Response.OutputStream.Close();
                        continue;
                    }

                    byte[] data = File.ReadAllBytes(fullPath);
                    context.Response.ContentType = Tool.GetContentTypeFromPath(fullPath);
                    context.Response.ContentLength64 = data.Length;
                    await context.Response.OutputStream.WriteAsync(data, 0, data.Length);

                    timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    item = listView.Items.Add($"[INFO] {timeStamp} - 200 OK - Method: {method}, Path: {path}, IP: {ipAddress}");
                    item.ForeColor = Color.DarkGreen;

                    context.Response.OutputStream.Close();
                }
            }
            catch (Exception err)
            {
                timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                item = listView.Items.Add($"[ERRO] {timeStamp} - Exception: {err.Message} - Method: {method}, Path: {path}, IP: {ipAddress}");
                item.ForeColor = Color.DarkRed;
            }
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (listView.SelectedItems.Count == 0) return;

                // 選択されたアイテムのテキストを1行ずつ連結
                var lines = listView.SelectedItems
                    .Cast<ListViewItem>()
                    .Select(item => item.Text);

                string text = string.Join(Environment.NewLine, lines);

                Clipboard.SetText(text);
            }
        }
    }
}

﻿using CefSharp.WinForms;
using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Text;
using ChromiumBrowser.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Reflection.Emit;
using CefSharp.DevTools.DOMSnapshot;
using CefSharp.DevTools.Browser;

namespace ChromiumBrowser
{

    public partial class Browser : Form
    {
        ChromiumWebBrowser chromiumBrowser = null;
        List<ChromiumWebBrowser> chromiumBrowsers = new List<ChromiumWebBrowser>();
        List<string> visitedPages = new List<string>();
        int TabNum = 1;

        public Browser()
        {
            InitializeComponent();
            InitializeBrowser();
            panel.Width = 0;

            this.Text = "Cockroach Browser";
            this.Icon = Resources.chromium;
            this.Update();
        }

        public void InitializeBrowser()
        {
            var settings = new CefSettings();
            Cef.Initialize(settings);

            chromiumBrowser = new ChromiumWebBrowser("https://google.com");
            BrowserTabs.TabPages[0].Controls.Add(chromiumBrowser);
            chromiumBrowser.Dock = DockStyle.Fill;
            BrowserTabs.TabPages[0].Name = $"Tab {TabNum}";
            TabNum = TabNum++;
        }

        private void BrowserResize(object sender, EventArgs e)
        {
            //This should expand the search bar when browser is enlargened
            Address.Width = this.ClientSize.Width - (Back.Width * 7) - 15;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (chromiumBrowser.CanGoBack)
            {
                chromiumBrowser.Back();
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (chromiumBrowser.CanGoForward)
            {
                chromiumBrowser.Forward();
            }
        }
        private void SearchBarKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var tabPage = BrowserTabs.SelectedTab;
                e.SuppressKeyPress = true;

                // Get the ChromiumWebBrowser control from the tab page
                var browser = tabPage.Controls.OfType<ChromiumWebBrowser>().FirstOrDefault();

                string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                if (BrowserTabs.SelectedTab.Name == "History")
                {
                    browser = new ChromiumWebBrowser("https://google.com");
                    BrowserTabs.SelectedTab.Controls.Add(chromiumBrowser);
                    browser.Dock = DockStyle.Fill;

                    BrowserTabs.TabPages[BrowserTabs.TabCount].Controls.Add(BrowserTabs.SelectedTab);
                }

                if (Rgx.IsMatch(Address.Text))
                {
                    browser.Load(Address.Text);
                    if (!incognitoModeOn) { visitedPages.Add(Address.Text); }
                }
                else
                {
                    browser.Load("https://www.google.com/search?q=" + Address.Text.Replace(" ", "+"));
                    if (!incognitoModeOn) { visitedPages.Add("https://www.google.com/search?q=" + Address.Text.Replace(" ", "+")); }
                }
            }
        }

        private void AddBrowserTab_Click(object sender, EventArgs e)
        {
            CreateNewTab("google.com");
        }

        private void removeBrowserTab_Click(object sender, EventArgs e)
        {
            TabNum -= 1;
            if (BrowserTabs.SelectedTab != null)
            {
                if (BrowserTabs.TabCount == 1)
                {
                    this.Close();
                }
                else
                {
                    BrowserTabs.TabPages.Remove(BrowserTabs.SelectedTab);
                }
            }
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            TabPage page = BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(BrowserTabs.SelectedTab)];
            bool containsInstanceOfChromium = BrowserTabs.Controls.OfType<ChromiumWebBrowser>().Any();

            if (containsInstanceOfChromium)
            {
                chromiumBrowser.Reload();

            } 
            else
            {
                while (page.Controls.Count > 0)
                {
                    Control control = page.Controls[0];
                    page.Controls.Remove(control);
                }
                GenerateHistory(labelY, page);
            }
        }

        bool isClosed = true;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (isClosed)
            {
                panel.Width = 400;
            }
            else
            {
                panel.Width = 0;
            }
            isClosed = !isClosed;
        }

        byte redVal, greenVal, blueVal;
        private void redUpDown_ValueChanged(object sender, EventArgs e)
        {
            redVal = (byte)redUpDown.Value;
            changeControlColors();
        }


        private void greenUpDown_ValueChanged(object sender, EventArgs e)
        {
            greenVal = (byte)greenUpDown.Value;
            changeControlColors();
        }


        int labelY;
        int listLength;
        System.Windows.Forms.Button removeAll;
        private void historyBtn_Click(object sender, EventArgs e)
        {
            TabPage page = new TabPage();
            page.Text = "History";

            ChromiumWebBrowser browser = new ChromiumWebBrowser();
            browser.Dock = DockStyle.Fill;

            labelY = 5;
            BrowserTabs.TabPages.Add(page);

            removeAll = new System.Windows.Forms.Button();
            removeAll.AutoSize = true;
            removeAll.Font = new Font("Arial", 10, FontStyle.Regular);

            removeAll.Click += (s, args) =>
            {
                while (page.Controls.Count > 0)
                {
                    Control control = page.Controls[0];
                    page.Controls.Remove(control);
                }
                visitedPages.Clear();
            };


            removeAll.Text = "Clear history";

            labelY = GenerateHistory(labelY, page);
            listLength = visitedPages.Count-1;

            removeAll.Location = new Point(10, labelY);

            BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(removeAll);
        }

       
        private int GenerateHistory(int labelY, TabPage page)
        {
            labelY = 5;
            foreach (var url in visitedPages)
            {
                System.Windows.Forms.Label urlLabel = new System.Windows.Forms.Label();

                System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
                btn.AutoSize = true;
                btn.Text = "X";
                btn.Font = new Font("Arial", 20, FontStyle.Regular);

                urlLabel.Text = url.ToString();
                urlLabel.Font = new Font("Arial", 30, FontStyle.Regular);
                urlLabel.AutoSize = true;
                urlLabel.Location = new Point(10, labelY);
                urlLabel.ForeColor = Color.Blue;
                urlLabel.Cursor = Cursors.Hand;

                btn.Location = new Point(urlLabel.Width * 4 + 10 + btn.Width, labelY);

                urlLabel.Click += (s, args) => {
                    CreateNewTab(url);
                };

                btn.Click += (s, args) =>
                {
                    page.Controls.Remove(btn);
                    page.Controls.Remove(urlLabel);
                    visitedPages.Remove(url);
                };

                BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(urlLabel);
                BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(btn);

                labelY = urlLabel.Bottom + 5;
            }
            BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(removeAll);
            removeAll.Location = new Point(10, labelY);
            return labelY;
        }

        TabPage page;
        private void CreateNewTab(string url)
        {
            page = new TabPage();

            ChromiumWebBrowser chromiumWebBrowser = new ChromiumWebBrowser();

            if (url != null) { chromiumWebBrowser.Load(url); }
            else { chromiumWebBrowser.Load("google.com"); }

            chromiumWebBrowser.FrameLoadEnd += browser_FrameLoadEnd;
            chromiumWebBrowser.Dock = DockStyle.Fill;

            page.Text = $"Tab {TabNum}";

            TabNum = TabNum+=1;

            page.Controls.Add(chromiumWebBrowser);

            BrowserTabs.TabPages.Add(page);
        }

        private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                ChromiumWebBrowser browser = (ChromiumWebBrowser)sender;
                string url = browser.Address;
                string domainName = GetDomainName(url);

                this.Invoke(new Action(() =>
                {
                    BrowserTabs.SelectedTab.Text = domainName;
                }));

            }
        }

        private string GetDomainName(string url)
        {
            //Uri uri = new Uri(url);
            //string host = uri.Host;
            bool page_safe = url.Contains("https:");
            bool page_unsafe = url.Contains("http:");
            string domain_name = null;
            if (page_safe)
            {
                //s.Substring(url + 2)
                domain_name = url.Substring(12); //this is how many characters are in "https://www."
                domain_name = domain_name.Substring(0, domain_name.IndexOf("/")); //cuts the end off
            }
            else if (page_unsafe)
            {
                domain_name = url.Substring(11); //this is how many characters are in "http://www."
                domain_name = domain_name.Substring(0, domain_name.IndexOf("/")); //cuts the end off
            }
            return domain_name;
        }


        bool incognitoModeOn = false;

        private void button1_Click(object sender, EventArgs e)
        {
            incognitoModeOn = !incognitoModeOn;
            if (incognitoModeOn == true)
            {
                button1.BackColor = Color.Green;
            }
            else
            {
                button1.BackColor = DefaultBackColor;
            }
        }

        private void advancedSettings_Click(object sender, EventArgs e)
        {
            //TODO add something here
        }

        private void Address_Click(object sender, EventArgs e)
        {
           Address.Text = "";
        }

        private void blueUpDown_ValueChanged(object sender, EventArgs e)
        {
            blueVal = (byte)blueUpDown.Value;
            changeControlColors();
        }

        private void changeControlColors()
        {
            Color color = Color.FromArgb(redVal, greenVal, blueVal);
            panel.BackColor = color;
            ToolStrip.BackColor = color;
            Address.BackColor = color;
            BrowserTabs.BackColor = color;
        }

    }
}


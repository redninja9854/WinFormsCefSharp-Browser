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
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Reflection;

namespace ChromiumBrowser
{

    public partial class Browser : Form
    {
        ChromiumWebBrowser chromiumBrowser = null;
        List<ChromiumWebBrowser> chromiumBrowsers = new List<ChromiumWebBrowser>();

        List<Tuple<string, string>> visitedPagesList = new List<Tuple<string, string>>();
        List<TabPage> mainPages = new List<TabPage>();

        ImageList imgList = new ImageList();

        TabPage PlusPage = new TabPage();
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

            CreateNewTab("google.com");
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs e)
        {
            if (!incognitoModeOn) { visitedPages.Add(e.Address); }
        }
        int totalWidth = 0;
        private void BrowserResize(object sender, EventArgs e)
        {
            totalWidth = 0;
            foreach (ToolStripItem item in ToolStrip.Items)
            {
               if (item is ToolStripButton) { continue; }
               totalWidth += item.Width;
            }
            Address.Width = this.ClientSize.Width - totalWidth;
            MessageBox.Show($"{ClientSize.Width} width, {totalWidth} total, {Address.Width} address");
            MessageBox.Show($"{ClientSize.Width-totalWidth == Address.Width}");


        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            chromiumBrowser = BrowserTabs.SelectedTab.Controls.OfType<ChromiumWebBrowser>().FirstOrDefault();
            if (chromiumBrowser.CanGoBack)
            {
                chromiumBrowser.Back();
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            chromiumBrowser = BrowserTabs.SelectedTab.Controls.OfType<ChromiumWebBrowser>().FirstOrDefault();
            if (chromiumBrowser != null)
            {
                if (chromiumBrowser.CanGoForward)
                {
                    chromiumBrowser.Forward();
                }
            }
        }
        private void SearchBarKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var tabPage = BrowserTabs.SelectedTab;
                e.SuppressKeyPress = true;

                // Get the ChromiumWebBrowser control from the tab page
                chromiumBrowser = tabPage.Controls.OfType<ChromiumWebBrowser>().FirstOrDefault();

                string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                if (tabPage.Text == "History")
                {
                    chromiumBrowser = new ChromiumWebBrowser();
                    chromiumBrowser.TitleChanged += ChromiumBrowser_TitleChanged;
                    chromiumBrowser.FrameLoadEnd += browser_FrameLoadEnd;
                    chromiumBrowser.Dock = DockStyle.Fill;

                    while (tabPage.Controls.Count > 0)
                    {
                        Control control = tabPage.Controls[0];
                        tabPage.Controls.Remove(control);
                    }
                    tabPage.Controls.Add(chromiumBrowser);
                }
                if (tabPage.Text.StartsWith("Tab"))
                {
                    chromiumBrowser = new ChromiumWebBrowser();
                    chromiumBrowser.FrameLoadEnd += browser_FrameLoadEnd;
                    chromiumBrowser.Dock = DockStyle.Fill;

                    while (tabPage.Controls.Count > 0)
                    {
                        Control control = tabPage.Controls[0];
                        tabPage.Controls.Remove(control);
                    }
                    tabPage.Controls.Add(chromiumBrowser);
                }

                SearchAdress(chromiumBrowser, Address.Text);
            }
        }

        private void SearchAdress(ChromiumWebBrowser browser)
        {
            if (Address.Text == null) { return; } 

            string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);


            if (Rgx.IsMatch(Address.Text))
            {
                if (visitedPagesList.Last().Item1 != tuple.Item1 && !incognitoModeOn)
                {
                    if (!incognitoModeOn) { visitedPagesList.Add(tuple); }
                }
            } 
            catch (Exception)
            {
                browser.Load("https://www.google.com/search?q=" + Address.Text.Replace(" ", "+"));
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
            TabPage page = BrowserTabs.SelectedTab;
            bool containsInstanceOfChromium = page.Controls.OfType<ChromiumWebBrowser>().Any();
            chromiumBrowser = BrowserTabs.SelectedTab.Controls.OfType<ChromiumWebBrowser>().FirstOrDefault();

            if (containsInstanceOfChromium)
            {
                chromiumBrowser.Reload();
            } 
            else if(page.Text == "History")
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

        int labelY;
 
        System.Windows.Forms.Button removeAll;
        System.Windows.Forms.Button removeSelected;
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

            removeSelected = new System.Windows.Forms.Button();
            removeSelected.AutoSize = true;
            removeSelected.Font = new Font("Arial", 10, FontStyle.Regular);

            removeAll.Click += (s, args) =>
            {
                while (page.Controls.Count > 0)
                {
                    Control control = page.Controls[0];
                    page.Controls.Remove(control);
                }
                visitedPages.Clear();
            };

            removeSelected.Click += (s, args) =>
            {
                while (controlsToRemove.Count > 0)
                {
                    page.Controls.Remove(controlsToRemove[0]);
                    controlsToRemove.RemoveAt(0);
                }
            };

            removeAll.Text = "Clear history";
            removeSelected.Text = "Clear selected";

            labelY = GenerateHistory(labelY, page);

            removeAll.Location = new Point(10, labelY);
            removeSelected.Location = new Point(10, labelY+35);

            BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(removeAll);
            BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(removeSelected);
        }

        List<Control> controlsToRemove = new List<Control>();
        private int GenerateHistory(int labelY, TabPage page)
        {
            labelY = 5;
            foreach (var url in visitedPages)
            {
                System.Windows.Forms.Label urlLabel = new System.Windows.Forms.Label();

                CheckBox btn = new CheckBox();
                btn.AutoSize = true;
                btn.Text = "X";
                btn.Font = new Font("Arial", 10, FontStyle.Regular);

                urlLabel.Text = url.ToString();
                urlLabel.Font = new Font("Arial", 10, FontStyle.Regular);
                urlLabel.AutoSize = true;
                urlLabel.Location = new Point(30, labelY);
                urlLabel.ForeColor = Color.Blue;
                urlLabel.Cursor = Cursors.Hand;

                btn.Location = new Point(10, labelY);

                urlLabel.Click += (s, args) => {
                    CreateNewTab(url);
                };

                btn.CheckedChanged += (s, args) =>
                {
                    if (btn.Checked)
                    {
                        controlsToRemove.Add(btn);
                        controlsToRemove.Add(urlLabel);
                        visitedPages.Remove(urlLabel.Text);
                    }
                    else
                    {
                        controlsToRemove.Remove(btn);
                        controlsToRemove.Remove(urlLabel); 
                        visitedPages.Add(urlLabel.Text);
                    }
                };

                BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(urlLabel);
                BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(btn);

                labelY = urlLabel.Bottom + 5;
            }

            BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(removeAll);
            BrowserTabs.TabPages[BrowserTabs.TabPages.IndexOf(page)].Controls.Add(removeSelected);

            removeSelected.Location = new Point(10, labelY+35);
            removeAll.Location = new Point(10, labelY);
            return labelY;
        }

        TabPage page;
        Image image;
        List<System.Windows.Forms.TextBox> textBoxes = new List<System.Windows.Forms.TextBox>();
        ChromiumWebBrowser chromiumWebBrowser;
        private void CreateNewTab(string url)
        {
            page = new TabPage();

            chromiumWebBrowser = new ChromiumWebBrowser();
            chromiumWebBrowser.TitleChanged += ChromiumBrowser_TitleChanged;
            chromiumWebBrowser.FrameLoadEnd += browser_FrameLoadEnd;
            chromiumWebBrowser.Dock = DockStyle.Fill;

            page.Text = $"Tab {TabNum}";

            TabNum = TabNum+=1;

            page.Controls.Add(chromiumWebBrowser);

            txtbox.Name = "MainSearch";
            txtbox.Text = "Search";
            txtbox.Font = new Font("Arial", 50);


            int pos = BrowserTabs.TabCount > 0 ? BrowserTabs.TabCount - 1 : 0;

            if (pos > 0) { BrowserTabs.TabPages.Insert(pos, page); } else { BrowserTabs.TabPages.Add(page); }
            chromiumWebBrowser.Focus();

            txtbox.Width = 1500;
            txtbox.Height = 70;

            txtbox.AutoSize = false;

            int x = (page.Size.Width - txtbox.Size.Width) / 2;
            int y = (page.Size.Height - txtbox.Size.Height) / 2;

            txtbox.Location = new Point(x, y);
            this.Controls.Add(txtbox);

            txtbox.Click += (s, args) =>
            {
                txtbox.Text = "";
            };
            txtbox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    page.Controls.Add(chromiumWebBrowser);
                    SearchAdress(chromiumWebBrowser, txtbox.Text);
                    page.Controls.Remove(txtbox);
                    mainPages.Remove(page);
                    textBoxes.Remove(txtbox);
                }
            };
            page.Controls.Add(txtbox);
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
                    if (BrowserTabs.SelectedTab.Text == "History") { return; }
                    BrowserTabs.SelectedTab.Text = title;
                    
                    try
                    {
                        iconUrl = new Uri("https://" + new Uri(url).Host + "/favicon.ico");

                        stream = client.OpenRead(iconUrl);
                        Bitmap bmp = new Bitmap(stream);

                        BrowserTabs.ImageList.Images.Add(bmp);
                        int imageIndex = BrowserTabs.ImageList.Images.Count-1;
                        BrowserTabs.SelectedTab.ImageIndex = BrowserTabs.ImageList.Images.Count - 1;
                        tabImageIndex[BrowserTabs.SelectedTab] = imageIndex;

                        BrowserTabs.Invalidate();
                    }
                    catch (Exception)
                    {
                        tabImageIndex[BrowserTabs.SelectedTab] = 0;
                        BrowserTabs.SelectedTab.ImageIndex = 0;
                    }
                }));
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

        byte redVal, greenVal, blueVal;
        private void redUpDown_ValueChanged(object sender, EventArgs e)
        {
            redVal = (byte)redUpDown.Value;
            changeControlColors();
        }

        bool repeatingBg = true;
        private void ToggleRepeatingBg(object sender, EventArgs e)
        {
            repeatingBg = !repeatingBg;
            radioButton1.Checked = repeatingBg;

            foreach(TabPage page in mainPages)
            {
                page.BackgroundImage = image;
                if (repeatingBg) 
                { 
                    page.BackgroundImageLayout = ImageLayout.Tile; 
                } 
                else 
                { 
                    page.BackgroundImageLayout = ImageLayout.Stretch; 
                }
            }
        }

        System.Drawing.Color color, txtColor, borderColor = new System.Drawing.Color();
        private void changeControlColors()
        {
            Color color = Color.FromArgb(redVal, greenVal, blueVal);
            panel.BackColor = color;
            ToolStrip.BackColor = color;
            Address.BackColor = color;
            BrowserTabs.BackColor = color;
        }

        private void backGroundNumericChange(object sender, EventArgs e)
        {
            redVal = (byte)redUpDown.Value;
            greenVal = (byte)greenUpDown.Value;
            blueVal = (byte)blueUpDown.Value;

            changeControlColors();
        }
    }
}


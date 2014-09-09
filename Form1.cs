using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;

// Requires reference to WebDriver.Support.dll
using OpenQA.Selenium.Support.UI;
using System.Timers;
using System.Threading;
using OpenQA.Selenium.Remote;
using System.Text.RegularExpressions;
namespace FloatingChair
{
    public partial class Form1 : Form
    {
        System.Timers.Timer aTimer;
        IWebDriver driver;
        Random rng = new Random();
        /* lists all the ids of the items that you can use at any time like food items
         * someone please come up with a better idea or figure out how to get the categories
         * from the tooltips, this is ridiculous*/
        List<string> Clickables = new List<string> {
        "29","102","157","212","254", "354"
        };
        bool ready;
        int gameState = 0;
        /* 0 = Game lobby, not started
         * 1 = Started, not our turn
         * 2 = Our turn! Do shit, hurry hurry!
         * 3 = Game is over.
         */
        public Form1()
        {
            InitializeComponent();
            
            /*Events for dragging the window.*/
            label1.MouseDown += new MouseEventHandler(label1_MouseDown);
            label1.MouseUp += new MouseEventHandler(label1_MouseUp);
            label1.MouseMove += new MouseEventHandler(label1_MouseMove);
            /*The bot does things whenever the timer reaches the set time*/
            aTimer = new System.Timers.Timer(50);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

        }
        /*Safely changes the text of the textbox from different threads. AKA it doesn't crash.*/
        public void SetTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(SetTextBox), new object[] { value });
                return;
            }
            textBox1.Text = value;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            ready = true;
            aTimer.Start();
            SetTextBox("Started.");
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (ready)
            {
                /*                                                                          *
                 * Checks if the game has started by looking at the continueButton class.   *
                 * If the button exists the game has started.                               *  
                 * If the button exists and is clickable its our turn to click.             *
                 *                                                                          */
                IList<IWebElement> continueButton = driver.FindElements(By.ClassName("continueButton"));
                if (continueButton.Count > 0)
                {
                    if (gameState != 1 && !continueButton[0].Enabled)
                        gameState = 1;
                    else if (gameState != 2 && continueButton[0].Enabled)
                        gameState = 2;
                }
                else if (gameState != 3 && driver.FindElements(By.ClassName("eventTitle"))[0].Text.ToLower().EndsWith(" won!"))
                    gameState = 3;
                DoStuff();
            }
        }
        /*                                          *
         *  This is where the bot does everything.  *
         *                                          */
        private void DoStuff()
        {
            ready = false;
            #region Lobby Actions
            if (gameState == 0)
            {
                if (textBox1.Text != "Waiting for game to start...")
                {
                    SetTextBox("Waiting for game to start...");
                }
                ready = true;
                return;
            }
#endregion
            #region Opponent's Turn Actions
            else if (gameState == 1)
            {
                /*
                 TODO: Put in options for using items and skills in inventory.
                 */
                IList<IWebElement> Items = driver.FindElements(By.ClassName("itemIcon"));
                foreach (IWebElement i in Items)
                {
                    string id = i.GetAttribute("id");
                    id = Regex.Match(id, @"\d+").Value;
                    SetTextBox("item id:"+id);
                }
                ready = true;
                return;
            }
            #endregion
            #region Our Turn Actions
            else if (gameState == 2)
            {
                if (textBox1.Text != "Playing turn...")
                {
                    SetTextBox("Playing turn...");
                }
                IList<IWebElement> continueButton = driver.FindElements(By.ClassName("continueButton"));
                IList<IWebElement> playerSelectButton = driver.FindElements(By.ClassName("playerSelectButton"));
                IList<IWebElement> itemSelectButton = driver.FindElements(By.ClassName("itemSelectButton"));
                IList<IWebElement> numberInput = driver.FindElements(By.ClassName("numberInput"));
                IList<IWebElement> eventTitle = driver.FindElements(By.ClassName("eventTitle"));
                #region Clicking actions
                if (continueButton.Count == 0){}
                #region Click button 1
                    //linear search can take forever should prolly fix it someday
                else if (continueButton[0].Text == "Continue" ||
                    continueButton[0].Text.StartsWith("Roll ") ||
                    continueButton[0].Text == "Proceed..." ||
                    continueButton[0].Text == "Let's go" ||
                    continueButton[0].Text == "Yes" ||
                    continueButton[0].Text == "Yes!" ||
                    continueButton[0].Text == "Go in!" ||
                    continueButton[0].Text == "I'll join you!" ||
                    continueButton[0].Text == "Awesome!" ||
                    continueButton[0].Text == "Deal!" ||
                    continueButton[0].Text == "Mathematical!" ||
                    continueButton[0].Text == "Aim it forward." ||
                    continueButton[0].Text == "I'll launch myself!" ||
                    continueButton[0].Text == "Going down!" ||
                    continueButton[0].Text == "Charge items with spells" ||
                    continueButton[0].Text == "Hmmm... okay.")
                {
                    continueButton[0].Click();
                    ready = true;
                    return;
                }
                #endregion
                #region Click button 2
                else if (continueButton[0].Text == "Go in blindly." ||
                         continueButton[0].Text == "Go Forward!") //gandalf
                {
                    continueButton[1].Click();
                    ready = true;
                    return;
                }
                #endregion
                #region Click button 1 or 2
                else if (continueButton[0].Text == "Cowboys" ||
                         continueButton[0].Text == "Ya rly!")
                {
                    continueButton[rng.Next(0, 2)].Click();
                    ready = true;
                    return;
                }
                #endregion
                #region Click any button
                else if (continueButton[0].FindElements(By.XPath("*")).Count > 0 ||//Item buttons contain shit so i look for anything at all
                         continueButton[0].Text == "Ask the bartender for a drink." ||
                         continueButton[0].Text == "The Church." ||
                         eventTitle[0].Text == "Crossroads" ||
                         eventTitle[0].Text == "Church" ||
                         eventTitle[0].Text == "Reference Time!" ||
                         eventTitle[0].Text == "Dungeon Entrance" ||
                         eventTitle[0].Text == "Ooo" ||
                         eventTitle[0].Text == "Middle Ages" ||
                         eventTitle[0].Text == "Specialized Adventuring!" ||
                         continueButton[0].Text == "Pick a random option")
                {
                    continueButton[rng.Next(0, continueButton.Count())].Click();
                    ready = true;
                    return;
                }
                #endregion
                #region Click any button but the last one.
                else if (continueButton[0].Text == "Play" ||
                         continueButton[0].Text == "Buy" ||
                         eventTitle[0].Text == "Dark Wand" ||
                         eventTitle[0].Text == "Light Wand" ||
                         eventTitle[0].Text == "Staff" ||
                         eventTitle[0].Text == "Orb")
                {
                    continueButton[rng.Next(0, continueButton.Count() - 1)].Click();
                    ready = true;
                    return;
                }
                #endregion
                
#endregion
                #region Non-clicking Actions
                if (playerSelectButton.Count == 0){}
                else
                {
                    IList<IWebElement> radioButton = driver.FindElements(By.ClassName("playerSelectInput"));
                    radioButton[rng.Next(0, radioButton.Count)].Click();
                    playerSelectButton[0].Click();
                    ready = true;
                    return;
                }
                if (itemSelectButton.Count == 0) { }
                else
                {
                    itemSelectButton[rng.Next(0, itemSelectButton.Count())].Click();
                    ready = true;
                    return;
                }
                if (numberInput.Count == 0) { }
                else
                {
                    numberInput[0].SendKeys(""+rng.Next(0,21));
                    driver.FindElement(By.ClassName("numberInputButton")).Click();
                    ready = true;
                    return;
                }
                #endregion
                //Last Resort.  When playing with people having this active is recommended.
                if (continueButton.Count != 0)
                {
                    //continueButton[rng.Next(0, continueButton.Count())].Click();
                }
                ready = true;
                return;
            }
            #endregion
            #region Game Over Actions
            else if (gameState == 3)
            {
                if (textBox1.Text != "Game over, naviate to a new game and press 'Start'.")
                {
                    IWebElement chatBox = driver.FindElement(By.Id("chatbox_textInput"));
                    aTimer.Stop();
                    chatBox.SendKeys("gg");
                    chatBox.Submit();
                    SetTextBox("Game over, naviate to a new game and press 'Start'.");
                }
                ready = true;
                return;
            }
            #endregion
            else
            {
                ready = true;
                return;
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            aTimer.Stop();
            SetTextBox("Stopped.");
        }
#region Stolen dragging code from SO
        private bool _Moving = false;
        private Point _Offset;
        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            _Moving = true;
            _Offset = new Point(e.X, e.Y);
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_Moving)
            {
                Point newlocation = this.Location;
                newlocation.X += e.X - _Offset.X;
                newlocation.Y += e.Y - _Offset.Y;
                this.Location = newlocation;
            }
        }
        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_Moving)
            {
                _Moving = false;
            }
        }
#endregion
        private void exitImage_Click(object sender, EventArgs e)
        {
            aTimer.Stop();
            if (driver!=null)
                driver.Quit();
            this.Close();
        }

        private void browserButton_Click(object sender, EventArgs e)
        {
            SetTextBox("Loading...");
            driver = new FirefoxDriver();
            driver.Navigate().GoToUrl("http://www.boardgame-online.com");
            startButton.Visible = true;
            stopButton.Visible = true;
            browserButton.Visible = false;
            SetTextBox("Navigate to the game lobby and press 'Start' whenever.");
        }
    }
}
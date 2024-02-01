using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gui_Miner.Classes
{
    internal class Extensions
    {
    }
    public static class RichTextBoxExtensions
    {
        public static void AppendTextThreadSafe(this RichTextBox richTextBox, string text)
        {
            if (richTextBox == null) return;
            
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.AppendText(text))); }
                catch { }
            }
            else
            {
                try { richTextBox.AppendText(text); }
                catch { }
            }
        }
        public static void SetTextThreadSafe(this RichTextBox richTextBox, string text)
        {
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.Text = text)); }
                catch { }
            }
            else
            {
                try { richTextBox.Text = text; }
                catch { }
            }
        }
        public static string GetTextThreadSafe(this RichTextBox richTextBox)
        {
            string s = "";
            if (richTextBox.InvokeRequired)
            {
                try { s = (string)richTextBox.Invoke(new Func<string>(() => richTextBox.Text)); }
                catch { }
                return s;
            }
            else
            {
                try { s = richTextBox.Text; }
                catch { } 
                return s;
            }
        }
        public static void ForeColorSetThreadSafe(this RichTextBox richTextBox, Color color)
        {
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.ForeColor = color)); }
                catch { }
            }
            else
            {
                try { richTextBox.ForeColor = color; }
                catch { }
            }
        }
        public static Color ForeColorGetThreadSafe(this RichTextBox richTextBox)
        {
            Color foreColor = new Color();

            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => foreColor = richTextBox.ForeColor)); }
                catch { }
            }
            else
            {
                try { foreColor = richTextBox.ForeColor; }
                catch { }
            }

            return foreColor;
        }
        public static void SelectionStartThreadSafe(this RichTextBox richTextBox, int start)
        {
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.SelectionStart = start)); }
                catch { }
                
            }
            else
            {
                try { richTextBox.SelectionStart = start; }
                catch { }
            }
        }
        public static void SelectionLengthThreadSafe(this RichTextBox richTextBox, int length)
        {
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.SelectionLength = length)); }
                catch { }
            }
            else
            {
                try { richTextBox.SelectionLength = length; }
                catch { }
            }
        }
        public static int TextLengthGetThreadSafe(this RichTextBox richTextBox)
        {
            if (richTextBox == null)
            {
                throw new ArgumentNullException(nameof(richTextBox));
            }

            int i = 0;

            if (richTextBox.InvokeRequired)
            {
                // If called from a different thread, invoke the method on the UI thread
                try { i = (int)richTextBox.Invoke(new Func<int>(() => richTextBox.TextLength)); }
                catch { }
                return i;
            }
            else
            {
                // If called from the UI thread, access the TextLength property directly
                try { i = richTextBox.TextLength; }
                catch { }
                return i;
            }
        }
        public static void SelectionColorThreadSafe(this RichTextBox richTextBox, Color color)
        {
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.SelectionColor = color)); }
                catch { }
            }
            else
            {
                try { richTextBox.SelectionColor = color; }
                catch { }
            }
        }
        public static void ScrollToCaretThreadSafe(this RichTextBox richTextBox)
        {
            if (richTextBox.InvokeRequired)
            {
                try { richTextBox.Invoke(new Action(() => richTextBox.ScrollToCaret())); }
                catch { }
            }
            else
            {
                try { richTextBox.ScrollToCaret(); }
                catch { }
            }
        }
    }

    public static class LabelExtensions
    {
        public static void SetTextThreadSafe(this Label label, string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }
        public static string GetTextThreadSafe(this Label label)
        {
            if (label.InvokeRequired)
            {
                return (string)label.Invoke(new Func<string>(() => label.Text));
            }
            else
            {
                return label.Text;
            }
        }
        public static void ForeColorThreadSafe(this Label label, Color color)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.ForeColor = color));
            }
            else
            {
                label.ForeColor = color;
            }
        }
        public static void ShowThreadSafe(this Label label)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));

            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Show()));
            }
            else
            {
                label.Show();
            }
        }

        public static void HideThreadSafe(this Label label)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));

            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Hide()));
            }
            else
            {
                label.Hide();
            }
        }
        public static void ShowTextForDuration(this Label label, string text, int durationMillisecs)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => ShowTextForDuration(label, text, durationMillisecs)));
                return;
            }
            
            if(!string.IsNullOrEmpty(text))
                label.Text = text;

            label.Visible = true;

            Task.Delay(durationMillisecs).ContinueWith(_ =>
            {
                if (label.InvokeRequired)
                {
                    label.Invoke(new Action(() => label.Visible = false));
                }
                else
                {
                    label.Visible = false;
                }
            });
        }
    }

    public static class PictureBoxExtensions
    {
        public static object GetTagThreadSafe(this PictureBox pictureBox)
        {
            if (pictureBox.InvokeRequired)
            {
                return pictureBox.Invoke(new Func<object>(() => pictureBox.Tag));
            }
            else
            {
                return pictureBox.Tag;
            }
        }
        public static void SetTagThreadSafe(this PictureBox pictureBox, object value)
        {
            if (pictureBox == null)
            {
                throw new ArgumentNullException(nameof(pictureBox));
            }

            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => pictureBox.Tag = value));
            }
            else
            {
                pictureBox.Tag = value;
            }
        }
        public static void SetBackgroundImageThreadSafe(this PictureBox pictureBox, Image image)
        {
            if (pictureBox == null)
            {
                throw new ArgumentNullException(nameof(pictureBox));
            }

            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => pictureBox.BackgroundImage = image));
            }
            else
            {
                pictureBox.BackgroundImage = image;
            }
        }
        public static void SetBackgroundColorThreadSafe(this PictureBox pictureBox, Color color)
        {
            if (pictureBox == null)
            {
                throw new ArgumentNullException(nameof(pictureBox));
            }

            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => pictureBox.BackColor = color));
            }
            else
            {
                pictureBox.BackColor = color;
            }
        }
    }


    public static class PanelExtensions
    {
        public static void AddControlThreadSafe(this Panel panel, Control control)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() => panel.Controls.Add(control)));
            }
            else
            {
                panel.Controls.Add(control);
            }
        }
    }

    public static class TabPageExtensions
    {
        public static void SetNameThreadSafe(this TabPage tabPage, string name)
        {
            if (tabPage.InvokeRequired)
            {
                tabPage.Invoke(new Action(() => tabPage.Name = name));
            }
            else
            {
                tabPage.Name = name;
            }
        }

        public static void SetTextThreadSafe(this TabPage tabPage, string text)
        {
            if (tabPage.InvokeRequired)
            {
                tabPage.Invoke(new Action(() => tabPage.Text = text));
            }
            else
            {
                tabPage.Text = text;
            }
        }
    }

    public static class ComboBoxExtensions
    {
        public static void SetTextThreadSafe(this ComboBox comboBox, string text)
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(new Action(() => comboBox.Text = text));
            }
            else
            {
                comboBox.Text = text;
            }
        }
    }
}

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
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.AppendText(text)));
            }
            else
            {
                richTextBox.AppendText(text);
            }
        }
        public static void ForeColorThreadSafe(this RichTextBox richTextBox, Color color)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.ForeColor = color));
            }
            else
            {
                richTextBox.ForeColor = color;
            }
        }
        public static void SelectionStartThreadSafe(this RichTextBox richTextBox, int start)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.SelectionStart = start));
                
            }
            else
            {
                richTextBox.SelectionStart = start;
            }
        }

        public static void SelectionLengthThreadSafe(this RichTextBox richTextBox, int length)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.SelectionLength = length));
            }
            else
            {
                richTextBox.SelectionLength = length;
            }
        }

        public static void SelectionColorThreadSafe(this RichTextBox richTextBox, Color color)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.SelectionColor = color));
            }
            else
            {
                richTextBox.SelectionColor = color;
            }
        }
        public static void ScrollToCaretThreadSafe(this RichTextBox richTextBox)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.ScrollToCaret()));
            }
            else
            {
                richTextBox.ScrollToCaret();
            }
        }
    }

    public static class LabelExtensions
    {
        public static void TextThreadSafe(this Label label, string text)
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
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OS_6
{
    public partial class Form1 : Form
    {
        File[] files;
        int fileNum;
        int[] table;
        
        public Form1()
        {
            InitializeComponent();
            
            
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            fileNum = 0;
            files = new File[1000];
            table = new int[1000];

            Oper.init_table(table, listView1);
            flush_table();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0014) return;
            base.WndProc(ref m);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            int count = int.Parse(textBox1.Text);

            for (int i = 0; i < count; i++)
            {
                double temp = r.NextDouble() * 8 + 2;
                files[fileNum] = new File(temp, (i + 1).ToString() + ".txt");

                int hasFind = 0;
                for (int j = 0; j < table.Length; j++)
                {
                    if (table[j] == 0)
                    {
                        files[fileNum].index[hasFind] = j + 1;
                        table[j] = (hasFind == 0 ? 2 : 1);
                        hasFind++;

                        if (hasFind == files[fileNum].index.Length) break;
                    }
                }

                Oper.insertFile(files[fileNum], listView2, listView3);

                fileNum++;
            }

            flush_table();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string fileName;
            double fileTiji;
            try
            {
                fileName = textBox2.Text;
                fileTiji = double.Parse(textBox3.Text);
            }catch(Exception ex)
            {
                MessageBox.Show("输入错误！请检查后输入。");
                return;
            }
            int hasFind = 0;
            File file = new File(fileTiji, fileName);
            for (int j = 0; j < table.Length; j++)
            {
                if (table[j] == 0)
                {
                    file.index[hasFind] = j + 1;
                    table[j] = (hasFind == 0 ? 2 : 1);
                    hasFind++;
                }
                if (hasFind == file.index.Length) break;
            }
            files[fileNum++] = file;

            Oper.insertFile(file, listView2, listView3);
            this.flush_table();

        }

        public void flush_table(File file = null)
        {
            if (file == null)
            {
                int[] disk_table = table;
                ListView list = listView1;
                list.Items.Clear();
                for (int i = 0; i < 25; i++)
                {
                    ListViewItem lt = list.Items.Add((i + 1).ToString());

                    for (int j = 0; j < 20; j++)
                    {
                        lt.UseItemStyleForSubItems = false;
                        lt.SubItems.Add(disk_table[i * 20 + j].ToString());
                        if (disk_table[i * 20 + j] == 0) lt.SubItems[j + 1].BackColor = Color.LightGreen;
                        else if (disk_table[i * 20 + j] == 1) lt.SubItems[j + 1].BackColor = Color.OrangeRed;
                        else lt.SubItems[j + 1].BackColor = Color.Yellow;
                    }
                }
            }
            else
            {
                int[] disk_table = table;
                ListView list = listView1;
                list.Items.Clear();
                for (int i = 0; i < 25; i++)
                {
                    ListViewItem lt = list.Items.Add((i + 1).ToString());

                    for (int j = 0; j < 20; j++)
                    {
                        lt.UseItemStyleForSubItems = false;
                        lt.SubItems.Add(disk_table[i * 20 + j].ToString());
                    }
                }
                for (int i = 0; i < file.index.Length; i++)
                {
                    int index = file.index[i] - 1;
                    if (i != 0) listView1.Items[index / 20].SubItems[index % 20 + 1].BackColor = Color.OrangeRed;
                    else listView1.Items[index / 20].SubItems[index % 20 + 1].BackColor = Color.Yellow;
                }
            }
            

            
        }

        public void flushView()
        {
            listView2.Items.Clear();
            listView3.Items.Clear();
            for (int i = 0; i < fileNum; i++)
            {
                ListViewItem l1 = listView2.Items.Add(files[i].fileName);
                l1.SubItems.Add(Math.Round(files[i].tiji, 2).ToString() + "K");

                ListViewItem m = listView3.Items.Add(files[i].fileName);
                m.SubItems.Add(files[i].index[0].ToString());

                string s = "";
                for (int j = 1; j < files[i].index.Length; j++) s += files[i].index[j].ToString() + " ";
                m.SubItems.Add(s);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int all = fileNum;
            for (int i = 0; i < all; i++)
            {
                string fileName = files[i].fileName.Split('.')[0];
                int x = -1;
                try
                {
                    x = int.Parse(fileName);
                }catch(Exception ex)
                {
                    goto next;
                }
                if (x % 2 == 1)
                {
                    Oper.deleteFile(table, files[i]);
                    files[i] = null;
                    fileNum--;
                }
            next:;
            }
            Oper.flushFile(files, fileNum);
            flushView();
            flush_table();
        
        }

        private void button4_Click(object sender, EventArgs e)
        {
            File file = new File(7, "A.txt");
            File file1 = new File(5, "B.txt");
            File file2 = new File(2, "C.txt");
            File file3 = new File(9, "D.txt");
            File file4 = new File(3.5, "E.txt");



            findAndChangeTable(file);
            findAndChangeTable(file1);
            findAndChangeTable(file2);
            findAndChangeTable(file3);
            findAndChangeTable(file4);

            flush_table();
            flushView();

        }

        // 寻找空闲单元
        public void findAndChangeTable(File file)
        {
            // 定义是否找到空闲单元
            int hasFind = 0;

            // 对于磁盘状态进行遍历
            for (int j = 0; j < table.Length; j++)
            {
                // 找到空闲块，修改状态并记录
                if (table[j] == 0)
                {
                    file.index[hasFind] = j + 1;
                    table[j] = (hasFind == 0 ? 2 : 1);
                    hasFind++;
                }
                if (hasFind == file.index.Length) break;
            }
            files[fileNum++] = file;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            flush_table();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                int x = listView2.Items.IndexOf(listView2.FocusedItem);
                Oper.deleteFile(table, files[x]);

                files[x] = null;
                fileNum--;
                Oper.flushFile(files, fileNum);
                flushView();

                flush_table();
            }
            

        }

        private void button7_Click(object sender, EventArgs e)
        {
            int x = listView2.Items.IndexOf(listView2.FocusedItem);
            flush_table(files[x]);

        }
    }

    public class Oper
    {
        
        public static void init_table(int [] disk_table, ListView list)
        {
            for (int i = 0; i < 1000; i++) disk_table[i] = 0;
        }

        public static void insertFile(File file, ListView l1, ListView l2)
        {
            ListViewItem l = l1.Items.Add(file.fileName);
            l.SubItems.Add(Math.Round(file.tiji, 2).ToString() + "K");

            ListViewItem m = l2.Items.Add(file.fileName);
            m.SubItems.Add(file.index[0].ToString());

            string s = "";
            for (int j = 1; j < file.index.Length; j++) s += file.index[j].ToString() + " ";
            m.SubItems.Add(s);
        }

        public static void flushFile(File[] files, int fileNum)
        {
            // i为寻找非空的指针，j为保存后的指针
            for (int i = 0, j = 0; i < files.Length; i++)
            {
                if (files[i] != null) files[j++] = files[i];
                if (j == fileNum) return;
            }
        }

        public static void deleteFile(int[] table, File file)
        {
            for (int i = 0; i < file.index.Length; i++)
            {
                table[file.index[i] - 1] = 0;
            }
        }

    }

    public class File
    {
        public double tiji;         // 文件体积
        public int[] index;         // 文件编号
        public string fileName;     // 文件名

        // 构造函数
        public File(double tiji,string fileName)
        {
            this.tiji = tiji;
            int need = (int)(tiji) / 2 + 1;


            this.index = new int[need];
            this.fileName = fileName;
        }
    }
}

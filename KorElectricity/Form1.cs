using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using WordCloud;
using System.IO;

namespace KorElectricity
{
    public partial class Form1 : Form
    {
        private SolidBrush[] Palette = new SolidBrush[10];
        private Pen[] pen = new Pen[10];
        private int Gred, Ggreen, Gblue;
        private float[] valueArray = new float[10];
        private string path = @"C:\\KorElectricity\\WordCloud.JPG";
        private AnaysisData analData = new AnaysisData();
        private WordCloud.WordCloud wc = new WordCloud.WordCloud(521, 744, true);
        private List<string> forCloudW = new List<string>();
        private List<int> forCloudC = new List<int>();
        


        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                listBox2.Items.Add("");
            }
            for (int i=0; i<30; i++)
            {
                forCloudC.Add(0);
                forCloudW.Add(null);
            }
            this.Paint += Form_Paint;
        }
        private new void Cloudinng_Word()
        {
            for(int i=0; i<analData.textList.Count; i++)
            {
                if (i == 29)
                    break;
                else
                {
                    forCloudW[i] = analData.textList[i].word;
                    forCloudC[i] = analData.textList[i].count;
                }
            }
            var cloud = wc.Draw(forCloudW, forCloudC);
            pictureBox1.Image = cloud;
        }
        private void Form_Load()
        {
            for (int i = 0; i < analData.textList.Count; i++)
            {
                if (i == 10)
                    break;
                else
                {
                    int value = analData.textList[i].count;
                    this.valueArray[i] = (float)value;
                }
            }
        }
        private void load_Data()
        {
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                if (listBox2.Items.Count == 15)
                    break;
                else
                    listBox2.Items[i] = analData.textList[i].word + "\t" + analData.textList[i].count.ToString();
            }
        }
        private void MakeColor()
        {
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                Gred = rand.Next(80, 250);
                Ggreen = rand.Next(80, 250);
                Gblue = rand.Next(80, 250);
                Palette[i] = new SolidBrush(Color.FromArgb(255, Gred, Ggreen, Gblue));
                pen[i] = new Pen(Color.FromArgb(255, Gred, Ggreen, Gblue));
            }
        }
        private void Form_Paint(object sendor, PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int size = 250;
            int left = 12;
            int top = ClientSize.Height / 2;

            Rectangle rectangle = new Rectangle(left, top, size, size);
            MakeColor();
            DrawPieChart(e.Graphics, rectangle, this.Palette, this.pen, this.valueArray);
        }
        private void DrawPieChart(Graphics graphics, Rectangle rectangle, SolidBrush[] Palette, Pen[] pen, float[] valueArray)
        {
            float totalValue = valueArray.Sum();
            float startAngle = 0;
            for (int i = 0; i < valueArray.Length; i++)
            {
                float sweepAngle = valueArray[i] * 360f / totalValue;
                graphics.FillPie(Palette[i % Palette.Length], rectangle, startAngle, sweepAngle);
                graphics.DrawPie(pen[i % pen.Length], rectangle, startAngle, sweepAngle);

                startAngle += sweepAngle;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Add(textBox1.Text);
                analData.AnalysisText(textBox1.Text);
                Form_Load();
                load_Data();
            }
            catch (ArgumentOutOfRangeException except)
            {
                Console.WriteLine(except.Message);
            }
            finally
            {
                if (Directory.Exists(path))
                    File.Delete(path);
                Cloudinng_Word();
                textBox1.Text = "";   
                Refresh();
            }
        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    listBox1.Items.Add(textBox1.Text);
                    analData.AnalysisText(textBox1.Text);
                    Form_Load();
                    load_Data();
                }
                catch (ArgumentOutOfRangeException except)
                {
                    Console.WriteLine(except.Message);
                }
                finally
                {
                    if (Directory.Exists(path))
                        File.Delete(path);
                    Cloudinng_Word();
                    textBox1.Text = "";
                    Refresh();
                }
            }
        }
    }
    class AnaysisData
    {
        public List<TextData> textList = new List<TextData>();
        public List<string> sourceList = new List<string>();
        public List<string> orginList = new List<string>();
        private string[] sepText;
        private char[] textPiece1, textPiece2;
        private string compareText1, compareText2, AnalyzedText;
        private int value;
        public AnaysisData()
        {

        }
        public void AnalysisText(string str1)
        {
            sepText = str1.Split(' ');      //띄어쓰기를 기준으로 분할
            foreach (var item in sepText)
                sourceList.Add(item);
            try
            {
                for (int i = 0; i < sepText.Length; i++)        //분할된 수 만큼 반복
                {
                    textPiece1 = sepText[i].ToCharArray();
                    for (int j = 0; j < sourceList.Count; j++)      //비교할 모든 단어수 만큼 반복
                    {
                        textPiece2 = sourceList[j].ToCharArray();
                        if (textPiece1.Length <= textPiece2.Length)
                            value = textPiece1.Length;
                        else
                            value = textPiece2.Length;
                        if (textPiece1.Length == 1 || textPiece2.Length == 1)
                            continue;
                        else
                        {
                            if (textPiece1[0].Equals(textPiece2[0]) && textPiece1[1].Equals(textPiece2[1]))
                            {
                                for (int k = 0; k < value; k++)
                                {
                                    CompareText(textPiece1, textPiece2, k);
                                    if (compareText1.Equals(compareText2))
                                    {
                                        AnalyzedText = compareText1;
                                        continue;
                                    }
                                    else
                                        break;
                                }
                                textList.Add(new TextData() { word = AnalyzedText, count = 1 });
                                compareText1 = null;
                                compareText2 = null;
                            }
                            else
                                continue;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Source);
            }
            finally
            {
                textList = textList.GroupBy(p => p.getWord()).Select(g => g.First()).ToList();  //비교하여 만들어진 데이터가 중복으로 존재하는 것을 방지한다.
                for (int i = 0; i < textList.Count; i++)
                {
                    str1 = textList[i].word;        //만들어진 문자열 데이터를
                    textList[i].count = sourceList.FindAll(x => x.Contains(str1)).Count; //sourceList에서 해당 단어가 나온 횟수를 세어준다.
                }
                textList.Sort((x1, x2) => x2.count.CompareTo(x1.count));     //빠른 검색을 위해 count(나온횟수)를 기준으로 내림차순 정렬
                sepText = null;
            }
        }
        private void CompareText(char[] cha1, char[] cha2, int num1)
        {
            compareText1 += cha1[num1];
            compareText2 += cha2[num1];
        }
    }
    class TextData
    {
        public string word {get; set;}
        public int count { get; set; }

        public string getWord()
        {
            return word;
        }
    }
}
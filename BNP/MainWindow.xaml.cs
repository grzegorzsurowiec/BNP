using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;

namespace BNP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<OdcinkiPrzeliczone> ObliczoneOdcinki = new List<OdcinkiPrzeliczone>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var itemList = new List<Odcinki>();
            try
            {

                using (Stream stream = File.Open("data.bin", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    var odcinki = (List<Odcinki>)bin.Deserialize(stream);
                    foreach (Odcinki o in odcinki)
                    {
                        itemList.Add(o);
                    }
                }
            }
            catch (IOException)
            {
            }

            CollectionViewSource itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = itemList;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            this.lvObliczone.Items.Clear();
            this.ObliczoneOdcinki.Clear();

            ushort km = 0;
            ushort czas = 0;

            CollectionViewSource itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));

            List<KeyValuePair<DateTime, ushort>> punkty = new List<KeyValuePair<DateTime, ushort>>();

            int count = 0;

            foreach (Odcinki od in (List<Odcinki>)itemCollectionViewSource.Source)
            {
                count ++;

                try
                {
                    string[] sTempo = od.Tempo.Split(':');

                    ushort tempo = Convert.ToUInt16(Convert.ToUInt16(sTempo[0]) * 60 + Convert.ToUInt16(sTempo[1]));

                    for (ushort i = 0; i < od.Dlugosc; i++)
                    {
                        km++;

                        czas += tempo;

                        OdcinkiPrzeliczone oo = new OdcinkiPrzeliczone() { KM = km, TempoKM = tempo, TempoSrednie = Convert.ToUInt16(czas / km) };
                        this.ObliczoneOdcinki.Add(oo);
                        this.lvObliczone.Items.Add(oo);

                        ushort tempoM = Convert.ToUInt16(oo.TempoSrednie / 60);
                        ushort tempoS = Convert.ToUInt16(oo.TempoSrednie - tempoM * 60);
                        punkty.Add(new KeyValuePair<DateTime, ushort>(DateTime.ParseExact((string.Format("{0}:{1}", tempoM.ToString("D2"), tempoS.ToString("D2"))), "mm:ss", System.Globalization.CultureInfo.CurrentCulture), oo.KM));

                    }
                }

                catch (Exception) { MessageBox.Show(string.Format("Wartość w wieszy ({0}) są nieprawidłowe i zostaną pominiętę!!!", count)); }

               
            }

            ((LineSeries)this.cWykres.Series[0]).ItemsSource = punkty;

           


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                using (Stream stream = File.Open("data.bin", FileMode.Create))
                {
                    CollectionViewSource itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, (List<Odcinki>)itemCollectionViewSource.Source);
                }
            }
            catch (IOException)
            {
            }
        }
    }

    public class CourseValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            Odcinki odc = (value as BindingGroup).Items[0] as Odcinki;

            bool valid = true;
            string errormsg = "";

            if (odc.Dlugosc < 0)
            {
                valid = false;
                errormsg = "Długość odcinka musi być większa od zera.";
            }

            if (odc.Tempo != null)
            {
                if (odc.Tempo == string.Empty || !new Regex(@"^[0-9]{1,2}[:][0-9]{2}$").IsMatch(odc.Tempo))
                {
                    valid = false;
                    errormsg = "Nieprawidłowy format pola tempo";
                }
            }


            if (valid) return ValidationResult.ValidResult;
            return new ValidationResult(false, errormsg);

        }
    }

    [Serializable]
    public class Odcinki
    {
        public ushort Dlugosc { get; set; }
        public string Tempo { get; set; }
    }

    public class OdcinkiPrzeliczone
    {
        public ushort KM { get; set; }
        public ushort TempoKM { get; set; }
        public ushort TempoSrednie { get; set; }

        public string sTempoKM {
            get
            {
                ushort min = Convert.ToUInt16( TempoKM/60 ); 
                ushort sek = Convert.ToUInt16( TempoKM -  min*60 );
                return string.Format("{0}:{1}", min.ToString("D2"), sek.ToString("D2"));
            }
        }
        public string sTempoSrednie
        {
            get
            {
                ushort min = Convert.ToUInt16(TempoSrednie / 60);
                ushort sek = Convert.ToUInt16(TempoSrednie - min * 60);
                return string.Format("{0}:{1}", min.ToString("D2"), sek.ToString("D2"));
            }
        }
    } 
}





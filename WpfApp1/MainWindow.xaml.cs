using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //indeksy poszczególnych paliw wyglądają następująco
        //0: Pb95, 1: Pb98, 2: Disel, 3: gaz
        float[] sumaPaliw = { 0, 0, 0, 0 };
        float sumaZasieg = 0;
        float sumaKoszt = 0;
        public float cenapb95, cenapb98, cenadisel, cenagaz;

        //Funkcja zajmująca się przygotowaniem i eksportem danych
        private void BT_export_Click(object sender, RoutedEventArgs e)
        {
            //Streamwriter zajmuje się zapisaniem do ścieżki. Zamiast ścieżki podaje nazwę pliku a jego domyślnym
            //miejscem zapisu będzie miejsce gdzie znajduje się plik
            StreamWriter SaveFile = new StreamWriter("export.txt");
            //kolejno funkcja WriteLine dodaje linijki do pliku
            SaveFile.WriteLine("------------------------------------\nCENY\n------------------------------------");
            SaveFile.WriteLine("Pb95: " + cenapb95 + "PLN/L\nPb98: " + cenapb98 + "PLN/L\nDisel: " + cenadisel + "PLN/L\nGaz: " + cenagaz +"PLN/L");
            SaveFile.WriteLine("------------------------------------\nPOSIADANE SAMOCHODY\n------------------------------------");

            //Dodaje wszystkie elementy z zapisanych pojazdów oraz sume
            foreach (var item in LB_pojazdy.Items)
            {
                SaveFile.WriteLine(item.ToString());
            }
            SaveFile.WriteLine("------------------------------------\nSUMA\n------------------------------------");
            foreach (var item in LB_wyniki.Items)
            {
                SaveFile.WriteLine(item.ToString());
            }

            //konwersja linijek do tekstu "string" i zamknięcie pliku. Potwierdzenie sukcesu okienkiem
            SaveFile.ToString();
            SaveFile.Close();

            MessageBox.Show("Obliczenia wyeksportowano do tego samego folderu, w którym znajduje się program!");
        }

        //lista niezbędna do importu danych.
        List<string> lista = new List<string>();
        private void BT_import_Click(object sender, RoutedEventArgs e)
        {
            String line;
            try
            {
                //kolejno pobierane sa linie z pliku export.txt i dodawane do listy. try catch wyświetli błędy w razie
                //ich wystąpienia
                StreamReader sr = new StreamReader("export.txt");
                line = sr.ReadLine();
                while (line != null)
                {
                    Console.WriteLine(line);
                    lista.Add(line);
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: " + ex.Message);
                return;
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            //przygotowanie listBoxów do wpisania zaimportowanych danych
            LB_pojazdy.Items.Clear();
            LB_wyniki.Items.Clear();
            LB_wyniki.Items.Add(lista[lista.Count - 1]); //metoda .Count zwraca długość listy. Inaczej niż w przypadku tablicy gdzie wykorzystuje się .length
            
            //dodawanie pojazdów aż nie trafimy na zakończenie sekcji
            for (int i = 10; i < lista.Count; i++)
            {
                if(lista[i] == "------------------------------------")
                {
                    break;
                }
                LB_pojazdy.Items.Add(lista[i]);
            }

            //wklejamy dane cen, które zawsze znajdują się w tych samych linijkach pliku.
            //Opcja .Split() dzieli nam tekst w miejscach wyznaczonego znaku i zwraca tablice
            //Drugie miejsce (index 1) to cena z dopiskiem PLN/L
            //Celem pozbycia się dopisku, używam opcji Remove() usuwając ostatnie 5 znaków, pozostawiąjąc samą liczbe
            cenapb95 = float.Parse(lista[3].Split(' ')[1].Remove(lista[3].Split(' ')[1].Length - 5, 5));
            cenapb98 = float.Parse(lista[4].Split(' ')[1].Remove(lista[4].Split(' ')[1].Length - 5, 5));
            cenadisel = float.Parse(lista[5].Split(' ')[1].Remove(lista[5].Split(' ')[1].Length - 5, 5));
            cenagaz = float.Parse(lista[6].Split(' ')[1].Remove(lista[6].Split(' ')[1].Length - 5, 5));
            
            TB_cena_pb95.Text = cenapb95.ToString();
            TB_cena_pb98.Text = cenapb98.ToString();
            TB_cena_d.Text = cenadisel.ToString();
            TB_cena_gaz.Text = cenagaz.ToString();

            //identyczna sytuacja co w przypadku cen paliw. Tutaj jednak podzielony tekst dzielę ponownie
            sumaPaliw[0] = float.Parse(lista[lista.Count - 1].Split(':')[1].Split('L')[0]);
            sumaPaliw[1] = float.Parse(lista[lista.Count - 1].Split(':')[2].Split('L')[0]);
            sumaPaliw[2] = float.Parse(lista[lista.Count - 1].Split(':')[3].Split('L')[0]);
            sumaPaliw[3] = float.Parse(lista[lista.Count - 1].Split(':')[4].Split('L')[0]);

            sumaKoszt = float.Parse(lista[lista.Count - 1].Split(':')[5].Split('P')[0]);
            sumaZasieg = float.Parse(lista[lista.Count - 1].Split(':')[6].Split('k')[0]);

            //Aktywacja opcji dodawania pojazdów oraz deaktywacja możliwości zmian cen
            CB_typPaliwa.IsEnabled = true;
            CB_typPojazdu.IsEnabled = true;
            BT1.IsEnabled = true;
            TB_spalanie.IsEnabled = true;
            TB_zatankowano.IsEnabled = true;
            TB_cena_d.IsEnabled = false;
            TB_cena_gaz.IsEnabled = false;
            TB_cena_pb95.IsEnabled = false;
            TB_cena_pb98.IsEnabled = false;
            BTN_cena.IsEnabled = false;
            MessageBox.Show("Pomyślnie pobrano dane"); //informacja o pomyślnym imporcie
        }

        //Dodawanie pojacdu
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            float zatankowano, spalanie;
            string typ, paliwo;

            //try catch celem znalezienia błędów imporu oraz if()
            try
            {
                zatankowano = float.Parse(TB_zatankowano.Text);
                spalanie = float.Parse(TB_spalanie.Text);
                typ = CB_typPojazdu.Text;
                paliwo = CB_typPaliwa.Text;
                if (zatankowano <= 0 || spalanie <= 0 || typ == "" || paliwo == "")
                {
                    MessageBox.Show("Wpisano niepoprawne lub niepełne dane");
                    return;
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            float koszt = 10;
            switch (paliwo)
            {
                case "Pb95":
                    koszt = zatankowano * cenapb95;
                    sumaPaliw[0] += zatankowano;
                    break;
                case "Pb98":
                    koszt = zatankowano * cenapb98;
                    sumaPaliw[1] += zatankowano;
                    break;
                case "Disel":
                    koszt = zatankowano * cenadisel;
                    sumaPaliw[2] += zatankowano;
                    break;
                case "Gaz":
                    koszt = zatankowano * cenagaz;
                    sumaPaliw[3] += zatankowano;
                    break;
            }
            LB_pojazdy.Items.Add(typ + " (zasięg: " + (100 / spalanie * zatankowano).ToString("n2") + "km, " + paliwo + ", koszt: " + koszt.ToString("n2") + "zł)");
            sumaZasieg += 100 / spalanie * zatankowano;
            sumaKoszt += koszt;
            Podlicz();
            
        }

        //funkcja podlicz wywoływana po każdym dodaniu pojazdu celem sumowania pewnych elementów.
        void Podlicz()
        {
            BT_export.IsEnabled = true;
            BT_import.IsEnabled = true;
            LB_wyniki.Items.Clear();
            LB_wyniki.Items.Add("Zatankowano Pb95: "+sumaPaliw[0].ToString("n2") + "L Pb98: "+sumaPaliw[1].ToString("n2") + "L Disel: "+sumaPaliw[2].ToString("n2") + "L Gaz: "+sumaPaliw[3].ToString("n2") + "L. Łączny koszt: "+sumaKoszt.ToString("n2") + "PLN, Łączny zasięg: "+sumaZasieg.ToString("n2") + "km");
        }


        //Zapisanie ustalonych cen
        private void BTN_cena_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                cenapb95 = float.Parse(TB_cena_pb95.Text);
                cenapb98 = float.Parse(TB_cena_pb98.Text);
                cenadisel = float.Parse(TB_cena_d.Text);
                cenagaz = float.Parse(TB_cena_gaz.Text);
                if (cenapb95 <= 0 || cenapb98 <= 0 || cenadisel <= 0 || cenagaz <= 0)
                {
                    MessageBox.Show("Liczba nie może być mniejsza od 0");
                    return;
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //aktywacja opcji dodania pojazdów oraz deaktywacja edycji cen.
            CB_typPaliwa.IsEnabled = true;
            CB_typPojazdu.IsEnabled = true;
            BT1.IsEnabled = true;
            TB_spalanie.IsEnabled = true;
            TB_zatankowano.IsEnabled = true;
            TB_cena_d.IsEnabled = false;
            TB_cena_gaz.IsEnabled = false;
            TB_cena_pb95.IsEnabled = false;
            TB_cena_pb98.IsEnabled = false;
            BTN_cena.IsEnabled = false;
        }
    }
}
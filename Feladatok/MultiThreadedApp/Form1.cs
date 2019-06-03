using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiThreadedApp
{
    public partial class Form : System.Windows.Forms.Form
    {
        delegate void BikeAction(Button bike); 
        ManualResetEvent Start = new ManualResetEvent(false);
        AutoResetEvent Depo = new AutoResetEvent(false);
        Random random = new Random();
        long pixels;
        private object sync = new object();
        int resetpos;

        public void MoveBike(Button bike) //biciklimozgató függvény, random értékkel előrébb viszi a biciklit
        {
            if(InvokeRequired)
            {
                Invoke(new BikeAction(MoveBike), bike);
            }
            else
            {
                int rand = random.Next(3, 9);
                bike.Left += rand;
                increasePixels(rand);
            }
        }

        public void BikeThreadFunction(object param) //ezzel a függvénnyel mozgatjuk a bicikliket
        {
            try
            {
                Button bike = (Button)param; //megkapott objektum paraméterből gombot csinálunk
                bike.Tag = Thread.CurrentThread; //aktuális szál lekérdezése

                while (bike.Left < pStart.Left + 55) //amíg nem érünk el a startpanel kb végéig, addig megy a bicikli 0,1 mpnként
                {
                    MoveBike(bike);
                    Thread.Sleep(100);
                }
                if (Start.WaitOne()) //
                {
                    while (bike.Left < pDepo.Left + 65 )//amíg nem értünk el a pihenő végéig, addig megy a bicikli
                    {
                        MoveBike(bike);
                        Thread.Sleep(100);
                    }
                }
                if (Depo.WaitOne())
                {
                    while (bike.Left < pTarget.Left) //amíg nem értünk célba addig megyünk
                    {
                        MoveBike(bike);
                        Thread.Sleep(100);
                    }
                }
            }
            catch(ThreadInterruptedException)
            {

            }
        }

        public Form()
        {
            InitializeComponent();
            resetpos = bBike1.Left; //itt lekérdezzük az egyik bicikli kezdőpozícióját, hogy tudjuk használni a resethez
        }

        private void bStart_Click(object sender, EventArgs e) //ha startot indítunk akkor elindulnak a biciklik egyszerre
        {
            StartBike(bBike1);
            StartBike(bBike2);
            StartBike(bBike3);
        }

        private void StartBike(Button bBike) //a biciklik indítása új szál segítségével
        {
            Thread t = new Thread(BikeThreadFunction);
            bBike.Tag = t;
            t.IsBackground = true;
            t.Start(bBike);
        }

        private void bStep1_Click(object sender, EventArgs e) //az első step gomb segítségével el tudjuk indítani a startnál várakozó bicikliket
        {
            Start.Set(); //ezzel "szólunk" a szálaknak, hogy mehetnek
        }

        private void bStep2_Click(object sender, EventArgs e) //a második step gombbal pedig a pihenőből mennek tovább egyenként a biciklik
        {
            Depo.Set(); //ezzel "szólunk" a következő szálnak, hogy mehet (autoreseteventnél egy szálnak szólunk)
        }

        private void increasePixels(long dist) //ezzel a függvénnyel növeljük a távolságot
        {
            lock(sync)
            {
                pixels += dist;
            }
        }

        private long getPixels() //ezzel a függvénnyel kapjuk vissza a távolságot
        {
            lock(sync)
            {
                return pixels;
            }
        }

        private void bPixels_Click(object sender, EventArgs e) //ha megnyomjuk a pixeles gombot akkor a szöveg kicserélődik a távra
        {
            bPixels.Text = pixels.ToString();
        }

        private void bBike1_Click(object sender, EventArgs e)
        {
            bike_Click(sender, e);
        }

        private void bBike2_Click(object sender, EventArgs e)
        {
            bike_Click(sender, e);
        }

        private void bBike3_Click(object sender, EventArgs e)
        {
            bike_Click(sender, e);
        }

        private void bike_Click(object sender, EventArgs e) //ha rákattintunk a biciklire akkor visszarakjuk az eredeti helyére és megszakítjük és reseteljük a szálat
        {
            Button bike = (Button)sender;
            Thread thread = (Thread)bike.Tag;

            if(thread == null)
            {
                return;
            }

            thread.Interrupt();
            thread.Join();
            Start.Reset();
            Depo.Reset();
            bike.Left = resetpos;
            StartBike(bike);
        }
    }
}

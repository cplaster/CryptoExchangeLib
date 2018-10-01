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
using CryptoExchangeLib.Common;

namespace CryptoExchangeLib
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IExchange cryptopia = new Cryptopia();
            TradePair tp = new TradePair() { BaseLabel = "BTC", CurrencyLabel = "ETN" };
            MarketOrdersResponse r = cryptopia.GetMarketOrders(new MarketOrdersRequest(tp));

            //IExchange bittrex = new Bittrex();
            //TradePair tp = new TradePair() { BaseLabel = "BTC", CurrencyLabel = "RDD" };
            //MarketOrdersRequest mr = new MarketOrdersRequest(tp);
            //MarketOrdersResponse r = bittrex.GetMarketOrders(mr);
            int i = 1;


            
        }

    }
}

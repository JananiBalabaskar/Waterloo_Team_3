using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Digident_Group3.Interfaces;
using Digident_Group3.Services;
using System.Configuration;

namespace Digident_Group3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IMessageBoxService _messageBoxService;
        public MainWindow()
        {
            InitializeComponent();
            // Retrieve the connection string from App.config
            string connectionString = GetConnectionString("MyDbConnectionString");


            _databaseService = new DatabaseService(connectionString);
            _messageBoxService = new MessageBoxService();
            //ShowLoginPage();

        }
        private string GetConnectionString(string name)
        {
            var settings = ConfigurationManager.ConnectionStrings[name];
            return settings?.ConnectionString;
        }

        private void ShowLoginPage()
        {

            PatientLoginPage loginPage = new PatientLoginPage(_databaseService, _messageBoxService);
            ChangePage(loginPage);
        }
        private void Booknow(object sender, RoutedEventArgs e)
        {
            Register registerPage = new Register(_databaseService, _messageBoxService);
            ChangePage(registerPage);
        }

        private void aboutus(object sender, RoutedEventArgs e)
        {
            Aboutus aboutusPage = new Aboutus();
            ChangePage(aboutusPage);

        }

        private void services(object sender, RoutedEventArgs e)
        {
            ServicesImage1.BringIntoView();
        }

        private void Contactus(object sender, RoutedEventArgs e)
        {
            Contactus contactusPage = new Contactus();
            ChangePage(contactusPage);
        }

        private void Register(object sender, RoutedEventArgs e)
        {

            Register registerPage = new Register(_databaseService, _messageBoxService);
            ChangePage(registerPage);

        }

        private void QuestionForm(object sender, RoutedEventArgs e)
        {

        }

        private void Loginbutton(object sender, RoutedEventArgs e)
        {
            
            Login loginPage = new Login();
            ChangePage(loginPage);
          
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        public void ChangePage(Page page)
        {
            Content = page;
        }
    }
}
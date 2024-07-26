using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Digident_Group3.Interfaces;
using Digident_Group3.Services;
using System.Configuration;

namespace Digident_Group3
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        private readonly IDatabaseService _databaseService;
        private readonly IMessageBoxService _messageBoxService;

        // Constructor that accepts IDatabaseService
        public Register(IDatabaseService databaseService, IMessageBoxService messageBoxService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _messageBoxService = messageBoxService;
        }

        // Default constructor for scenarios where no service is injected
        public Register()
        : this(new DatabaseService(ConfigurationManager.ConnectionStrings["MyDbConnectionString"].ConnectionString), new MessageBoxService())
        {
        }


        private void Homebutton(object sender, RoutedEventArgs e)
        {
            MainWindow window1 = new MainWindow();
            window1.Show();
            Window.GetWindow(this)?.Close();
        }
        internal void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ValidateEmail();
            ValidatePassword();
            ValidateFirstName();
            ValidateLastName();
            ValidateDateOfBirth();
            ValidateAddress();
            ValidatePhoneNumber();

            if (!string.IsNullOrEmpty(EmailErrorText.Text) ||
                !string.IsNullOrEmpty(PasswordErrorText.Text) ||
                !string.IsNullOrEmpty(ConfirmPasswordErrorText.Text) ||
                !string.IsNullOrEmpty(FirstNameErrorText.Text) ||
                !string.IsNullOrEmpty(LastNameErrorText.Text) ||
                !string.IsNullOrEmpty(DateOfBirthErrorText.Text) ||
                !string.IsNullOrEmpty(AddressErrorText.Text) ||
                !string.IsNullOrEmpty(PhoneNumberErrorText.Text))
            {
                _messageBoxService?.Show("Please check the fields!", "Error", MessageBoxButton.OK);
                return;
            }

            string email = EmailTextBox?.Text;
            string password = PasswordBox?.Password;
            string firstName = FirstNameTextBox?.Text;
            string lastName = LastNameTextBox?.Text;
            DateTime dateOfBirth = DateOfBirthPicker?.SelectedDate ?? DateTime.MinValue;
            string address = AddressTextBox?.Text;
            string phoneNumber = PhoneNumberTextBox?.Text;

            if (_databaseService == null)
            {
                _messageBoxService?.Show("Database service is not available.", "Error", MessageBoxButton.OK);
                return;
            }

            bool isRegistered = _databaseService.RegisterUser(email, password, firstName, lastName, dateOfBirth, address, phoneNumber);

            if (isRegistered)
            {
                MessageBoxResult? result = _messageBoxService?.Show("User registered successfully! Click OK to proceed to login.", "Success", MessageBoxButton.OK);

                if (result == MessageBoxResult.OK)
                {
                    MainWindow window1 = new MainWindow();
                    window1.Show();
                    Window.GetWindow(this)?.Close();
                }
            }
            else
            {
                _messageBoxService?.Show("User registration failed.", "Error", MessageBoxButton.OK);
            }
        }

        public void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EmailErrorText.Text = "";
        }

        public void ValidateEmail()
        {
            string email = EmailTextBox.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                EmailErrorText.Text = "Email is required.";
                return;
            }

            if (!IsValidEmail(email))
            {
                EmailErrorText.Text = "Please enter a valid email address.";
                return;
            }

            EmailErrorText.Text = "";
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            return Regex.IsMatch(email, pattern);
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordErrorText.Text = "";
        }

        public void ValidatePassword()
        {
            string password = PasswordBox.Password.Trim();
            if (string.IsNullOrEmpty(password))
            {
                PasswordErrorText.Text = "Password is required.";
                return;
            }

            if (!IsPasswordValid(password))
            {
                PasswordErrorText.Text = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";
                return;
            }

            PasswordErrorText.Text = "";
        }

        public bool IsPasswordValid(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$");
        }

        public void ConfirmPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordErrorText.Text = "";
        }

        public void ValidateConfirmPassword()
        {
            string password = PasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(confirmPassword))
            {
                ConfirmPasswordErrorText.Text = "Confirm password is required.";
                return;
            }

            if (password != confirmPassword)
            {
                ConfirmPasswordErrorText.Text = "Passwords do not match.";
                return;
            }

            ConfirmPasswordErrorText.Text = "";
        }

        public void FirstNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateFirstName();
        }

        public void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                FirstNameErrorText.Text = "First Name is required.";
            }
            else if (!Regex.IsMatch(FirstNameTextBox.Text, "^[a-zA-Z'-]+$"))
            {
                FirstNameErrorText.Text = "First Name can only contain letters, apostrophes, and hyphens.";
            }
            else
            {
                FirstNameErrorText.Text = string.Empty;
            }
        }

        public void ValidateLastName()
        {
            string lastName = LastNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(lastName))
            {
                LastNameErrorText.Text = "Last Name is required.";
                return;
            }

            if (!Regex.IsMatch(lastName, "^[a-zA-Z'-]+$"))
            {
                LastNameErrorText.Text = "Last Name can only contain letters, apostrophes, and hyphens.";
                return;
            }

            if (lastName.Length > 50)
            {
                LastNameErrorText.Text = "Last Name cannot exceed 50 characters.";
                return;
            }

            LastNameErrorText.Text = "";
        }

        public void DateOfBirthPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateDateOfBirth();
        }

        public void ValidateDateOfBirth()
        {
            if (DateOfBirthPicker.SelectedDate == null || DateOfBirthPicker.SelectedDate > DateTime.Now)
            {
                DateOfBirthErrorText.Text = "Please enter a valid date of birth.";
            }
            else
            {
                DateOfBirthErrorText.Text = string.Empty;
            }
        }

        public void ValidateAddress()
        {
            string address = AddressTextBox.Text.Trim();

            if (string.IsNullOrEmpty(address))
            {
                AddressErrorText.Text = "Address is required.";
                return;
            }

            if (address.Length > 100)
            {
                AddressErrorText.Text = "Address cannot exceed 100 characters.";
                return;
            }

            string pattern = @"^\d+\s+[a-zA-Z]+(\s[a-zA-Z]+)*\s+(?i:St|Ave|Blvd|Rd|Dr)$";
            if (!Regex.IsMatch(address, pattern))
            {
                AddressErrorText.Text = "Invalid address format. Example: 123 Albert St";
                return;
            }

            AddressErrorText.Text = "";
        }

        public void ValidatePhoneNumber()
        {
            string phoneNumber = PhoneNumberTextBox.Text.Trim();

            if (string.IsNullOrEmpty(phoneNumber))
            {
                PhoneNumberErrorText.Text = "Phone Number is required.";
                return;
            }

            if (!Regex.IsMatch(phoneNumber, @"^[0-9]+$"))
            {
                PhoneNumberErrorText.Text = "Phone Number can only contain numbers.";
                return;
            }

            if (phoneNumber.Length != 10)
            {
                PhoneNumberErrorText.Text = "Phone Number must be 10 digits long.";
                return;
            }

            PhoneNumberErrorText.Text = "";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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



namespace Digident_Group3
{
    public partial class Book_Appointment : Page
    {
        private const string connectionString = @"Data Source=JANANIDESK\MSSQLSERVER05;Initial Catalog=Digidentdb;Integrated Security=True;TrustServerCertificate=True";

        public Book_Appointment()
        {
            InitializeComponent();
        }

        private bool ValidateAppointment()
        {
            bool isValid = true;

            // Check Patient Name
            isValid &= ValidatePatientName(txtPatientName.Text);
            // Check Phone Number
            isValid &= ValidatePhoneNumber(txtPhoneNumber.Text);
            // Check Address
            isValid &= ValidateAddress(txtAddress.Text);
            // Check Appointment Type
            isValid &= ValidateAppointmentType(cmbAppointmentType.Text);
            // Check Appointment Date
            isValid &= ValidateAppointmentDate(dpAppointmentDate.SelectedDate);
            // Check Appointment Time
            isValid &= !string.IsNullOrWhiteSpace(cmbAppointmentTime.Text);
            // Check Patient Allergy Info
            isValid &= ValidatePatientAllergy(txtPatientinfo.Text);

            return isValid;
        }

        // Error Handling
        private bool ValidatePatientName(string PatientName)
        {
            if (string.IsNullOrWhiteSpace(PatientName))
            {
                SetError(txtPatientName, "Invalid Input.");
                return false;
            }
            else if (!Regex.IsMatch(PatientName, @"^[a-zA-Z\s]+$"))
            {
                SetError(txtPatientName, "Only alphabets and spaces are allowed.");
                return false;
            }

            ClearError(txtPatientName);
            return true;
        }

        private bool ValidatePhoneNumber(string PhoneNumber)
        {
            if (!Regex.IsMatch(PhoneNumber, @"^\d{10}$") || PhoneNumber.All(c => c == '0'))
            {
                SetError(txtPhoneNumber, "Invalid Input");
                return false;
            }

            ClearError(txtPhoneNumber);
            return true;
        }

        private bool ValidateAddress(string Address)
        {
            // The regex pattern breakdown:
            // ^ - Start of the string
            // \d+ - One or more digits
            // \s+ - One or more whitespace characters
            // [a-zA-Z]+ - One or more alphabetic characters (for the street name)
            // (\s[a-zA-Z]+)* - Zero or more occurrences of a space followed by more alphabetic characters (to handle multi-word street names)
            // \s(St|Ave|Blvd|Rd|Dr)$ - A space followed by one of the allowed street types, and end of string
            string pattern = @"^\d+\s+[a-zA-Z]+(\s[a-zA-Z]+)*\s+(?i:St|Ave|Blvd|Rd|Dr|Street)$";

            if (!Regex.IsMatch(Address, pattern))
            {
                 SetError(txtAddress, "Invalid address format. Example: 123 Albert St");
                return false;
            }
             ClearError(txtAddress);
            return true;
        }

        private bool ValidateAppointmentType(string appointmentType)
        {
            if (string.IsNullOrWhiteSpace(appointmentType))
            {
                SetError(cmbAppointmentType, "Please select an appointment type.");
                return false;
            }

            ClearError(cmbAppointmentType);
            return true;
        }

        private bool ValidateAppointmentDate(DateTime? appointmentDate)
        {
            if (!appointmentDate.HasValue)
            {
                SetError(dpAppointmentDate, "Please select an appointment date.");
                return false;
            }

            if (appointmentDate.Value.Date < DateTime.Now.Date)
            {
                SetError(dpAppointmentDate, "Appointment date cannot be in the past.");
                return false;
            }

            ClearError(dpAppointmentDate);
            return true;
        }

        private bool ValidatePatientAllergy(string PatientAllergy)
        {
            if (!Regex.IsMatch(PatientAllergy, @"^[a-zA-Z\s]*$"))
            {
                SetError(txtPatientinfo, "Invalid Input");
                return false;
            }

            ClearError(txtPatientinfo);
            return true;
        }

        private void SetError(Control control, string errorMessage)
        {
            TextBlock err = GetErrorTextBlock(control);
            err.Text = errorMessage;
            err.Visibility = Visibility.Visible;
        }

        // Remove error message 
        private void ClearError(Control control)
        {
            TextBlock err = GetErrorTextBlock(control);
            err.Visibility = Visibility.Collapsed;
        }

        // Get error message when handling error
        private TextBlock GetErrorTextBlock(Control control)
        {
            switch (control.Name)
            {
                case "txtPatientName":
                    return PatientNameError;
                case "txtPhoneNumber":
                    return PhoneNumberError;
                case "txtAddress":
                    return AddressError;
                case "txtPatientinfo":
                    return PatientError;
                case "cmbAppointmentType":
                    return AppointmentError;
                case "dpAppointmentDate":
                    return AppointmentDateError;
                default:
                    throw new Exception("Invalid Control name.");
            }
        }

        // Clearing all input fields
        private void ClearInputFields()
        {
            txtPatientName.Clear();
            txtPhoneNumber.Clear();
            txtAddress.Clear();
            cmbAppointmentType.SelectedIndex = -1;
            dpAppointmentDate.SelectedDate = null;
            cmbAppointmentTime.SelectedIndex = -1;
            txtPatientinfo.Clear();
        }

        public class Appointment
        {
            public string PatientName { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public string Address { get; set; } = "";
            public string AppointmentType { get; set; } = "";
            public DateTime AppointmentDate { get; set; }
            public string AppointmentTime { get; set; } = "";
            public string PatientInfo { get; set; } = "";
        }

        private void Bookbutton(object sender, RoutedEventArgs e)
        {
            if (ValidateAppointment())
            {
                Appointment appointment = new Appointment
                {
                    PatientName = txtPatientName.Text,
                    PhoneNumber = txtPhoneNumber.Text,
                    Address = txtAddress.Text,
                    AppointmentType = cmbAppointmentType.Text,
                    AppointmentDate = dpAppointmentDate.SelectedDate.Value,
                    AppointmentTime = cmbAppointmentTime.Text,
                    PatientInfo = txtPatientinfo.Text
                };

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("InsertAppointments", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@PatientName", appointment.PatientName);
                            command.Parameters.AddWithValue("@PhoneNumber", appointment.PhoneNumber);
                            command.Parameters.AddWithValue("@Address", appointment.Address);
                            command.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType);
                            command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                            command.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                            command.Parameters.AddWithValue("@PatientAllergy", appointment.PatientInfo);

                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Appointment booked successfully!");
                    MainWindow? mainWindow = Window.GetWindow(this) as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.ChangePage(new PatientDashboard());
                    }

                    ClearInputFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while booking the appointment: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please fill in all required fields with valid data.");
            }
        }

        private void Backbutton(object sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ChangePage(new PatientDashboard());
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Digident_Group3
{
    public partial class Book_Appointment : Page
    {
        private const string connectionString = @"Data Source=JANANIDESK\MSSQLSERVER05;Initial Catalog=Digidentdb;Integrated Security=True;TrustServerCertificate=True";
        private int _userID;

        public Book_Appointment(int userID)
        {
            InitializeComponent();
            _userID = userID;

            LoadDentists();
            RadioButton_Checked(null, null);
        }

        private void LoadDentists()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT FirstName, LastName FROM Users";

                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = $"{reader["FirstName"]} {reader["LastName"]}",
                            Tag = reader["DentistID"]
                        };
                        cmbDentist.Items.Add(item);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading dentists: {ex.Message}");
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (rbSelf.IsChecked == true)
            {
                lblPatientName.Visibility = Visibility.Collapsed;
                txtPatientName.Visibility = Visibility.Collapsed;
                PatientNameError.Visibility = Visibility.Collapsed;

                lblAddress.Visibility = Visibility.Collapsed;
                txtAddress.Visibility = Visibility.Collapsed;
                AddressError.Visibility = Visibility.Collapsed;

                // Automatically fill in the user's details
                txtPatientName.Text = GetFirstNameFromDatabase(_userID);
                txtPhoneNumber.Text = GetPhoneNumberFromDatabase(_userID);
                txtAddress.Text = GetAddressFromDatabase(_userID);
            }
            else if (rbOther.IsChecked == true)
            {
                lblPatientName.Visibility = Visibility.Visible;
                txtPatientName.Visibility = Visibility.Visible;
                PatientNameError.Visibility = Visibility.Visible;

                lblPhoneNumber.Visibility = Visibility.Visible;
                txtPhoneNumber.Visibility = Visibility.Visible;
                PhoneNumberError.Visibility = Visibility.Visible;

                lblAddress.Visibility = Visibility.Visible;
                txtAddress.Visibility = Visibility.Visible;
                AddressError.Visibility = Visibility.Visible;

                txtPatientName.Clear();
                txtPhoneNumber.Clear();
                txtAddress.Clear();
            }
        }

        private bool ValidateAppointment(bool isBookingForSelf)
        {
            bool isValid = true;

            if (!isBookingForSelf)
            {
                isValid &= ValidatePatientName(txtPatientName.Text);
                isValid &= ValidatePhoneNumber(txtPhoneNumber.Text);
                isValid &= ValidateAddress(txtAddress.Text);
            }

            isValid &= ValidateAppointmentType(cmbAppointmentType.Text);
            isValid &= ValidateAppointmentDate(dpAppointmentDate.SelectedDate);
            isValid &= !string.IsNullOrWhiteSpace(cmbAppointmentTime.Text);
            isValid &= ValidatePatientAllergy(txtPatientinfo.Text);

            return isValid;
        }

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

        private void ClearError(Control control)
        {
            TextBlock err = GetErrorTextBlock(control);
            err.Visibility = Visibility.Collapsed;
        }

        private TextBlock GetErrorTextBlock(Control control)
        {
            return control.Name switch
            {
                "txtPatientName" => PatientNameError,
                "txtPhoneNumber" => PhoneNumberError,
                "txtAddress" => AddressError,
                "txtPatientinfo" => PatientError,
                "cmbAppointmentType" => AppointmentError,
                "dpAppointmentDate" => AppointmentDateError,
                _ => throw new Exception("Invalid Control name."),
            };
        }

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

        private string GetFirstNameFromDatabase(int userID)
        {
            string firstName = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT FirstName FROM Users WHERE UserID = @UserID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        firstName = reader["FirstName"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching first name: {ex.Message}");
                }
            }

            return firstName;
        }

        private string GetPhoneNumberFromDatabase(int userID)
        {
            string phoneNumber = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT PhoneNumber FROM Users WHERE UserID = @UserID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        phoneNumber = reader["PhoneNumber"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching phone number: {ex.Message}");
                }
            }

            return phoneNumber;
        }

        private string GetAddressFromDatabase(int userID)
        {
            string address = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Address FROM Users WHERE UserID = @UserID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        address = reader["Address"].ToString();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching address: {ex.Message}");
                }
            }

            return address;
        }

        private bool IsAppointmentTimeAvailable(DateTime date, string time)
        {
            bool isAvailable = true;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Appointments WHERE AppointmentDate = @AppointmentDate AND AppointmentTime = @AppointmentTime";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AppointmentDate", date);
                command.Parameters.AddWithValue("@AppointmentTime", time);

                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    if (count > 0)
                    {
                        isAvailable = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking appointment availability: {ex.Message}");
                }
            }

            return isAvailable;
        }

        private int GetRandomDentistID()
        {
            int randomDentistID = -1;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 1 DentistID FROM Dentists ORDER BY NEWID()"; // SQL Server specific syntax

                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    randomDentistID = (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching random dentist: {ex.Message}");
                }
            }

            return randomDentistID;
        }

        private void Bookbutton(object sender, RoutedEventArgs e)
        {
            bool isBookingForSelf = rbSelf.IsChecked == true;

            if (ValidateAppointment(isBookingForSelf))
            {
                if (!IsAppointmentTimeAvailable(dpAppointmentDate.SelectedDate.Value, cmbAppointmentTime.Text))
                {
                    MessageBox.Show("The selected date and time are already booked. Please choose another time.");
                    return;
                }
                int selectedDentistID;

                if (cmbDentist.Text == "Random")
                {
                    selectedDentistID = GetRandomDentistID();
                }
                else
                {
                    if (cmbDentist.SelectedItem != null)
                    {
                        ComboBoxItem selectedItem = cmbDentist.SelectedItem as ComboBoxItem;
                        if (selectedItem != null && selectedItem.Tag != null)
                        {
                            // Retrieve DentistID from the database using the selected dentist's details
                            string licenseNumber = selectedItem.Tag.ToString(); // Assuming Tag stores the LicenseNumber or some unique identifier
                            selectedDentistID = GetDentistIDFromDatabase(licenseNumber);

                            if (selectedDentistID == 0)
                            {
                                MessageBox.Show("Unable to retrieve DentistID from the database.");
                                return; // Exit if DentistID retrieval fails
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select a valid dentist.");
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a dentist.");
                        return;
                    }
                }

                Appointment appointment = new Appointment
                {
                    UserID = _userID,
                    PatientName = isBookingForSelf ? GetFirstNameFromDatabase(_userID) : txtPatientName.Text,
                    PhoneNumber = isBookingForSelf ? GetPhoneNumberFromDatabase(_userID) : txtPhoneNumber.Text,
                    Address = isBookingForSelf ? GetAddressFromDatabase(_userID) : txtAddress.Text,
                    AppointmentType = cmbAppointmentType.Text,
                    AppointmentDate = dpAppointmentDate.SelectedDate.Value,
                    AppointmentTime = cmbAppointmentTime.Text,
                    PatientInfo = txtPatientinfo.Text,
                    DentistID = selectedDentistID
                };

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("InsertAppointments", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@UserID", appointment.UserID);
                            command.Parameters.AddWithValue("@PatientName", appointment.PatientName);
                            command.Parameters.AddWithValue("@PhoneNumber", appointment.PhoneNumber);
                            command.Parameters.AddWithValue("@Address", appointment.Address);
                            command.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType);
                            command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                            command.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                            command.Parameters.AddWithValue("@PatientAllergy", appointment.PatientInfo);
                            command.Parameters.AddWithValue("@DentistID", appointment.DentistID);

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

        // Move this method outside the Bookbutton method
        private int GetDentistIDFromDatabase(string licenseNumber)
        {
            int dentistID = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT DentistID FROM Dentists WHERE LicenseNumber = @LicenseNumber";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LicenseNumber", licenseNumber);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        dentistID = Convert.ToInt32(result);
                        MessageBox.Show("Fetching DentistID" + dentistID);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving DentistID: {ex.Message}");
                }
            }

            return dentistID;
        }
    }

    public partial class Appointment
    {
        public int UserID { get; set; }
        public string PatientName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string AppointmentType { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = "";
        public string PatientInfo { get; set; } = "";
        public int DentistID { get; set; } 
    }
}

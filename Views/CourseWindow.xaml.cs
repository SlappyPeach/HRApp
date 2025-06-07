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
using System.Windows.Shapes;
using HRApp.Data;
using HRApp.Models;

namespace HRApp.Views
{
    public partial class CourseWindow : Window
    {
        private readonly int employeeId;

        public CourseWindow(int empId)
        {
            InitializeComponent();
            employeeId = empId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DateTime.TryParse(StartDateTextBox.Text, out var startDate) ||
                !DateTime.TryParse(EndDateTextBox.Text, out var endDate))
            {
                MessageBox.Show("Неверный формат дат.");
                return;
            }

            var course = new Course
            {
                EmployeeId = employeeId,
                CourseType = CourseTypeTextBox.Text.Trim(),
                Institution = InstitutionTextBox.Text.Trim(),
                Certificate = CertificateTextBox.Text.Trim(),
                StartDate = startDate,
                EndDate = endDate
            };

            using var context = new HRDbContext();
            context.Courses.Add(course);
            context.SaveChanges();

            MessageBox.Show("Курс сохранён.");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}


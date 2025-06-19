using System;
using System.Windows;
using HRApp.Models;
using System.Linq;
using HRApp.Data;
using HRApp.ViewModels;

namespace HRApp.Views
{
    public partial class T2FormWindow : Window
    {
        private T2FormViewModel viewModel;

        public T2FormWindow(int? empId = null)
        {
            InitializeComponent();
            viewModel = new T2FormViewModel(empId);
            DataContext = viewModel;
        }

        private void AddEducation_Click(object sender, RoutedEventArgs e)
        {
            viewModel.EducationList.Add(new Education { EducationType = "Тип", InstitutionName = "Учреждение", DocName = "Документ", EndYear = DateTime.Now.Year });
        }

        private void DeleteEducation_Click(object sender, RoutedEventArgs e)
        {
            if (EducationDataGrid.SelectedItem is Education edu)
                viewModel.EducationList.Remove(edu);
        }

        private void AddFamily_Click(object sender, RoutedEventArgs e)
        {
            viewModel.FamilyList.Add(new Family { Relation = "Родство", FIO = "ФИО", BirthYear = DateTime.Now.Year - 30 });
        }

        private void DeleteFamily_Click(object sender, RoutedEventArgs e)
        {
            if (FamilyDataGrid.SelectedItem is Family fam)
                viewModel.FamilyList.Remove(fam);
        }

        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            viewModel.LanguageList.Add(new LanguageDisplay { LanguageName = "Английский", LanguageLevel = "Intermediate" });
        }

        private void DeleteLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageDataGrid.SelectedItem is LanguageDisplay lang)
                viewModel.LanguageList.Remove(lang);
        }

        private void AddCertification_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.Employee.Id == 0)
            {
                MessageBox.Show("Сначала сохраните данные сотрудника.");
                return;
            }

            var certWindow = new CertificationWindow(viewModel.Employee.Id);
            certWindow.ShowDialog();

            // Перезагрузим список после закрытия окна
            viewModel = new T2FormViewModel(viewModel.Employee.Id);
            DataContext = viewModel;
        }

        private void DeleteCertification_Click(object sender, RoutedEventArgs e)
        {
            if (CertificationDataGrid.SelectedItem is Certification selectedCert)
            {
                var result = MessageBox.Show("Удалить выбранную аттестацию?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using var context = new Data.HRDbContext();
                    var cert = context.Certifications.FirstOrDefault(c => c.Id == selectedCert.Id);
                    if (cert != null)
                    {
                        context.Certifications.Remove(cert);
                        context.SaveChanges();
                        viewModel = new T2FormViewModel(viewModel.Employee.Id);
                        DataContext = viewModel;
                    }
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

    }

 }

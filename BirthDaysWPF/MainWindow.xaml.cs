using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace BirthDaysWPF
{
    public partial class MainWindow : Window
    {
        private List<Person> people = new List<Person>();

        public MainWindow()
        {
            InitializeComponent();
            cmbFilterCategory.SelectedIndex = 0;
            cmbSortBy.SelectedIndex = 0;

            CheckTodaysBirthdays();
        }

        private void CheckTodaysBirthdays()
        {
            var todaysBirthdays = people.Where(p =>
                p.BirthDate.Day == DateTime.Today.Day &&
                p.BirthDate.Month == DateTime.Today.Month).ToList();

            if (todaysBirthdays.Any())
            {
                string names = string.Join(", ", todaysBirthdays.Select(p => p.Name));
                MessageBox.Show($"Сегодня празднуют день рождения:\n{names}",
                    "Именинники",
                    MessageBoxButton.OK,
                    MessageBoxImage.None);
            }
        }


        private void AddPerson_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || dpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, введите имя и выберите дату рождения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.None);
                return;
            }

            var person = new Person
            {
                Name = txtName.Text,
                BirthDate = dpBirthDate.SelectedDate.Value,
                Category = cmbCategory.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "Друзья"
            };

            people.Add(person);
            UpdateList();
            txtName.Clear();
            dpBirthDate.SelectedDate = null;
        }


        private void UpdateList()
        {
            IEnumerable<Person> filtered = people;

            if (!string.IsNullOrWhiteSpace(txtFilter.Text))
            {
                string filter = txtFilter.Text.ToLower();
                filtered = filtered.Where(p => p.Name.ToLower().Contains(filter));
            }

            if (cmbFilterCategory.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Все")
            {
                string category = item.Content.ToString();
                filtered = filtered.Where(p => p.Category == category);
            }

            if (cmbSortBy.SelectedItem is ComboBoxItem sortItem)
            {
                switch (sortItem.Content.ToString())
                {
                    case "По имени (А-Я)":
                        filtered = filtered.OrderBy(p => p.Name);
                        break;
                    case "По имени (Я-А)":
                        filtered = filtered.OrderByDescending(p => p.Name);
                        break;
                    case "По дате":
                        filtered = filtered.OrderBy(p => p.BirthDate);
                        break;
                    case "По возрасту":
                        filtered = filtered.OrderByDescending(p => p.Age);
                        break;
                }
            }
            if (lvPeople is null)
            {
                return;
            }

            lvPeople.ItemsSource = filtered.ToList();
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateList();
        }

        private void cmbFilterCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void cmbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void CongratulateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Person person)
            {
                string congratulation = $"Дорогой(ая) {person.Name}!\n\n" +
                                      $"Поздравляем Вас с {person.Age}-летием!\n" +
                                      $"Желаем здоровья, счастья и успехов во всех начинаниях!\n\n";

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Поздравление для {person.Name}.txt",
                    DefaultExt = ".txt",
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, congratulation);
                        MessageBox.Show($"Поздравление сохранено в:\n{saveFileDialog.FileName}",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.None);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.None);
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Person person)
            {
                var result = MessageBox.Show($"Удалить {person.Name} из списка?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.None);

                if (result == MessageBoxResult.Yes)
                {
                    people.Remove(person);
                    UpdateList();

                }
            }
        }

        private void ShowTodaysBirthdays_Click(object sender, RoutedEventArgs e)
        {
            var todaysBirthdays = people.Where(p =>
                p.BirthDate.Day == DateTime.Today.Day &&
                p.BirthDate.Month == DateTime.Today.Month).ToList();

            if (todaysBirthdays.Any())
            {
                lvPeople.ItemsSource = todaysBirthdays;
            }
            else
            {
                MessageBox.Show("Сегодня никто не празднует день рождения",
                    "Именинники",
                    MessageBoxButton.OK,
                    MessageBoxImage.None);
                UpdateList();
            }
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public string Category { get; set; }

        public int Age => DateTime.Today.Year - BirthDate.Year;

        public Brush Background
        {
            get
            {
                var nextBirthday = new DateTime(DateTime.Today.Year, BirthDate.Month, BirthDate.Day);
                if (nextBirthday < DateTime.Today)
                    nextBirthday = nextBirthday.AddYears(1);

                int daysToBirthday = (nextBirthday - DateTime.Today).Days;

                if (daysToBirthday == 0)
                    return new SolidColorBrush(Color.FromRgb(255, 200, 200));
                else if (daysToBirthday <= 7)
                    return new SolidColorBrush(Color.FromRgb(255, 255, 200));

                return Brushes.White;
            }
        }
    }
}
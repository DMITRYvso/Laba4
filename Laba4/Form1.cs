using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Laba4
{
    public partial class Form1 : Form
    {
        private string dataFilePath = "";

        public Form1()
        {
            InitializeComponent();
            InitializeDataGridViewColumns();

            if (!LoadFileOnStartup())
            {
                dataFilePath = "data.txt";
                LoadDataFromFile();
            }

            SetFullAccessMode();
        }

        public Form1(bool readOnlyMode) : this()
        {
            if (readOnlyMode)
            {
                SetReadOnlyMode();
            }
        }

        private bool LoadFileOnStartup()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите файл с данными сотрудников";
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dataFilePath = openFileDialog.FileName;
                    LoadDataFromFile();
                    return true;
                }
            }
            return false;
        }

        private void SetFullAccessMode()
        {
            this.Text = $"Система управления сотрудниками - Полный доступ [{Path.GetFileName(dataFilePath)}]";
            dataGridView1.ReadOnly = false;
            dataGridView1.BackColor = SystemColors.Window;

            button1.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button2.Enabled = true;

            button1.Text = "Сохранить";
            button3.Text = "Удалить";
        }

        private void SetReadOnlyMode()
        {
            this.Text = $"Система управления сотрудниками - Только просмотр [{Path.GetFileName(dataFilePath)}]";
            dataGridView1.ReadOnly = true;
            dataGridView1.BackColor = Color.LightGray;

            button1.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = true;
            button2.Enabled = true;

            button1.Text = "Сохранение отключено";
            button3.Text = "Удаление отключено";
        }

        private void InitializeDataGridViewColumns()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("Id", "ID");
            dataGridView1.Columns.Add("Name", "Имя");
            dataGridView1.Columns.Add("Age", "Возраст");
            dataGridView1.Columns.Add("Department", "Отдел");
            dataGridView1.Columns.Add("Salary", "Зарплата");
            dataGridView1.Columns.Add("HireDate", "Дата приема");
            dataGridView1.Columns["Salary"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["HireDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
        }

        private void LoadDataFromFile()
        {
            try
            {
                if (string.IsNullOrEmpty(dataFilePath) || !File.Exists(dataFilePath))
                {
                    MessageBox.Show("Файл не найден. Будет создан новый файл.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.Rows.Clear();
                string[] lines = File.ReadAllLines(dataFilePath, Encoding.UTF8);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("==="))
                        continue;

                    string[] parts = line.Split('|');

                    if (parts.Length >= 6)
                    {
                        int id = ExtractValue(parts[0], "ID:", 0);
                        string name = ExtractValue(parts[1], "Имя:", "NOT FOUND");
                        int age = ExtractValue(parts[2], "Возраст:", 0);
                        string department = ExtractValue(parts[3], "Отдел:", "DEFAULT_DEPARTMENT");
                        decimal salary = ExtractValue(parts[4], "Зарплата:", 0);
                        DateTime hireDate = ExtractValue(parts[5], "ДатаПриема:", DateTime.Now);

                        dataGridView1.Rows.Add(id, name, age, department, salary, hireDate);
                    }
                    else if (parts.Length >= 5)
                    {
                        int id = ExtractValue(parts[0], "ID:", 0);
                        string name = ExtractValue(parts[1], "Имя:", "Default name");
                        int age = ExtractValue(parts[2], "Возраст:", 0);
                        string department = ExtractValue(parts[3], "Отдел:", "DEFAULT_DEPARTMENT");
                        decimal salary = ExtractValue(parts[4], "Зарплата:", 0);

                        dataGridView1.Rows.Add(id, name, age, department, salary, DateTime.Now);
                    }
                }

                MessageBox.Show($"Данные загружены из файла: {Path.GetFileName(dataFilePath)}", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка");
            }
        }

        private T ExtractValue<T>(string text, string prefix, T defaultValue = default)
        {
            string value = text.Replace(prefix, "").Trim();

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            try {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
        
                if (typeof(T) == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, out DateTime dateResult))
                        return (T)(object)dateResult;
                }

                return defaultValue;
            }
        }

        private bool ContainsOnlyLetters(string text)
        {
            return Regex.IsMatch(text, @"^[a-zA-Zа-яА-ЯёЁ\s]+$");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateDataBeforeSave())
            {
                SaveDataToFileWithDialog();
            }
        }

        private void SaveDataToFileWithDialog()
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Сохранить данные сотрудников";
                    saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    // Устанавливаем текущий файл по умолчанию
                    if (!string.IsNullOrEmpty(dataFilePath))
                    {
                        saveFileDialog.FileName = Path.GetFileName(dataFilePath);
                        saveFileDialog.InitialDirectory = Path.GetDirectoryName(dataFilePath);
                    }

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        dataFilePath = saveFileDialog.FileName;
                        SaveDataToFile();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateDataBeforeSave()
        {
            StringBuilder errors = new StringBuilder();
            int rowNumber = 1;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    if (row.Cells["Id"].Value == null || string.IsNullOrWhiteSpace(row.Cells["Id"].Value.ToString()))
                    {
                        errors.AppendLine($"Строка {rowNumber}: ID не может быть пустым");
                    }
                    else if (!int.TryParse(row.Cells["Id"].Value.ToString(), out int id) || id <= 0)
                    {
                        errors.AppendLine($"Строка {rowNumber}: ID должен быть положительным числом");
                    }

                    if (row.Cells["Name"].Value == null || string.IsNullOrWhiteSpace(row.Cells["Name"].Value.ToString()))
                    {
                        errors.AppendLine($"Строка {rowNumber}: Имя не может быть пустым");
                    }
                    else if (row.Cells["Name"].Value.ToString().Length < 2)
                    {
                        errors.AppendLine($"Строка {rowNumber}: Имя должно содержать минимум 2 символа");
                    }
                    else if (!ContainsOnlyLetters(row.Cells["Name"].Value.ToString()))
                    {
                        errors.AppendLine($"Строка {rowNumber}: Имя должно содержать только буквы и пробелы");
                    }

                    if (row.Cells["Age"].Value == null || string.IsNullOrWhiteSpace(row.Cells["Age"].Value.ToString()))
                    {
                        errors.AppendLine($"Строка {rowNumber}: Возраст не может быть пустым");
                    }
                    else if (!int.TryParse(row.Cells["Age"].Value.ToString(), out int age) || age < 18 || age > 65)
                    {
                        errors.AppendLine($"Строка {rowNumber}: Возраст должен быть числом от 18 до 65");
                    }

                    if (row.Cells["Department"].Value == null || string.IsNullOrWhiteSpace(row.Cells["Department"].Value.ToString()))
                    {
                        errors.AppendLine($"Строка {rowNumber}: Отдел не может быть пустым");
                    }

                    if (row.Cells["Salary"].Value == null || string.IsNullOrWhiteSpace(row.Cells["Salary"].Value.ToString()))
                    {
                        errors.AppendLine($"Строка {rowNumber}: Зарплата не может быть пустой");
                    }
                    else if (!decimal.TryParse(row.Cells["Salary"].Value.ToString(), out decimal salary) || salary < 0)
                    {
                        errors.AppendLine($"Строка {rowNumber}: Зарплата должна быть положительным числом");
                    }
                    else if (salary < 10000 || salary > 1000000)
                    {
                        errors.AppendLine($"Строка {rowNumber}: Зарплата должна быть в диапазоне от 10,000 до 1,000,000");
                    }

                    if (row.Cells["HireDate"].Value == null)
                    {
                        row.Cells["HireDate"].Value = DateTime.Now;
                    }

                    if (row.Cells["Id"].Value != null)
                    {
                        string currentId = row.Cells["Id"].Value.ToString();
                        int duplicateCount = 0;

                        foreach (DataGridViewRow otherRow in dataGridView1.Rows)
                        {
                            if (!otherRow.IsNewRow && otherRow != row &&
                                otherRow.Cells["Id"].Value != null &&
                                otherRow.Cells["Id"].Value.ToString() == currentId)
                            {
                                duplicateCount++;
                            }
                        }

                        if (duplicateCount > 0)
                        {
                            errors.AppendLine($"Строка {rowNumber}: ID '{currentId}' повторяется в таблице");
                        }
                    }
                    if (row.Cells["Name"].Value != null)
                    {
                        string name = row.Cells["Name"].Value.ToString();
                        if (name.Length > 100)
                        {
                            errors.AppendLine($"Строка {rowNumber}: Имя слишком длинное (максимум 100 символов, сейчас {name.Length})");
                        }
                    }

                    // Проверка длины отдела
                    if (row.Cells["Department"].Value != null)
                    {
                        string department = row.Cells["Department"].Value.ToString();
                        if (department.Length > 100)
                        {
                            errors.AppendLine($"Строка {rowNumber}: Название отдела слишком длинное (максимум 100 символов, сейчас {department.Length})");
                        }
                    }
                }
                rowNumber++;
            }

            if (dataGridView1.Rows.Count == 1 && dataGridView1.Rows[0].IsNewRow)
            {
                errors.AppendLine("Таблица не содержит данных");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show($"Обнаружены ошибки в данных:\n\n{errors.ToString()}",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void SaveDataToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(dataFilePath, false, Encoding.UTF8))
                {
                    writer.WriteLine("# Файл данных для DataGridView");
                    writer.WriteLine($"# Время сохранения: {DateTime.Now}");
                    writer.WriteLine($"# Файл: {Path.GetFileName(dataFilePath)}");
                    writer.WriteLine();

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["Id"].Value != null)
                        {
                            string hireDate = "";
                            if (row.Cells["HireDate"].Value != null)
                            {
                                DateTime dateValue;
                                if (DateTime.TryParse(row.Cells["HireDate"].Value.ToString(), out dateValue))
                                {
                                    hireDate = dateValue.ToString("dd.MM.yyyy");
                                }
                                else
                                {
                                    hireDate = DateTime.Now.ToString("dd.MM.yyyy");
                                }
                            }
                            else
                            {
                                hireDate = DateTime.Now.ToString("dd.MM.yyyy");
                            }

                            writer.WriteLine($"ID: {row.Cells["Id"].Value} | " +
                                           $"Имя: {row.Cells["Name"].Value} | " +
                                           $"Возраст: {row.Cells["Age"].Value} | " +
                                           $"Отдел: {row.Cells["Department"].Value} | " +
                                           $"Зарплата: {row.Cells["Salary"].Value} | " +
                                           $"ДатаПриема: {hireDate}");
                        }
                    }
                }

                if (this.Text.Contains("Полный доступ"))
                {
                    this.Text = $"Система управления сотрудниками - Полный доступ [{Path.GetFileName(dataFilePath)}]";
                }
                else
                {
                    this.Text = $"Система управления сотрудниками - Только просмотр [{Path.GetFileName(dataFilePath)}]";
                }

                MessageBox.Show($"Данные сохранены в файл: {Path.GetFileName(dataFilePath)}", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите файл для загрузки данных";
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dataFilePath = openFileDialog.FileName;
                    LoadDataFromFile();

                    if (this.Text.Contains("Полный доступ"))
                    {
                        SetFullAccessMode();
                    }
                    else
                    {
                        SetReadOnlyMode();
                    }
                }
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.IsNewRow && row.Cells["HireDate"].Value == null)
                {
                    row.Cells["HireDate"].Value = DateTime.Now;
                }
            }
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Name"].Index && e.RowIndex >= 0)
            {
                string newValue = e.FormattedValue?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(newValue) && !ContainsOnlyLetters(newValue))
                {
                    dataGridView1.Rows[e.RowIndex].ErrorText = "ФИО должно содержать только буквы и пробелы";
                    e.Cancel = true;
                }
                else
                {
                    dataGridView1.Rows[e.RowIndex].ErrorText = "";
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            FindEmployeeWithMaxSalary();
        }

        private void FindEmployeeWithMaxSalary()
        {
            try
            {
                if (dataGridView1.Rows.Count == 0 || (dataGridView1.Rows.Count == 1 && dataGridView1.Rows[0].IsNewRow))
                {
                    MessageBox.Show("Таблица не содержит данных для поиска", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DataGridViewRow maxSalaryRow = null;
                decimal maxSalary = decimal.MinValue;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow && row.Cells["Salary"].Value != null)
                    {
                        if (decimal.TryParse(row.Cells["Salary"].Value.ToString(), out decimal currentSalary))
                        {
                            if (currentSalary > maxSalary)
                            {
                                maxSalary = currentSalary;
                                maxSalaryRow = row;
                            }
                        }
                    }
                }

                if (maxSalaryRow != null)
                {
                    dataGridView1.ClearSelection();
                    maxSalaryRow.Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = maxSalaryRow.Index;

                    string name = maxSalaryRow.Cells["Name"].Value?.ToString() ?? "Не указано";
                    string department = maxSalaryRow.Cells["Department"].Value?.ToString() ?? "Не указано";

                    MessageBox.Show($"Сотрудник с максимальной зарплатой:\n\n" +
                                  $"Имя: {name}\n" +
                                  $"Отдел: {department}\n" +
                                  $"Зарплата: {maxSalary:C2}",
                                  "Результат поиска",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось найти сотрудников с указанной зарплатой",
                        "Результат поиска", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске сотрудника: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteSelectedRow();
        }

        private void DeleteSelectedRow()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Пожалуйста, выберите строку для удаления", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                if (selectedRow.IsNewRow)
                {
                    MessageBox.Show("Нельзя удалить новую пустую строку", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string employeeName = selectedRow.Cells["Name"].Value?.ToString() ?? "Неизвестный сотрудник";
                string employeeId = selectedRow.Cells["Id"].Value?.ToString() ?? "N/A";

                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить сотрудника?\n\n" +
                    $"ID: {employeeId}\n" +
                    $"Имя: {employeeName}",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    dataGridView1.Rows.Remove(selectedRow);

                    MessageBox.Show($"Сотрудник {employeeName} успешно удален", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении строки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SearchEmployeesByName();
        }

        private void SearchEmployeesByName()
        {
            try
            {
                string searchText = textBox1.Text.Trim();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    MessageBox.Show("Введите ФИО или часть ФИО для поиска", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Focus();
                    return;
                }

                if (dataGridView1.Rows.Count == 0 || (dataGridView1.Rows.Count == 1 && dataGridView1.Rows[0].IsNewRow))
                {
                    MessageBox.Show("Таблица не содержит данных для поиска", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                List<DataGridViewRow> foundRows = new List<DataGridViewRow>();
                int matchCount = 0;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow && row.Cells["Name"].Value != null)
                    {
                        string employeeName = row.Cells["Name"].Value.ToString();

                        if (employeeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundRows.Add(row);
                            matchCount++;
                        }
                    }
                }

                if (matchCount > 0)
                {
                    dataGridView1.ClearSelection();
                    foreach (DataGridViewRow row in foundRows)
                    {
                        row.Selected = true;
                    }

                    if (foundRows.Count > 0)
                    {
                        dataGridView1.FirstDisplayedScrollingRowIndex = foundRows[0].Index;
                    }

                    MessageBox.Show($"Найдено сотрудников: {matchCount}", "Результаты поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dataGridView1.ClearSelection();
                    MessageBox.Show($"Сотрудники по запросу '{searchText}' не найдены", "Результаты поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            FindEmployeeWithMaxSalary();
        }

        private void AddNewEmployeeWithTodayDate()
        {
            int newId = 1;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells["Id"].Value != null)
                {
                    if (int.TryParse(row.Cells["Id"].Value.ToString(), out int currentId))
                    {
                        if (currentId >= newId)
                        {
                            newId = currentId + 1;
                        }
                    }
                }
            }

            dataGridView1.Rows.Add(newId, "", 0, "", 0, DateTime.Now);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Text = $"Система управления сотрудниками - Только просмотр [{Path.GetFileName(dataFilePath)}]";
            dataGridView1.ReadOnly = true;
            dataGridView1.BackColor = Color.LightGray;

            button1.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = true;
            button2.Enabled = true;

            // Добавьте настройку кнопки 5
           

            button1.Text = "Сохранение отключено";
            button3.Text = "Удаление отключено";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Text = $"Система управления сотрудниками - Полный доступ [{Path.GetFileName(dataFilePath)}]";
            dataGridView1.ReadOnly = false;
            dataGridView1.BackColor = SystemColors.Window;

            button1.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button2.Enabled = true;

            // Добавьте настройку кнопки 5
            

            button1.Text = "Сохранить";
            button3.Text = "Удалить";
        }
    }
}
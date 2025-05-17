using CsvHelper;
using CsvHelper.Configuration;
using EmployeeChurnAnalysis.Models;
using System.Formats.Asn1;
using System.Globalization;

namespace EmployeeChurnAnalysis.Data
{
    public class DataLoader
    {
        /// <summary>
        /// Загрузка действующих сотрудников из файла
        /// </summary>
        public List<Employee> LoadCurrentEmployees(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            var employees = new List<Employee>();

            while (csv.Read())
            {
                try
                {
                    var employee = new Employee
                    {
                        PersonId = csv.GetField<string>("PERSON_ID"),
                        BirthDate = DateTime.Parse(csv.GetField<string>("BIRTH_DT")),
                        Sex = csv.GetField<int>("SEX"),
                        ReceptionDate = DateTime.Parse(csv.GetField<string>("RECEPTION_DT")),
                        City = csv.GetField<string>("CITY"),
                        Grade = csv.GetField<string>("GRADE"),
                        HasLeft = false
                    };

                    employees.Add(employee);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении записи: {ex.Message}");
                }
            }

            return employees;
        }

        /// <summary>
        /// Загрузка уволенных сотрудников из файла
        /// </summary>
        public List<Employee> LoadFormerEmployees(string filePath, string reasonsFilePath)
        {
            // Загрузка справочника причин увольнения
            var reasonsDict = LoadReasonsDictionary(reasonsFilePath);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            var employees = new List<Employee>();

            while (csv.Read())
            {
                try
                {
                    var reason = csv.GetField<string>("Причины увольнения");
                    var isVoluntary = reasonsDict.ContainsKey(reason) &&
                                     reasonsDict[reason] == "Voluntary";

                    var employee = new Employee
                    {
                        PersonId = csv.GetField<string>("PERSON_ID"),
                        LeaveDate = DateTime.Parse(csv.GetField<string>("Дата операции")),
                        LeaveReason = reason,
                        IsVoluntary = isVoluntary,
                        Department = csv.GetField<string>("Отдел"),
                        Grade = csv.GetField<string>("Грейд"),
                        HasLeft = true
                    };

                    employees.Add(employee);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении записи: {ex.Message}");
                }
            }

            return employees;
        }

        /// <summary>
        /// Загрузка информации о прохождении стажировки
        /// </summary>
        public Dictionary<string, bool> LoadTraineeInfo(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            var result = new Dictionary<string, bool>();

            while (csv.Read())
            {
                try
                {
                    var personId = csv.GetField<string>("PERSON_ID");
                    var wasTrainee = csv.GetField<string>("WAS_TRAINEE") == "YES";
                    result[personId] = wasTrainee;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении записи: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Загрузка дополнительных сведений о сотрудниках
        /// </summary>
        public void LoadAdditionalInfo(string filePath, List<Employee> employees)
        {
            var employeeDict = employees.ToDictionary(e => e.PersonId);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            while (csv.Read())
            {
                try
                {
                    var personId = csv.GetField<string>("PERSON_ID");

                    if (employeeDict.TryGetValue(personId, out var employee))
                    {
                        employee.MaritalStatus = csv.GetField<string>("Состояние в браке");
                        employee.MilitaryService = csv.GetField<string>("Отношение к военной службе");
                        employee.Education = csv.GetField<string>("Образование");
                        employee.Age = csv.GetField<int>("Возраст");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении записи: {ex.Message}");
                }
            }
        }

        // Аналогичные методы для других файлов...

        /// <summary>
        /// Загрузка словаря причин увольнения
        /// </summary>
        private Dictionary<string, string> LoadReasonsDictionary(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            var result = new Dictionary<string, string>();

            while (csv.Read())
            {
                try
                {
                    var reason = csv.GetField<string>("Причины увольнения");
                    var type = csv.GetField<string>("Voluntary / Non-voluntary");
                    result[reason] = type;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении записи: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Объединение данных сотрудников из разных источников
        /// </summary>
        public List<Employee> MergeEmployeeData(
            List<Employee> currentEmployees,
            List<Employee> formerEmployees,
            Dictionary<string, bool> traineeInfo,
            // Другие параметры для остальных данных
            )
        {
            var allEmployees = new List<Employee>();

            // Добавляем действующих сотрудников
            allEmployees.AddRange(currentEmployees);

            // Добавляем уволенных сотрудников, если их нет среди действующих
            foreach (var former in formerEmployees)
            {
                if (!allEmployees.Any(e => e.PersonId == former.PersonId))
                {
                    allEmployees.Add(former);
                }
                else
                {
                    // Обновляем информацию о сотруднике, если он уже есть в списке
                    var existing = allEmployees.First(e => e.PersonId == former.PersonId);
                    existing.HasLeft = true;
                    existing.LeaveDate = former.LeaveDate;
                    existing.LeaveReason = former.LeaveReason;
                    existing.IsVoluntary = former.IsVoluntary;
                    existing.Department = former.Department;
                }
            }

            // Добавляем информацию о прохождении стажировки
            foreach (var employee in allEmployees)
            {
                if (traineeInfo.TryGetValue(employee.PersonId, out var wasTrainee))
                {
                    employee.WasTrainee = wasTrainee;
                }
            }

            // Добавление других данных...

            return allEmployees;
        }
    }
}

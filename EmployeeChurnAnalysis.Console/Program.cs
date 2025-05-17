using EmployeeChurnAnalysis.Data;
using EmployeeChurnAnalysis.ML;
using Microsoft.ML;

namespace EmployeeChurnAnalysis.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("===== Анализ оттока сотрудников =====");

            // Пути к файлам данных
            var dataPath = Path.Combine(Environment.CurrentDirectory, "Data");

            string currentEmployeesPath = Path.Combine(dataPath, "Действующие сотрудники на 31.12.2024.csv");
            string formerEmployeesPath = Path.Combine(dataPath, "Уволенные сотрудники за 2024 год.csv");
            string reasonsPath = Path.Combine(dataPath, "Причины оттока.csv");
            string additionalInfoPath = Path.Combine(dataPath, "Дополнительные сведения на 31.12.2024.csv");
            string traineesPath = Path.Combine(dataPath, "Был стажером.csv");
            string careerChangesPath = Path.Combine(dataPath, "Карьерные изменения 31.12.2024.csv");
            string vacationsPath = Path.Combine(dataPath, "Общее_число_отпусков_2024.csv");
            string absencesPath = Path.Combine(dataPath, "Общее_число_прогулов_2024.csv");
            string sickLeavesPath = Path.Combine(dataPath, "Общее_число_пропусков_по_болезням_2024.csv");
            string performancePath = Path.Combine(dataPath, "Оценки работы 2022-2023.csv");
            string seniorityPath = Path.Combine(dataPath, "Стажи 31.12.2024.csv");

            // Путь для выходных файлов
            var processedDataPath = Path.Combine(dataPath, "ProcessedData.csv");
            var modelPath = Path.Combine(Environment.CurrentDirectory, "Models", "ChurnModel.zip");

            // Создаем директорию для моделей, если она не существует
            Directory.CreateDirectory(Path.GetDirectoryName(modelPath));

            try
            {
                // 1. Загрузка данных
                System.Console.WriteLine("Загрузка данных...");
                var dataLoader = new DataLoader();

                var currentEmployees = dataLoader.LoadCurrentEmployees(currentEmployeesPath);
                System.Console.WriteLine($"Загружено действующих сотрудников: {currentEmployees.Count}");

                var formerEmployees = dataLoader.LoadFormerEmployees(formerEmployeesPath, reasonsPath);
                System.Console.WriteLine($"Загружено уволенных сотрудников: {formerEmployees.Count}");

                var traineeInfo = dataLoader.LoadTraineeInfo(traineesPath);
                System.Console.WriteLine($"Загружена информация о стажерах: {traineeInfo.Count} записей");

                // Загрузка остальных данных...

                // 2. Объединение данных
                System.Console.WriteLine("Объединение данных...");
                var allEmployees = dataLoader.MergeEmployeeData(
                    currentEmployees,
                    formerEmployees,
                    traineeInfo
                // Остальные параметры
                );
                System.Console.WriteLine($"Всего сотрудников после объединения: {allEmployees.Count}");

                // 3. Подготовка данных для машинного обучения
                System.Console.WriteLine("Подготовка данных для ML...");
                var dataProcessor = new DataProcessor();
                var mlData = dataProcessor.PrepareDataForML(allEmployees);

                // Сохраняем подготовленные данные в CSV
                dataProcessor.SaveProcessedData(mlData, processedDataPath);
                System.Console.WriteLine($"Данные сохранены в {processedDataPath}");

                // 4. Анализ данных
                System.Console.WriteLine("\n===== Анализ данных =====");

                // Анализ оттока по месяцам
                var churnByMonth = dataProcessor.GetChurnByMonth(allEmployees);
                System.Console.WriteLine("Динамика оттока по месяцам:");
                foreach (var item in churnByMonth.OrderBy(x => x.Key))
                {
                    System.Console.WriteLine($"Месяц {item.Key}: {item.Value} сотрудников");
                }

                // Анализ причин оттока
                var churnReasons = dataProcessor.GetChurnReasons(allEmployees);
                System.Console.WriteLine("\nОсновные причины оттока:");
                foreach (var item in churnReasons.Take(5))
                {
                    System.Console.WriteLine($"{item.Key}: {item.Value} сотрудников");
                }

                // Анализ оттока по отделам
                var churnByDepartment = dataProcessor.GetChurnRateByDepartment(allEmployees);
                System.Console.WriteLine("\nУровень оттока по отделам (%):");
                foreach (var item in churnByDepartment.OrderByDescending(x => x.Value))
                {
                    System.Console.WriteLine($"{item.Key}: {item.Value}%");
                }

                // 5. Машинное обучение
                System.Console.WriteLine("\n===== Машинное обучение =====");

                // Создаем контекст ML.NET
                var mlContext = new MLContext(seed: 42);

                // Загружаем данные в IDataView
                var dataView = dataProcessor.LoadDataView(mlContext, processedDataPath);

                // Разделяем данные на обучающую и тестовую выборки
                var dataSplit = dataProcessor.SplitData(mlContext, dataView);
                System.Console.WriteLine("Данные разделены на обучающую и тестовую выборки");

                // Обучаем модель
                var churnPredictor = new ChurnPredictor();
                var model = churnPredictor.TrainModel(dataSplit.TrainSet);

                // Оцениваем модель
                System.Console.WriteLine("\nОценка модели:");
                churnPredictor.EvaluateModel(model, dataSplit.TestSet);

                // Получаем важность признаков
                System.Console.WriteLine("\nВажность признаков:");
                var featureImportance = churnPredictor.GetFeatureImportance(model, dataSplit.TrainSet);

                foreach (var feature in featureImportance)
                {
                    System.Console.WriteLine($"{feature.FeatureName}: {feature.Importance:F4}");
                }

                // 6. Проверка гипотез
                System.Console.WriteLine("\n===== Проверка гипотез =====");

                var hypothesisTester = new HypothesisTester();
                hypothesisTester.TestTraineeHypothesis(allEmployees);
                hypothesisTester.TestPerformanceHypothesis(allEmployees);
                hypothesisTester.TestCareerGrowthHypothesis(allEmployees);

                // 7. Генерация рекомендаций
                System.Console.WriteLine("\n===== Рекомендации =====");

                var recommendationGenerator = new RecommendationGenerator();
                var recommendations = recommendationGenerator.GenerateRecommendations(allEmployees, featureImportance);

                foreach (var recommendation in recommendations)
                {
                    System.Console.WriteLine($"- {recommendation}");
                }

                // 8. Определение сотрудников с высоким риском оттока
                var employeesAtRisk = recommendationGenerator.IdentifyEmployeesAtRisk(
                    allEmployees, churnPredictor, model, dataProcessor);

                System.Console.WriteLine($"\nСотрудники с высоким риском оттока (топ 10 из {employeesAtRisk.Count}):");
                foreach (var item in employeesAtRisk.Take(10))
                {
                    System.Console.WriteLine($"ID: {item.Employee.PersonId}, " +
                        $"Отдел: {item.Employee.Department}, " +
                        $"Грейд: {item.Employee.Grade}, " +
                        $"Риск: {item.Risk:P2}");
                }

                // 9. Сохранение модели
                churnPredictor.SaveModel(model, modelPath);
                System.Console.WriteLine($"\nМодель сохранена в {modelPath}");

                System.Console.WriteLine("\nАнализ успешно завершен!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Ошибка: {ex.Message}");
                System.Console.WriteLine(ex.StackTrace);
            }

            System.Console.WriteLine("\nНажмите любую клавишу для завершения...");
            System.Console.ReadKey();
        }
    }
}

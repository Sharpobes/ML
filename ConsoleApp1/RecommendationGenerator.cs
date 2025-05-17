using EmployeeChurnAnalysis.Models;
using Microsoft.ML;

namespace EmployeeChurnAnalysis.ML
{
    public class RecommendationGenerator
    {
        /// <summary>
        /// Генерация рекомендаций на основе анализа данных и модели
        /// </summary>
        public List<string> GenerateRecommendations(
            List<Employee> employees,
            List<FeatureImportance> featureImportance)
        {
            var recommendations = new List<string>();

            // Анализ влияния стажерской программы
            var traineeImportance = featureImportance.FirstOrDefault(f => f.FeatureName == "WasTrainee");
            if (traineeImportance != null)
            {
                var traineeEffect = traineeImportance.Importance < 0
                    ? "положительно влияет на удержание"
                    : "не влияет или отрицательно влияет на удержание";

                recommendations.Add($"Стажерская программа {traineeEffect} сотрудников. " +
                    (traineeImportance.Importance < 0
                        ? "Рекомендуется расширять программу."
                        : "Рекомендуется пересмотреть программу для повышения её эффективности."));
            }

            // Анализ влияния карьерного роста
            var careerGrowthImportance = featureImportance.FirstOrDefault(f => f.FeatureName == "GradeUpChange");
            if (careerGrowthImportance != null)
            {
                if (careerGrowthImportance.Importance < 0)
                {
                    recommendations.Add("Наличие карьерного роста снижает риск оттока. " +
                        "Рекомендуется создать больше возможностей для повышения и карьерного развития.");
                }
            }

            // Анализ влияния отсутствий
            var absenceImportance = featureImportance.FirstOrDefault(f => f.FeatureName == "AbsenceDays");
            if (absenceImportance != null && absenceImportance.Importance > 0)
            {
                recommendations.Add("Частые отсутствия на работе связаны с повышенным риском оттока. " +
                    "Рекомендуется усилить контроль и провести работу с сотрудниками, имеющими много пропусков.");
            }

            // Анализ влияния обучающих курсов
            var coursesImportance = featureImportance.FirstOrDefault(f => f.FeatureName == "CourseCount");
            if (coursesImportance != null && coursesImportance.Importance < 0)
            {
                recommendations.Add("Прохождение обучающих курсов снижает риск оттока. " +
                    "Рекомендуется активно предлагать сотрудникам образовательные программы и курсы повышения квалификации.");
            }

            // Анализ влияния оценок производительности
            var performanceImportance = featureImportance.FirstOrDefault(f => f.FeatureName == "Performance2023");
            if (performanceImportance != null)
            {
                if (performanceImportance.Importance < 0)
                {
                    recommendations.Add("Высокие оценки производительности связаны с меньшим риском оттока. " +
                        "Рекомендуется проводить работу с низкопроизводительными сотрудниками и предлагать им помощь.");
                }
            }

            // Анализ по отделам
            var departmentChurnRates = employees
                .GroupBy(e => e.Department)
                .Where(g => g.Count() > 5 && !string.IsNullOrEmpty(g.Key))
                .Select(g => new
                {
                    Department = g.Key,
                    ChurnRate = (double)g.Count(e => e.HasLeft) / g.Count(),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.ChurnRate)
                .ToList();

            if (departmentChurnRates.Any())
            {
                var worstDepartments = departmentChurnRates.Take(3)
                    .Where(d => d.ChurnRate > 0.1) // Отделы с оттоком выше 10%
                    .ToList();

                if (worstDepartments.Any())
                {
                    var deptList = string.Join(", ", worstDepartments.Select(d =>
                        $"{d.Department} ({d.ChurnRate:P1})"));
                    recommendations.Add($"Отделы с самым высоким уровнем оттока: {deptList}. " +
                        "Рекомендуется провести детальный анализ причин оттока в этих отделах.");
                }
            }

            return recommendations;
        }

        /// <summary>
        /// Определение сотрудников с высоким риском оттока
        /// </summary>
        public List<(Employee Employee, float Risk)> IdentifyEmployeesAtRisk(
            List<Employee> employees,
            ChurnPredictor predictor,
            ITransformer model,
            DataProcessor dataProcessor)
        {
            // Отбираем только действующих сотрудников
            var activeEmployees = employees.Where(e => !e.HasLeft).ToList();

            // Получаем предсказания риска оттока
            var predictions = predictor.PredictChurnForEmployees(model, activeEmployees, dataProcessor);

            // Отбираем сотрудников с высоким риском (вероятность > 0.7)
            return predictions.Where(p => p.Risk > 0.7).ToList();
        }
    }
}

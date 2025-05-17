using EmployeeChurnAnalysis.Models;

namespace EmployeeChurnAnalysis.ML
{
    public class HypothesisTester
    {
        /// <summary>
        /// Тестирование гипотезы о влиянии стажерской программы на отток
        /// </summary>
        public void TestTraineeHypothesis(List<Employee> employees)
        {
            var trainees = employees.Where(e => e.WasTrainee).ToList();
            var nonTrainees = employees.Where(e => !e.WasTrainee).ToList();

            var traineeChurnRate = trainees.Any()
                ? (double)trainees.Count(e => e.HasLeft) / trainees.Count
                : 0;

            var nonTraineeChurnRate = nonTrainees.Any()
                ? (double)nonTrainees.Count(e => e.HasLeft) / nonTrainees.Count
                : 0;

            Console.WriteLine("=== Гипотеза 1: Влияние стажерской программы на отток ===");
            Console.WriteLine($"Всего стажеров: {trainees.Count}");
            Console.WriteLine($"Всего не-стажеров: {nonTrainees.Count}");
            Console.WriteLine($"Доля уволившихся стажеров: {traineeChurnRate:P2}");
            Console.WriteLine($"Доля уволившихся не-стажеров: {nonTraineeChurnRate:P2}");
            Console.WriteLine($"Разница: {Math.Abs(traineeChurnRate - nonTraineeChurnRate):P2}");
            Console.WriteLine($"Вывод: {(traineeChurnRate < nonTraineeChurnRate
                ? "Стажеры увольняются реже, чем не-стажеры. Стажерская программа положительно влияет на удержание."
                : "Стажеры увольняются чаще или так же, как не-стажеры. Требуется дополнительный анализ.")}");
        }

        /// <summary>
        /// Тестирование гипотезы о влиянии оценок производительности на отток
        /// </summary>
        public void TestPerformanceHypothesis(List<Employee> employees)
        {
            var employeesWithPerformance = employees.Where(e => e.Performance2023.HasValue).ToList();
            var lowPerformers = employeesWithPerformance.Where(e => e.Performance2023 < 3).ToList();
            var highPerformers = employeesWithPerformance.Where(e => e.Performance2023 >= 3).ToList();

            var lowPerformerChurnRate = lowPerformers.Any()
                ? (double)lowPerformers.Count(e => e.HasLeft) / lowPerformers.Count
                : 0;

            var highPerformerChurnRate = highPerformers.Any()
                ? (double)highPerformers.Count(e => e.HasLeft) / highPerformers.Count
                : 0;

            Console.WriteLine("=== Гипотеза 2: Влияние оценок производительности на отток ===");
            Console.WriteLine($"Сотрудников с низкой производительностью (<3): {lowPerformers.Count}");
            Console.WriteLine($"Сотрудников с высокой производительностью (>=3): {highPerformers.Count}");
            Console.WriteLine($"Доля уволившихся с низкой производительностью: {lowPerformerChurnRate:P2}");
            Console.WriteLine($"Доля уволившихся с высокой производительностью: {highPerformerChurnRate:P2}");
            Console.WriteLine($"Разница: {Math.Abs(lowPerformerChurnRate - highPerformerChurnRate):P2}");
            Console.WriteLine($"Вывод: {(lowPerformerChurnRate > highPerformerChurnRate
                ? "Сотрудники с низкой производительностью увольняются чаще. Оценка производительности - фактор риска."
                : "Сотрудники с низкой производительностью увольняются реже или так же часто. Требуется дополнительный анализ.")}");
        }

        /// <summary>
        /// Тестирование гипотезы о влиянии карьерного роста на отток
        /// </summary>
        public void TestCareerGrowthHypothesis(List<Employee> employees)
        {
            var withGrowth = employees.Where(e => e.GradeUpChange > 0).ToList();
            var withoutGrowth = employees.Where(e => e.GradeUpChange == 0).ToList();

            var withGrowthChurnRate = withGrowth.Any()
                ? (double)withGrowth.Count(e => e.HasLeft) / withGrowth.Count
                : 0;

            var withoutGrowthChurnRate = withoutGrowth.Any()
                ? (double)withoutGrowth.Count(e => e.HasLeft) / withoutGrowth.Count
                : 0;

            Console.WriteLine("=== Гипотеза 3: Влияние карьерного роста на отток ===");
            Console.WriteLine($"Сотрудников с карьерным ростом: {withGrowth.Count}");
            Console.WriteLine($"Сотрудников без карьерного роста: {withoutGrowth.Count}");
            Console.WriteLine($"Доля уволившихся с карьерным ростом: {withGrowthChurnRate:P2}");
            Console.WriteLine($"Доля уволившихся без карьерного роста: {withoutGrowthChurnRate:P2}");
            Console.WriteLine($"Разница: {Math.Abs(withGrowthChurnRate - withoutGrowthChurnRate):P2}");
            Console.WriteLine($"Вывод: {(withGrowthChurnRate < withoutGrowthChurnRate
                ? "Сотрудники с карьерным ростом увольняются реже. Карьерный рост положительно влияет на удержание."
                : "Сотрудники с карьерным ростом увольняются чаще или так же часто. Требуется дополнительный анализ.")}");
        }
    }
}

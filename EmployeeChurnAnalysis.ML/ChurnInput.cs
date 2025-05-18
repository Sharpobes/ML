using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;

namespace EmployeeChurnAnalysis.ML
{
    public class ChurnInput
    {
        [LoadColumn(0)] public string PERSON_ID;
        [LoadColumn(1)] public string SEX;
        [LoadColumn(2)] public float Age;
        [LoadColumn(3)] public string GRADE;
        [LoadColumn(4)] public string WAS_TRAINEE;
        [LoadColumn(5)] public float FULL_SENIORITY;
        [LoadColumn(6)] public float ILL_DAYS;
        [LoadColumn(7)] public float DAYS_WITHOUT_VALID_REASONS;
        [LoadColumn(8)] public float VACATION_COUNT_2024;
        [LoadColumn(9)] public float YEAR_2022;
        [LoadColumn(10)] public float YEAR_2023;
        [LoadColumn(11)] public float COURSES_07;
        [LoadColumn(12)] public float COURSES_08;
        [LoadColumn(13)] public float COURSES_09;
        [LoadColumn(14)] public float COURSES_10;
        [LoadColumn(15)] public float COURSES_11;
        [LoadColumn(16)] public float COURSES_12;

        [LoadColumn(17)] public string VOLUNTARY_TYPE;
    }
}

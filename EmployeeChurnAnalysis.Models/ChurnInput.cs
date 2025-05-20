using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;

namespace EmployeeChurnAnalysis.Models
{
    public class ChurnInput
    {
        [LoadColumn(0)] public string PERSON_ID;
        [LoadColumn(1)] public string WAS_TRAINEE;
        [LoadColumn(3)] public string SEX;
        [LoadColumn(8)] public float Age;
        [LoadColumn(6)] public string GRADE;
        [LoadColumn(20)] public float ILL_DAYS;
        [LoadColumn(17)] public float DAYS_WITHOUT_VALID_REASONS;
        [LoadColumn(15)] public float VACATION_COUNT_2024;
        [LoadColumn(24)] public float YEAR_2022;
        [LoadColumn(25)] public float YEAR_2023;
        [LoadColumn(33)] public float COURSES_07;
        [LoadColumn(34)] public float COURSES_08;
        [LoadColumn(35)] public float COURSES_09;
        [LoadColumn(36)] public float COURSES_10;
        [LoadColumn(37)] public float COURSES_11;
        [LoadColumn(38)] public float COURSES_12;
        [LoadColumn(26)] public float FULL_SENIORITY;

        [LoadColumn(32), ColumnName("Label")]
        public bool IsVoluntary  { get; set; }
    }
}
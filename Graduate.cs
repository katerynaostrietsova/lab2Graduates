using System.Collections.Generic;

namespace lab2Graduates.Models
{
    public class Graduate
    {
        public string FullName { get; set; } = string.Empty;

        public string Faculty { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Speciality { get; set; } = string.Empty;

        public int? AdmissionYear { get; set; }

        public int? GraduationYear { get; set; }

        public List<string> CareerMoves { get; } = new();

        public string StudyPeriod =>
            AdmissionYear.HasValue && GraduationYear.HasValue
                ? $"{AdmissionYear}-{GraduationYear}"
                : string.Empty;
        public override string ToString()
        {
            var baseInfo = $"{FullName} — {Faculty}";

            if (!string.IsNullOrWhiteSpace(Department))
            {
                baseInfo += $", {Department}";
            }

            if (!string.IsNullOrWhiteSpace(Speciality))
            {
                baseInfo += $", спец. {Speciality}";
            }

            if (!string.IsNullOrWhiteSpace(StudyPeriod))
            {
                baseInfo += $", {StudyPeriod}";
            }

            if (CareerMoves.Count == 0)
            {
                return baseInfo;
            }

            var career = string.Join("; ", CareerMoves);
            return $"{baseInfo}. Кар’єра: {career}";
        }
    }
}

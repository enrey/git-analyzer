namespace GitAnalyzer.Web.Application.Configuration
{
    /// <summary>
    /// Конфигурация для оценки рабочего времени
    /// </summary>
    public class WorkEstimateConfig
    {
        /// <summary>
        /// Количество рабочих дней
        /// </summary>
        public double WorkDayHours { get; set; }

        /// <summary>
        /// Количество часов для временных границ
        /// </summary>
        public double PaddingHours { get; set; }
    }
}

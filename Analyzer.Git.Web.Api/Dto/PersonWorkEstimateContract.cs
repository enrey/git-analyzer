namespace Analyzer.Git.Web.Api.Dto
{
    /// <summary>
    /// Контракт для данных оценки затраченного времени
    /// </summary>
    public class PersonWorkEstimateContract
    {
        public string Email { get; set; }

        public double Hours { get; set; }

        public double Days { get; set; }
    }
}

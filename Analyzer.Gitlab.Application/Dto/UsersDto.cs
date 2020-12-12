namespace Analyzer.Git.Application.Dto.GitLab
{
    /// <summary>
    /// DTO для данных о пользователях GitLab
    /// </summary>
    public class UsersDto
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username пользователя
        /// </summary>
        public string Username { get; set; }

        public string Url { get; set; }
    }
}

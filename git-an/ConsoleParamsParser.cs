using log4net;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace git_an
{
    public class ConsoleParamsParser
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string AUTHORS_FILE = "authors.txt";

        private const string DEFAULT_DT_FROM = "08.01.2018";

        private const string DEFAULT_DT_TILL = "30.12.2018";

        public static InputParams GetParams(string[] args)
        {
            var input = new InputParams();

            if (args.Length == 0) throw new Exception("1-st argument must be a repo-path, e.g. c:\\work\\my-git-project");

            input.RepoPath = args[0];

            if (args.Length == 2) throw new Exception("2-nd and 3-rd argument (if specified) must be start date and end date in format dd.MM.yyyy, eg 08.01.2018 30.12.2018");

            if (args.Length >= 2)
            {
                input.DtFrom = ParseDate(args[1]);
                input.DtTill = ParseDate(args[2]);
            }
            else
            {
                input.DtFrom = ParseDate(DEFAULT_DT_FROM);
                input.DtTill = ParseDate(DEFAULT_DT_TILL);
            }

            input.AuthorsToDedupe = File.ReadAllLines(AUTHORS_FILE).Select(o => o.Split(',')).ToList();

            Logger.Info($"Date from: {input.DtFrom}");
            Logger.Info($"Date till: {input.DtTill}");
            Logger.Info($"Repo path: {input.RepoPath}");

            return input;
        }

        private static DateTime ParseDate(string dt)
        {
            return DateTime.ParseExact(dt, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}

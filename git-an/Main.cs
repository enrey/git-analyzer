using ConsoleTableExt;
using log4net;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace git_an
{
    internal class Main
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int INTERVAL = 7;

        internal static void Work(string[] args)
        {
            var inputData = ConsoleParamsParser.GetParams(args);

            var calculator = new Calculator(inputData);
            calculator.Init();

            var resultTable = new DataTable();
            resultTable.Columns.Add("Автор", typeof(string));

            var results = new List<Result>();

            var totalDays = (inputData.DtTill - inputData.DtFrom).Days;
            for (int i = 0; i <= totalDays; i = i + INTERVAL)
            {
                var dtStepFrom = inputData.DtFrom.AddDays(i);
                var dtStepTill = inputData.DtFrom.AddDays(i + INTERVAL);

                resultTable.Columns.Add(dtStepFrom.ToShortDateString() + " - " + dtStepTill.ToShortDateString(), typeof(int));

                var result = calculator.Calculate(dtStepFrom, dtStepTill);
                results.Add(result);
            }

            FillResultTable(results, resultTable);

            if (resultTable.Rows.Count != 0)
            {
                ConsoleTableBuilder.From(resultTable).ExportAndWriteLine();
            }
        }

        private static void FillResultTable(List<Result> results, DataTable resultTable)
        {
            var authors = results.SelectMany(o => o.Authors.Keys).Distinct().OrderBy(o => o).ToList();

            foreach (var author in authors)
            {
                var row = new List<object>();
                row.Add(author);

                foreach (var result in results)
                {
                    var r = result.Authors.Select(o => o).Where(o => o.Key == author).ToList();

                    if (r.Count != 0)
                        row.Add(r.Single().Value.Modified.ToString());
                    else
                        row.Add(0);
                }
                resultTable.Rows.Add(row.ToArray());
            }
        }
    }
}
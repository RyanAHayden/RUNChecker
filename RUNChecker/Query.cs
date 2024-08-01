using RUNChecker.Models;

namespace RUNChecker
{
    internal class Query
    {
        public static string GetArea<T>(RunCheckerContext runCheckerContext, string propertyName, Func<Area, T> selectExpression)
        {
            var query = (
                from app in runCheckerContext.Applications
                where app.Name.Equals(propertyName)
                join area in runCheckerContext.Areas
                on app.AreaId equals area.AreaId
                select selectExpression(area)).FirstOrDefault();

            return query?.ToString() ?? string.Empty;
        }
    }
}

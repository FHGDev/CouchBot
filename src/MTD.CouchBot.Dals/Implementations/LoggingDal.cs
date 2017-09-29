using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Dals.Implementations
{
    public class LoggingDal : ILoggingDal
    {
        //string MyConnection2 = "";
        //string MyConnection2 = "";

        public async Task LogPerformance(PerformanceMetrics metrics)
        {
            //var Query = "INSERT INTO `PerformanceLog`(`Platform`, `IsOwner`, `RunTime`, `ScheduledInterval`, `CreatedDate`) " +
            //    "VALUES('" + metrics.Platform + "'," + metrics.IsOwner + "," + metrics.RunTime + "," + 
            //    metrics.ScheduledInterval + ",'" + metrics.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss") + "')";

            //MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
            //MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            //MySqlDataReader MyReader2;
            //MyConn2.Open();
            //MyReader2 = MyCommand2.ExecuteReader();

            //while (MyReader2.Read())
            //{
            //}
            //MyConn2.Close();
            //await MyConn2.ClearPoolAsync(MyConn2);
            //await MyConn2.ClearAllPoolsAsync();
        }
    }
}

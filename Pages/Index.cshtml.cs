using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
namespace SBSTest.Pages;

public class IndexModel : PageModel
{
    //public NBAStat statData = new NBAStat();
    //public string? Name { get; set; }
    //public string? Stadium { get; set; }
    //public int? TotalPlayed { get; set; }
    //public int? won { get; set; }
    //public int? loss { get; set; }
    //public int? home { get; set; }
    //public int? away { get; set; }
    //public DateTime? lastDate { get; set; }

    public IActionResult OnGet()
    {
        //NBAStat stat = new NBAStat();
        //statData = stat.getData();

        //string htmlStr = "";
        string connectionString = "Server=localhost;Initial Catalog=NBA;User ID=sa;Password=myPassw0rd;Encrypt=False";
        SqlConnection con = new SqlConnection(connectionString);
        con.Open();

        string sqlQuery = @"
            with teamwon as(
            select id , sum(won) as won from (
            select hometeamid as id, count(* ) as won from games  g1
            where homescore>awayscore
            group by hometeamid
            union
            select awayteamid as id, count(* ) as won from games  g1
            where homescore<awayscore
            group by awayteamid ) hw
            group by id)
            , teamloss as(
            select id , sum(loss) as loss from (
            select hometeamid as id, count(* ) as loss from games  g1
            where homescore<awayscore
            group by hometeamid
            union
            select awayteamid as id, count(* ) as loss from games  g1
            where homescore>awayscore
            group by awayteamid ) hw
            group by id),
            lastplayed as (SELECT ID, MAX(LASTDATE)  as lastdate FROM (
            select HOMETEAMID AS ID,CAST(MAX(gamedatetime) AS DATE) AS LASTDATE from gamEs GROUP BY HOMETEAMID
            UNION  ALL
            select AWAYTEAMID AS ID ,CAST(MAX(gamedatetime) AS DATE) AS LASTDATE from gamEs GROUP BY AWAYTEAMID
            ) LASTDATE
            GROUP BY ID),
            palyedhome as (
            select hometeamid as id, count(*) as home from games 
            group by hometeamid
            ),
            palyedaway as (
            select awayteamid as id , count(*)as away from games
            group by  awayteamid )
            select teamid, name, stadium ,logo,url, ph.home+pa.away as totalplayed, won, loss ,home, away, lastdate from teams t join teamwon tw on t.teamid = tw.id
            join teamloss tl on t.teamid=tl.id
            join lastplayed lp on t.teamid=lp.id
            join palyedhome ph on t.teamid =ph.id
            join palyedaway pa on t.teamid= pa.id";


        SqlCommand cmd = new SqlCommand(sqlQuery, con);

        SqlDataReader? dr = null;
        dr = cmd.ExecuteReader();

        //NBAStat stat = new NBAStat();
        StringBuilder htmlTable = new StringBuilder();
        htmlTable.Append("<table width=100% border=1>");
        htmlTable.Append("<tr>");
        htmlTable.Append("<th>Name</th>");
        htmlTable.Append("<th>Stadium</th>");
        htmlTable.Append("<th>Logo</th>");
        htmlTable.Append("<th>TotalPlayed</th>");
        htmlTable.Append("<th>Won</th>");
        htmlTable.Append("<th>Loss</th>");
        htmlTable.Append("<th>Home</th>");
        htmlTable.Append("<th>Away</th>");
        htmlTable.Append("<th>LastDate</th>");
        htmlTable.Append("</tr>");

        while (dr.Read())
        {
            string Name = dr.GetString(1);
            string Stadium = dr.GetString(2);
            string logo = dr.GetString(3);
            string url = dr.GetString(4);
            int TotalPlayed = dr.GetInt32(5);
            int won = dr.GetInt32(6);
            int loss = dr.GetInt32(7);
            int home = dr.GetInt32(8);
            int away = dr.GetInt32(9);
            DateTime lastDate = dr.GetDateTime(10);
            
            htmlTable.Append("<tr style=dispay:block;padding-bottom:20px>");
            htmlTable.Append("<td align=center>" + Name + "</td>");
            htmlTable.Append("<td align=center>" + Stadium + "</td>");
            htmlTable.Append("<td align=center>");
            htmlTable.Append("<a href=" + url + " target=_blank>");
            htmlTable.Append("<img src="+logo+">");
            htmlTable.Append("</a>");
            htmlTable.Append("</td>");
            htmlTable.Append("<td align=center>" + TotalPlayed + "</td>");
            htmlTable.Append("<td align=center>" + won + "</td>");
            htmlTable.Append("<td align=center>" + loss + "</td>");
            htmlTable.Append("<td align=center>" + home + "</td>");
            htmlTable.Append("<td align=center>" + away + "</td>");
            htmlTable.Append("<td align=center>" + lastDate + "</td>");
            htmlTable.Append("<tr>");

            //htmlStr += "<table><tr><td>" + Name + "</td><td>" + Stadium + "</td><td>" + TotalPlayed + "</td><td>" + won + "</td><td>" + loss + "</td><td>" + home + "</td><td>" + away + "</td><td>" + lastDate + "</td><tr></table>";
        }
       
        htmlTable.Append("</table>");
        con.Close();

        return Content(htmlTable.ToString(), "text/html", System.Text.Encoding.UTF8);
    }
}



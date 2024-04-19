using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.seb
{
    public class RouterOverhead
    {
        virtual public Tuple<bool,string> CheckToken(NpgsqlCommand cmd, string token)
        {
            cmd.Parameters.AddWithValue("token", token);
            cmd.CommandText = "SELECT username FROM users WHERE token = @token";
            string? username=(string?)cmd.ExecuteScalar();
            if (String.IsNullOrEmpty( username))
                return Tuple.Create(false, "");
            return Tuple.Create(true, username);
        }
        virtual public NpgsqlCommand GetCommand()
        {
            var con = new NpgsqlConnection(
            connectionString: "Host=localhost;Username=postgres;Password=pwd;Database=seb");
            con.Open();
            var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            return cmd;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace RegToDatabase
{
    class Program
    {
        
        const string DB_IP = "front-office-operations.cpqyx233kbaf.us-east-1.rds.amazonaws.com";
        const string DB_USER = "admin";
        const string DB_PASS = "Farina100!";
        const string DB_DB = "front_office_operations";

        static string m_connectionString = "SERVER=" + DB_IP + ";" +
                                          "DATABASE=" + DB_DB + ";" +
                                          "UID=" + DB_USER + ";" +
                                          "PASSWORD=" + DB_PASS;

        static MySqlConnection m_sqlConn = new MySqlConnection(m_connectionString);
        static void Main(string[] args)
        {
            ParseRegFiles();

        }

      



        public static void ParseRegFiles()
        {
           // string path = System.IO.Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            FileInfo[] fiarr = di.GetFiles("*.reg");
           
            List<Group> groups = new List<Group>();
            foreach(FileInfo fri in fiarr)
            {
                Group group = new Group();
                List<EndPoint> endpoints = new List<EndPoint>();
              //  Console.WriteLine(fri.ToString());
               // Console.WriteLine("========================================");
                StreamReader sr = new StreamReader(fri.FullName, System.Text.Encoding.Default);

                string line;
                bool isEndpoint = false;
                string name = "";
                int index = -1;
                bool firstEndpoint = true;
                EndPoint endpoint = new EndPoint();
                while((line = sr.ReadLine())!=null)
                {
                    if (line.Length == 0 || line == "")
                        continue;


                    if (line.StartsWith("[HKEY")) 
                    {    
                        //tell if is a group or endpoint
                        IsEndpoint(line, out isEndpoint, out name, out index);


                        if (isEndpoint)
                        {
                            if (!firstEndpoint)
                                endpoints.Add(endpoint);
                            firstEndpoint = false;
                            endpoint = new EndPoint();
                            endpoint.index = index;
                            
                        }
                       
                        continue;                
                    }

                    if (!isEndpoint)
                    {
                        if (name.Length > 0)
                            group.name = name;

                        if (line.StartsWith("\"m_strServerTypes\""))
                        {
                            group.server_types = getValue(line);
                            continue;
                        }

                        if (line.StartsWith("\"m_zNoManFillFees\""))
                        {
                            group.no_manual_fill_fees = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_iPositionBackColorAdjust\""))
                        {
                            group.back_color = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_zManFillsEnabled\""))
                        {
                            group.manual_fills_enabled = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_zAltFillDB\""))
                        {
                            group.alt_fill_db = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_strFrontMonth\""))
                        {
                            group.front_month = getValue(line);
                            continue;
                        }


                        if (line.StartsWith("\"m_zManFillWipeEnabled\""))
                        {
                            group.manual_fill_wipe_enabled = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_zPositionSnapshots\""))
                        {
                            group.position_snapshot = Int32.Parse(getValue(line));
                            continue;
                        }


                        if (line.StartsWith("\"m_strManFillFilter\""))
                        {
                            group.maunal_fill_filter = getValue(line);
                            continue;
                        }


                        if (line.StartsWith("\"m_zAllDayPnlLogging\""))
                        {
                            group.all_day_pnl_logging = Int32.Parse(getValue(line));
                            continue;
                        }
                    

                        if (line.StartsWith("\"m_strAxPriceServer\""))
                        {
                            group.ax_price_server = getValue(line);
                            continue;
                        }

                    }
                    else
                    {
                        if (line.StartsWith("\"m_strName\""))
                        {
                            endpoint.name = getValue(line);
                            continue;
                        }


                        if (line.StartsWith("\"m_strServer\""))
                        {
                            endpoint.server = getValue(line);
                            continue;
                        }

                        if (line.StartsWith("\"m_iDisabled\""))
                        {
                            endpoint.is_disabled = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_zRequestHeartbeat\""))
                        {
                            endpoint.request_heartbeat = Int32.Parse(getValue(line));
                            continue;
                        }

                        if (line.StartsWith("\"m_iPort\""))
                        {
                            endpoint.port = getValue(line);
                            continue;
                        }

                        if (line.StartsWith("\"m_strContextPrefix\""))
                        {
                            endpoint.context_prefix = getValue(line);
                            continue;
                        }
                       
                    }

                }
               
                endpoints.Add(endpoint);
                group.endpoints = endpoints;
                groups.Add(group);
                    
                    
            }

            //foreach (Group g in groups)
            //{
            //    Console.WriteLine(g.name);
            //    foreach (EndPoint e in g.endpoints)
            //    {
            //        Console.WriteLine(e.index);
            //        Console.WriteLine(e.name);
            //    }
            //}

            StoreGroups2DB(groups)
            foreach (Group g in groups)
                StoreEndPoints2DB(g);

           
        }


        public static void StoreGroups2DB(List<Group> groups)
        {
           
            foreach (Group g in groups)
            {
                try
                {
                    string str = @"INSERT INTO groups
                               (name,server_types,back_color,manual_fills_enabled,alt_fill_db,no_manual_fill_fees,ax_price_server,man_fill_filter,is_position_snapshots,all_day_pnl_logging,front_month,manual_fill_wipe_enabled)
                               VALUES(?name,?server_types,?back_color,?manual_fills_enabled,?alt_fill_db,?no_manual_fill_fees,?ax_price_server,?man_fill_filter,?is_position_snapshots,?all_day_pnl_logging,?front_month,?manual_fill_wipe_enabled) ";
                    MySqlCommand cmd = new MySqlCommand(str, m_sqlConn);

                    cmd.Parameters.AddWithValue("?name", g.name.Length == 0 ? "" : g.name);
                    cmd.Parameters.AddWithValue("?server_types", g.server_types==null ? "" : g.server_types);
                    cmd.Parameters.AddWithValue("?back_color", g.back_color);
                    cmd.Parameters.AddWithValue("?manual_fills_enabled", g.manual_fills_enabled);
                    cmd.Parameters.AddWithValue("?alt_fill_db", g.alt_fill_db);
                    cmd.Parameters.AddWithValue("?no_manual_fill_fees", g.no_manual_fill_fees);
                    cmd.Parameters.AddWithValue("?ax_price_server", g.ax_price_server==null ? "" : g.ax_price_server);
                    cmd.Parameters.AddWithValue("?man_fill_filter", g.maunal_fill_filter==null? "" : g.maunal_fill_filter);
                    cmd.Parameters.AddWithValue("?is_position_snapshots", g.position_snapshot);
                    cmd.Parameters.AddWithValue("?all_day_pnl_logging", g.all_day_pnl_logging);
                    cmd.Parameters.AddWithValue("?front_month", g.front_month==null? "" : g.front_month);
                    cmd.Parameters.AddWithValue("?manual_fill_wipe_enabled", g.manual_fill_wipe_enabled);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                catch (MySqlException ex) 
                {
                    Console.WriteLine(ex.ToString());                
                }
            }
        }

        public static void StoreEndPoints2DB(Group group) 
        {       

            string str = @"Select groups.index from groups where name = ?name";
            MySqlCommand cmd = new MySqlCommand(str, m_sqlConn);
            cmd.Parameters.AddWithValue("?name", group.name);
            cmd.Connection.Open();

            MySqlDataReader reader = cmd.ExecuteReader();
            int group_index = -1;
            while (reader.Read())
            {
                group_index = reader.GetInt32(0);
            }

            cmd.Connection.Close();

            if (group_index == -1)
                return;

            foreach (EndPoint e in group.endpoints)
            {

                try
                {
                    str = @"INSERT INTO endpoints
                        (index_endpoint, index_group, name, server, request_heartbeat, port, context_prefix, conn_group)
                         VALUES(?index_endpoint, ?index_group, ?name, ?server, ?request_heartbeat, ?port, ?context_prefix, ?conn_group)";
                    cmd = new MySqlCommand(str, m_sqlConn);

                    cmd.Parameters.AddWithValue("?index_endpoint", e.index);
                    cmd.Parameters.AddWithValue("?index_group", group_index);
                    cmd.Parameters.AddWithValue("?name", e.name == null ? "" : e.name);
                    cmd.Parameters.AddWithValue("?server", e.server == null ? "" : e.server);
                    cmd.Parameters.AddWithValue("?request_heartbeat", e.request_heartbeat);
                    cmd.Parameters.AddWithValue("?port", e.port == null ? "" : e.port);
                    cmd.Parameters.AddWithValue("?context_prefix", e.context_prefix == null ? "" : e.context_prefix);
                    cmd.Parameters.AddWithValue("?conn_group", e.is_conn_group);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        public static string getValue(string str)
        {
            
            string[] tmps = str.Split('=');
            string value;
            if (tmps[1].StartsWith("dword"))
            {
                value = tmps[1].Substring(6).TrimStart(new Char[] { '0' });
                if (value.Length == 0)
                    value = "0";
            }
            else
                value = tmps[1].Substring(1, tmps[1].Length - 2);
            return value;
            
        }

        public static void IsEndpoint(string str, out bool is_endpoint, out string name, out int index)
        {
           // Console.WriteLine(str);
            string[] strs = str.Split('\\');
            name = "";
            index = -1;

            string tmp = strs[strs.Length - 1];
            tmp = tmp.Substring(0, tmp.Length - 1);
            int n;
            bool isNumeric = int.TryParse(tmp,out n);
            if (!isNumeric)
                name = tmp;
            else
                index = Int32.Parse(tmp);

            if (name == "ConsolidatedPositions")
                name = "prod";
            is_endpoint = isNumeric;
         
        }

        
    }

    public class Group
    {
        public List<EndPoint> endpoints;
        public string name { get; set; }
        public string server_types { get; set; }
        public int back_color { get; set; }
        public int manual_fills_enabled { get; set; }
        public int alt_fill_db { get; set; }
        public int no_manual_fill_fees { get; set; }

        public string front_month { get; set; }

        public int manual_fill_wipe_enabled { get; set; }
        public int position_snapshot { get; set; }

        public string maunal_fill_filter { get; set; }
        public int all_day_pnl_logging { get; set; }
        public string ax_price_server { get; set; }
    }


    public class EndPoint
    {
        public int index   {get;set;}
        public string name { get; set; }
        public string server { get; set; }
        public int is_disabled { get; set; }
        public int request_heartbeat { get; set; }
        public string port { get; set; }
        public string context_prefix { get; set; }
        public int is_conn_group { get; set; }
    }

}

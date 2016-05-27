//REFERENCE C:\Program Files (x86)\MySQL\Connector NET 6.8.3\Assemblies\v4.0\MySql.Data.dll
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using MySql.Data.MySqlClient;

namespace BioSero.ReadyLabel.Scripting
{
    public class Script
    {
        //MySqlConnection Connection;
        public void OnStart(int numberOfLabels)
        {
            //This procedure is called a the start of a batch generate. One use for this would be to open a data connection. See Below.
            //string server = "localhost";
            //string database = "sequence";
            //string uid = "testuser";
            //string password = "pa55word";

            //string connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            //Connection = new MySqlConnection(connectionString);
            //Connection.Open();
        }

        public void Run(Dictionary<string,string> data, BioSero.ReadyLabel.LabelTemplate template, BioSero.ReadyLabel.Label label)
        {
            //This procedure is called everytime a label is printed. Use it to add data to a label.
            if (!data.ContainsKey("Data"))
            {
                data.Add("Data", "Data Test"); //Data Test will be aplied to the field {Data}
            }
            else
            {
                data["Data"] = "Data Test";
            }

            //This can also pull in data from the source
            //MySqlCommand command = new MySqlCommand("Select nextval('Sequence1')", Connection);
            //string result = command.ExecuteScalar().ToString();
            //if (!data.ContainsKey("Result"))
            //{
            //    data.Add("Result", result); //the result of the sql evaluation will be aplied to the field {Result}
            //}
            //else
            //{
            //    data["Result"] = result;
            //}
        }

        public void OnEnd()
        {
            //This procedure is called a the end of a batch generate. Here you can terminate any processes or connections.
            //Connection.Close();
        }
    }
}

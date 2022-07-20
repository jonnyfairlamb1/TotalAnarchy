using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Helpers;
using System.Threading;
using System.Data.SQLite;

namespace DatabaseServer.Network {
    class Database {
        public static SQLiteConnection db_connection;

        public static void InitializeDatabase() {
            db_connection = new SQLiteConnection("Data Source=TotalAnarchyData.db");
            db_connection.Open();
        }

        public static bool CheckUsername(string Username) {
            string sql = "SELECT * FROM Accounts WHERE Username='" + Username + "'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read()) {
                reader.Close();
                //a user with that username already exists
                return true;
            } else {
                reader.Close();
                //a user with that username doesnt exist
                return false;
            }
        }

        public static bool CheckEmail(string Email) {
            string sql = "SELECT * FROM Accounts WHERE Email='" + Email + "'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read()) {
                reader.Close();
                //a user with that username already exists
                return true;
            } else {
                reader.Close();
                //a user with that username doesnt exist
                return false;
            }
        }

        public static void CreateAccount(string Username, string Password, string Email, int connectionID) {
            string sql = "INSERT INTO Accounts (Email, Username, Password) VALUES ('" + Email + "','" + Username + "','" + Password + "')";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();

            Logger.WriteLog("Account Created With Name: " + Username);
            
        }

        public static bool AccountDetailsAreCorrect(string Email, string Password) {
            string sql = "SELECT * FROM Accounts WHERE Email='" + Email + "' AND Password='" + Password + "'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read()) {
                reader.Close();
                return true;
            } else {
                reader.Close();
                return false;
            }
        }
    }
}

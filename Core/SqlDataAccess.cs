/// Selim Özbudak
/*
 *  24.11.2008 SÖ  - SqlDataAccess is created.
 *  03.12.2008 SÖ  - Connectionstring is made singleton.
 *  12.12.2008 SÖ  - ExecuteNonQuery has been changed. 
 *                   (now it is being use for insert, update, delete queries.)
 *  12.12.2008 SÖ  - ExecuteStoredProcedure has been created.                
 */
///

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data.SqlClient;

/// <summary>
/// SqlDataAccess is being used for accessing MSSQL database quickly and easily. 
/// Requires a connection string that is named MsSql defined on web.config file. This connection string is used as default. 
/// For using different connection strings you should pass the name of the connection string as a parameter with methods.
/// </summary>
public class SqlDataAccess
{
    // Default connection string. a connection named MsSql should be defined in web.config file.
    public const string CONNECTION_STRING_NAME = "DefaultConnection";

    //This returns the connection string  
    private static string _connectionString = string.Empty;
    public static string ConnectionString
    {
        get
        {
            if (_connectionString == string.Empty)
            {
                _connectionString = ""; // ConfigurationManager.ConnectionStrings[CONNECTION_STRING_NAME].ConnectionString;
            }
            return _connectionString;
        }
    }


    /// <summary>
    /// Brings a SqlCommand object to be able to add some parameters in it. After you send this to Execute method.
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public SqlCommand GetCommand(string sql)
    {   
        SqlConnection conn = new SqlConnection(ConnectionString);
        SqlCommand sqlCmd = new SqlCommand(sql, conn);
        return sqlCmd;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public DataTable Execute(string sql)
    {
        DataTable dt = new DataTable();
        SqlCommand cmd = GetCommand(sql);
        cmd.Connection.Open();
        dt.Load(cmd.ExecuteReader());
        cmd.Connection.Close();
        return dt;
    }

    /// <summary>
    /// Datatable Döndür
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public DataTable Execute(SqlCommand command)
    {
        DataTable dt = new DataTable();
        command.Connection.Open();
        //command.ExecuteNonQuery();
        dt.Load(command.ExecuteReader());
        command.Connection.Close();
        return dt;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public int ExecuteNonQuery(string sql)
    {
        SqlCommand cmd = GetCommand(sql);
        cmd.Connection.Open();
        int result = cmd.ExecuteNonQuery();
        cmd.Connection.Close();
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public int ExecuteNonQuery(SqlCommand command)
    {
        command.Connection.Open();
        int result = command.ExecuteNonQuery();
        command.Connection.Close();
        return result;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="spName"></param>
    /// <returns></returns>
    public int ExecuteStoredProcedure(string spName)
    {
        SqlCommand cmd = GetCommand(spName);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection.Open();
        int result = cmd.ExecuteNonQuery();
        cmd.Connection.Close();
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public int ExecuteStoredProcedure(SqlCommand command)
    {
        command.CommandType = CommandType.StoredProcedure;
        command.Connection.Open();
        int result = command.ExecuteNonQuery();
        command.Connection.Close();
        return result;
    }

}

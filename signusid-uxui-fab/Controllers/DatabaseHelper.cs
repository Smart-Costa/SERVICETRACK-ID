using System.Configuration;
using System.Data.SqlClient;

public class DatabaseHelper
{
  public SqlConnection GetConnection()
  {
    // Obtiene la cadena de conexión desde el archivo app.config
    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServerDiverscan"].ConnectionString;

    // Crea y devuelve una nueva conexión SQL utilizando la cadena de conexión obtenida
    return new SqlConnection(connectionString);
  }
}

using FlexSpace.DAL.Entities;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL;

public class ClienteRepository
{
    private const string Columnas = "Id, Nombre, Email, TipoCliente, SancionesActivas";

    public Cliente? ObtenerPorId(int id)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columnas} FROM Cliente WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public void IncrementarSanciones(int id)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Cliente SET SancionesActivas = SancionesActivas + 1 WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public List<Cliente> ListarSancionados()
    {
        var lista = new List<Cliente>();
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columnas} FROM Cliente WHERE SancionesActivas > 0 ORDER BY SancionesActivas DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    private static Cliente Mapear(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Nombre = r.GetString(1),
        Email = r.GetString(2),
        TipoCliente = Enum.Parse<TipoCliente>(r.GetString(3)),
        SancionesActivas = r.GetInt32(4)
    };
}
